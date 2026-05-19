// ============================================================
// CatastroTools Plugin — EntryPoint.cs
// Punto de entrada del plugin para ZWCAD y AutoCAD
// ============================================================

using System;
using CatastroTools.CAD;
using CatastroTools.CAD.Interfaces;
using CatastroTools.Core.Models;

#if ZWCAD
using ZwCAD.Runtime;
using ZwCAD.ApplicationServices;
using AcRx  = ZwCAD.Runtime;
using AcApp = ZwCAD.ApplicationServices.Application;
#elif AUTOCAD
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using AcRx  = Autodesk.AutoCAD.Runtime;
using AcApp = Autodesk.AutoCAD.ApplicationServices.Application;
#endif

// Registro del plugin
[assembly: ExtensionApplication(typeof(CatastroTools.Plugin.CatastroPlugin))]
[assembly: CommandClass(typeof(CatastroTools.Plugin.Commands.ComandoVentanaPrincipal))]
[assembly: CommandClass(typeof(CatastroTools.Plugin.Commands.ComandosManzaneo))]
[assembly: CommandClass(typeof(CatastroTools.Plugin.Commands.ComandosLotizacion))]
[assembly: CommandClass(typeof(CatastroTools.Plugin.Commands.ComandosEtiquetado))]
[assembly: CommandClass(typeof(CatastroTools.Plugin.Commands.ComandosTablas))]
[assembly: CommandClass(typeof(CatastroTools.Plugin.Commands.ComandosExport))]
[assembly: CommandClass(typeof(CatastroTools.Plugin.Commands.ComandosSistema))]

namespace CatastroTools.Plugin
{
    public class CatastroPlugin : IExtensionApplication
    {
        // Instancia global de la plataforma — accesible desde todos los comandos
        public static ICadPlatform Plataforma { get; private set; }
        public static ServicioDibujo Dibujo   { get; private set; }
        public static ConfigTexto Config      { get; private set; }

        // Estado del proyecto en sesión
        public static PredioMatriz ProyectoActual { get; set; }

        public void Initialize()
        {
            try
            {
                // Detectar y crear la plataforma correcta
#if ZWCAD
                Plataforma = new CatastroTools.CAD.ZwCAD.ZwPlatform();
#elif AUTOCAD
                Plataforma = new CatastroTools.CAD.AutoCAD.AcPlatform();
#endif
                Config = new ConfigTexto();
                Dibujo = new ServicioDibujo(Plataforma, Config);

                // Inicializar proyecto vacío
                ProyectoActual = new PredioMatriz();

                // Mostrar banner
                Plataforma.MensajeConsola("═══════════════════════════════════════════");
                Plataforma.MensajeConsola($"  CatastroTools v2.0 — {Plataforma.NombrePlataforma}");
                Plataforma.MensajeConsola("  Sistema Catastral Perú — SUNARP / RNE");
                Plataforma.MensajeConsola("  CT-PANEL → Abrir panel visual completo");
                Plataforma.MensajeConsola("  CT       → Ver todos los comandos");
                Plataforma.MensajeConsola("═══════════════════════════════════════════");

                // Inicializar capas automáticamente
                Dibujo.InicializarCapas();
            }
            catch (Exception ex)
            {
                try { Plataforma?.MensajeError($"Error al inicializar: {ex.Message}"); }
                catch { /* silenciar si ni la plataforma está lista */ }
            }
        }

        public void Terminate()
        {
            try
            {
                Plataforma?.MensajeConsola("CatastroTools descargado.");
            }
            catch { }
        }
    }
}
