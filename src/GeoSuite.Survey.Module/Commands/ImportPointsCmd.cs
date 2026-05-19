using GeoSuite.Core.Models;
using GeoSuite.Platform;

namespace GeoSuite.Survey.Module.Commands;

/// <summary>
/// Comando: GS-T-IMP
/// Importa puntos desde archivo CSV (N,E,Z,Codigo,Descripcion)
/// </summary>
public class ImportPointsCmd
{
    private readonly ICadHost _cad;

    public ImportPointsCmd(ICadHost cad)
    {
        _cad = cad;
    }

    public void Execute(string filePath)
    {
        if (!File.Exists(filePath))
        {
            _cad.ShowMessage("Error", $"El archivo no existe: {filePath}");
            return;
        }

        var points = new List<SurveyPoint>();
        var lines = File.ReadAllLines(filePath);

        foreach (var line in lines.Skip(1)) // Skip header si existe
        {
            var parts = line.Split(',');
            if (parts.Length < 3) continue;

            if (int.TryParse(parts[0], out int id) &&
                double.TryParse(parts[1], out double e) &&
                double.TryParse(parts[2], out double n) &&
                double.TryParse(parts.Length > 3 ? parts[3] : "0", out double z))
            {
                string desc = parts.Length > 4 ? parts[4] : "";
                string code = parts.Length > 5 ? parts[5] : "";
                points.Add(new SurveyPoint(id, e, n, z, desc, code));
            }
        }

        if (points.Count == 0)
        {
            _cad.ShowMessage("Advertencia", "No se encontraron puntos válidos en el archivo.");
            return;
        }

        // Crear capas
        _cad.CreateLayerIfNotExists("TOPO-PUNTOS", "1"); // Rojo
        _cad.CreateLayerIfNotExists("TOPO-DATOS", "3"); // Verde
        _cad.CreateLayerIfNotExists("TOPO-NODES", "2"); // Amarillo

        // Dibujar puntos
        foreach (var pt in points)
        {
            _cad.DrawCircle(pt.Location, 0.5, "TOPO-PUNTOS");
            
            string label = $"{pt.Id}\n{pt.Z:N2}";
            _cad.AddText(label, new Coordinate3(pt.Location.X, pt.Location.Y + 1.5, pt.Location.Z), 1.5, "TOPO-DATOS");
        }

        _cad.ShowMessage("Éxito", $"{points.Count} puntos importados correctamente.");
    }
}
