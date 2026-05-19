using System.Windows.Forms;
using GeoSuite.Platform;

namespace GeoSuite.UI.WinForms;

/// <summary>
/// Formulario principal tipo paleta (Toolbox) para GeoSuite.
/// Diseño compacto y moderno inspirado en CivilCAD.
/// </summary>
public class MainPaletteForm : Form
{
    private readonly ICadHost _cadHost;
    private Panel _mainPanel;
    private FlowLayoutPanel _buttonPanel;

    public MainPaletteForm()
    {
        _cadHost = CadServiceFactory.Create();
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        // Configuración base del formulario
        this.Text = "GeoSuite Tools";
        this.Size = new Size(220, 500);
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.StartPosition = FormStartPosition.Manual;
        this.TopMost = true; // Siempre visible sobre CAD
        this.BackColor = Color.FromArgb(30, 30, 30); // Fondo oscuro moderno
        
        // Posición inicial (esquina superior derecha típica)
        var screen = Screen.PrimaryScreen!.WorkingArea;
        this.Location = new Point(screen.Right - this.Width - 50, 100);

        // Panel principal
        _mainPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(8),
            BackColor = Color.FromArgb(30, 30, 30)
        };

        // Título
        var lblTitle = new Label
        {
            Text = "GEOSUITE",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.FromArgb(0, 122, 204), // Azul técnico
            AutoSize = true,
            Location = new Point(10, 10),
            BackColor = Color.Transparent
        };
        _mainPanel.Controls.Add(lblTitle);

        // Subtítulo
        var lblSubtitle = new Label
        {
            Text = "Ingeniería & Catastro",
            Font = new Font("Segoe UI", 8),
            ForeColor = Color.Gray,
            AutoSize = true,
            Location = new Point(12, 38),
            BackColor = Color.Transparent
        };
        _mainPanel.Controls.Add(lblSubtitle);

        // Línea separadora
        var line = new Panel
        {
            Size = new Size(190, 1),
            Location = new Point(10, 65),
            BackColor = Color.FromArgb(60, 60, 60)
        };
        _mainPanel.Controls.Add(line);

        // Panel de botones con scroll
        _buttonPanel = new FlowLayoutPanel
        {
            Location = new Point(10, 75),
            Size = new Size(190, 380),
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            BackColor = Color.FromArgb(30, 30, 30)
        };

        // Generar botones por categoría
        CreateCategoryButtons();

        _mainPanel.Controls.Add(_buttonPanel);
        this.Controls.Add(_mainPanel);
    }

    private void CreateCategoryButtons()
    {
        // --- SECCIÓN: TOPOGRAFÍA ---
        AddSectionHeader("TOPOGRAFÍA");
        AddActionButton("Importar Puntos", "GS-T-IMP", Color.FromArgb(0, 150, 136)); // Verde azulado
        AddActionButton("Generar TIN", "GS-T-TIN", Color.FromArgb(0, 150, 136));
        AddActionButton("Curvas Nivel", "GS-T-CN", Color.FromArgb(0, 150, 136));
        AddActionButton("Perfiles", "GS-T-PERF", Color.FromArgb(0, 150, 136));

        // --- SECCIÓN: CATASTRO ---
        AddSectionHeader("CATASTRO");
        AddActionButton("Dibujar Polígono", "GS-C-POLY", Color.FromArgb(255, 152, 0)); // Naranja
        AddActionButton("Etiquetar Lote", "GS-C-LBL", Color.FromArgb(255, 152, 0));
        AddActionButton("Subdividir", "GS-C-SUB", Color.FromArgb(255, 152, 0));
        AddActionButton("Tabla Técnica", "GS-C-TAB", Color.FromArgb(255, 152, 0));

        // --- SECCIÓN: VÍAS ---
        AddSectionHeader("VÍAS / CARRETERAS");
        AddActionButton("Alineamiento", "GS-R-ALINE", Color.FromArgb(33, 150, 243)); // Azul
        AddActionButton("Perfil Long", "GS-R-PERF", Color.FromArgb(33, 150, 243));
        AddActionButton("Secciones", "GS-R-SECC", Color.FromArgb(33, 150, 243));
        AddActionButton("Volúmenes", "GS-R-VOL", Color.FromArgb(33, 150, 243));

        // --- SECCIÓN: HIDRÁULICA ---
        AddSectionHeader("HIDRÁULICA");
        AddActionButton("Red Drenaje", "GS-H-DREN", Color.FromArgb(156, 39, 176)); // Morado
        AddActionButton("Cunetas", "GS-H-CUN", Color.FromArgb(156, 39, 176));
        AddActionButton("Perfil Tubería", "GS-H-PERF", Color.FromArgb(156, 39, 176));

        // Botón cerrar
        var btnClose = new Button
        {
            Text = "Cerrar Panel",
            Size = new Size(180, 35),
            Margin = new Padding(0, 20, 0, 0),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(211, 47, 47), // Rojo
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9, FontStyle.Bold)
        };
        btnClose.FlatAppearance.BorderSize = 0;
        btnClose.Click += (s, e) => this.Close();
        _buttonPanel.Controls.Add(btnClose);
    }

    private void AddSectionHeader(string text)
    {
        var lbl = new Label
        {
            Text = text,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = Color.FromArgb(180, 180, 180),
            AutoSize = true,
            Margin = new Padding(0, 15, 0, 5),
            BackColor = Color.Transparent
        };
        _buttonPanel.Controls.Add(lbl);
    }

    private void AddActionButton(string text, string command, Color color)
    {
        var btn = new Button
        {
            Text = text,
            Size = new Size(180, 32),
            Margin = new Padding(0, 2, 0, 2),
            FlatStyle = FlatStyle.Flat,
            BackColor = color,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9),
            Tag = command // Guardamos el comando en Tag
        };
        
        // Estilo plano personalizado
        btn.FlatAppearance.BorderSize = 0;
        btn.FlatAppearance.MouseOverBackColor = ControlPaint.Lighter(color, 0.2f);
        
        // Evento click
        btn.Click += Btn_Click;
        
        _buttonPanel.Controls.Add(btn);
    }

    private void Btn_Click(object? sender, EventArgs e)
    {
        if (sender is Button btn && btn.Tag is string command)
        {
            // Ejecutar comando en CAD
            _cadHost.SendCommand($"{command}\n");
            
            // Feedback visual opcional
            btn.BackColor = ControlPaint.Lighter(((Button)sender).BackColor, 0.3f);
            System.Threading.Thread.Sleep(100);
            btn.BackColor = ((Button)sender).Tag is string cmd ? 
                GetColorForCommand(cmd) : Color.Gray;
        }
    }

    private Color GetColorForCommand(string cmd)
    {
        if (cmd.StartsWith("GS-T")) return Color.FromArgb(0, 150, 136);
        if (cmd.StartsWith("GS-C")) return Color.FromArgb(255, 152, 0);
        if (cmd.StartsWith("GS-R")) return Color.FromArgb(33, 150, 243);
        if (cmd.StartsWith("GS-H")) return Color.FromArgb(156, 39, 176);
        return Color.Gray;
    }
}
