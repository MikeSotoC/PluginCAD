using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CatastroTools.Core.Models;

namespace CatastroTools.UI.Views
{
    public partial class VentanaPrincipal : Window, INotifyPropertyChanged
    {
        // ─── ViewModel inline ────────────────────────────────────
        private string _plataformaNombre = "ZWCAD";
        private string _plataformaVersion = "";
        private string _zonaUTM = "18S";
        private int    _manzanasCount = 0;
        private int    _lotesCount = 0;

        public string PlataformaNombre  { get => _plataformaNombre;  set => Set(ref _plataformaNombre, value); }
        public string PlataformaVersion { get => _plataformaVersion; set => Set(ref _plataformaVersion, value); }
        public string ZonaUTM           { get => _zonaUTM;           set => Set(ref _zonaUTM, value); }
        public int    ManzanasCount     { get => _manzanasCount;     set => Set(ref _manzanasCount, value); }
        public int    LotesCount        { get => _lotesCount;        set => Set(ref _lotesCount, value); }

        // Acción ejecutora — asignada desde el Plugin
        public Action<string> EjecutarComando { get; set; }

        // Paneles registrados
        private readonly Dictionary<string, Func<UIElement>> _paneles;
        private Button _navActivo;

        public VentanaPrincipal()
        {
            InitializeComponent();
            DataContext = this;

            // Registrar todos los paneles
            _paneles = new Dictionary<string, Func<UIElement>>
            {
                ["panel_principal"]       = () => new PanelPrincipal(this),
                ["panel_importar"]        = () => new PanelImportarCoords(EjecutarComando),
                ["panel_via_eje"]         = () => new PanelViaEje(EjecutarComando),
                ["panel_vias_grilla"]     = () => new PanelViasGrilla(EjecutarComando),
                ["panel_seccion"]         = () => new PanelSeccionVia(EjecutarComando),
                ["panel_manzaneo"]        = () => new PanelManzaneo(EjecutarComando),
                ["panel_manzaneo_grilla"] = () => new PanelManzaneoGrilla(EjecutarComando),
                ["panel_lotizar"]         = () => new PanelLotizar(EjecutarComando),
                ["panel_habilitacion"]    = () => new PanelHabilitacion(EjecutarComando),
                ["panel_subdiv"]          = () => new PanelSubdivision(EjecutarComando),
                ["panel_etiqueta"]        = () => new PanelEtiquetaLote(EjecutarComando),
                ["panel_acotar"]          = () => new PanelAcotar(EjecutarComando),
                ["panel_vertices"]        = () => new PanelVertices(EjecutarComando),
                ["panel_tabla"]           = () => new PanelTablaTecnica(EjecutarComando),
                ["panel_tabla_coords"]    = () => new PanelTablaCoords(EjecutarComando),
                ["panel_tabla_colin"]     = () => new PanelTablaColindancias(EjecutarComando),
                ["panel_html"]            = () => new PanelExportHTML(EjecutarComando),
                ["panel_csv"]             = () => new PanelExportCSV(EjecutarComando),
                ["panel_config"]          = () => new PanelConfiguracion(),
            };

            // Mostrar panel principal al iniciar
            Loaded += (s, e) => NavEgarA("panel_principal", BtnNavPrincipal);
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

            NavEgarA(tag, btn);
        }

        public void NavEgarA(string panelKey, Button btnOrigen = null)
        {
            if (!_paneles.TryGetValue(panelKey, out var factory)) return;

            // Actualizar estado visual del botón activo
            if (_navActivo != null)
                _navActivo.Tag = _navActivo.Tag; // forzar refresh del trigger

            _navActivo = btnOrigen;

            // Crear y mostrar el panel
            try
            {
                ContenidoPrincipal.Content = factory();
                TxtBreadcrumb.Text = BtnNavPrincipal?.Content?.ToString()?.TrimStart() ?? panelKey;
                if (btnOrigen != null)
                    TxtBreadcrumb.Text = btnOrigen.Content?.ToString()?.TrimStart('🗺','🔧','📍',
                        '🛣','⊞','📐','🏙','🏠','✂','🏷','📏','📌','📋','📊','🧭','🌐','📄','🗑',' ') ?? panelKey;
            }
            catch (Exception ex)
            {
                SetEstado($"Error al cargar panel: {ex.Message}", true);
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

    // ─── ESTILO DE BOTÓN DE NAVEGACIÓN (code-behind) ─────────────
    // Se define en App.xaml pero aquí lo declaramos como referencia
}
