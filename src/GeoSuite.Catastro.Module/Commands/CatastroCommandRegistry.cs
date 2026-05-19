using Autodesk.AutoCAD.Runtime;
using GeoSuite.Platform;
using GeoSuite.Catastro.Module.Commands;
using GeoSuite.Core.Models;

[assembly: CommandClass(typeof(GeoSuite.Catastro.Module.Commands.CatastroCommandRegistry))]

namespace GeoSuite.Catastro.Module.Commands;

public class CatastroCommandRegistry
{
    private static ICadHost GetCadHost() => CadServiceFactory.Create();

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

    [CommandMethod("GS-C-LBL")]
    public static void LabelLots()
    {
        var cad = GetCadHost();
        
        // Placeholder: En implementación real, obtener polígonos de selección
        var polygons = new List<Polygon2D>
        {
            new Polygon2D
            {
                Vertices = new List<Coordinate3>
                {
                    new Coordinate3(0, 0, 0),
                    new Coordinate3(10, 0, 0),
                    new Coordinate3(10, 8, 0),
                    new Coordinate3(0, 8, 0)
                }
            }
        };

        new LabelLotsCmd(cad).Execute(polygons);
    }

    [CommandMethod("GS-CONFIG")]
    public static void OpenConfig()
    {
        // Abrir formulario de configuración WinForms
        var configCmd = new GeoSuite.UI.WinForms.Commands.ConfigCommand();
        configCmd.Execute();
    }
}
