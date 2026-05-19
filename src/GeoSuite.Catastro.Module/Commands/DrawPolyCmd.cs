using GeoSuite.Core.Models;
using GeoSuite.Platform;
using GeoSuite.Settings.Services;

namespace GeoSuite.Catastro.Module.Commands;

/// <summary>
/// Comando: GS-C-POLY
/// Dibuja polígono catastral desde coordenadas o selección
/// Usa configuración global para tamaños de texto dinámicos.
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

        // Obtener configuración global para tamaño de texto dinámico
        var settings = SettingsManager.Load();
        double textSize = SettingsManager.GetScaledTextSize(1.0); // Tamaño base
        
        string label = $"Área: {area:N{settings.Catastro.AreaDecimals}} m²\nPerím: {perimeter:N{settings.Catastro.DistanceDecimals}} m";
        _cad.AddText(label, centroid, textSize, "CATASTRO-TEXTO");

        _cad.ShowMessage("Éxito", $"Polígono dibujado. Área: {area:N2} m²");
    }
}
