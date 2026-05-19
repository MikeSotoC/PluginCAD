using Autodesk.AutoCAD.Runtime;
using GeoSuite.Platform.AcadImpl;
using GeoSuite.Hydraulics.Module.Commands;
using GeoSuite.Core.Models;

[assembly: CommandClass(typeof(GeoSuite.Hydraulics.Module.Commands.HydraulicsCommandRegistry))]

namespace GeoSuite.Hydraulics.Module.Commands;

public class HydraulicsCommandRegistry
{
    private static ICadHost GetCadHost() => new AcadHost();

    [CommandMethod("GS-H-DRAIN")]
    public static void CreateDrainage()
    {
        var cad = GetCadHost();
        
        // Placeholder: En implementación real, obtener datos de selección o input
        var manholes = new List<Coordinate3>
        {
            new Coordinate3(0, 0, 100),
            new Coordinate3(20, 0, 99.5),
            new Coordinate3(40, 10, 99)
        };

        var pipes = new List<(Coordinate3, Coordinate3)>
        {
            (manholes[0], manholes[1]),
            (manholes[1], manholes[2])
        };

        new CreateDrainageCmd(cad).Execute(manholes, pipes);
    }
}
