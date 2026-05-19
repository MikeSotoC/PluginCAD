using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CatastroTools.UI.Views
{
    /// <summary>
    /// Clase base para todos los paneles de CatastroTools.
    /// Los recursos (brushes, estilos) vienen del App.xaml global.
    /// </summary>
    public class PanelBase : UserControl
    {
        protected readonly Action<string> _ejecutar;

        public PanelBase(Action<string> ejecutar)
        {
            _ejecutar = ejecutar;
            // Los recursos ya están disponibles globalmente desde App.xaml
        }

        // ─── HELPERS DE EJECUCIÓN ─────────────────────────────────
        protected void Ejecutar(string comando)
        {
            try { _ejecutar?.Invoke(comando); }
            catch (Exception ex) { MostrarError(ex.Message); }
        }

        // ─── HELPERS DE UI ────────────────────────────────────────
        protected StackPanel CrearContenedor(string titulo, string subtitulo = null)
        {
            var sp = new StackPanel();

            var txtTitulo = new TextBlock
            {
                Text = titulo,
                FontSize = 17,
                FontWeight = FontWeights.SemiBold,
                Foreground = (Brush)FindResource("BrushTexto"),
                Margin = new Thickness(0, 0, 0, 4)
            };
            sp.Children.Add(txtTitulo);

            if (subtitulo != null)
            {
                sp.Children.Add(new TextBlock
                {
                    Text = subtitulo,
                    FontSize = 12,
                    Foreground = (Brush)FindResource("BrushTextoSec"),
                    Margin = new Thickness(0, 0, 0, 20)
                });
            }
            return sp;
        }

        protected Border CrearTarjeta(UIElement contenido, bool accent = false)
        {
            var b = new Border
            {
                Background = (Brush)FindResource("BrushTarjeta"),
                BorderBrush = (Brush)FindResource(accent ? "BrushAccent" : "BrushBorde"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(16),
                Margin = new Thickness(0, 0, 0, 12),
                Child = contenido
            };
            return b;
        }

        protected StackPanel CrearSeccion(string titulo)
        {
            var sp = new StackPanel { Margin = new Thickness(0, 4, 0, 0) };
            sp.Children.Add(new TextBlock
            {
                Text = titulo.ToUpper(),
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = (Brush)FindResource("BrushAccent"),
                Margin = new Thickness(0, 16, 0, 8)
            });
            return sp;
        }

        protected Grid CrearFilaLabel(string label, UIElement control)
        {
            var g = new Grid { Margin = new Thickness(0, 0, 0, 12) };
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(160) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var lbl = new TextBlock
            {
                Text = label,
                Foreground = (Brush)FindResource("BrushTextoSec"),
                FontSize = 11,
                FontWeight = FontWeights.Medium,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 12, 0)
            };
            Grid.SetColumn(lbl, 0);
            Grid.SetColumn(control, 2);
            g.Children.Add(lbl);
            g.Children.Add(control);
            return g;
        }

        protected TextBox CrearInput(string valor = "", string placeholder = "")
        {
            var tb = new TextBox
            {
                Text = valor,
                Style = (Style)FindResource("InputBase"),
                MinWidth = 200
            };
            return tb;
        }

        protected ComboBox CrearCombo(string[] opciones, int seleccionado = 0)
        {
            var cb = new ComboBox
            {
                Style = (Style)FindResource("ComboBase"),
                MinWidth = 200
            };
            foreach (var op in opciones) cb.Items.Add(op);
            cb.SelectedIndex = seleccionado;
            return cb;
        }

        protected CheckBox CrearCheck(string texto, bool valor = false)
        {
            return new CheckBox
            {
                Content = texto,
                IsChecked = valor,
                Style = (Style)FindResource("CheckBase")
            };
        }

        protected TextBlock CrearInfoBox(string texto)
        {
            return new TextBlock
            {
                Text = texto,
                FontSize = 11,
                Foreground = (Brush)FindResource("BrushTextoSec"),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 8)
            };
        }

        protected Border CrearAlertaInfo(string texto)
        {
            var b = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(30, 59, 130, 246)),
                BorderBrush = (Brush)FindResource("BrushAccent"),
                BorderThickness = new Thickness(1, 0, 0, 0),
                CornerRadius = new CornerRadius(0, 4, 4, 0),
                Padding = new Thickness(12, 8, 12, 8),
                Margin = new Thickness(0, 0, 0, 12)
            };
            b.Child = new TextBlock
            {
                Text = texto,
                FontSize = 11,
                Foreground = (Brush)FindResource("BrushTexto"),
                TextWrapping = TextWrapping.Wrap
            };
            return b;
        }

        protected StackPanel CrearBotones(params (string texto, string cmd, bool primario)[] botones)
        {
            var sp = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 16, 0, 0)
            };
            foreach (var item in botones)
            {
                string texto = item.texto;
                string cmd = item.cmd;
                bool primario = item.primario;

                var btn = new Button
                {
                    Content = texto,
                    Style = primario
                        ? (Style)FindResource("BtnPrimario")
                        : (Style)FindResource("BtnSecundario"),
                    Margin = new Thickness(8, 0, 0, 0),
                    MinWidth = 100
                };

                var cmdLocal = cmd;

                btn.Click += (s, e) => Ejecutar(cmdLocal);

                sp.Children.Add(btn);
            }
            return sp;
        }

        protected void MostrarError(string msg) =>
            MessageBox.Show(msg, "CatastroTools", MessageBoxButton.OK, MessageBoxImage.Warning);

        protected void MostrarOk(string msg) =>
            MessageBox.Show(msg, "CatastroTools", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
