using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CatastroTools.Core.Models;

namespace CatastroTools.UI.Views
{
    // ═══════════════════════════════════════════════════════════
    // PANEL: IMPORTAR COORDENADAS UTM
    // ═══════════════════════════════════════════════════════════
    public class PanelImportarCoords : PanelBase
    {
        private readonly ListBox  _listaVertices;
        private readonly TextBox  _txEste, _txNorte;
        private readonly CheckBox _chkMarcar, _chkArea;
        private readonly List<(double E, double N)> _puntos = new List<(double, double)>();

        public PanelImportarCoords(Action<string> exec) : base(exec)
        {
            var sp = CrearContenedor("📍 Importar Coordenadas UTM",
                "Ingresa los vértices manualmente y crea la polilínea del predio");

            sp.Children.Add(CrearAlertaInfo(
                "ℹ Los valores deben estar en metros (UTM WGS84). " +
                "Este (E) = columna X,  Norte (N) = columna Y."));

            // Lista de puntos
            var secVerts = CrearSeccion("VÉRTICES INGRESADOS");
            _listaVertices = new ListBox
            {
                Style  = (Style)FindResource("ListaBase"),
                Height = 160,
                Margin = new Thickness(0, 0, 0, 8)
            };
            secVerts.Children.Add(_listaVertices);

            // Input de nuevo punto
            var grid = new Grid { Margin = new Thickness(0, 0, 0, 8) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            _txEste  = CrearInput("", "Este E (m)");  Grid.SetColumn(_txEste, 0);
            _txNorte = CrearInput("", "Norte N (m)"); Grid.SetColumn(_txNorte, 2);

            var btnAgregar  = new Button { Content = "Agregar", Style = (Style)FindResource("BtnPrimario"),  Height = 36, MinWidth = 80 };
            var btnEliminar = new Button { Content = "Eliminar", Style = (Style)FindResource("BtnSecundario"), Height = 36, MinWidth = 80 };
            Grid.SetColumn(btnAgregar, 4);
            Grid.SetColumn(btnEliminar, 6);

            btnAgregar.Click  += AgregarPunto;
            btnEliminar.Click += EliminarPunto;

            grid.Children.Add(_txEste);
            grid.Children.Add(_txNorte);
            grid.Children.Add(btnAgregar);
            grid.Children.Add(btnEliminar);
            secVerts.Children.Add(grid);

            // Opciones
            _chkMarcar = CrearCheck("Marcar vértices con coordenadas UTM", true);
            _chkArea   = CrearCheck("Mostrar área calculada", true);
            secVerts.Children.Add(_chkMarcar);
            secVerts.Children.Add(_chkArea);

            sp.Children.Add(CrearTarjeta(secVerts));
            sp.Children.Add(CrearBotones(
                ("Crear Polilínea en ZWCAD/AutoCAD", "CT-IMPORTAR-COORDS", true)));

            Content = sp;
        }

        private void AgregarPunto(object s, RoutedEventArgs e)
        {
            if (!double.TryParse(_txEste.Text.Replace(",", "."),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double este) ||
                !double.TryParse(_txNorte.Text.Replace(",", "."),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double norte))
            {
                MostrarError("Ingresa valores numéricos válidos para E y N."); return;
            }
            _puntos.Add((este, norte));
            _listaVertices.Items.Add($"V-{_puntos.Count}    E={este:F4}    N={norte:F4}");
            _txEste.Text = _txNorte.Text = "";
            _txEste.Focus();
        }

        private void EliminarPunto(object s, RoutedEventArgs e)
        {
            int idx = _listaVertices.SelectedIndex;
            if (idx < 0) return;
            _puntos.RemoveAt(idx);
            _listaVertices.Items.RemoveAt(idx);
            // Re-numerar
            for (int i = 0; i < _listaVertices.Items.Count; i++)
                _listaVertices.Items[i] = $"V-{i + 1}    E={_puntos[i].E:F4}    N={_puntos[i].N:F4}";
        }
    }

    // ═══════════════════════════════════════════════════════════
    // PANEL: VÍA POR EJE
    // ═══════════════════════════════════════════════════════════
    public class PanelViaEje : PanelBase
    {
        private readonly ComboBox _cbTipo;
        private readonly TextBox  _txNombre, _txAncho, _txCalzada, _txVereda;

