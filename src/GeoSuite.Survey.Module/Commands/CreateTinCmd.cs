using GeoSuite.Core.Algorithms;
using GeoSuite.Core.Models;
using GeoSuite.Platform;

namespace GeoSuite.Survey.Module.Commands;

/// <summary>
/// Comando: GS-T-TIN
/// Genera malla triangular (TIN) desde puntos topográficos
/// </summary>
public class CreateTinCmd
{
    private readonly ICadHost _cad;

    public CreateTinCmd(ICadHost cad)
    {
        _cad = cad;
    }

    public void Execute(List<SurveyPoint> points)
    {
        if (points.Count < 3)
        {
            _cad.ShowMessage("Error", "Se requieren al menos 3 puntos para generar TIN.");
            return;
        }

        _cad.ShowMessage("Generar TIN", $"Triangulando {points.Count} puntos...");

        try
        {
            var triangles = Triangulation.GenerateDelaunay(points);

            _cad.CreateLayerIfNotExists("TOPO-TIN", "4"); // Cyan

            foreach (var tri in triangles)
            {
                var vertices = new List<Coordinate3> { tri.P1, tri.P2, tri.P3 };
                _cad.DrawPolyline(vertices, true, "TOPO-TIN");
            }

            _cad.ShowMessage("Éxito", $"TIN generado con {triangles.Count} triángulos.");
        }
        catch (Exception ex)
        {
            _cad.ShowMessage("Error", $"Falló la triangulación: {ex.Message}");
        }
    }
}
