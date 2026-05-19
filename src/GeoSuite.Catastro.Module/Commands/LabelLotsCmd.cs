using GeoSuite.Core.Models;
using GeoSuite.Platform;
using GeoSuite.Settings.Models;
using GeoSuite.Settings.Services;

namespace GeoSuite.Catastro.Module.Commands;

/// <summary>
/// Comando: GS-C-LBL
/// Etiqueta lotes catastrales con numeración automática según configuración.
/// Soporta numeración numérica (1, 2, 3) o alfabética (A, B, C).
/// Usa tamaños de texto dinámicos basados en la escala del dibujo.
/// </summary>
public class LabelLotsCmd
{
    private readonly ICadHost _cad;

    public LabelLotsCmd(ICadHost cad)
    {
        _cad = cad;
    }

    public void Execute(List<Polygon2D> polygons)
    {
        if (polygons.Count == 0)
        {
            _cad.ShowMessage("Error", "No se seleccionaron polígonos para etiquetar.");
            return;
        }

        // Cargar configuración global
        var settings = SettingsManager.Load();
        var catastroSettings = settings.Catastro;

        // Capas
        _cad.CreateLayerIfNotExists("CATASTRO-LOTES-TEXTO", "3"); // Verde
        _cad.CreateLayerIfNotExists("CATASTRO-COLINDANCIAS", "8"); // Gris

        int loteIndex = 1;

        foreach (var polygon in polygons)
        {
            var centroid = polygon.Centroid;
            double area = polygon.Area;
            double perimeter = polygon.Perimeter;

            // Generar ID de lote según tipo de numeración configurada
            string loteId = SettingsManager.GenerateLotId(loteIndex, catastroSettings);

            // Construir etiqueta principal
            var labelLines = new List<string>();
            labelLines.Add(loteId);

            if (catastroSettings.ShowAreaInLabel)
                labelLines.Add($"Área: {area:N{catastroSettings.AreaDecimals}} m²");

            if (catastroSettings.ShowPerimeterInLabel)
                labelLines.Add($"Perím: {perimeter:N{catastroSettings.DistanceDecimals}} m");

            string mainLabel = string.Join("\n", labelLines);

            // Calcular tamaño de texto dinámico
            double textSize = SettingsManager.GetScaledTextSize(1.0);
            double vertexTextSize = SettingsManager.GetScaledTextSize(catastroSettings.LabelColindances ? 0.7 : 0.0);

            // Insertar etiqueta principal en el centroide
            _cad.AddText(mainLabel, centroid, textSize * 1.2, "CATASTRO-LOTES-TEXTO");

            // Etiquetar colindancias (distancias entre vértices) si está configurado
            if (catastroSettings.LabelColindances && polygon.Vertices.Count >= 2)
            {
                for (int i = 0; i < polygon.Vertices.Count; i++)
                {
                    var p1 = polygon.Vertices[i];
                    var p2 = polygon.Vertices[(i + 1) % polygon.Vertices.Count];

                    double distance = p1.DistanceTo(p2);
                    var midPoint = new Coordinate3(
                        (p1.X + p2.X) / 2,
                        (p1.Y + p2.Y) / 2,
                        (p1.Z + p2.Z) / 2
                    );

                    // Offset pequeño hacia el interior del polígono
                    var offset = GetOffsetTowardsCentroid(midPoint, centroid, 1.0);
                    var labelPos = new Coordinate3(offset.X, offset.Y, midPoint.Z);

                    string distLabel = $"{distance:N{catastroSettings.DistanceDecimals}}m";
                    _cad.AddText(distLabel, labelPos, vertexTextSize, "CATASTRO-COLINDANCIAS");
                }
            }

            loteIndex++;
        }

        _cad.ShowMessage("Éxito", $"{polygons.Count} lotes etiquetados correctamente.");
    }

    /// <summary>
    /// Calcula un punto desplazado desde midPoint hacia centroid por una distancia dada.
    /// </summary>
    private Coordinate3 GetOffsetTowardsCentroid(Coordinate3 from, Coordinate3 to, double offsetDistance)
    {
        double dx = to.X - from.X;
        double dy = to.Y - from.Y;
        double distance = Math.Sqrt(dx * dx + dy * dy);

        if (distance == 0) return from;

        double ratio = offsetDistance / distance;
        return new Coordinate3(
            from.X + dx * ratio,
            from.Y + dy * ratio,
            from.Z
        );
    }
}