        private static readonly (string Nombre, double Ancho, double Calzada, double Vereda)[] TiposVia =
        {
            ("Avenida Principal",   22.0, 14.4, 3.0),
            ("Avenida Secundaria",  18.0, 11.0, 3.0),
            ("Vía Colectora",       15.0,  7.2, 2.4),
            ("Calle Local",          8.0,  5.4, 1.8),
            ("Vía Local",            8.0,  5.4, 1.8),
            ("Pasaje Vehicular",     6.0,  3.0, 1.5),
            ("Pasaje Peatonal",      3.0,  0.0, 3.0),
            ("Personalizado",        0.0,  0.0, 0.0),
        };

        public PanelViaEje(Action<string> exec) : base(exec)
        {
            var sp = CrearContenedor("🛣 Trazar Vía por Eje",
                "Define el eje de la vía y el sistema genera los bordes automáticamente");

            sp.Children.Add(CrearAlertaInfo(
                "ℹ Anchos según RNE GH.020 — Artículo 10. " +
                "Selecciona el tipo o ingresa un valor personalizado."));

            var sec = CrearSeccion("TIPO DE VÍA");

            _cbTipo = CrearCombo(new[]
            {
                "Avenida Principal   — 22.00 m",
                "Avenida Secundaria  — 18.00 m",
                "Vía Colectora       — 15.00 m",
                "Calle Local         — 8.00 m",
                "Vía Local           — 8.00 m",
                "Pasaje Vehicular    — 6.00 m",
                "Pasaje Peatonal     — 3.00 m",
                "Personalizado"
            }, 3);
            _cbTipo.SelectionChanged += TipoChanged;

            sec.Children.Add(CrearFilaLabel("Tipo de vía:", _cbTipo));

            _txNombre  = CrearInput("Calle");
            _txAncho   = CrearInput("8.00");
            _txCalzada = CrearInput("5.40");
            _txVereda  = CrearInput("1.80");

            sec.Children.Add(CrearFilaLabel("Nombre de la vía:", _txNombre));
            sec.Children.Add(CrearFilaLabel("Ancho total (m):", _txAncho));
            sec.Children.Add(CrearFilaLabel("Ancho calzada (m):", _txCalzada));
            sec.Children.Add(CrearFilaLabel("Ancho vereda c/u (m):", _txVereda));

            sp.Children.Add(CrearTarjeta(sec));
            sp.Children.Add(CrearBotones(
                ("Trazar Eje en CAD", "CT-VIA-EJE", true),
                ("Ver Sección", "CT-SECCION-VIA", false)));

            Content = sp;
        }

        private void TipoChanged(object s, SelectionChangedEventArgs e)
        {
            int idx = _cbTipo.SelectedIndex;
            if (idx < 0 || idx >= TiposVia.Length) return;
            var t = TiposVia[idx];
            if (t.Ancho > 0)
            {
                _txNombre.Text  = t.Nombre;
                _txAncho.Text   = t.Ancho.ToString("F2");
                _txCalzada.Text = t.Calzada.ToString("F2");
                _txVereda.Text  = t.Vereda.ToString("F2");
            }
        }
    }

