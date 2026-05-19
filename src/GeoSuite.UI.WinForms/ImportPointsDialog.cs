using System.Windows.Forms;

namespace GeoSuite.UI.WinForms;

/// <summary>
/// Diálogo de importación de puntos con vista previa.
/// Diseño compacto y funcional.
/// </summary>
public class ImportPointsDialog : Form
{
    private TextBox _txtFilePath;
    private ListBox _lstPreview;
    private Label _lblStatus;

    public string? SelectedFilePath { get; private set; }
    public bool Use3DPolylines { get; private set; }
    public string LayerName { get; private set; } = "TOPO-PUNTOS";

    public ImportPointsDialog()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Text = "Importar Puntos - GeoSuite";
        this.Size = new Size(450, 400);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.StartPosition = FormStartPosition.CenterParent;
        this.BackColor = Color.FromArgb(245, 245, 245);

        // Panel superior: Selección de archivo
        var pnlTop = new Panel
        {
            Dock = DockStyle.Top,
            Height = 60,
            Padding = new Padding(15),
            BackColor = Color.White
        };

        var lblFile = new Label
        {
            Text = "Archivo CSV/TXT:",
            AutoSize = true,
            Location = new Point(15, 10),
            Font = new Font("Segoe UI", 9)
        };
        pnlTop.Controls.Add(lblFile);

        _txtFilePath = new TextBox
        {
            Location = new Point(15, 30),
            Size = new Size(300, 25),
            Font = new Font("Segoe UI", 9),
            ReadOnly = true
        };
        pnlTop.Controls.Add(_txtFilePath);

        var btnBrowse = new Button
        {
            Text = "Examinar...",
            Location = new Point(325, 30),
            Size = new Size(90, 25),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(0, 122, 204),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9)
        };
        btnBrowse.FlatAppearance.BorderSize = 0;
        btnBrowse.Click += BtnBrowse_Click;
        pnlTop.Controls.Add(btnBrowse);

        this.Controls.Add(pnlTop);

        // Panel central: Vista previa
        var pnlMiddle = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(15, 5, 15, 5)
        };

        var lblPreview = new Label
        {
            Text = "Vista Previa (Primeras 10 filas):",
            AutoSize = true,
            Location = new Point(15, 10),
            Font = new Font("Segoe UI", 9, FontStyle.Bold)
        };
        pnlMiddle.Controls.Add(lblPreview);

        _lstPreview = new ListBox
        {
            Location = new Point(15, 35),
            Size = new Size(390, 200),
            Font = new Font("Consolas", 8),
            HorizontalScrollbar = true
        };
        pnlMiddle.Controls.Add(_lstPreview);

        _lblStatus = new Label
        {
            Text = "Estado: Esperando archivo...",
            AutoSize = true,
            Location = new Point(15, 245),
            ForeColor = Color.Gray,
            Font = new Font("Segoe UI", 8)
        };
        pnlMiddle.Controls.Add(_lblStatus);

        this.Controls.Add(pnlMiddle);

        // Panel inferior: Opciones y botones
        var pnlBottom = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 90,
            Padding = new Padding(15),
            BackColor = Color.White
        };

        var chk3D = new CheckBox
        {
            Text = "Conectar con polilínea 3D",
            Location = new Point(15, 10),
            AutoSize = true,
            Font = new Font("Segoe UI", 9)
        };
        chk3D.CheckedChanged += (s, e) => Use3DPolylines = chk3D.Checked;
        pnlBottom.Controls.Add(chk3D);

        var lblLayer = new Label
        {
            Text = "Capa:",
            Location = new Point(15, 40),
            AutoSize = true,
            Font = new Font("Segoe UI", 9)
        };
        pnlBottom.Controls.Add(lblLayer);

        var txtLayer = new TextBox
        {
            Location = new Point(55, 37),
            Size = new Size(150, 25),
            Font = new Font("Segoe UI", 9),
            Text = "TOPO-PUNTOS"
        };
        txtLayer.TextChanged += (s, e) => LayerName = txtLayer.Text;
        pnlBottom.Controls.Add(txtLayer);

        // Botones Aceptar/Cancelar
        var btnAccept = new Button
        {
            Text = "Importar",
            Location = new Point(240, 35),
            Size = new Size(90, 30),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(0, 150, 136),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9, FontStyle.Bold)
        };
        btnAccept.FlatAppearance.BorderSize = 0;
        btnAccept.Click += BtnAccept_Click;
        pnlBottom.Controls.Add(btnAccept);

        var btnCancel = new Button
        {
            Text = "Cancelar",
            Location = new Point(340, 35),
            Size = new Size(90, 30),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(180, 180, 180),
            ForeColor = Color.Black,
            Font = new Font("Segoe UI", 9)
        };
        btnCancel.FlatAppearance.BorderSize = 0;
        btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
        pnlBottom.Controls.Add(btnCancel);

        this.Controls.Add(pnlBottom);
    }

    private void BtnBrowse_Click(object? sender, EventArgs e)
    {
        using var dlg = new OpenFileDialog
        {
            Filter = "Archivos de coordenadas|*.csv;*.txt;*.dat|Todos los archivos|*.*",
            Title = "Seleccionar archivo de puntos"
        };

        if (dlg.ShowDialog() == DialogResult.OK)
        {
            SelectedFilePath = dlg.FileName;
            _txtFilePath.Text = dlg.FileName;
            LoadPreview();
        }
    }

    private void LoadPreview()
    {
        if (string.IsNullOrEmpty(SelectedFilePath)) return;

        try
        {
            var lines = File.ReadLines(SelectedFilePath).Take(10).ToArray();
            _lstPreview.Items.Clear();
            
            foreach (var line in lines)
            {
                _lstPreview.Items.Add(line);
            }

            _lblStatus.Text = $"Estado: {lines.Length} filas leídas correctamente.";
            _lblStatus.ForeColor = Color.Green;
        }
        catch (Exception ex)
        {
            _lblStatus.Text = $"Error: {ex.Message}";
            _lblStatus.ForeColor = Color.Red;
        }
    }

    private void BtnAccept_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(SelectedFilePath))
        {
            MessageBox.Show("Seleccione un archivo primero.", "Error", 
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        this.DialogResult = DialogResult.OK;
        this.Close();
    }
}
