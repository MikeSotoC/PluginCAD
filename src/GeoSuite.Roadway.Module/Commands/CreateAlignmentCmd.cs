using GeoSuite.Core.Models;
using GeoSuite.Platform;

namespace GeoSuite.Roadway.Module.Commands;

/// <summary>
/// Comando: GS-R-ALIGN
/// Crea alineamiento horizontal de vía
/// </summary>
public class CreateAlignmentCmd
{
    private readonly ICadHost _cad;

    public CreateAlignmentCmd(ICadHost cad)
    {
        _cad = cad;
    }

    public void Execute(List<Coordinate3> points)
    {
        if (points.Count < 2)
        {
            _cad.ShowMessage("Error", "Se requieren al menos 2 puntos para el alineamiento.");
            return;
        }

        _cad.CreateLayerIfNotExists("VIAS-EJE", "1"); // Rojo
        _cad.CreateLayerIfNotExists("VIAS-PI", "3"); // Verde

        // Dibujar eje
        _cad.DrawPolyline(points, false, "VIAS-EJE");

        // Marcar PIs (Puntos de Intersección)
        for (int i = 1; i < points.Count - 1; i++)
        {
            _cad.DrawCircle(points[i], 0.75, "VIAS-PI");
            _cad.AddText($"PI-{i}", new Coordinate3(points[i].X, points[i].Y + 1.5, points[i].Z), 1.5, "VIAS-PI");
        }

        _cad.ShowMessage("Éxito", $"Alineamiento creado con {points.Count} vértices.");
    }
}
