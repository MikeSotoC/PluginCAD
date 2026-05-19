using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CatastroTools.Core.Models;

// ============================================================
// CatastroTools.UI — Ventanas de diálogo
// Cada ventana corresponde a un comando del plugin.
// Todas devuelven DialogResult = true si el usuario confirma.
// ============================================================

namespace CatastroTools.Plugin.UI
{
    // ═══════════════════════════════════════════════════════════
    // VENTANA: VÍA POR EJE
    // ═══════════════════════════════════════════════════════════
    public class VentanaVia : CatastroWindow
    {
        public Via ResultadoVia { get; private set; }

        private ComboBox _cbTipo;
        private TextBox  _txNombre, _txAncho, _txCalzada, _txVereda, _txBerma;

        private static readonly (string Label, TipoVia Tipo)[] Tipos =
        {
            ("Avenida Principal   — 22.00 m", TipoVia.AvenidaPrincipal),
            ("Avenida Secundaria  — 18.00 m", TipoVia.AvenidaSecundaria),
            ("Vía Colectora       — 15.00 m", TipoVia.ViaColectora),
            ("Calle Local         —  8.00 m", TipoVia.Calle),
            ("Vía Local           —  8.00 m", TipoVia.ViaLocal),
            ("Pasaje Vehicular    —  6.00 m", TipoVia.PasajeVehicular),
            ("Pasaje Peatonal     —  3.00 m", TipoVia.PasajePeatonal),
            ("Personalizado",                  TipoVia.Personalizado),
        };

        public VentanaVia()
        {
            Title  = "CatastroTools — Vía por Eje";
            Width  = 420; Height = 360;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ResizeMode = ResizeMode.NoResize;

            var grid = BuildLayout();
            Content = grid;
        }

        private Grid BuildLayout()
        {
            var g = new Grid { Margin = new Thickness(20) };
            g.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            g.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            g.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Título
            var title = new TextBlock
            {
                Text = "Definir Vía (RNE GH.020)",
                FontSize = 15, FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 16)
            };
            Grid.SetRow(title, 0);

            // Campos
            var form = new StackPanel();
            _cbTipo   = AddCombo(form,  "Tipo de vía:",           Tipos.Select(t => t.Label).ToArray(), 3);
            _txNombre = AddInput(form,  "Nombre:",                 "Calle");
            _txAncho  = AddInput(form,  "Ancho total (m):",        "8.00");
            _txCalzada= AddInput(form,  "Calzada (m):",            "5.40");
            _txVereda = AddInput(form,  "Vereda c/u (m):",         "1.80");
            _txBerma  = AddInput(form,  "Berma c/u (m) [0=sin]:", "0.00");
            _cbTipo.SelectionChanged += (s, e) => OnTipoChanged();
            Grid.SetRow(form, 1);

