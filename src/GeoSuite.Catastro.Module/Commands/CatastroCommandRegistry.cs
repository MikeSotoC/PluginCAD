using Autodesk.AutoCAD.Runtime;
using GeoSuite.Platform.AcadImpl;
using GeoSuite.Catastro.Module.Commands;
using GeoSuite.Core.Models;

[assembly: CommandClass(typeof(GeoSuite.Catastro.Module.Commands.CatastroCommandRegistry))]

namespace GeoSuite.Catastro.Module.Commands;

public class CatastroCommandRegistry
{
    private static ICadHost GetCadHost() => new AcadHost();

    [CommandMethod("GS-C-POLY")]
    public static void DrawPolygon()
    {
        var cad = GetCadHost();
        
        // Placeholder: En implementación real, obtener vértices de selección o input
        var vertices = new List<Coordinate3>
        {
            new Coordinate3(0, 0, 0),
            new Coordinate3(10, 0, 0),
            new Coordinate3(10, 8, 0),
            new Coordinate3(0, 8, 0)
        };

        new DrawPolyCmd(cad).Execute(vertices);
    }
}
