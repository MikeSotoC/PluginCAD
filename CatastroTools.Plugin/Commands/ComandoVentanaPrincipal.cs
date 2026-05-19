using System;
using System.Windows;
using System.Windows.Threading;
using CatastroTools.UI.Views;

#if ZWCAD
using ZwCAD.Runtime;
using ZwCAD.ApplicationServices;
using AcApp = ZwCAD.ApplicationServices.Application;
#elif AUTOCAD
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using AcApp = Autodesk.AutoCAD.ApplicationServices.Application;
#endif

namespace CatastroTools.Plugin.Commands
{
    public class ComandoVentanaPrincipal
    {
        private static VentanaPrincipal _ventana;

        [CommandMethod("CT-PANEL")]
        public void AbrirPanel()
        {
            // Si ya está abierta, traerla al frente
            if (_ventana != null && _ventana.IsVisible)
            {
                _ventana.Activate();
                return;
            }

            // Crear en el hilo de UI del CAD
            try
            {
                _ventana = new VentanaPrincipal();

                // Inyectar el ejecutor de comandos
                _ventana.EjecutarComando = cmd =>
                {
                    // Enviar el comando a la línea de comandos del CAD
                    AcApp.DocumentManager.MdiActiveDocument
                         .SendStringToExecute($"{cmd} ", true, false, false);
                };

                // Datos de plataforma
                _ventana.PlataformaNombre  = CatastroPlugin.Plataforma.NombrePlataforma;
                _ventana.PlataformaVersion = CatastroPlugin.Plataforma.Version;

                // Mostrar como ventana no modal (el usuario puede seguir usando el CAD)
                _ventana.Owner = null;
                _ventana.Show();

                CatastroPlugin.Plataforma.MensajeConsola("Panel CatastroTools abierto.");
            }
            catch (Exception ex)
            {
                CatastroPlugin.Plataforma.MensajeError($"Error al abrir el panel: {ex.Message}");
            }
        }

        [CommandMethod("CT-PANEL-CERRAR")]
        public void CerrarPanel()
        {
            _ventana?.Close();
            _ventana = null;
        }
    }
}
