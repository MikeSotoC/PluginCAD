using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CatastroTools.UI.Views
{
    public partial class VentanaPrincipal : Window, INotifyPropertyChanged
    {
        // ─────────────────────────────────────────────
        // VIEWMODEL
        // ─────────────────────────────────────────────

        private string _zonaUTM = "18S";
        private int _manzanasCount = 0;
        private int _lotesCount = 0;

        public string ZonaUTM
        {
            get => _zonaUTM;
            set => Set(ref _zonaUTM, value);
        }

        public int ManzanasCount
        {
            get => _manzanasCount;
            set => Set(ref _manzanasCount, value);
        }

        public int LotesCount
        {
            get => _lotesCount;
            set => Set(ref _lotesCount, value);
        }

        // Acción asignada desde el plugin
        public Action<string> EjecutarComando { get; set; }

        // ─────────────────────────────────────────────
        // CONSTRUCTOR
        // ─────────────────────────────────────────────

        public VentanaPrincipal()
        {
            InitializeComponent();

            DataContext = this;

            Left = SystemParameters.WorkArea.Width - Width - 50;
            Top = 100;
        }

        // ─────────────────────────────────────────────
        // NAVEGACIÓN
        // ─────────────────────────────────────────────

        private void NavBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var btn = (Button)sender;
                var tag = btn.Tag?.ToString() ?? "";

                var cmdMap = new Dictionary<string, string>
                {
                    ["panel_importar"] = "CT-IMPORTAR-COORDS",
                    ["panel_via_eje"] = "CT-VIA-EJE",
                    ["panel_manzaneo"] = "CT-MANZANEO",
                    ["panel_lotizar"] = "CT-LOTIZAR",
                    ["panel_tabla"] = "CT-TABLA",
                    ["panel_html"] = "CT-EXPORT-HTML",
                };

                if (cmdMap.TryGetValue(tag, out var comando))
                {
                    EjecutarComando?.Invoke(comando);
                    SetEstado($"Ejecutando: {comando}");
                }
                else
                {
                    SetEstado($"No existe: {tag}", true);
                }
            }
            catch (Exception ex)
            {
                SetEstado(ex.Message, true);
            }
        }

        // ─────────────────────────────────────────────
        // ESTADO
        // ─────────────────────────────────────────────

        public void SetEstado(string mensaje, bool error = false)
        {
            TxtEstado.Text = mensaje;

            TxtEstado.Foreground = error
                ? Brushes.OrangeRed
                : Brushes.LightGray;
        }

        // ─────────────────────────────────────────────
        // EVENTOS
        // ─────────────────────────────────────────────

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        // ─────────────────────────────────────────────
        // INPC
        // ─────────────────────────────────────────────

        public event PropertyChangedEventHandler PropertyChanged;

        private void Set<T>(
            ref T field,
            T value,
            [CallerMemberName] string name = null)
        {
            if (Equals(field, value))
                return;

            field = value;

            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(name));
        }
    }
}