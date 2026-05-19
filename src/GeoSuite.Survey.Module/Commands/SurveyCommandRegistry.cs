using Autodesk.AutoCAD.Runtime;
using GeoSuite.Platform;
using GeoSuite.Survey.Module.Commands;

[assembly: CommandClass(typeof(GeoSuite.Survey.Module.Commands.SurveyCommandRegistry))]

namespace GeoSuite.Survey.Module.Commands;

/// <summary>
/// Registro de comandos del módulo de Topografía.
/// Detecta automáticamente la plataforma (AutoCAD/ZWCAD) mediante CadServiceFactory.
/// </summary>
public class SurveyCommandRegistry
{
    [CommandMethod("GS-T-IMP")]
    public static void ImportPoints()
    {
        try
        {
            var cad = CadServiceFactory.Create();
            new ImportPointsCmd(cad).Execute();
        }
        catch (System.Exception ex)
        {
            var cad = CadServiceFactory.Create();
            cad.ShowMessage("Error GS-T-IMP", $"Error al importar puntos: {ex.Message}");
        }
    }

    [CommandMethod("GS-T-TIN")]
    public static void CreateTin()
    {
        try
        {
            var cad = CadServiceFactory.Create();
            new CreateTinCmd(cad).Execute();
        }
        catch (System.Exception ex)
        {
            var cad = CadServiceFactory.Create();
            cad.ShowMessage("Error GS-T-TIN", $"Error al generar TIN: {ex.Message}");
        }
    }

    [CommandMethod("GS-T-CN")]
    public static void CreateContours()
    {
        try
        {
            var cad = CadServiceFactory.Create();
            new ContoursCmd(cad).Execute();
        }
        catch (System.Exception ex)
        {
            var cad = CadServiceFactory.Create();
            cad.ShowMessage("Error GS-T-CN", $"Error al generar curvas de nivel: {ex.Message}");
        }
    }

    [CommandMethod("GS-T-PERF")]
    public static void CreateProfile()
    {
        try
        {
            var cad = CadServiceFactory.Create();
            new ProfileCmd(cad).Execute();
        }
        catch (System.Exception ex)
        {
            var cad = CadServiceFactory.Create();
            cad.ShowMessage("Error GS-T-PERF", $"Error al generar perfil: {ex.Message}");
        }
    }

    [CommandMethod("GS-T-VOL")]
    public static void CalculateVolume()
    {
        try
        {
            var cad = CadServiceFactory.Create();
            new VolumeCmd(cad).Execute();
        }
        catch (System.Exception ex)
        {
            var cad = CadServiceFactory.Create();
            cad.ShowMessage("Error GS-T-VOL", $"Error al calcular volúmenes: {ex.Message}");
        }
    }
}
