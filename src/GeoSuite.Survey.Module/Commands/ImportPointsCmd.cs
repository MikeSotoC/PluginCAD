using GeoSuite.Core.Models;
using GeoSuite.Platform;
using GeoSuite.UI.WinForms;
using System.Windows.Forms;

namespace GeoSuite.Survey.Module.Commands;

/// <summary>
/// Comando: GS-T-IMP
/// Importa puntos desde archivo CSV (E,N,Z,Codigo,Descripcion)
/// Muestra diálogo WinForms con vista previa.
/// </summary>
public class ImportPointsCmd
{
    private readonly ICadHost _cad;

    public ImportPointsCmd(ICadHost cad)
    {
        _cad = cad;
    }

    public void Execute()
    {
        // Mostrar diálogo WinForms personalizado
        using var dlg = new ImportPointsDialog();
        
        if (dlg.ShowDialog() != DialogResult.OK)
            return;

        if (string.IsNullOrEmpty(dlg.SelectedFilePath))
            return;

        try
        {
            var points = ParseCsvFile(dlg.SelectedFilePath);
            
            if (points.Count == 0)
            {
                _cad.ShowMessage("Error", "No se encontraron puntos válidos en el archivo.");
                return;
            }

            _cad.CreateLayerIfNotExists(dlg.LayerName, "1"); // Rojo
            _cad.CreateLayerIfNotExists("TOPO-DATOS", "3"); // Verde

            foreach (var pt in points)
            {
                // Dibujar punto (círculo pequeño)
                _cad.DrawCircle(pt.Location, 0.5, dlg.LayerName);
                
                // Etiquetar
                string label = $"{pt.Id}\n{pt.Z:N2}";
                _cad.AddText(label, new Coordinate3(pt.Location.X, pt.Location.Y + 1.5, pt.Location.Z), 1.5, "TOPO-DATOS");
            }

            // Opcional: conectar con polilínea 3D
            if (dlg.Use3DPolylines && points.Count > 1)
            {
                _cad.CreateLayerIfNotExists("TOPO-ENLACE", "4"); // Cyan
                _cad.DrawPolyline(points.Select(p => p.Location).ToList(), false, "TOPO-ENLACE");
            }

            _cad.ShowMessage("Éxito", $"{points.Count} puntos importados correctamente en capa '{dlg.LayerName}'.");
        }
        catch (Exception ex)
        {
            _cad.ShowMessage("Error de Importación", ex.Message);
        }
    }

    private List<SurveyPoint> ParseCsvFile(string filePath)
    {
        var points = new List<SurveyPoint>();
        int idCounter = 1;

        foreach (var line in File.ReadLines(filePath))
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                continue;

            var parts = line.Split(new[] { ',', ';', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            
            if (parts.Length >= 3)
            {
                // Formato: Este, Norte, Elevacion (X, Y, Z)
                if (double.TryParse(parts[0], out double e) &&
                    double.TryParse(parts[1], out double n) &&
                    double.TryParse(parts[2], out double z))
                {
                    string desc = parts.Length > 4 ? parts[4].Trim() : "";
                    string code = parts.Length > 3 ? parts[3].Trim() : "";
                    
                    points.Add(new SurveyPoint(idCounter++, e, n, z, desc, code));
                }
            }
        }

        return points;
    }
}