            // Botones
            var btns = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 16, 0, 0)
            };
            var btnOk  = new Button { Content = "Trazar Eje", Width = 110, Height = 34, Margin = new Thickness(8,0,0,0), IsDefault = true };
            var btnCnl = new Button { Content = "Cancelar",   Width = 80,  Height = 34, IsCancel = true };
            btnOk.Click  += (s, e) => { BuildResult(); DialogResult = true; };
            btnCnl.Click += (s, e) => { DialogResult = false; };
            btns.Children.Add(btnCnl);
            btns.Children.Add(btnOk);
            Grid.SetRow(btns, 2);

            g.Children.Add(title); g.Children.Add(form); g.Children.Add(btns);
            return g;
        }

        private void OnTipoChanged()
        {
            int idx = _cbTipo.SelectedIndex;
            if (idx < 0 || idx >= Tipos.Length - 1) return;
            var tipo = Tipos[idx].Tipo;
            double ancho   = Via.AnchoEstandar(tipo);
            _txNombre.Text = Via.NombreTipo(tipo);
            _txAncho.Text  = ancho.ToString("F2");
            // Componentes estándar aproximados
            _txVereda.Text  = (tipo == TipoVia.PasajePeatonal) ? ancho.ToString("F2") : (ancho * 0.225).ToString("F2");
            _txCalzada.Text = (tipo == TipoVia.PasajePeatonal) ? "0.00"              : (ancho * 0.675).ToString("F2");
        }

        private void BuildResult()
        {
            double ancho   = ParseD(_txAncho.Text,   8.0);
            double calzada = ParseD(_txCalzada.Text, 5.4);
            double vereda  = ParseD(_txVereda.Text,  1.8);
            double berma   = ParseD(_txBerma.Text,   0.0);
            int    idx     = _cbTipo.SelectedIndex;
            var    tipo    = (idx >= 0 && idx < Tipos.Length) ? Tipos[idx].Tipo : TipoVia.Calle;

            ResultadoVia = new Via
            {
                Nombre       = _txNombre.Text.Trim(),
                Tipo         = tipo,
                Ancho        = ancho,
                AnchoCalzada = calzada,
                AnchoVereda  = vereda,
                AnchoBerma   = berma
            };
        }

        // ── Helpers ──────────────────────────────────────────
        private static TextBox AddInput(StackPanel sp, string label, string val)
        {
            sp.Children.Add(new TextBlock { Text = label, FontSize = 11, Margin = new Thickness(0,8,0,3) });
            var tb = new TextBox { Text = val, Height = 30, Padding = new Thickness(8,0,0,0) };
            sp.Children.Add(tb);
            return tb;
        }

        private static ComboBox AddCombo(StackPanel sp, string label, string[] items, int sel)
        {
            sp.Children.Add(new TextBlock { Text = label, FontSize = 11, Margin = new Thickness(0,8,0,3) });
            var cb = new ComboBox { Height = 30 };
            foreach (var it in items) cb.Items.Add(it);
            cb.SelectedIndex = sel;
            sp.Children.Add(cb);
            return cb;
        }

        private static double ParseD(string s, double def)
            => double.TryParse(s.Replace(",", "."),
               System.Globalization.NumberStyles.Any,
               System.Globalization.CultureInfo.InvariantCulture, out double v) ? v : def;
    }

    // ═══════════════════════════════════════════════════════════
    // VENTANA: GRILLA DE VÍAS
    // ═══════════════════════════════════════════════════════════
    public class VentanaViasGrilla : CatastroWindow
    {
        public (double AnchoH, double SepH, string NombreH,
                double AnchoV, double SepV, string NombreV) Parametros { get; private set; }

        private TextBox _txAnchoH, _txSepH, _txNomH, _txAnchoV, _txSepV, _txNomV;

        public VentanaViasGrilla()
        {
            Title = "CatastroTools — Grilla de Vías";
            Width = 400; Height = 380;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ResizeMode = ResizeMode.NoResize;
            Content = Build();
        }

        private UIElement Build()
        {
            var sp = new StackPanel { Margin = new Thickness(20) };
            sp.Children.Add(Header("Grilla de Vías Ortogonales"));

            sp.Children.Add(Seccion("CALLES HORIZONTALES (E–O)"));
            _txAnchoH = Row(sp, "Ancho (m):",       "8.00");
            _txSepH   = Row(sp, "Separación (m):",  "60.00");
            _txNomH   = Row(sp, "Nombre base:",      "Calle");

            sp.Children.Add(Seccion("CALLES VERTICALES (N–S)"));
            _txAnchoV = Row(sp, "Ancho (m):",       "8.00");
            _txSepV   = Row(sp, "Separación (m):",  "100.00");
            _txNomV   = Row(sp, "Nombre base:",      "Pasaje");

            sp.Children.Add(BotonesOkCancelar("Generar Grilla", () =>
            {
                Parametros = (ParseD(_txAnchoH.Text, 8),  ParseD(_txSepH.Text, 60),  _txNomH.Text.Trim(),
                              ParseD(_txAnchoV.Text, 8),  ParseD(_txSepV.Text, 100), _txNomV.Text.Trim());
            }));
            return sp;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // VENTANA: SECCIÓN DE VÍA
    // ═══════════════════════════════════════════════════════════
    public class VentanaSeccionVia : CatastroWindow
    {
        public Via ResultadoVia { get; private set; }
        private TextBox _txNombre, _txAncho, _txCalzada, _txVereda, _txBerma;

        public VentanaSeccionVia()
        {
            Title = "CatastroTools — Sección de Vía en Planta";
            Width = 400; Height = 340;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ResizeMode = ResizeMode.NoResize;
            Content = Build();
        }

        private UIElement Build()
        {
            var sp = new StackPanel { Margin = new Thickness(20) };
            sp.Children.Add(Header("Sección Vial — RNE GH.020"));
            _txNombre  = Row(sp, "Nombre de la vía:",   "Calle Local");
            _txAncho   = Row(sp, "Ancho total (m):",    "8.00");
            _txCalzada = Row(sp, "Calzada (m):",        "5.40");
            _txVereda  = Row(sp, "Vereda c/u (m):",     "1.80");
            _txBerma   = Row(sp, "Berma c/u (m):",      "0.00");
            sp.Children.Add(Info("La sección se insertará en el punto que indiques en el plano."));
            sp.Children.Add(BotonesOkCancelar("Insertar Sección", () =>
            {
                ResultadoVia = new Via
                {
                    Nombre       = _txNombre.Text.Trim(),
                    Tipo         = TipoVia.Calle,
                    Ancho        = ParseD(_txAncho.Text,   8),
                    AnchoCalzada = ParseD(_txCalzada.Text, 5.4),
                    AnchoVereda  = ParseD(_txVereda.Text,  1.8),
                    AnchoBerma   = ParseD(_txBerma.Text,   0)
                };
            }));
            return sp;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // VENTANA: MANZANEO
    // ═══════════════════════════════════════════════════════════
    public class VentanaManzaneo : CatastroWindow
    {
        public SistemaNomenclatura Sistema     { get; private set; }
        public int                 Inicio      { get; private set; }
        public double              AlturaTexto { get; private set; }

        private ComboBox _cbSistema;
        private TextBox  _txInicio, _txAltura;

        public VentanaManzaneo()
        {
            Title = "CatastroTools — Manzaneo";
            Width = 380; Height = 280;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ResizeMode = ResizeMode.NoResize;
            Content = Build();
        }

        private UIElement Build()
        {
            var sp = new StackPanel { Margin = new Thickness(20) };
            sp.Children.Add(Header("Etiquetar Manzanas"));
            sp.Children.Add(new TextBlock { Text = "Sistema:", FontSize = 11, Margin = new Thickness(0,8,0,3) });
            _cbSistema = new ComboBox { Height = 30, Margin = new Thickness(0,0,0,4) };
            _cbSistema.Items.Add("Alfabético  (MZ A, MZ B...)");
            _cbSistema.Items.Add("Numérico    (MZ 1, MZ 2...)");
            _cbSistema.Items.Add("Personalizado");
            _cbSistema.SelectedIndex = 0;
            sp.Children.Add(_cbSistema);
            _txInicio  = Row(sp, "Inicio en:",        "1");
            _txAltura  = Row(sp, "Altura texto (m):", "4.00");
            sp.Children.Add(Info("Selecciona las polilíneas de manzanas en el CAD."));
            sp.Children.Add(BotonesOkCancelar("Etiquetar", () =>
            {
                Sistema     = (SistemaNomenclatura)Math.Min(_cbSistema.SelectedIndex, 2);
                Inicio      = ParseI(_txInicio.Text, 1);
                AlturaTexto = ParseD(_txAltura.Text, 4);
            }));
            return sp;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // VENTANA: MANZANEO GRILLA
    // ═══════════════════════════════════════════════════════════
    public class VentanaManzaneoGrilla : CatastroWindow
    {
        public double              AnchoViaH   { get; private set; }
        public double              AnchoViaV   { get; private set; }
        public double              SepH        { get; private set; }
        public double              SepV        { get; private set; }
        public SistemaNomenclatura Sistema     { get; private set; }
        public int                 Inicio      { get; private set; }
        public double              AlturaTexto { get; private set; }

        private TextBox _txAnchoH, _txAnchoV, _txSepH, _txSepV, _txInicio, _txAltura;
        private ComboBox _cbSistema;

        public VentanaManzaneoGrilla()
        {
            Title = "CatastroTools — Manzaneo Grilla";
            Width = 400; Height = 420;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ResizeMode = ResizeMode.NoResize;
            Content = Build();
        }

        private UIElement Build()
        {
            var sp = new StackPanel { Margin = new Thickness(20) };
            sp.Children.Add(Header("Manzanas Automáticas desde Grilla"));
            _txSepH    = Row(sp, "Separación calles H (m):", "60.00");
            _txSepV    = Row(sp, "Separación calles V (m):", "100.00");
            _txAnchoH  = Row(sp, "Ancho vía H (m):",         "8.00");
            _txAnchoV  = Row(sp, "Ancho vía V (m):",         "8.00");
            sp.Children.Add(new TextBlock { Text = "Nomenclatura:", FontSize = 11, Margin = new Thickness(0,8,0,3) });
            _cbSistema = new ComboBox { Height = 30, Margin = new Thickness(0,0,0,4) };
            _cbSistema.Items.Add("Alfabético (MZ A, MZ B...)");
            _cbSistema.Items.Add("Numérico   (MZ 1, MZ 2...)");
            _cbSistema.SelectedIndex = 0;
            sp.Children.Add(_cbSistema);
            _txInicio = Row(sp, "Inicio en:",        "1");
            _txAltura = Row(sp, "Altura texto (m):", "4.00");
            sp.Children.Add(BotonesOkCancelar("Generar Manzanas", () =>
            {
                AnchoViaH   = ParseD(_txAnchoH.Text,  8);
                AnchoViaV   = ParseD(_txAnchoV.Text,  8);
                SepH        = ParseD(_txSepH.Text,    60);
                SepV        = ParseD(_txSepV.Text,    100);
                Sistema     = (SistemaNomenclatura)_cbSistema.SelectedIndex;
                Inicio      = ParseI(_txInicio.Text,  1);
                AlturaTexto = ParseD(_txAltura.Text,  4);
            }));
            return sp;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // VENTANA: LOTIZACIÓN
    // ═══════════════════════════════════════════════════════════
    public class VentanaLotizacion : CatastroWindow
    {
        public string  Prefijo             { get; private set; }
        public int     NumInicial          { get; private set; }
        public double  AlturaTexto         { get; private set; }
        public bool    DibujarColindancias { get; private set; }

        private TextBox  _txPrefijo, _txNum, _txAltura;
        private CheckBox _chkColin;

        public VentanaLotizacion()
        {
            Title = "CatastroTools — Lotización por Recorrido";
            Width = 400; Height = 320;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ResizeMode = ResizeMode.NoResize;
            Content = Build();
        }

        private UIElement Build()
        {
            var sp = new StackPanel { Margin = new Thickness(20) };
            sp.Children.Add(Header("Lotización por Recorrido"));
            sp.Children.Add(Info("Selecciona los lotes → traza el recorrido de numeración → el sistema etiqueta en orden."));
            _txPrefijo = Row(sp, "Prefijo:",          "Lote ");
            _txNum     = Row(sp, "Número inicial:",   "1");
            _txAltura  = Row(sp, "Altura texto (m):", "2.50");
            _chkColin  = new CheckBox { Content = "Dibujar colindancias automáticamente", IsChecked = true, Margin = new Thickness(0,8,0,0) };
            sp.Children.Add(_chkColin);
            sp.Children.Add(BotonesOkCancelar("Iniciar Lotización", () =>
            {
                Prefijo             = _txPrefijo.Text;
                NumInicial          = ParseI(_txNum.Text, 1);
                AlturaTexto         = ParseD(_txAltura.Text, 2.5);
                DibujarColindancias = _chkColin.IsChecked == true;
            }));
            return sp;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // VENTANA: HABILITACIÓN
    // ═══════════════════════════════════════════════════════════
    public class VentanaHabilitacion : CatastroWindow
    {
        public double AnchoLote  { get; private set; }
        public double ProfLote   { get; private set; }
        public int    NumInicial { get; private set; }
        public bool   Numerar    { get; private set; }
        public double AlturaTexto{ get; private set; }

        private TextBox  _txAncho, _txProf, _txNum, _txAltura;
        private CheckBox _chkNum;

        public VentanaHabilitacion()
        {
            Title = "CatastroTools — Habilitación en Grilla (RNE GH.020)";
            Width = 400; Height = 320;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ResizeMode = ResizeMode.NoResize;
            Content = Build();
        }

        private UIElement Build()
        {
            var sp = new StackPanel { Margin = new Thickness(20) };
            sp.Children.Add(Header("Grilla de Lotes dentro de Manzana"));
            _txAncho  = Row(sp, "Frente del lote (m):",       "8.00");
            _txProf   = Row(sp, "Profundidad del lote (m):",  "15.00");
            _txNum    = Row(sp, "Número inicial:",             "1");
            _txAltura = Row(sp, "Altura texto (m):",           "2.50");
            _chkNum   = new CheckBox { Content = "Numerar automáticamente", IsChecked = true, Margin = new Thickness(0,8,0,0) };
            sp.Children.Add(_chkNum);
            sp.Children.Add(BotonesOkCancelar("Generar Lotes", () =>
            {
                AnchoLote   = ParseD(_txAncho.Text,  8);
                ProfLote    = ParseD(_txProf.Text,   15);
                NumInicial  = ParseI(_txNum.Text,    1);
                AlturaTexto = ParseD(_txAltura.Text, 2.5);
                Numerar     = _chkNum.IsChecked == true;
            }));
            return sp;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // VENTANA: ETIQUETAR LOTE
    // ═══════════════════════════════════════════════════════════
    public class VentanaEtiquetaLote : CatastroWindow
    {
        public Lote       ResultadoLote { get; private set; }
        public ConfigTexto ConfigTexto  { get; private set; }

        private TextBox _txNum, _txMz, _txProp, _txPart, _txUso, _txAltura;
        private CheckBox _chkPart, _chkUso;
        private readonly double _area, _perim;
        private readonly int    _nverts;

        public VentanaEtiquetaLote(double area, double perim, int nverts)
        {
            _area = area; _perim = perim; _nverts = nverts;
            Title = "CatastroTools — Etiquetar Lote (SUNARP)";
            Width = 440; Height = 440;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ResizeMode = ResizeMode.NoResize;
            Content = Build();
        }

        private UIElement Build()
        {
            var sp = new StackPanel { Margin = new Thickness(20) };
            sp.Children.Add(Header("Etiqueta de Lote — Formato SUNARP"));
            sp.Children.Add(Info($"Área: {_area:F2} m²   |   Perímetro: {_perim:F3} m   |   Vértices: {_nverts}"));
            _txNum    = Row(sp, "N° Lote:",          "");
            _txMz     = Row(sp, "Manzana:",          "");
            _txProp   = Row(sp, "Propietario:",      "");
            _txPart   = Row(sp, "Partida SUNARP:",   "");
            _txUso    = Row(sp, "Zonificación:",     "");
            _txAltura = Row(sp, "Altura texto (m):", "2.50");
            _chkPart  = new CheckBox { Content = "Mostrar partida registral", IsChecked = true,  Margin = new Thickness(0,4,0,0) };
            _chkUso   = new CheckBox { Content = "Mostrar zonificación",      IsChecked = false, Margin = new Thickness(0,4,0,0) };
            sp.Children.Add(_chkPart); sp.Children.Add(_chkUso);
            sp.Children.Add(BotonesOkCancelar("Etiquetar en CAD", () =>
            {
                ResultadoLote = new Lote
                {
                    Numero           = _txNum.Text.Trim(),
                    NombreManzana    = _txMz.Text.Trim(),
                    Propietario      = _txProp.Text.Trim(),
                    PartidaRegistral = _chkPart.IsChecked == true ? _txPart.Text.Trim() : "",
                    Zonificacion     = _chkUso.IsChecked  == true ? _txUso.Text.Trim()  : "",
                };
                ConfigTexto = new ConfigTexto
                {
                    AlturaNumeroLote = ParseD(_txAltura.Text, 2.5),
                    AlturaArea       = ParseD(_txAltura.Text, 2.5) * 0.85,
                    MostrarPartida   = _chkPart.IsChecked == true,
                    MostrarZonificacion = _chkUso.IsChecked == true
                };
            }));
            return sp;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // VENTANA: ACOTAR COMPLETO
    // ═══════════════════════════════════════════════════════════
    public class VentanaAcotar : CatastroWindow
    {
        public Lote       ResultadoLote { get; private set; }
        public ConfigTexto ConfigTexto  { get; private set; }

        private TextBox  _txNum, _txProp, _txPart, _txHLabel, _txHLind;
        private CheckBox _chkRumbo, _chkPart;
        private readonly double _area, _perim;
        private readonly int    _nverts;

        public VentanaAcotar(double area, double perim, int nverts)
        {
            _area = area; _perim = perim; _nverts = nverts;
            Title = "CatastroTools — Acotación Completa";
            Width = 440; Height = 420;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ResizeMode = ResizeMode.NoResize;
            Content = Build();
        }

        private UIElement Build()
        {
            var sp = new StackPanel { Margin = new Thickness(20) };
            sp.Children.Add(Header("Acotación Completa — Etiqueta + Linderos"));
            sp.Children.Add(Info($"Área: {_area:F2} m²   |   Perímetro: {_perim:F3} m   |   {_nverts} lados"));
            _txNum   = Row(sp, "N° Lote:",              "");
            _txProp  = Row(sp, "Propietario:",          "");
            _txPart  = Row(sp, "Partida SUNARP:",       "");
            _txHLabel = Row(sp, "Altura etiqueta (m):", "2.50");
            _txHLind  = Row(sp, "Altura linderos (m):", "1.80");
            _chkRumbo = new CheckBox { Content = "Mostrar rumbo (N°E / S°O)", IsChecked = true,  Margin = new Thickness(0,8,0,0) };
            _chkPart  = new CheckBox { Content = "Mostrar partida registral", IsChecked = true,  Margin = new Thickness(0,4,0,0) };
            sp.Children.Add(_chkRumbo); sp.Children.Add(_chkPart);
            sp.Children.Add(BotonesOkCancelar("Acotar en CAD", () =>
            {
                ResultadoLote = new Lote
                {
                    Numero           = _txNum.Text.Trim(),
                    Propietario      = _txProp.Text.Trim(),
                    PartidaRegistral = _txPart.Text.Trim()
                };
                ConfigTexto = new ConfigTexto
                {
                    AlturaNumeroLote = ParseD(_txHLabel.Text, 2.5),
                    AlturaArea       = ParseD(_txHLabel.Text, 2.5) * 0.85,
                    AlturaLindero    = ParseD(_txHLind.Text,  1.8),
                    MostrarRumbo     = _chkRumbo.IsChecked == true,
                    MostrarPartida   = _chkPart.IsChecked  == true
                };
            }));
            return sp;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // VENTANA: VÉRTICES
    // ═══════════════════════════════════════════════════════════
    public class VentanaVertices : CatastroWindow
    {
        public ConfigTexto ConfigTexto  { get; private set; }
        public string      TipoSimbolo { get; private set; }

        private ComboBox _cbSimbolo;
        private TextBox  _txPrefijo, _txNum, _txAltura, _txOffX, _txOffY;

        public VentanaVertices()
        {
            Title = "CatastroTools — Vértices / Mojones";
            Width = 400; Height = 380;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ResizeMode = ResizeMode.NoResize;
            Content = Build();
        }

        private UIElement Build()
        {
            var sp = new StackPanel { Margin = new Thickness(20) };
            sp.Children.Add(Header("Marcar Vértices con Coordenadas UTM"));
            sp.Children.Add(new TextBlock { Text = "Símbolo:", FontSize = 11, Margin = new Thickness(0,8,0,3) });
            _cbSimbolo = new ComboBox { Height = 30, Margin = new Thickness(0,0,0,4) };
            foreach (var s in new[] { "Cruz (estándar catastral)", "Círculo", "Triángulo", "Rombo" })
                _cbSimbolo.Items.Add(s);
            _cbSimbolo.SelectedIndex = 0;
            sp.Children.Add(_cbSimbolo);
            _txPrefijo = Row(sp, "Prefijo:",           "V-");
            _txNum     = Row(sp, "Número inicial:",    "1");
            _txAltura  = Row(sp, "Altura texto (m):", "1.80");
            _txOffX    = Row(sp, "Offset X (m):",     "2.00");
            _txOffY    = Row(sp, "Offset Y (m):",     "1.00");
            sp.Children.Add(BotonesOkCancelar("Marcar en CAD", () =>
            {
                int symIdx = _cbSimbolo.SelectedIndex;
                if      (symIdx == 1) TipoSimbolo = "CIRCULO";
                else if (symIdx == 2) TipoSimbolo = "TRIANGULO";
                else if (symIdx == 3) TipoSimbolo = "CUADRADO";
                else                  TipoSimbolo = "CRUZ";
                ConfigTexto = new ConfigTexto
                {
                    PrefijoVertice     = _txPrefijo.Text,
                    NumVerticeInicial  = ParseI(_txNum.Text, 1),
                    AlturaVertice      = ParseD(_txAltura.Text, 1.8),
                    OffsetVerticeX     = ParseD(_txOffX.Text, 2),
                    OffsetVerticeY     = ParseD(_txOffY.Text, 1)
                };
            }));
            return sp;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // VENTANA: IMPORTAR COORDS
    // ═══════════════════════════════════════════════════════════
    public class VentanaImportarCoords : CatastroWindow
    {
        public List<Punto2D> Puntos      { get; private set; } = new List<Punto2D>();
        public bool          MarcarVertices { get; private set; }
        public ConfigTexto   ConfigTexto   { get; private set; }

        private ListBox  _lista;
        private TextBox  _txE, _txN;
        private CheckBox _chkMarcar;

        public VentanaImportarCoords()
        {
            Title = "CatastroTools — Importar Coordenadas UTM";
            Width = 480; Height = 420;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ResizeMode = ResizeMode.NoResize;
            Content = Build();
        }

        private UIElement Build()
        {
            var sp = new StackPanel { Margin = new Thickness(20) };
            sp.Children.Add(Header("Crear Polilínea desde Coordenadas UTM"));
            _lista = new ListBox { Height = 140, Margin = new Thickness(0,0,0,8) };
            sp.Children.Add(_lista);
            var row = new Grid { Margin = new Thickness(0,0,0,8) };
            for (int i = 0; i < 5; i++) row.ColumnDefinitions.Add(new ColumnDefinition { Width = i % 2 == 1 ? new GridLength(8) : new GridLength(1, GridUnitType.Star) });
            _txE = new TextBox { Height = 30, Padding = new Thickness(8,0,0,0) }; Grid.SetColumn(_txE, 0);
            _txN = new TextBox { Height = 30, Padding = new Thickness(8,0,0,0) }; Grid.SetColumn(_txN, 2);
            var bA = new Button { Content = "Agregar", Height = 30, MinWidth = 70 };  Grid.SetColumn(bA, 4);
            bA.Click += AddPunto;
            row.Children.Add(_txE); row.Children.Add(_txN); row.Children.Add(bA);
            sp.Children.Add(row);
            _chkMarcar = new CheckBox { Content = "Marcar vértices con coords UTM", IsChecked = true, Margin = new Thickness(0,4,0,0) };
            sp.Children.Add(_chkMarcar);
            sp.Children.Add(BotonesOkCancelar("Crear Polilínea", () =>
            {
                MarcarVertices = _chkMarcar.IsChecked == true;
                ConfigTexto    = new ConfigTexto();
            }));
            return sp;
        }

        private void AddPunto(object s, RoutedEventArgs e)
        {
            if (!TryParseUTM(_txE.Text, _txN.Text, out double east, out double north))
            { MessageBox.Show("Valores inválidos.", "CatastroTools"); return; }
            Puntos.Add(new Punto2D(east, north));
            _lista.Items.Add($"V-{Puntos.Count}   E={east:F4}   N={north:F4}");
            _txE.Text = _txN.Text = "";
            _txE.Focus();
        }

        private static bool TryParseUTM(string se, string sn, out double e, out double n)
        {
            var ci = System.Globalization.CultureInfo.InvariantCulture;
            bool okE = double.TryParse(se.Replace(",", "."), System.Globalization.NumberStyles.Any, ci, out e);
            bool okN = double.TryParse(sn.Replace(",", "."), System.Globalization.NumberStyles.Any, ci, out n);
            return okE && okN;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // VENTANA: TABLA TÉCNICA
    // ═══════════════════════════════════════════════════════════
    public class VentanaTabla : CatastroWindow
    {
        public Lote   ResultadoLote { get; private set; }
        public double AnchoTabla   { get; private set; }
        public double AltoFila     { get; private set; }
        public double AlturaTexto  { get; private set; }

        private TextBox _txProp, _txDni, _txDir, _txDist, _txProv, _txDepto;
        private TextBox _txLote, _txMz,  _txHab, _txPart, _txZon,  _txEsc;
        private TextBox _txArea, _txPer, _txAncho, _txAltoF, _txAltura;
        private readonly double _area, _perim;

        public VentanaTabla(double area, double perim)
        {
            _area = area; _perim = perim;
            Title = "CatastroTools — Cuadro de Datos Técnicos (SUNARP)";
            Width = 500; Height = 580;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ResizeMode = ResizeMode.CanResize;
            Content = Build();
        }

        private UIElement Build()
        {
            var scroll = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            var sp = new StackPanel { Margin = new Thickness(20) };
            scroll.Content = sp;
            sp.Children.Add(Header("Cuadro de Datos Técnicos del Predio"));
            _txProp   = Row(sp, "Propietario / Titular:", "");
            _txDni    = Row(sp, "DNI / RUC:",             "");
            _txDir    = Row(sp, "Dirección:",             "");
            _txDist   = Row(sp, "Distrito:",              "");
            _txProv   = Row(sp, "Provincia:",             "");
            _txDepto  = Row(sp, "Departamento:",          "Tacna");
            _txLote   = Row(sp, "N° Lote:",               "");
            _txMz     = Row(sp, "Manzana:",               "");
            _txHab    = Row(sp, "Habilitación urbana:",   "");
            _txPart   = Row(sp, "Partida registral:",     "");
            _txZon    = Row(sp, "Zonificación:",          "");
            _txEsc    = Row(sp, "Escala:",                "1/200");
            _txArea   = Row(sp, "Área (m²):",             _area.ToString("F2"));
            _txPer    = Row(sp, "Perímetro (m):",         _perim.ToString("F3"));
            _txAncho  = Row(sp, "Ancho tabla (m):",       "120.00");
            _txAltoF  = Row(sp, "Alto fila (m):",         "7.00");
            _txAltura = Row(sp, "Altura texto (m):",      "2.00");
            sp.Children.Add(BotonesOkCancelar("Insertar Tabla en CAD", () =>
            {
                ResultadoLote = new Lote
                {
                    Propietario      = _txProp.Text.Trim(),
                    Dni              = _txDni.Text.Trim(),
                    Direccion        = _txDir.Text.Trim(),
                    Distrito         = _txDist.Text.Trim(),
                    Provincia        = _txProv.Text.Trim(),
                    Departamento     = _txDepto.Text.Trim(),
                    Numero           = _txLote.Text.Trim(),
                    NombreManzana    = _txMz.Text.Trim(),
                    HabilitacionUrbana = _txHab.Text.Trim(),
                    PartidaRegistral = _txPart.Text.Trim(),
                    Zonificacion     = _txZon.Text.Trim(),
                };
                AnchoTabla   = ParseD(_txAncho.Text,  120);
                AltoFila     = ParseD(_txAltoF.Text,  7);
                AlturaTexto  = ParseD(_txAltura.Text, 2);
            }));
            return scroll;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // VENTANA: COLINDANCIAS
    // ═══════════════════════════════════════════════════════════
    public class VentanaColindancias : CatastroWindow
    {
        public string Norte { get; private set; }
        public string Sur   { get; private set; }
        public string Este  { get; private set; }
        public string Oeste { get; private set; }

        private TextBox _txN, _txS, _txE, _txO;

        public VentanaColindancias()
        {
            Title = "CatastroTools — Cuadro de Colindancias";
            Width = 460; Height = 300;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ResizeMode = ResizeMode.NoResize;
            Content = Build();
        }

        private UIElement Build()
        {
            var sp = new StackPanel { Margin = new Thickness(20) };
            sp.Children.Add(Header("Colindancias del Predio"));
            _txN = Row(sp, "NORTE:", "");
            _txS = Row(sp, "SUR:",   "");
            _txE = Row(sp, "ESTE:",  "");
            _txO = Row(sp, "OESTE:", "");
            sp.Children.Add(BotonesOkCancelar("Insertar Tabla", () =>
            {
                Norte = _txN.Text.Trim(); Sur   = _txS.Text.Trim();
                Este  = _txE.Text.Trim(); Oeste = _txO.Text.Trim();
            }));
            return sp;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // VENTANA: EXPORT HTML
    // ═══════════════════════════════════════════════════════════
    public class VentanaExportHTML : CatastroWindow
    {
        public Lote ResultadoLote { get; private set; }
        private TextBox _txProp, _txDni, _txDir, _txDist, _txPart, _txLote;

        public VentanaExportHTML()
        {
            Title = "CatastroTools — Exportar Reporte HTML";
            Width = 420; Height = 360;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ResizeMode = ResizeMode.NoResize;
            Content = Build();
        }

        private UIElement Build()
        {
            var sp = new StackPanel { Margin = new Thickness(20) };
            sp.Children.Add(Header("Reporte HTML — Expediente SUNARP"));
            sp.Children.Add(Info("El archivo se guardará en la carpeta del DWG."));
            _txProp  = Row(sp, "Propietario:", "");
            _txDni   = Row(sp, "DNI / RUC:",  "");
            _txDir   = Row(sp, "Dirección:",  "");
            _txDist  = Row(sp, "Distrito:",   "");
            _txPart  = Row(sp, "Partida:",    "");
            _txLote  = Row(sp, "N° Lote:",    "");
            sp.Children.Add(BotonesOkCancelar("Generar HTML", () =>
            {
                ResultadoLote = new Lote
                {
                    Propietario      = _txProp.Text.Trim(),
                    Dni              = _txDni.Text.Trim(),
                    Direccion        = _txDir.Text.Trim(),
                    Distrito         = _txDist.Text.Trim(),
                    PartidaRegistral = _txPart.Text.Trim(),
                    Numero           = _txLote.Text.Trim()
                };
            }));
            return sp;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // VENTANA: CONFIGURACIÓN
    // ═══════════════════════════════════════════════════════════
    public class VentanaConfiguracion : CatastroWindow
    {
        public ConfigTexto ConfigResultado { get; private set; }
        private readonly ConfigTexto _cfg;
        private TextBox _txNumLote, _txArea, _txProp, _txLindero, _txVtx, _txMz;
        private TextBox _txPrefijo, _txNumVtx, _txOffX, _txOffY;
        private CheckBox _chkRumbo, _chkPartida;

        public VentanaConfiguracion(ConfigTexto cfg)
        {
            _cfg   = cfg;
            Title  = "CatastroTools — Configuración";
            Width  = 420; Height = 480;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ResizeMode = ResizeMode.NoResize;
            Content = Build();
        }

        private UIElement Build()
        {
            var scroll = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            var sp = new StackPanel { Margin = new Thickness(20) };
            scroll.Content = sp;
            sp.Children.Add(Header("Configuración Global"));
            _txNumLote  = Row(sp, "Altura número lote (m):", _cfg.AlturaNumeroLote.ToString("F2"));
            _txArea     = Row(sp, "Altura área (m):",         _cfg.AlturaArea.ToString("F2"));
            _txProp     = Row(sp, "Altura propietario (m):",  _cfg.AlturaPropietario.ToString("F2"));
            _txLindero  = Row(sp, "Altura linderos (m):",     _cfg.AlturaLindero.ToString("F2"));
            _txVtx      = Row(sp, "Altura vértices (m):",     _cfg.AlturaVertice.ToString("F2"));
            _txMz       = Row(sp, "Altura manzana (m):",      _cfg.AlturaManzana.ToString("F2"));
            _txPrefijo  = Row(sp, "Prefijo vértice:",         _cfg.PrefijoVertice);
            _txNumVtx   = Row(sp, "N° inicial vértice:",      _cfg.NumVerticeInicial.ToString());
            _txOffX     = Row(sp, "Offset vértice X (m):",    _cfg.OffsetVerticeX.ToString("F2"));
            _txOffY     = Row(sp, "Offset vértice Y (m):",    _cfg.OffsetVerticeY.ToString("F2"));
            _chkRumbo   = new CheckBox { Content = "Mostrar rumbo en linderos",   IsChecked = _cfg.MostrarRumbo,   Margin = new Thickness(0,8,0,4) };
            _chkPartida = new CheckBox { Content = "Mostrar partida registral",   IsChecked = _cfg.MostrarPartida, Margin = new Thickness(0,0,0,4) };
            sp.Children.Add(_chkRumbo); sp.Children.Add(_chkPartida);
            sp.Children.Add(BotonesOkCancelar("Guardar", () =>
            {
                ConfigResultado = new ConfigTexto
                {
                    AlturaNumeroLote  = ParseD(_txNumLote.Text,  3),
                    AlturaArea        = ParseD(_txArea.Text,     2.5),
                    AlturaPropietario = ParseD(_txProp.Text,     2),
                    AlturaLindero     = ParseD(_txLindero.Text,  1.8),
                    AlturaVertice     = ParseD(_txVtx.Text,      1.8),
                    AlturaManzana     = ParseD(_txMz.Text,       4),
                    PrefijoVertice    = _txPrefijo.Text,
                    NumVerticeInicial = ParseI(_txNumVtx.Text,   1),
                    OffsetVerticeX    = ParseD(_txOffX.Text,     2),
                    OffsetVerticeY    = ParseD(_txOffY.Text,     1),
                    MostrarRumbo      = _chkRumbo.IsChecked   == true,
                    MostrarPartida    = _chkPartida.IsChecked == true
                };
            }));
            return scroll;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // HELPERS COMPARTIDOS (extensión de Window)
    // ═══════════════════════════════════════════════════════════
    internal static class WindowHelpers
    {
        internal static TextBlock Header(string texto) =>
            new TextBlock { Text = texto, FontSize = 15, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0,0,0,14) };

        internal static TextBlock Info(string texto) =>
            new TextBlock { Text = texto, FontSize = 11, Foreground = System.Windows.Media.Brushes.Gray, Margin = new Thickness(0,0,0,10), TextWrapping = TextWrapping.Wrap };

        internal static TextBlock Seccion(string texto) =>
            new TextBlock { Text = texto, FontSize = 11, FontWeight = FontWeights.SemiBold, Foreground = System.Windows.Media.Brushes.CornflowerBlue, Margin = new Thickness(0,12,0,6) };

        internal static TextBox Row(StackPanel sp, string label, string val)
        {
            sp.Children.Add(new TextBlock { Text = label, FontSize = 11, Margin = new Thickness(0,6,0,3) });
            var tb = new TextBox { Text = val, Height = 30, Padding = new Thickness(8,0,0,0), VerticalContentAlignment = VerticalAlignment.Center };
            sp.Children.Add(tb);
            return tb;
        }

        internal static UIElement BotonesOkCancelar(string textoOk, Action onOk, Window owner = null)
        {
            var sp = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0,16,0,0) };
            var btnCnl = new Button { Content = "Cancelar", Width = 80,  Height = 34, IsCancel  = true, Margin = new Thickness(0,0,8,0) };
            var btnOk  = new Button { Content = textoOk,    Width = 140, Height = 34, IsDefault = true };
            btnCnl.Click += (s, e) => { var w = ((Button)s).FindAncestor<Window>(); if (w != null) w.DialogResult = false; };
            btnOk.Click  += (s, e) => { onOk?.Invoke(); var w = ((Button)s).FindAncestor<Window>(); if (w != null) w.DialogResult = true; };
            sp.Children.Add(btnCnl);
            sp.Children.Add(btnOk);
            return sp;
        }

        internal static double ParseD(string s, double def)
        {
            return double.TryParse((s ?? "").Replace(",", "."),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double v) ? v : def;
        }

        internal static int ParseI(string s, int def)
            => int.TryParse(s, out int v) ? v : def;

        internal static T FindAncestor<T>(this DependencyObject obj) where T : DependencyObject
        {
            var parent = System.Windows.Media.VisualTreeHelper.GetParent(obj);
            while (parent != null)
            {
                if (parent is T t) return t;
                parent = System.Windows.Media.VisualTreeHelper.GetParent(parent);
            }
            return null;
        }
    }

    // Extender Window con los helpers para uso idiomático
    public abstract class CatastroWindow : Window
    {
        protected static TextBlock Header(string t)    => WindowHelpers.Header(t);
        protected static TextBlock Info(string t)      => WindowHelpers.Info(t);
        protected static TextBlock Seccion(string t)   => WindowHelpers.Seccion(t);
        protected static TextBox   Row(StackPanel sp, string l, string v) => WindowHelpers.Row(sp, l, v);
        protected static UIElement BotonesOkCancelar(string t, Action a)  => WindowHelpers.BotonesOkCancelar(t, a);
        protected static double    ParseD(string s, double d) => WindowHelpers.ParseD(s, d);
        protected static int       ParseI(string s, int d)    => WindowHelpers.ParseI(s, d);
    }
}
