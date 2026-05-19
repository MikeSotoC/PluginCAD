using System;
using System.Windows;
using System.Windows.Controls;
using CatastroTools.Core.Models;

namespace CatastroTools.UI.Views
{
    public partial class PanelPrincipal : UserControl
    {
        private readonly VentanaPrincipal _ventana;

        public PanelPrincipal(VentanaPrincipal ventana)
        {
            InitializeComponent();
            _ventana = ventana;
            ActualizarEstado();
        }

        private void ActualizarEstado()
        {
            // Actualizar contadores desde el proyecto actual
            // (accede al plugin si está disponible)
            try
            {
                // En tiempo de diseño o sin plugin, usar valores por defecto
                TxtManzanas.Text  = "0";
                TxtLotes.Text     = "0";
                TxtVias.Text      = "0";
                TxtAreaTotal.Text = "—";
            }
            catch { }
        }

        private void AccesoRapido_Click(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            var tag = btn.Tag?.ToString() ?? "";
            _ventana?.EjecutarComando?.Invoke(tag);
        }
    }
}
