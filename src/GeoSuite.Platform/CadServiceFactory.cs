using System;
using GeoSuite.Platform.AcadImpl;
using GeoSuite.Platform.ZwCadImpl;

namespace GeoSuite.Platform;

/// <summary>
/// Factory para detectar la plataforma CAD y crear la instancia apropiada de ICadHost.
/// Detecta automáticamente si se ejecuta en AutoCAD o ZWCAD.
/// </summary>
public static class CadServiceFactory
{
    private static ICadHost? _cachedHost;

    /// <summary>
    /// Crea una instancia de ICadHost según la plataforma detectada.
    /// Usa caché para evitar recrear instancias.
    /// </summary>
    public static ICadHost Create()
    {
        if (_cachedHost != null)
            return _cachedHost;

        try
        {
            // Intentar detectar AutoCAD primero
            // Si estamos en AutoCAD, la clase Application existirá en Autodesk.AutoCAD.ApplicationServices
            var acadAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name?.StartsWith("AcMgd") == true || 
                                     a.GetName().Name?.StartsWith("AcDbMgd") == true);

            if (acadAssembly != null)
            {
                _cachedHost = new AcadHost();
                System.Diagnostics.Debug.WriteLine("[GeoSuite] Plataforma detectada: AutoCAD");
                return _cachedHost;
            }

            // Intentar detectar ZWCAD
            // Si estamos en ZWCAD, la clase Application existirá en ZwSoft.ZwCAD.ApplicationServices
            var zwAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name?.StartsWith("ZwTxMgd") == true || 
                                     a.GetName().Name?.StartsWith("ZwDbMgd") == true);

            if (zwAssembly != null)
            {
                _cachedHost = new ZwCadHost();
                System.Diagnostics.Debug.WriteLine("[GeoSuite] Plataforma detectada: ZWCAD");
                return _cachedHost;
            }

            // Si no hay ningún CAD detectado (ejecución fuera de CAD), usar Mock
            System.Diagnostics.Debug.WriteLine("[GeoSuite] Ningún CAD detectado, usando MockCadHost");
            _cachedHost = new MockCadHost();
            return _cachedHost;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[GeoSuite] Error detectando plataforma: {ex.Message}");
            _cachedHost = new MockCadHost();
            return _cachedHost;
        }
    }

    /// <summary>
    /// Limpia el caché para forzar una nueva detección.
    /// Útil cuando se cambia de documento o sesión.
    /// </summary>
    public static void Reset()
    {
        _cachedHost = null;
    }
}
