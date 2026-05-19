using System;
using System.IO;
using System.Text.Json;
using GeoSuite.Settings.Models;

namespace GeoSuite.Settings.Services;

/// <summary>
/// Gestiona la carga y guardado de la configuración global.
/// Persiste en: %APPDATA%/GeoSuite/geosuite_config.json (Windows)
/// o ./geosuite_config.json (Linux/Mac)
/// </summary>
public static class SettingsManager
{
    private static readonly string ConfigFileName = "geosuite_config.json";
    private static AppSettings? _cachedSettings;
    private static string? _configPath;

    /// <summary>
    /// Obtiene la ruta del archivo de configuración según el SO.
    /// </summary>
    private static string GetConfigPath()
    {
        if (_configPath != null) return _configPath;

        try
        {
            // Intentar usar AppData en Windows
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var geoSuiteDir = Path.Combine(appData, "GeoSuite");
            
            if (!Directory.Exists(geoSuiteDir))
                Directory.CreateDirectory(geoSuiteDir);
            
            _configPath = Path.Combine(geoSuiteDir, ConfigFileName);
        }
        catch
        {
            // Fallback a directorio actual
            _configPath = Path.Combine(Directory.GetCurrentDirectory(), ConfigFileName);
        }

        return _configPath;
    }

    /// <summary>
    /// Carga la configuración desde disco o devuelve valores por defecto.
    /// Usa caché para evitar lecturas repetidas.
    /// </summary>
    public static AppSettings Load()
    {
        if (_cachedSettings != null)
            return _cachedSettings;

        var path = GetConfigPath();

        if (!File.Exists(path))
        {
            _cachedSettings = GetDefaultSettings();
            Save(_cachedSettings); // Crear archivo con defaults
            return _cachedSettings;
        }

        try
        {
            var json = File.ReadAllText(path);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            };
            
            _cachedSettings = JsonSerializer.Deserialize<AppSettings>(json, options) ?? GetDefaultSettings();
            return _cachedSettings;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GeoSuite] Error cargando configuración: {ex.Message}. Usando defaults.");
            _cachedSettings = GetDefaultSettings();
            return _cachedSettings;
        }
    }

    /// <summary>
    /// Guarda la configuración en disco y limpia el caché.
    /// </summary>
    public static void Save(AppSettings settings)
    {
        var path = GetConfigPath();
        
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            
            var json = JsonSerializer.Serialize(settings, options);
            File.WriteAllText(path, json);
            
            _cachedSettings = settings; // Actualizar caché
            Console.WriteLine($"[GeoSuite] Configuración guardada en: {path}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GeoSuite] Error guardando configuración: {ex.Message}");
        }
    }

    /// <summary>
    /// Fuerza recarga desde disco (útil si otro proceso modificó el archivo).
    /// </summary>
    public static void Reload()
    {
        _cachedSettings = null;
        _configPath = null;
        Load();
    }

    /// <summary>
    /// Calcula el tamaño de texto real basado en la escala actual.
    /// Fórmula: BaseSize * (CurrentScale / 1000)
    /// </summary>
    public static double GetScaledTextSize(double baseSizeFactor = 1.0)
    {
        var settings = Load();
        return settings.TextBaseSize * (settings.CurrentDrawingScale / 1000.0) * baseSizeFactor;
    }

    /// <summary>
    /// Genera el siguiente identificador de lote según el tipo de numeración configurado.
    /// </summary>
    public static string GenerateLotId(int index, CatastroSettings? settings = null)
    {
        settings ??= Load().Catastro;
        
        return settings.LotNumberingType switch
        {
            NumberingType.Numeric => $"{settings.LotPrefix}-{index}",
            NumberingType.Alphabetic => $"{settings.LotPrefix}-{IndexToLetters(index)}",
            _ => $"{settings.LotPrefix}-{index}"
        };
    }

    /// <summary>
    /// Convierte índice a letras (1=A, 2=B, 26=Z, 27=AA, etc.)
    /// </summary>
    private static string IndexToLetters(int index)
    {
        if (index <= 0) return "A";
        
        var letters = "";
        index--; // 0-based
        
        while (index >= 0)
        {
            letters = (char)('A' + (index % 26)) + letters;
            index = (index / 26) - 1;
        }
        
        return letters;
    }

    private static AppSettings GetDefaultSettings()
    {
        return new AppSettings
        {
            CurrentDrawingScale = 1000.0,
            TextBaseSize = 2.5,
            Catastro = new CatastroSettings
            {
                LotNumberingType = NumberingType.Numeric,
                LotPrefix = "L",
                BlockPrefix = "M",
                ShowAreaInLabel = true,
                LabelColindances = true
            },
            Topography = new TopographySettings
            {
                MajorContourInterval = 5.0,
                MinorContourInterval = 1.0,
                ContourTextSize = 1.5
            }
        };
    }
}
