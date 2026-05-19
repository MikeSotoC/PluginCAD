namespace GeoSuite.Core.Models;

public class Polygon2D
{
    public List<Coordinate3> Vertices { get; set; } = new();
    public bool IsClosed { get; set; } = true;

    public double Area
    {
        get
        {
            if (Vertices.Count < 3) return 0;
            
            double sum = 0;
            for (int i = 0; i < Vertices.Count; i++)
            {
                var p1 = Vertices[i];
                var p2 = Vertices[(i + 1) % Vertices.Count];
                sum += (p1.X * p2.Y) - (p2.X * p1.Y);
            }
            return Math.Abs(sum) / 2.0;
        }
    }

    public double Perimeter
    {
        get
        {
            if (Vertices.Count < 2) return 0;
            
            double sum = 0;
            for (int i = 0; i < Vertices.Count; i++)
            {
                var p1 = Vertices[i];
                var p2 = Vertices[(i + 1) % Vertices.Count];
                sum += p1.Distance2D(p2);
            }
            return sum;
        }
    }

    public Coordinate3 Centroid
    {
        get
        {
            if (Vertices.Count == 0) return new Coordinate3(0, 0, 0);
            
            double cx = 0, cy = 0;
            foreach (var v in Vertices)
            {
                cx += v.X;
                cy += v.Y;
            }
            return new Coordinate3(cx / Vertices.Count, cy / Vertices.Count, 0);
        }
    }
}
