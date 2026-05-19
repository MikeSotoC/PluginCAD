using GeoSuite.Core.Models;

namespace GeoSuite.Platform;

public class MockCadHost : ICadHost
{
    public string ActiveDocumentName => "Documento_Prueba.dwg";

    public void DrawPoint(Coordinate3 pt, string layer = "0")
        => System.Diagnostics.Debug.WriteLine($"[CAD] Draw Point at {pt} on layer {layer}");

    public void DrawLine(Coordinate3 start, Coordinate3 end, string layer = "0")
        => System.Diagnostics.Debug.WriteLine($"[CAD] Draw Line {start} -> {end}");

    public void DrawPolyline(List<Coordinate3> vertices, bool closed, string layer = "0")
        => System.Diagnostics.Debug.WriteLine($"[CAD] Draw Polyline ({(closed ? "closed" : "open")}) with {vertices.Count} verts");

    public void DrawCircle(Coordinate3 center, double radius, string layer = "0")
        => System.Diagnostics.Debug.WriteLine($"[CAD] Draw Circle center={center} r={radius}");

    public void AddText(string content, Coordinate3 location, double height, string layer = "TEXTOS")
        => System.Diagnostics.Debug.WriteLine($"[CAD] Text '{content}' at {location} h={height}");

    public void SetCurrentLayer(string layerName)
        => System.Diagnostics.Debug.WriteLine($"[CAD] Set layer: {layerName}");

    public void CreateLayerIfNotExists(string name, string colorIndex = "7")
        => System.Diagnostics.Debug.WriteLine($"[CAD] Create layer: {name} (color {colorIndex})");

    public void ShowMessage(string title, string message)
        => System.Diagnostics.Debug.WriteLine($"[MSG] {title}: {message}");

    public void SendCommand(string commandStr)
        => System.Diagnostics.Debug.WriteLine($"[CMD] Executing: {commandStr}");
}
