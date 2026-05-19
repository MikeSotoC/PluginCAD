using GeoSuite.Core.Models;
using GeoSuite.Core.Algorithms;
using GeoSuite.Platform;

namespace GeoSuite.Survey.Module.Commands;

public class VolumeCmd
{
    public void Execute()
    {
        var cad = CadServiceFactory.Create();
        
        cad.ShowMessage("Cálculo de Volúmenes", "Método: Promedio de Áreas");
        
        // Simulación: Dos superficies (Terreno Natural y Subrasante)
        // En producción, esto viene de dos TINs diferentes
        var surface1 = new List<(double Station, double Area)> 
        { 
            (0, 0), (10, 25), (20, 30), (30, 28), (40, 20), (50, 0) 
        };
        
        var surface2 = new List<(double Station, double Area)> 
        { 
            (0, 0), (10, 20), (20, 25), (30, 22), (40, 15), (50, 0) 
        };

        double totalCut = 0;
        double totalFill = 0;
        var reportLines = new List<string>();
        
        reportLines.Add("ESTACIÓN     CORTE(m³)     RELLENO(m³)");
        reportLines.Add("----------   -----------   -----------");

        for (int i = 0; i < surface1.Count - 1; i++)
        {
            double dist = surface1[i + 1].Station - surface1[i].Station;
            
            // Área promedio entre dos secciones
            double areaCut1 = Math.Max(0, surface1[i].Area - surface2[i].Area);
            double areaCut2 = Math.Max(0, surface1[i + 1].Area - surface2[i + 1].Area);
            
            double areaFill1 = Math.Max(0, surface2[i].Area - surface1[i].Area);
            double areaFill2 = Math.Max(0, surface2[i + 1].Area - surface1[i + 1].Area);
            
            // Volumen por promedio de áreas
            double volCut = (areaCut1 + areaCut2) / 2.0 * dist;
            double volFill = (areaFill1 + areaFill2) / 2.0 * dist;
            
            totalCut += volCut;
            totalFill += volFill;
            
            reportLines.Add($"{surface1[i].Station,8:F1}     {volCut,10:F2}     {volFill,10:F2}");
        }

        reportLines.Add("----------   -----------   -----------");
        reportLines.Add($"TOTALES:     {totalCut,10:F2}     {totalFill,10:F2}");
        
        // Mostrar reporte en línea de comandos
        foreach (var line in reportLines)
        {
            System.Diagnostics.Debug.WriteLine(line);
        }
        
        // Agregar texto resumen en el dibujo
        cad.AddText(
            $"CORTE: {totalCut:F2} m³\nRELLENO: {totalFill:F2} m³",
            new Coordinate3(60, 50, 0),
            3.0,
            "TOPO-VOLUMENES"
        );

        cad.ShowMessage("Volúmenes Calculados", 
            $"Corte: {totalCut:F2} m³\nRelleno: {totalFill:F2} m³\nBalance: {(totalCut - totalFill):F2} m³");
    }
}
