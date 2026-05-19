using GeoSuite.Core.Models;
using GeoSuite.Platform;

namespace GeoSuite.Survey.Module.Commands;

/// <summary>
/// Comando: GS-T-CN
/// Genera curvas de nivel desde una superficie TIN.
/// </summary>
public class ContoursCmd
{
    private readonly ICadHost _cad;

    public ContoursCmd(ICadHost cad)
    {
        _cad = cad;
    }

    public void Execute()
    {
        _cad.ShowMessage("Curvas de Nivel", "Generando curvas de nivel...");

        // TODO: Implementar generación real de curvas de nivel
        // Por ahora dibuja líneas horizontales de ejemplo
        
        try
        {
            _cad.CreateLayerIfNotExists("TOPO-CURVAS-1M", "2"); // Amarillo
            _cad.CreateLayerIfNotExists("TOPO-CURVAS-5M", "6"); // Magenta

            // Dibujar curvas de ejemplo (líneas horizontales)
            for (double z = 100; z <= 110; z += 1.0)
            {
                string layer = (z % 5 == 0) ? "TOPO-CURVAS-5M" : "TOPO-CURVAS-1M";
                
                // Línea horizontal simulada
                var start = new Coordinate3(0, 0, z);
                var end = new Coordinate3(50, 0, z);
                _cad.DrawLine(start, end, layer);
                
                // Etiquetar curva maestra
                if (z % 5 == 0)
                {
                    _cad.AddText(z.ToString("F1"), new Coordinate3(25, 2, z), 2.0, layer);
                }
            }

            _cad.ShowMessage("Curvas Generadas", "Se crearon curvas de nivel cada 1m (maestras cada 5m).");
        }
        catch (System.Exception ex)
        {
            _cad.ShowMessage("Error Curvas", ex.Message);
        }
    }
}
