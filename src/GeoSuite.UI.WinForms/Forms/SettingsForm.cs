using System;
using System.Drawing;
using System.Windows.Forms;
using GeoSuite.Settings.Models;
using GeoSuite.Settings.Services;

namespace GeoSuite.UI.WinForms.Forms;

/// <summary>
/// Formulario de configuración global de GeoSuite.
/// Permite ajustar escalas, tamaños de texto dinámicos y comportamientos por módulo.
/// </summary>
public class SettingsForm : Form
{
    // Controles principales
    private TabControl tabControl;
    private TabPage tabGeneral, tabCatastro, tabTopo;
    
    // General
    private NumericUpDown nudScale, nudTextBaseSize;
    private TextBox txtLayerPrefix;
    private CheckBox chkAutoLayers;
    
    // Catastro
    private ComboBox cboNumberingType;
    private TextBox txtLotPrefix, txtBlockPrefix;
    private CheckBox chkShowArea, chkShowPerimeter, chkLabelColindances;
    private NumericUpDown nudAreaDecimals, nudDistDecimals;
    
    // Topografía
    private NumericUpDown nudMajorInterval, nudMinorInterval, nudContourText;
    private CheckBox chkShowPointDesc;
    private TextBox txtPointLayer;
    
    private Button btnSave, btnLoadDefaults, btnCancel;

    public SettingsForm()
    {
        InitializeComponents();
        LoadSettings();
    }

    private void InitializeComponents()
    {
        this.Text = "GeoSuite - Configuración";
        this.Size = new Size(650, 550);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Font = new Font("Segoe UI", 9F);

        // TabControl
        tabControl = new TabControl
        {
            Dock = DockStyle.Fill,
            Location = new Point(10, 10),
            Size = new Size(610, 450)
        };

        // Pestañas
        tabGeneral = new TabPage("General y Escalas");
        tabCatastro = new TabPage("Catastro");
        tabTopo = new TabPage("Topografía");

        tabControl.TabPages.AddRange(new[] { tabGeneral, tabCatastro, tabTopo });

        // --- GENERAL ---
        SetupGeneralTab();

        // --- CATASTRO ---
        SetupCatastroTab();

        // --- TOPOGRAFÍA ---
        SetupTopoTab();

        // Botones inferiores
        var panelButtons = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 50,
            Padding = new Padding(10)
        };

        btnSave = new Button
        {
            Text = "Guardar Configuración",
            Size = new Size(140, 30),
            Location = new Point(10, 10),
            BackColor = Color.FromArgb(0, 120, 215),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnSave.Click += BtnSave_Click;

        btnLoadDefaults = new Button
        {
            Text = "Restaurar Defaults",
            Size = new Size(130, 30),
            Location = new Point(160, 10),
            FlatStyle = FlatStyle.Flat
        };
        btnLoadDefaults.Click += (s, e) => LoadDefaults();

        btnCancel = new Button
        {
            Text = "Cancelar",
            Size = new Size(100, 30),
            Location = new Point(500, 10),
            DialogResult = DialogResult.Cancel
        };

        panelButtons.Controls.AddRange(new Control[] { btnSave, btnLoadDefaults, btnCancel });

        this.Controls.Add(tabControl);
        this.Controls.Add(panelButtons);
        this.AcceptButton = btnSave;
        this.CancelButton = btnCancel;
    }

    private void SetupGeneralTab()
    {
        var y = 20;
        var labelWidth = 220;
        var controlX = 230;

        // Escala
        var lblScale = new Label { Text = "Escala del Dibujo (1:X):", Location = new Point(20, y), Size = new Size(labelWidth, 25) };
        nudScale = new NumericUpDown { Location = new Point(controlX, y), Size = new Size(120, 25), Minimum = 100, Maximum = 50000, Increment = 100, Value = 1000 };
        var lblScaleNote = new Label { Text = "Los textos se ajustan automáticamente", Location = new Point(controlX + 130, y + 5), Size = new Size(200, 20), ForeColor = Color.Gray };
        y += 35;

        // Tamaño base texto
        var lblTextBase = new Label { Text = "Tamaño Base de Texto (unidades):", Location = new Point(20, y), Size = new Size(labelWidth, 25) };
        nudTextBaseSize = new NumericUpDown { Location = new Point(controlX, y), Size = new Size(120, 25), Minimum = 0.5, Maximum = 20, Increment = 0.5, Value = 2.5, DecimalPlaces = 1 };
        y += 35;

        // Prefijo capas
        var lblPrefix = new Label { Text = "Prefijo para Capas:", Location = new Point(20, y), Size = new Size(labelWidth, 25) };
        txtLayerPrefix = new TextBox { Location = new Point(controlX, y), Size = new Size(120, 25), Text = "GS" };
        y += 35;

        // Auto capas
        chkAutoLayers = new CheckBox { Text = "Crear capas automáticamente", Location = new Point(controlX, y), Size = new Size(200, 25), Checked = true };
        y += 40;

        // Info box
        var grpInfo = new GroupBox { Text = "Cálculo de Textos Dinámicos", Location = new Point(20, y), Size = new Size(550, 100) };
        var lblFormula = new Label 
        { 
            Text = "Tamaño Real = Base × (Escala / 1000)\n\nEjemplo: Base=2.5, Escala=1:2000 → Texto=5.0 unidades", 
            Location = new Point(15, 20), 
            Size = new Size(520, 60),
            AutoSize = false
        };
        grpInfo.Controls.Add(lblFormula);

        tabGeneral.Controls.Add(lblScale);
        tabGeneral.Controls.Add(nudScale);
        tabGeneral.Controls.Add(lblScaleNote);
        tabGeneral.Controls.Add(lblTextBase);
        tabGeneral.Controls.Add(nudTextBaseSize);
        tabGeneral.Controls.Add(lblPrefix);
        tabGeneral.Controls.Add(txtLayerPrefix);
        tabGeneral.Controls.Add(chkAutoLayers);
        tabGeneral.Controls.Add(grpInfo);
    }

