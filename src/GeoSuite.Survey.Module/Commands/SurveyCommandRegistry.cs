using Autodesk.AutoCAD.Runtime;
using GeoSuite.Platform;
using GeoSuite.Platform.AcadImpl;
using GeoSuite.Survey.Module.Commands;

[assembly: CommandClass(typeof(GeoSuite.Survey.Module.Commands.SurveyCommandRegistry))]

namespace GeoSuite.Survey.Module.Commands;

public class SurveyCommandRegistry
{
    private static ICadHost GetCadHost() => new AcadHost();

    [CommandMethod("GS-T-IMP")]
    public static void ImportPoints()
    {
        var cad = GetCadHost();
        // En implementación real, mostrar diálogo para seleccionar archivo
        string filePath = "C:\\temp\\puntos.csv"; // Placeholder
        new ImportPointsCmd(cad).Execute(filePath);
    }

    [CommandMethod("GS-T-TIN")]
    public static void CreateTin()
    {
        var cad = GetCadHost();
        // En implementación real, obtener puntos de la base de datos o selección
        var points = new List<Core.Models.SurveyPoint>(); // Placeholder
        new CreateTinCmd(cad).Execute(points);
    }

    [CommandMethod("GS-T-CN")]
    public static void CreateContours()
    {
        var cad = GetCadHost();
        var points = new List<Core.Models.SurveyPoint>(); // Placeholder
        double interval = 1.0; // metros
        new ContoursCmd(cad).Execute(points, interval);
    }
}
