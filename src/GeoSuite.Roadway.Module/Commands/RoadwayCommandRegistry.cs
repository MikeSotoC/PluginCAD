using Autodesk.AutoCAD.Runtime;
using GeoSuite.Platform.AcadImpl;
using GeoSuite.Roadway.Module.Commands;
using GeoSuite.Core.Models;

[assembly: CommandClass(typeof(GeoSuite.Roadway.Module.Commands.RoadwayCommandRegistry))]

namespace GeoSuite.Roadway.Module.Commands;

public class RoadwayCommandRegistry
{
    private static ICadHost GetCadHost() => new AcadHost();

    [CommandMethod("GS-R-ALIGN")]
    public static void CreateAlignment()
    {
        var cad = GetCadHost();
        
        // Placeholder: En implementación real, obtener puntos de selección o input
        var points = new List<Coordinate3>
        {
            new Coordinate3(0, 0, 100),
            new Coordinate3(50, 20, 102),
            new Coordinate3(100, 0, 101),
            new Coordinate3(150, 30, 103)
        };

        new CreateAlignmentCmd(cad).Execute(points);
    }
}
