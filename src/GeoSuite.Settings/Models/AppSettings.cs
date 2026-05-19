using System.Text.Json.Serialization;

namespace GeoSuite.Settings.Models;

/// <summary>
/// Configuración global del sistema GeoSuite.
/// Se persiste en geosuite_config.json y controla escalas, textos y comportamientos.
/// </summary>
public class AppSettings
{
    // --- GENERAL ---
    public string DefaultLayerPrefix { get; set; } = "GS";
    public bool CreateLayersAutomatically { get; set; } = true;

    // --- ESCALAS Y TEXTOS DINÁMICOS ---
    /// <summary>
    /// Escala actual del dibujo (ej: 1000 para 1:1000).
    /// Los tamaños de texto se calculan como: BaseSize * (CurrentScale / 1000)
    /// </summary>
    public double CurrentDrawingScale { get; set; } = 1000.0;
    
    /// <summary>
    /// Tamaño base de texto a escala 1:1000 (en unidades de dibujo).
    /// Si CurrentDrawingScale cambia, el tamaño real = TextBaseSize * (CurrentDrawingScale / 1000)
    /// </summary>
    public double TextBaseSize { get; set; } = 2.5;
    
    /// <summary>
    /// Factor de altura para etiquetas de vértices (relativo al base).
    /// </summary>
    public double VertexTextFactor { get; set; } = 0.8;
    
    /// <summary>
    /// Factor de altura para cotas y distancias.
    /// </summary>
    public double DimensionTextFactor { get; set; } = 0.9;

    // --- CATASTRO ---
    public CatastroSettings Catastro { get; set; } = new CatastroSettings();

    // --- TOPOGRAFÍA ---
    public TopographySettings Topography { get; set; } = new TopographySettings();
}

public class CatastroSettings
{
    /// <summary>
    /// Tipo de numeración para lotes/manzanas: "Numeric" (1, 2, 3) o "Alphabetic" (A, B, C).
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public NumberingType LotNumberingType { get; set; } = NumberingType.Numeric;
    
    /// <summary>
    /// Prefijo para lotes (ej: "L-1", "M-1").
    /// </summary>
    public string LotPrefix { get; set; } = "L";
    
    /// <summary>
    /// Prefijo para manzanas.
    /// </summary>
    public string BlockPrefix { get; set; } = "M";
    
    /// <summary>
    /// Incluir área en la etiqueta del lote.
    /// </summary>
    public bool ShowAreaInLabel { get; set; } = true;
    
    /// <summary>
    /// Incluir perímetro en la etiqueta.
    /// </summary>
    public bool ShowPerimeterInLabel { get; set; } = false;
    
    /// <summary>
    /// Etiquetar colindancias (distancias entre vértices).
    /// </summary>
    public bool LabelColindances { get; set; } = true;
    
    /// <summary>
    /// Decimales para áreas.
    /// </summary>
    public int AreaDecimals { get; set; } = 2;
    
    /// <summary>
    /// Decimales para distancias.
    /// </summary>
    public int DistanceDecimals { get; set; } = 2;
}

public class TopographySettings
{
    /// <summary>
    /// Equidistancia para curvas de nivel principales (ej: 5m).
    /// </summary>
    public double MajorContourInterval { get; set; } = 5.0;
    
    /// <summary>
    /// Equidistancia para curvas de nivel secundarias (ej: 1m).
    /// </summary>
    public double MinorContourInterval { get; set; } = 1.0;
    
    /// <summary>
    /// Tamaño de texto para etiquetas de curvas.
    /// </summary>
    public double ContourTextSize { get; set; } = 1.5;
    
    /// <summary>
    /// Capa por defecto para puntos topográficos.
    /// </summary>
    public string PointLayer { get; set; } = "TOPO-PUNTOS";
    
    /// <summary>
    /// Mostrar descripción del punto en etiqueta.
    /// </summary>
    public bool ShowPointDescription { get; set; } = true;
}

public enum NumberingType
{
    Numeric,      // 1, 2, 3, 4...
    Alphabetic    // A, B, C, D...
}
