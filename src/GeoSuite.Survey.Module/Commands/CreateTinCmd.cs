using GeoSuite.Core.Algorithms;
using GeoSuite.Core.Models;
using GeoSuite.Platform;

namespace GeoSuite.Survey.Module.Commands;

/// <summary>
/// Comando: GS-T-TIN
/// Genera malla triangular (TIN) desde puntos de topografía.
/// </summary>
public class CreateTinCmd
{
    private readonly ICadHost _cad;

    public CreateTinCmd(ICadHost cad)
    {
        _cad = cad;
    }

    public void Execute()
    {
        _cad.ShowMessage("Generar TIN", "Seleccionando puntos de topografía...");

        // TODO: Implementar selección de puntos del dibujo o base de datos
        // Por ahora usa puntos de ejemplo
        var points = new List<SurveyPoint>
        {
            new SurveyPoint(1, 0, 0, 100, "BM", "BENCH"),
            new SurveyPoint(2, 10, 0, 102, "PI", "PATH"),
            new SurveyPoint(3, 5, 10, 105, "PI", "PATH"),
            new SurveyPoint(4, 15, 8, 103, "PI", "PATH")
        };

        try
        {
            _cad.CreateLayerIfNotExists("TOPO-TIN", "5"); // Azul
            
            // Generar triangulación (pendiente de implementación completa)
            // var triangles = Triangulation.GenerateDelaunay(points);
            
            // Dibujar triángulos de ejemplo
            for (int i = 0; i < points.Count - 1; i++)
            {
                for (int j = i + 1; j < points.Count; j++)
                {
                    _cad.DrawLine(points[i].Location, points[j].Location, "TOPO-TIN");
                }
            }

            _cad.ShowMessage("TIN Generado", $"Se crearon líneas de triangulación para {points.Count} puntos.");
        }
        catch (System.Exception ex)
        {
            _cad.ShowMessage("Error TIN", ex.Message);
        }
    }
}