    private void SetupCatastroTab()
    {
        var y = 20;
        var labelWidth = 200;
        var controlX = 210;

        // Tipo numeración
        var lblNumType = new Label { Text = "Tipo de Numeración:", Location = new Point(20, y), Size = new Size(labelWidth, 25) };
        cboNumberingType = new ComboBox { Location = new Point(controlX, y), Size = new Size(200, 25), DropDownStyle = ComboBoxStyle.DropDownList };
        cboNumberingType.Items.AddRange(new object[] { "Numérico (1, 2, 3...)", "Alfabético (A, B, C...)" });
        cboNumberingType.SelectedIndex = 0;
        y += 35;

        // Prefijos
        var lblLotPrefix = new Label { Text = "Prefijo Lotes:", Location = new Point(20, y), Size = new Size(labelWidth, 25) };
        txtLotPrefix = new TextBox { Location = new Point(controlX, y), Size = new Size(60, 25), Text = "L" };
        y += 35;

        var lblBlockPrefix = new Label { Text = "Prefijo Manzanas:", Location = new Point(20, y), Size = new Size(labelWidth, 25) };
        txtBlockPrefix = new TextBox { Location = new Point(controlX, y), Size = new Size(60, 25), Text = "M" };
        y += 45;

        // Opciones etiqueta
        var grpLabels = new GroupBox { Text = "Etiquetado de Lotes", Location = new Point(20, y), Size = new Size(550, 120) };
        
        chkShowArea = new CheckBox { Text = "Mostrar Área en etiqueta", Location = new Point(15, 25), Size = new Size(250, 25), Checked = true };
        chkShowPerimeter = new CheckBox { Text = "Mostrar Perímetro", Location = new Point(15, 50), Size = new Size(250, 25) };
        chkLabelColindances = new CheckBox { Text = "Etiquetar Colindancias (distancias)", Location = new Point(15, 75), Size = new Size(250, 25), Checked = true };
        
        grpLabels.Controls.AddRange(new Control[] { chkShowArea, chkShowPerimeter, chkLabelColindances });
        y += 135;

        // Decimales
        var lblAreaDec = new Label { Text = "Decimales para Áreas:", Location = new Point(20, y), Size = new Size(labelWidth, 25) };
        nudAreaDecimals = new NumericUpDown { Location = new Point(controlX, y), Size = new Size(60, 25), Minimum = 0, Maximum = 6, Value = 2 };
        y += 35;

        var lblDistDec = new Label { Text = "Decimales para Distancias:", Location = new Point(20, y), Size = new Size(labelWidth, 25) };
        nudDistDecimals = new NumericUpDown { Location = new Point(controlX, y), Size = new Size(60, 25), Minimum = 0, Maximum = 6, Value = 2 };

        tabCatastro.Controls.Add(lblNumType);
        tabCatastro.Controls.Add(cboNumberingType);
        tabCatastro.Controls.Add(lblLotPrefix);
        tabCatastro.Controls.Add(txtLotPrefix);
        tabCatastro.Controls.Add(lblBlockPrefix);
        tabCatastro.Controls.Add(txtBlockPrefix);
        tabCatastro.Controls.Add(grpLabels);
        tabCatastro.Controls.Add(lblAreaDec);
        tabCatastro.Controls.Add(nudAreaDecimals);
        tabCatastro.Controls.Add(lblDistDec);
        tabCatastro.Controls.Add(nudDistDecimals);
    }

