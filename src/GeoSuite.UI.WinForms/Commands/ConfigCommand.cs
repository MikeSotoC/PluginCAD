using System.Windows.Forms;
using GeoSuite.UI.WinForms.Forms;

namespace GeoSuite.UI.WinForms.Commands;

/// <summary>
/// Comando para abrir el formulario de configuración global.
/// Se registra como GS-CONFIG en AutoCAD/ZwCAD.
/// </summary>
public class ConfigCommand
{
    public void Execute()
    {
        // Mostrar formulario modal
        using var form = new SettingsForm();
        form.ShowDialog();
    }
}
