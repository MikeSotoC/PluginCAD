using GeoSuite.Core.Models;

namespace GeoSuite.Core.Algorithms;

public static class Triangulation
{
    /// <summary>
    /// Genera una malla TIN usando algoritmo Delaunay.
    /// </summary>
    public static List<(Coordinate3 P1, Coordinate3 P2, Coordinate3 P3)> GenerateDelaunay(List<SurveyPoint> points)
    {
        var triangles = new List<(Coordinate3, Coordinate3, Coordinate3)>();
        
        if (points.Count < 3) return triangles;

        // Placeholder: Implementar algoritmo Bowyer-Watson o usar Triangle.NET
        // Por ahora crea triángulos simples conectando puntos consecutivos
        for (int i = 0; i < points.Count - 2; i++)
        {
            triangles.Add((
                points[i].Location,
                points[i + 1].Location,
                points[i + 2].Location
            ));
        }

        return triangles;
    }
}