    // ═══════════════════════════════════════════════════════════
    // PANEL: GRILLA DE VÍAS
    // ═══════════════════════════════════════════════════════════
    public class PanelViasGrilla : PanelBase
    {
        public PanelViasGrilla(Action<string> exec) : base(exec)
        {
            var sp = CrearContenedor("⊞ Grilla de Vías",
                "Genera automáticamente la red vial ortogonal sobre el predio");

            var sec = CrearSeccion("CALLES HORIZONTALES (E-O)");
            sec.Children.Add(CrearFilaLabel("Ancho (m):",       CrearInput("8.00")));
            sec.Children.Add(CrearFilaLabel("Separación (m):",  CrearInput("60.00")));
            sec.Children.Add(CrearFilaLabel("Nombre base:",     CrearInput("Calle")));
            sp.Children.Add(CrearTarjeta(sec));

            var sec2 = CrearSeccion("CALLES VERTICALES (N-S)");
            sec2.Children.Add(CrearFilaLabel("Ancho (m):",      CrearInput("8.00")));
            sec2.Children.Add(CrearFilaLabel("Separación (m):", CrearInput("100.00")));
            sec2.Children.Add(CrearFilaLabel("Nombre base:",    CrearInput("Pasaje")));
            sp.Children.Add(CrearTarjeta(sec2));

            sp.Children.Add(CrearAlertaInfo(
                "ℹ Selecciona el predio matriz en CAD antes de ejecutar."));
            sp.Children.Add(CrearBotones(("Generar Grilla en CAD", "CT-VIAS-GRILLA", true)));

            Content = sp;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // PANEL: SECCIÓN VIAL EN PLANTA
    // ═══════════════════════════════════════════════════════════
    public class PanelSeccionVia : PanelBase
    {
        public PanelSeccionVia(Action<string> exec) : base(exec)
        {
            var sp = CrearContenedor("📐 Sección Vial en Planta",
                "Dibuja la sección transversal de una vía con todas sus componentes");

            sp.Children.Add(CrearAlertaInfo(
                "ℹ La sección se dibuja en planta con acotaciones de cada componente: " +
                "calzada, veredas, bermas. Basado en RNE GH.020."));

            var sec = CrearSeccion("COMPONENTES DE LA VÍA");
            sec.Children.Add(CrearFilaLabel("Nombre:",           CrearInput("Calle Los Pinos")));
            sec.Children.Add(CrearFilaLabel("Ancho total (m):",  CrearInput("8.00")));
            sec.Children.Add(CrearFilaLabel("Calzada (m):",      CrearInput("5.40")));
            sec.Children.Add(CrearFilaLabel("Vereda c/u (m):",   CrearInput("1.80")));
            sec.Children.Add(CrearFilaLabel("Berma c/u (m):",    CrearInput("0.00")));
            sp.Children.Add(CrearTarjeta(sec));

            // Previsualización esquemática
            var secPrev = CrearSeccion("ESQUEMA");
            secPrev.Children.Add(new Border
            {
                Height     = 80,
                Background = new SolidColorBrush(Color.FromRgb(26, 35, 56)),
                CornerRadius = new CornerRadius(6),
                Child      = new TextBlock
                {
                    Text              = "[ VEREDA ]  [ CALZADA ]  [ VEREDA ]",
                    Foreground        = (Brush)FindResource("BrushTextoSec"),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize          = 11
                }
            });
            sp.Children.Add(CrearTarjeta(secPrev));

            sp.Children.Add(CrearBotones(
                ("Insertar Sección en CAD", "CT-SECCION-VIA", true)));

            Content = sp;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // PANEL: MANZANEO MANUAL
    // ═══════════════════════════════════════════════════════════
    public class PanelManzaneo : PanelBase
    {
        public PanelManzaneo(Action<string> exec) : base(exec)
        {
            var sp = CrearContenedor("🏙 Manzaneo Manual",
                "Etiqueta manzanas ya dibujadas en el plano");

            var sec = CrearSeccion("NOMENCLATURA");
            sec.Children.Add(CrearFilaLabel("Sistema:",
                CrearCombo(new[] { "Alfabético (MZ A, MZ B...)", "Numérico (MZ 1, MZ 2...)", "Personalizado" })));
            sec.Children.Add(CrearFilaLabel("Inicio en:", CrearInput("1")));
            sec.Children.Add(CrearFilaLabel("Altura texto (m):", CrearInput("4.00")));
            sp.Children.Add(CrearTarjeta(sec));

            sp.Children.Add(CrearAlertaInfo(
                "ℹ Selecciona las polilíneas de las manzanas en CAD. " +
                "Se etiquetarán en el orden de selección."));
            sp.Children.Add(CrearBotones(("Etiquetar Manzanas", "CT-MANZANEO", true)));

            Content = sp;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // PANEL: MANZANEO GRILLA
    // ═══════════════════════════════════════════════════════════
    public class PanelManzaneoGrilla : PanelBase
    {
        public PanelManzaneoGrilla(Action<string> exec) : base(exec)
        {
            var sp = CrearContenedor("⊞ Manzaneo Grilla",
                "Genera manzanas automáticamente a partir de los parámetros de la grilla");

            var sec = CrearSeccion("PARÁMETROS");
            sec.Children.Add(CrearFilaLabel("Sep. calles H (m):", CrearInput("60.00")));
            sec.Children.Add(CrearFilaLabel("Sep. calles V (m):", CrearInput("100.00")));
            sec.Children.Add(CrearFilaLabel("Ancho vía H (m):",   CrearInput("8.00")));
            sec.Children.Add(CrearFilaLabel("Ancho vía V (m):",   CrearInput("8.00")));
            sp.Children.Add(CrearTarjeta(sec));

            var sec2 = CrearSeccion("NOMENCLATURA");
            sec2.Children.Add(CrearFilaLabel("Sistema:",
                CrearCombo(new[] { "Alfabético (MZ A, MZ B...)", "Numérico (MZ 1, MZ 2...)" })));
            sec2.Children.Add(CrearFilaLabel("Altura texto (m):", CrearInput("4.00")));
            sp.Children.Add(CrearTarjeta(sec2));

            sp.Children.Add(CrearBotones(("Generar Manzanas", "CT-MANZANEO-GRILLA", true)));

            Content = sp;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // PANEL: LOTIZAR
    // ═══════════════════════════════════════════════════════════
    public class PanelLotizar : PanelBase
    {
        public PanelLotizar(Action<string> exec) : base(exec)
        {
            var sp = CrearContenedor("🏠 Lotizar por Recorrido",
                "Numeración automática + etiquetas + colindancias en un solo paso");

            sp.Children.Add(CrearAlertaInfo(
                "PASO A PASO:\n" +
                "1. Selecciona todos los lotes (polilíneas cerradas)\n" +
                "2. Traza la polilínea de recorrido sobre los lotes en el orden de numeración\n" +
                "3. El sistema asigna números, calcula áreas y etiqueta colindancias automáticamente"));

            var sec = CrearSeccion("CONFIGURACIÓN");
            sec.Children.Add(CrearFilaLabel("Prefijo:",          CrearInput("Lote ")));
            sec.Children.Add(CrearFilaLabel("Número inicial:",   CrearInput("1")));
            sec.Children.Add(CrearFilaLabel("Altura texto (m):", CrearInput("2.50")));
            sec.Children.Add(CrearCheck("Dibujar colindancias automáticamente", true));
            sec.Children.Add(CrearCheck("Marcar vértices UTM", false));
            sp.Children.Add(CrearTarjeta(sec));

            sp.Children.Add(CrearBotones(
                ("Iniciar Lotización", "CT-LOTIZAR", true),
                ("Solo Colindancias", "CT-COLIN-AUTO", false)));

            Content = sp;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // PANEL: HABILITACIÓN GRILLA
    // ═══════════════════════════════════════════════════════════
    public class PanelHabilitacion : PanelBase
    {
        public PanelHabilitacion(Action<string> exec) : base(exec)
        {
            var sp = CrearContenedor("⊞ Habilitación en Grilla",
                "Genera lotes en grilla dentro de una manzana — RNE GH.020");

            var sec = CrearSeccion("DIMENSIONES DE LOTE");
            sec.Children.Add(CrearFilaLabel("Frente (m):",       CrearInput("8.00")));
            sec.Children.Add(CrearFilaLabel("Profundidad (m):",  CrearInput("15.00")));
            sec.Children.Add(CrearFilaLabel("Número inicial:",   CrearInput("1")));
            sec.Children.Add(CrearCheck("Numerar automáticamente", true));
            sec.Children.Add(CrearCheck("Etiquetar áreas", true));
            sp.Children.Add(CrearTarjeta(sec));

            sp.Children.Add(CrearBotones(("Generar Lotes en CAD", "CT-HABILITACION", true)));

            Content = sp;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // PANEL: SUBDIVISIÓN
    // ═══════════════════════════════════════════════════════════
    public class PanelSubdivision : PanelBase
    {
        public PanelSubdivision(Action<string> exec) : base(exec)
        {
            var sp = CrearContenedor("✂ Subdividir Lote",
                "Corta un lote existente con una línea y genera dos sublotes");

            sp.Children.Add(CrearAlertaInfo(
                "ℹ Selecciona la polilínea cerrada del lote, " +
                "luego indica 2 puntos de corte que la crucen."));

            sp.Children.Add(CrearBotones(("Subdividir en CAD", "CT-SUBDIV", true)));

            Content = sp;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // PANEL: ETIQUETAR LOTE
    // ═══════════════════════════════════════════════════════════
    public class PanelEtiquetaLote : PanelBase
    {
        public PanelEtiquetaLote(Action<string> exec) : base(exec)
        {
            var sp = CrearContenedor("🏷 Etiquetar Lote",
                "Inserta etiqueta completa en formato SUNARP / Municipalidad");

            var sec = CrearSeccion("DATOS DEL PREDIO");
            sec.Children.Add(CrearFilaLabel("N° Lote:",            CrearInput("")));
            sec.Children.Add(CrearFilaLabel("Manzana:",            CrearInput("")));
            sec.Children.Add(CrearFilaLabel("Propietario:",        CrearInput("")));
            sec.Children.Add(CrearFilaLabel("Partida registral:",  CrearInput("")));
            sec.Children.Add(CrearFilaLabel("Zonificación:",       CrearInput("")));
            sp.Children.Add(CrearTarjeta(sec));

            var sec2 = CrearSeccion("FORMATO");
            sec2.Children.Add(CrearFilaLabel("Altura texto (m):", CrearInput("2.50")));
            sec2.Children.Add(CrearCheck("Mostrar partida registral", true));
            sec2.Children.Add(CrearCheck("Mostrar zonificación", false));
            sp.Children.Add(CrearTarjeta(sec2));

            sp.Children.Add(CrearBotones(
                ("Etiquetar en CAD", "CT-ETIQUETA", true),
                ("Acotar Completo", "CT-ACOTAR", false)));

            Content = sp;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // PANEL: ACOTAR COMPLETO
    // ═══════════════════════════════════════════════════════════
    public class PanelAcotar : PanelBase
    {
        public PanelAcotar(Action<string> exec) : base(exec)
        {
            var sp = CrearContenedor("📏 Acotación Completa",
                "Etiqueta central + linderos con distancias y rumbos");

            var sec = CrearSeccion("DATOS");
            sec.Children.Add(CrearFilaLabel("N° Lote:",           CrearInput("")));
            sec.Children.Add(CrearFilaLabel("Propietario:",       CrearInput("")));
            sec.Children.Add(CrearFilaLabel("Partida:",           CrearInput("")));
            sp.Children.Add(CrearTarjeta(sec));

            var sec2 = CrearSeccion("FORMATO");
            sec2.Children.Add(CrearFilaLabel("Altura etiqueta (m):", CrearInput("2.50")));
            sec2.Children.Add(CrearFilaLabel("Altura linderos (m):", CrearInput("1.80")));
            sec2.Children.Add(CrearCheck("Mostrar rumbo (N°E/S°O)", true));
            sp.Children.Add(CrearTarjeta(sec2));

            sp.Children.Add(CrearBotones(
                ("Acotar en CAD", "CT-ACOTAR", true),
                ("Solo Linderos", "CT-LINDEROS", false)));

            Content = sp;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // PANEL: VÉRTICES / MOJONES
    // ═══════════════════════════════════════════════════════════
    public class PanelVertices : PanelBase
    {
        public PanelVertices(Action<string> exec) : base(exec)
        {
            var sp = CrearContenedor("📌 Vértices y Mojones",
                "Marca los vértices del lote con símbolo y coordenadas UTM");

            var sec = CrearSeccion("SÍMBOLO");
            sec.Children.Add(CrearFilaLabel("Tipo:",
                CrearCombo(new[] { "Cruz (catastral)", "Círculo", "Triángulo", "Rombo" })));
            sec.Children.Add(CrearFilaLabel("Tamaño símbolo (m):", CrearInput("1.50")));
            sp.Children.Add(CrearTarjeta(sec));

            var sec2 = CrearSeccion("ETIQUETA UTM");
            sec2.Children.Add(CrearFilaLabel("Prefijo:", CrearInput("V-")));
            sec2.Children.Add(CrearFilaLabel("Número inicial:", CrearInput("1")));
            sec2.Children.Add(CrearFilaLabel("Altura texto (m):", CrearInput("1.80")));
            sec2.Children.Add(CrearFilaLabel("Offset X (m):", CrearInput("2.00")));
            sec2.Children.Add(CrearFilaLabel("Offset Y (m):", CrearInput("1.00")));
            sec2.Children.Add(CrearFilaLabel("Decimales UTM:",
                CrearCombo(new[] { "2", "3", "4 (recomendado)", "5" }, 2)));
            sp.Children.Add(CrearTarjeta(sec2));

            sp.Children.Add(CrearBotones(("Marcar Vértices en CAD", "CT-VERTICES", true)));

            Content = sp;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // PANEL: TABLA TÉCNICA
    // ═══════════════════════════════════════════════════════════
    public class PanelTablaTecnica : PanelBase
    {
        public PanelTablaTecnica(Action<string> exec) : base(exec)
        {
            var sp = CrearContenedor("📋 Cuadro de Datos Técnicos",
                "Tabla SUNARP con todos los datos del predio");

            var sec = CrearSeccion("TITULAR");
            sec.Children.Add(CrearFilaLabel("Propietario:",       CrearInput("")));
            sec.Children.Add(CrearFilaLabel("DNI / RUC:",         CrearInput("")));
            sec.Children.Add(CrearFilaLabel("Dirección:",         CrearInput("")));
            sp.Children.Add(CrearTarjeta(sec));

            var sec2 = CrearSeccion("UBICACIÓN");
            sec2.Children.Add(CrearFilaLabel("Distrito:",         CrearInput("")));
            sec2.Children.Add(CrearFilaLabel("Provincia:",        CrearInput("")));
            sec2.Children.Add(CrearFilaLabel("Departamento:",     CrearInput("Tacna")));
            sp.Children.Add(CrearTarjeta(sec2));

            var sec3 = CrearSeccion("REGISTRO");
            sec3.Children.Add(CrearFilaLabel("Lote:",             CrearInput("")));
            sec3.Children.Add(CrearFilaLabel("Manzana:",          CrearInput("")));
            sec3.Children.Add(CrearFilaLabel("Habilitación:",     CrearInput("")));
            sec3.Children.Add(CrearFilaLabel("Partida SUNARP:",   CrearInput("")));
            sec3.Children.Add(CrearFilaLabel("Zonificación:",     CrearInput("")));
            sec3.Children.Add(CrearFilaLabel("Escala:",           CrearInput("1/200")));
            sp.Children.Add(CrearTarjeta(sec3));

            sp.Children.Add(CrearBotones(
                ("Insertar Tabla en CAD", "CT-TABLA", true),
                ("Coords UTM", "CT-TABLA-COORDS", false),
                ("Colindancias", "CT-TABLA-COLIN", false)));

            Content = sp;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // PANEL: TABLA COORDENADAS
    // ═══════════════════════════════════════════════════════════
    public class PanelTablaCoords : PanelBase
    {
        public PanelTablaCoords(Action<string> exec) : base(exec)
        {
            var sp = CrearContenedor("📊 Cuadro de Coordenadas UTM",
                "Tabla de vértices Este/Norte dibujada en el plano");

            var sec = CrearSeccion("FORMATO");
            sec.Children.Add(CrearFilaLabel("Ancho tabla (m):",  CrearInput("80.00")));
            sec.Children.Add(CrearFilaLabel("Alto fila (m):",    CrearInput("7.00")));
            sec.Children.Add(CrearFilaLabel("Altura texto (m):", CrearInput("2.00")));
            sec.Children.Add(CrearFilaLabel("Decimales UTM:",
                CrearCombo(new[] { "3", "4 (recomendado)", "5" }, 1)));
            sp.Children.Add(CrearTarjeta(sec));

            sp.Children.Add(CrearBotones(("Insertar Tabla en CAD", "CT-TABLA-COORDS", true)));

            Content = sp;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // PANEL: TABLA COLINDANCIAS
    // ═══════════════════════════════════════════════════════════
    public class PanelTablaColindancias : PanelBase
    {
        public PanelTablaColindancias(Action<string> exec) : base(exec)
        {
            var sp = CrearContenedor("🧭 Cuadro de Colindancias",
                "Tabla de colindancias por punto cardinal");

            var sec = CrearSeccion("COLINDANCIAS");
            sec.Children.Add(CrearFilaLabel("NORTE:", CrearInput("")));
            sec.Children.Add(CrearFilaLabel("SUR:",   CrearInput("")));
            sec.Children.Add(CrearFilaLabel("ESTE:",  CrearInput("")));
            sec.Children.Add(CrearFilaLabel("OESTE:", CrearInput("")));
            sp.Children.Add(CrearTarjeta(sec));

            sp.Children.Add(CrearBotones(("Insertar Tabla en CAD", "CT-TABLA-COLIN", true)));

            Content = sp;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // PANEL: EXPORTAR HTML
    // ═══════════════════════════════════════════════════════════
    public class PanelExportHTML : PanelBase
    {
        public PanelExportHTML(Action<string> exec) : base(exec)
        {
            var sp = CrearContenedor("🌐 Exportar Reporte HTML",
                "Genera ficha técnica catastral completa para expediente SUNARP");

            sp.Children.Add(CrearAlertaInfo(
                "ℹ El reporte incluye: datos del titular, ubicación, " +
                "cuadro de vértices UTM, linderos con rumbos y colindancias. " +
                "Se guarda en la misma carpeta que el DWG."));

            var sec = CrearSeccion("DATOS DEL EXPEDIENTE");
            sec.Children.Add(CrearFilaLabel("Propietario:",   CrearInput("")));
            sec.Children.Add(CrearFilaLabel("DNI / RUC:",    CrearInput("")));
            sec.Children.Add(CrearFilaLabel("Dirección:",    CrearInput("")));
            sec.Children.Add(CrearFilaLabel("Distrito:",     CrearInput("")));
            sec.Children.Add(CrearFilaLabel("Partida:",      CrearInput("")));
            sec.Children.Add(CrearFilaLabel("Lote:",         CrearInput("")));
            sec.Children.Add(CrearCheck("Abrir en navegador al generar", true));
            sp.Children.Add(CrearTarjeta(sec));

            sp.Children.Add(CrearBotones(("Generar Reporte HTML", "CT-EXPORT-HTML", true)));

            Content = sp;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // PANEL: EXPORTAR CSV
    // ═══════════════════════════════════════════════════════════
    public class PanelExportCSV : PanelBase
    {
        public PanelExportCSV(Action<string> exec) : base(exec)
        {
            var sp = CrearContenedor("📄 Exportar CSV",
                "Exporta coordenadas UTM para GIS, Excel o informe técnico");

            var sec = CrearSeccion("OPCIONES");
            sec.Children.Add(CrearFilaLabel("Exportar:",
                CrearCombo(new[] { "Vértices del lote seleccionado", "Todos los lotes del proyecto" })));
            sec.Children.Add(CrearFilaLabel("Decimales UTM:",
                CrearCombo(new[] { "3", "4 (recomendado)", "5" }, 1)));
            sp.Children.Add(CrearTarjeta(sec));

            sp.Children.Add(CrearBotones(
                ("Exportar CSV", "CT-EXPORT-CSV", true),
                ("Exportar Lotes", "CT-EXPORT-LOTES-CSV", false)));

            Content = sp;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // PANEL: CONFIGURACIÓN
    // ═══════════════════════════════════════════════════════════
    public class PanelConfiguracion : PanelBase
    {
        public PanelConfiguracion() : base(null)
        {
            var sp = CrearContenedor("🔧 Configuración",
                "Ajusta los valores por defecto del sistema");

            var sec = CrearSeccion("ALTURAS DE TEXTO (metros)");
            sec.Children.Add(CrearFilaLabel("Número de lote:",  CrearInput("3.00")));
            sec.Children.Add(CrearFilaLabel("Área:",            CrearInput("2.50")));
            sec.Children.Add(CrearFilaLabel("Propietario:",     CrearInput("2.00")));
            sec.Children.Add(CrearFilaLabel("Linderos:",        CrearInput("1.80")));
            sec.Children.Add(CrearFilaLabel("Vértices:",        CrearInput("1.80")));
            sec.Children.Add(CrearFilaLabel("Manzana:",         CrearInput("4.00")));
            sp.Children.Add(CrearTarjeta(sec));

            var sec2 = CrearSeccion("VÉRTICES");
            sec2.Children.Add(CrearFilaLabel("Prefijo:",        CrearInput("V-")));
            sec2.Children.Add(CrearFilaLabel("Número inicial:", CrearInput("1")));
            sec2.Children.Add(CrearFilaLabel("Decimales UTM:",
                CrearCombo(new[] { "3", "4", "5" }, 1)));
            sp.Children.Add(CrearTarjeta(sec2));

            var sec3 = CrearSeccion("LINDEROS");
            sec3.Children.Add(CrearCheck("Mostrar rumbo geográfico", true));
            sec3.Children.Add(CrearCheck("Mostrar partida registral", true));
            sec3.Children.Add(CrearFilaLabel("Offset texto (m):", CrearInput("2.50")));
            sp.Children.Add(CrearTarjeta(sec3));

            sp.Children.Add(CrearBotones(
                ("Guardar Configuración", "CT-CONFIG", true)));

            Content = sp;
        }
    }
}
