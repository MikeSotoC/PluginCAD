namespace GeoSuite.Core.Models;

public struct Coordinate3
{
    public double X { get; set; } // Este
    public double Y { get; set; } // Norte
    public double Z { get; set; } // Elevación

    public Coordinate3(double x, double y, double z = 0)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public double DistanceTo(Coordinate3 other)
    {
        double dx = X - other.X;
        double dy = Y - other.Y;
        double dz = Z - other.Z;
        return Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    public double Distance2D(Coordinate3 other)
    {
        double dx = X - other.X;
        double dy = Y - other.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    public override string ToString() => $"{X:N3}, {Y:N3}, {Z:N3}";
}