    private void SetupTopoTab()
    {
        var y = 20;
        var labelWidth = 220;
        var controlX = 230;

        // Equidistancias
        var lblMajor = new Label { Text = "Curvas Maestras (m):", Location = new Point(20, y), Size = new Size(labelWidth, 25) };
        nudMajorInterval = new NumericUpDown { Location = new Point(controlX, y), Size = new Size(80, 25), Minimum = 1, Maximum = 100, Value = 5 };
        y += 35;

        var lblMinor = new Label { Text = "Curvas Secundarias (m):", Location = new Point(20, y), Size = new Size(labelWidth, 25) };
        nudMinorInterval = new NumericUpDown { Location = new Point(controlX, y), Size = new Size(80, 25), Minimum = 0.5, Maximum = 10, Increment = 0.5, Value = 1 };
        y += 35;

        // Texto curvas
        var lblContourText = new Label { Text = "Tamaño Texto Curvas:", Location = new Point(20, y), Size = new Size(labelWidth, 25) };
        nudContourText = new NumericUpDown { Location = new Point(controlX, y), Size = new Size(80, 25), Minimum = 0.5, Maximum = 10, Increment = 0.5, Value = 1.5, DecimalPlaces = 1 };
        y += 45;

        // Capa puntos
        var lblPointLayer = new Label { Text = "Capa para Puntos:", Location = new Point(20, y), Size = new Size(labelWidth, 25) };
        txtPointLayer = new TextBox { Location = new Point(controlX, y), Size = new Size(150, 25), Text = "TOPO-PUNTOS" };
        y += 35;

        // Descripción
        chkShowPointDesc = new CheckBox { Text = "Mostrar descripción en etiqueta de punto", Location = new Point(controlX, y), Size = new Size(250, 25), Checked = true };

        tabTopo.Controls.Add(lblMajor);
        tabTopo.Controls.Add(nudMajorInterval);
        tabTopo.Controls.Add(lblMinor);
        tabTopo.Controls.Add(nudMinorInterval);
        tabTopo.Controls.Add(lblContourText);
        tabTopo.Controls.Add(nudContourText);
        tabTopo.Controls.Add(lblPointLayer);
        tabTopo.Controls.Add(txtPointLayer);
        tabTopo.Controls.Add(chkShowPointDesc);
    }

    private void LoadSettings()
    {
        var settings = SettingsManager.Load();

        // General
        nudScale.Value = (decimal)settings.CurrentDrawingScale;
        nudTextBaseSize.Value = (decimal)settings.TextBaseSize;
        txtLayerPrefix.Text = settings.DefaultLayerPrefix;
        chkAutoLayers.Checked = settings.CreateLayersAutomatically;

        // Catastro
        cboNumberingType.SelectedIndex = settings.Catastro.LotNumberingType == NumberingType.Numeric ? 0 : 1;
        txtLotPrefix.Text = settings.Catastro.LotPrefix;
        txtBlockPrefix.Text = settings.Catastro.BlockPrefix;
        chkShowArea.Checked = settings.Catastro.ShowAreaInLabel;
        chkShowPerimeter.Checked = settings.Catastro.ShowPerimeterInLabel;
        chkLabelColindances.Checked = settings.Catastro.LabelColindances;
        nudAreaDecimals.Value = settings.Catastro.AreaDecimals;
        nudDistDecimals.Value = settings.Catastro.DistanceDecimals;

        // Topo
        nudMajorInterval.Value = (decimal)settings.Topography.MajorContourInterval;
        nudMinorInterval.Value = (decimal)settings.Topography.MinorContourInterval;
        nudContourText.Value = (decimal)settings.Topography.ContourTextSize;
        txtPointLayer.Text = settings.Topography.PointLayer;
        chkShowPointDesc.Checked = settings.Topography.ShowPointDescription;
    }

    private void LoadDefaults()
    {
        if (MessageBox.Show("¿Restaurar configuración a valores por defecto?", "Confirmar", 
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            var defaults = new AppSettings();
            SettingsManager.Save(defaults);
            LoadSettings();
            MessageBox.Show("Configuración restaurada.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        var settings = new AppSettings
        {
            CurrentDrawingScale = (double)nudScale.Value,
            TextBaseSize = (double)nudTextBaseSize.Value,
            DefaultLayerPrefix = txtLayerPrefix.Text,
            CreateLayersAutomatically = chkAutoLayers.Checked,
            
            Catastro = new CatastroSettings
            {
                LotNumberingType = cboNumberingType.SelectedIndex == 0 ? NumberingType.Numeric : NumberingType.Alphabetic,
                LotPrefix = txtLotPrefix.Text,
                BlockPrefix = txtBlockPrefix.Text,
                ShowAreaInLabel = chkShowArea.Checked,
                ShowPerimeterInLabel = chkShowPerimeter.Checked,
                LabelColindances = chkLabelColindances.Checked,
                AreaDecimals = (int)nudAreaDecimals.Value,
                DistanceDecimals = (int)nudDistDecimals.Value
            },
            
            Topography = new TopographySettings
            {
                MajorContourInterval = (double)nudMajorInterval.Value,
                MinorContourInterval = (double)nudMinorInterval.Value,
                ContourTextSize = (double)nudContourText.Value,
                PointLayer = txtPointLayer.Text,
                ShowPointDescription = chkShowPointDesc.Checked
            }
        };

        SettingsManager.Save(settings);
        MessageBox.Show("Configuración guardada correctamente.\nLos cambios se aplicarán a nuevos elementos.", 
            "GeoSuite", MessageBoxButtons.OK, MessageBoxIcon.Information);
        
        this.DialogResult = DialogResult.OK;
        this.Close();
    }
}
