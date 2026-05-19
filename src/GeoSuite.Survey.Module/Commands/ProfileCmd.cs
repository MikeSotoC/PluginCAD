using GeoSuite.Core.Models;
using GeoSuite.Core.Algorithms;
using GeoSuite.Platform;

namespace GeoSuite.Survey.Module.Commands;

public class ProfileCmd
{
    public void Execute()
    {
        var cad = CadServiceFactory.Create();
        
        cad.ShowMessage("Perfil Longitudinal", "Seleccione polilínea o ingrese puntos...");
        
        // Simulación: Puntos a lo largo de un alineamiento
        var alignmentPoints = new List<Coordinate3>
        {
            new Coordinate3(0, 0, 0),
            new Coordinate3(10, 0, 0),
            new Coordinate3(20, 0, 0),
            new Coordinate3(30, 0, 0),
            new Coordinate3(40, 0, 0),
            new Coordinate3(50, 0, 0)
        };

        // Simulación de superficie TIN (en producción se interpola Z real)
        var surfacePoints = new List<SurveyPoint>
        {
            new SurveyPoint(1, 0, 0, 100),
            new SurveyPoint(2, 10, 0, 102),
            new SurveyPoint(3, 20, 0, 101.5),
            new SurveyPoint(4, 30, 0, 103),
            new SurveyPoint(5, 40, 0, 102.5),
            new SurveyPoint(6, 50, 0, 104)
        };

        // Interpolar elevaciones del terreno en los puntos del alineamiento
        var profileData = new List<(double Station, double Elev)>();
        foreach (var pt in alignmentPoints)
        {
            // En producción: Triangulation.InterpolateZ(surfacePoints, pt.X, pt.Y)
            double z = surfacePoints.Find(p => Math.Abs(p.Location.X - pt.X) < 0.1)?.Location.Z ?? 100;
            profileData.Add((pt.X, z));
        }

        // Dibujar cuadrícula del perfil
        cad.CreateLayerIfNotExists("TOPO-PERFIL-GRID", "8"); // Gris
        cad.CreateLayerIfNotExists("TOPO-PERFIL-LINE", "1"); // Rojo
        cad.CreateLayerIfNotExists("TOPO-PERFIL-TEXT", "3"); // Verde

        double startX = 100;
        double startY = 100;
        double scaleX = 10; // 10 unidades CAD = 1 estación
        double scaleY = 5;  // Escala vertical exagerada

        // Dibujar líneas horizontales (elevaciones)
        for (int i = 95; i <= 110; i += 1)
        {
            double y = startY - (i - 95) * scaleY;
            cad.DrawLine(
                new Coordinate3(startX, y, 0),
                new Coordinate3(startX + 60, y, 0),
                "TOPO-PERFIL-GRID"
            );
            
            if (i % 5 == 0)
            {
                cad.AddText($"{i}", new Coordinate3(startX - 5, y, 0), 2.5, "TOPO-PERFIL-TEXT");
            }
        }

        // Dibujar líneas verticales (estaciones)
        for (int i = 0; i <= 50; i += 5)
        {
            double x = startX + (i / 10.0) * scaleX;
            cad.DrawLine(
                new Coordinate3(x, startY, 0),
                new Coordinate3(x, startY - 75, 0),
                "TOPO-PERFIL-GRID"
            );
            
            cad.AddText($"{i}", new Coordinate3(x, startY + 3, 0), 2.5, "TOPO-PERFIL-TEXT");
        }

        // Dibujar línea de terreno
        var terrainVertices = new List<Coordinate3>();
        foreach (var (station, elev) in profileData)
        {
            double x = startX + (station / 10.0) * scaleX;
            double y = startY - (elev - 95) * scaleY;
            terrainVertices.Add(new Coordinate3(x, y, 0));
        }
        
        if (terrainVertices.Count > 1)
        {
            cad.DrawPolyline(terrainVertices, false, "TOPO-PERFIL-LINE");
        }

        cad.ShowMessage("Perfil Generado", $"Perfil dibujado con {profileData.Count} estaciones.");
    }
}
