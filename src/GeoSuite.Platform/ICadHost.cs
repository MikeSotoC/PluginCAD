using GeoSuite.Core.Models;

namespace GeoSuite.Platform;

public interface ICadHost
{
    string ActiveDocumentName { get; }
    
    void DrawPoint(Coordinate3 pt, string layer = "0");
    void DrawLine(Coordinate3 start, Coordinate3 end, string layer = "0");
    void DrawPolyline(List<Coordinate3> vertices, bool closed, string layer = "0");
    void AddText(string content, Coordinate3 location, double height, string layer = "TEXTOS");
    void DrawCircle(Coordinate3 center, double radius, string layer = "0");
    
    void SetCurrentLayer(string layerName);
    void CreateLayerIfNotExists(string name, string colorIndex = "7");
    void ShowMessage(string title, string message);
    void SendCommand(string commandStr);
}
