using GeoSuite.Core.Models;
using GeoSuite.Platform;

namespace GeoSuite.Hydraulics.Module.Commands;

/// <summary>
/// Comando: GS-H-DRAIN
/// Crea red de drenaje (tuberías y pozos)
/// </summary>
public class CreateDrainageCmd
{
    private readonly ICadHost _cad;

    public CreateDrainageCmd(ICadHost cad)
    {
        _cad = cad;
    }

    public void Execute(List<Coordinate3> manholePoints, List<(Coordinate3 Start, Coordinate3 End)> pipeSegments)
    {
        _cad.CreateLayerIfNotExists("DRENAJE-POZOS", "1"); // Rojo
        _cad.CreateLayerIfNotExists("DRENAJE-TUBERIA", "4"); // Cyan
        _cad.CreateLayerIfNotExists("DRENAJE-TEXTO", "7"); // Blanco

        // Dibujar pozos de visita
        foreach (var mh in manholePoints)
        {
            _cad.DrawCircle(mh, 1.0, "DRENAJE-POZOS");
            _cad.AddText($"MH", new Coordinate3(mh.X, mh.Y + 1.5, mh.Z), 1.2, "DRENAJE-TEXTO");
        }

        // Dibujar tuberías
        foreach (var seg in pipeSegments)
        {
            _cad.DrawLine(seg.Start, seg.End, "DRENAJE-TUBERIA");
        }

        _cad.ShowMessage("Éxito", $"Red de drenaje creada: {manholePoints.Count} pozos, {pipeSegments.Count} tuberías.");
    }
}
