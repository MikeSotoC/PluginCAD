namespace GeoSuite.Core.Models;

public class SurveyPoint
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public Coordinate3 Location { get; set; }

    public SurveyPoint(int id, double e, double n, double z, string desc = "", string code = "")
    {
        Id = id;
        Location = new Coordinate3(e, n, z);
        Description = desc;
        Code = code;
    }

    public override string ToString() => $"Punto {Id}: {Location} ({Code})";
}
