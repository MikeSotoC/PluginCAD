using GeoSuite.Core.Models;
using GeoSuite.Platform;

namespace GeoSuite.Catastro.Module.Commands;

/// <summary>
/// Comando: GS-C-POLY
/// Dibuja polígono catastral desde coordenadas o selección
/// </summary>
public class DrawPolyCmd
{
    private readonly ICadHost _cad;

    public DrawPolyCmd(ICadHost cad)
    {
        _cad = cad;
    }

    public void Execute(List<Coordinate3> vertices)
    {
        if (vertices.Count < 3)
        {
            _cad.ShowMessage("Error", "Se requieren al menos 3 vértices para dibujar un polígono.");
            return;
        }

        var polygon = new Polygon2D { Vertices = vertices };

        _cad.CreateLayerIfNotExists("CATASTRO-LINDEROS", "1"); // Rojo
        _cad.CreateLayerIfNotExists("CATASTRO-TEXTO", "7"); // Blanco

        // Dibujar polígono cerrado
        _cad.DrawPolyline(vertices, true, "CATASTRO-LINDEROS");

        // Calcular y mostrar área
        double area = polygon.Area;
        double perimeter = polygon.Perimeter;
        var centroid = polygon.Centroid;

        string label = $"Área: {area:N2} m²\nPerím: {perimeter:N2} m";
        _cad.AddText(label, centroid, 2.5, "CATASTRO-TEXTO");

        _cad.ShowMessage("Éxito", $"Polígono dibujado. Área: {area:N2} m²");
    }
}
