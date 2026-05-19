using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CatastroTools.UI.Views
{
    public partial class VentanaPrincipal : Window, INotifyPropertyChanged
    {
        // ─── ViewModel inline ────────────────────────────────────
        private string _zonaUTM = "18S";
        private int    _manzanasCount = 0;
        private int    _lotesCount = 0;

        public string ZonaUTM           { get => _zonaUTM;           set => Set(ref _zonaUTM, value); }
        public int    ManzanasCount     { get => _manzanasCount;     set => Set(ref _manzanasCount, value); }
        public int    LotesCount        { get => _lotesCount;        set => Set(ref _lotesCount, value); }

        // Acción ejecutora — asignada desde el Plugin
        public Action<string> EjecutarComando { get; set; }

        public VentanaPrincipal()
        {
            InitializeComponent();
            DataContext = this;
            
            // Posicionar ventana en esquina superior derecha del área de dibujo
            Left = System.Windows.SystemParameters.WorkArea.Width - Width - 50;
            Top  = 100;
        }

        // ─── NAVEGACIÓN ──────────────────────────────────────────
        private void NavBtn_Click(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            var tag = btn.Tag?.ToString() ?? "";

            if (tag.StartsWith("cmd_"))
            {
                // Comando directo (sin panel)
                EjecutarComando?.Invoke(tag.Replace("cmd_", "CT-").ToUpper());
                return;
            }

            // Para la interfaz compacta, ejecutamos directamente el comando asociado
            var cmdMap = new System.Collections.Generic.Dictionary<string, string>
            {
                ["panel_importar"]        = "CT-IMPORTAR-COORDS",
                ["panel_via_eje"]         = "CT-VIA-EJE",
                ["panel_vias_grilla"]     = "CT-VIAS-GRILLA",
                ["panel_seccion"]         = "CT-SECCION-VIA",
                ["panel_manzaneo"]        = "CT-MANZANEO",
                ["panel_manzaneo_grilla"] = "CT-MANZANEO-GRILLA",
                ["panel_lotizar"]         = "CT-LOTIZAR",
                ["panel_habilitacion"]    = "CT-HABILITACION",
                ["panel_subdiv"]          = "CT-SUBDIV",
                ["panel_etiqueta"]        = "CT-ETIQUETA",
                ["panel_acotar"]          = "CT-ACOTAR",
                ["panel_vertices"]        = "CT-VERTICES",
                ["panel_tabla"]           = "CT-TABLA",
                ["panel_tabla_coords"]    = "CT-TABLA-COORDS",
                ["panel_tabla_colin"]     = "CT-TABLA-COLIN",
                ["panel_html"]            = "CT-EXPORT-HTML",
                ["panel_csv"]             = "CT-EXPORT-CSV",
                ["panel_config"]          = "CT-CONFIG",
            };

            if (cmdMap.TryGetValue(tag, out var comando))
            {
                EjecutarComando?.Invoke(comando);
                SetEstado($"Ejecutando: {comando}");
            }
        }

        // ─── ESTADO ──────────────────────────────────────────────
        public void SetEstado(string mensaje, bool error = false)
        {
            TxtEstado.Text = mensaje;
            TxtEstado.Foreground = error
                ? (Brush)FindResource("BrushError")
                : (Brush)FindResource("BrushTextoSec");
        }

        public void ActualizarContadores(int manzanas, int lotes)
        {
            ManzanasCount = manzanas;
            LotesCount    = lotes;
        }

        // ─── EVENTOS ─────────────────────────────────────────────
        private void BtnCerrar_Click(object sender, RoutedEventArgs e) => Close();

        // ─── INPC ────────────────────────────────────────────────
        public event PropertyChangedEventHandler PropertyChanged;
        private void Set<T>(ref T field, T value, [CallerMemberName] string name = null)
        {
            if (Equals(field, value)) return;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
