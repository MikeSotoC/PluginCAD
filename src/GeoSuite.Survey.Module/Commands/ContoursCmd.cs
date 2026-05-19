using GeoSuite.Core.Models;
using GeoSuite.Platform;

namespace GeoSuite.Survey.Module.Commands;

/// <summary>
/// Comando: GS-T-CN
/// Genera curvas de nivel desde una superficie TIN
/// </summary>
public class ContoursCmd
{
    private readonly ICadHost _cad;

    public ContoursCmd(ICadHost cad)
    {
        _cad = cad;
    }

    public void Execute(List<SurveyPoint> points, double interval)
    {
        if (points.Count < 3)
        {
            _cad.ShowMessage("Error", "Se requieren al menos 3 puntos para generar curvas.");
            return;
        }

        _cad.ShowMessage("Curvas de Nivel", $"Generando curvas con intervalo {interval}m...");

        // Calcular Z min y max
        double zMin = points.Min(p => p.Location.Z);
        double zMax = points.Max(p => p.Location.Z);

        // Redondear a múltiplos del intervalo
        double startZ = Math.Floor(zMin / interval) * interval;
        double endZ = Math.Ceiling(zMax / interval) * interval;

        _cad.CreateLayerIfNotExists("TOPO-CURVAS-PRINC", "2"); // Amarillo
        _cad.CreateLayerIfNotExists("TOPO-CURVAS-SEC", "8"); // Gris

        int contourCount = 0;
        for (double z = startZ; z <= endZ; z += interval)
        {
            bool isPrincipal = Math.Abs(z % (interval * 5)) < 0.001; // Cada 5 curvas es principal
            string layer = isPrincipal ? "TOPO-CURVAS-PRINC" : "TOPO-CURVAS-SEC";

            // Placeholder: Aquí iría el algoritmo de interpolación de curvas
            // Por ahora dibujamos un círculo示意 en el centroide
            var centroid = new Coordinate3(
                points.Average(p => p.Location.X),
                points.Average(p => p.Location.Y),
                z
            );

            // En implementación real, se interpola la intersección del plano Z=constante con el TIN
            contourCount++;
        }

        _cad.ShowMessage("Éxito", $"{contourCount} curvas de nivel generadas (simulado).");
    }
}
