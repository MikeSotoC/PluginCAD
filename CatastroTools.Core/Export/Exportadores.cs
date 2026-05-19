using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CatastroTools.Core.Models;

namespace CatastroTools.Core.Export
{
    public static class ExportadorHTML
    {
        public static void GenerarReporte(Lote lote, string rutaSalida)
        {
            var sb = new StringBuilder();
            var zona = CoordUtils.NombreZona(
                CoordUtils.DetectarZona(lote.Poligono.Vertices));

            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang='es'><head>");
            sb.AppendLine("<meta charset='UTF-8'>");
            sb.AppendLine($"<title>Ficha Catastral — {lote.NombreManzana} Lote {lote.Numero}</title>");
            sb.AppendLine("<style>");
            sb.AppendLine("body{font-family:Arial,sans-serif;margin:30px;font-size:13px;color:#222}");
            sb.AppendLine("h1{color:#1a3a6b;border-bottom:3px solid #1a3a6b;padding-bottom:8px}");
            sb.AppendLine("h2{color:#2c5f9e;font-size:14px;margin-top:22px;border-left:4px solid #2c5f9e;padding-left:8px}");
            sb.AppendLine("table{border-collapse:collapse;width:100%;margin-bottom:18px}");
            sb.AppendLine("th{background:#1a3a6b;color:#fff;padding:8px 10px;text-align:left;font-size:12px}");
            sb.AppendLine("td{padding:7px 10px;border:1px solid #ccc;font-size:12px}");
            sb.AppendLine("tr:nth-child(even)td{background:#f4f7ff}");
            sb.AppendLine(".resumen{background:#eaf0fb;padding:14px;border-left:4px solid #1a3a6b;margin:16px 0;border-radius:4px}");
            sb.AppendLine(".resumen p{margin:4px 0;font-size:13px}");
            sb.AppendLine(".badge{display:inline-block;background:#1a3a6b;color:#fff;padding:2px 8px;border-radius:3px;font-size:11px}");
            sb.AppendLine(".footer{margin-top:30px;font-size:11px;color:#999;border-top:1px solid #ddd;padding-top:10px}");
            sb.AppendLine("</style></head><body>");

            // Encabezado
            sb.AppendLine("<h1>&#128205; Ficha Técnica Catastral</h1>");
            sb.AppendLine($"<p><span class='badge'>SUNARP</span>&nbsp;<span class='badge'>RNE GH.020</span>&nbsp;");
            sb.AppendLine($"<span class='badge'>UTM WGS84 Z{zona}</span></p>");

            // Datos del titular
            sb.AppendLine("<h2>Datos del Titular</h2><table>");
            sb.AppendLine("<tr><th width='35%'>Campo</th><th>Valor</th></tr>");
            AgregarFila(sb, "Propietario / Titular", lote.Propietario);
            AgregarFila(sb, "DNI / RUC", lote.Dni);
            AgregarFila(sb, "Dirección del predio", lote.Direccion);
            AgregarFila(sb, "Distrito", lote.Distrito);
            AgregarFila(sb, "Provincia", lote.Provincia);
            AgregarFila(sb, "Departamento", lote.Departamento);
            sb.AppendLine("</table>");

            // Identificación registral
            sb.AppendLine("<h2>Identificación Registral</h2><table>");
            sb.AppendLine("<tr><th width='35%'>Campo</th><th>Valor</th></tr>");
            AgregarFila(sb, "Manzana", lote.NombreManzana);
            AgregarFila(sb, "N° Lote", lote.Numero);
            AgregarFila(sb, "Habilitación Urbana", lote.HabilitacionUrbana);
            AgregarFila(sb, "Partida Registral", lote.PartidaRegistral);
            AgregarFila(sb, "Zonificación", lote.Zonificacion);
            AgregarFila(sb, "Uso", lote.Uso);
            sb.AppendLine("</table>");

            // Medidas
            sb.AppendLine("<div class='resumen'>");
            sb.AppendLine($"<p><strong>Área total:</strong> {lote.Area:F2} m²</p>");
            sb.AppendLine($"<p><strong>Perímetro:</strong> {lote.Perimetro:F3} m</p>");
            sb.AppendLine($"<p><strong>N° vértices:</strong> {lote.Poligono.NumVertices}</p>");
            sb.AppendLine($"<p><strong>Sistema:</strong> UTM WGS84 Zona {zona}</p>");
            sb.AppendLine("</div>");

            // Cuadro de vértices
            sb.AppendLine("<h2>Cuadro de Vértices UTM</h2><table>");
            sb.AppendLine("<tr><th>Vértice</th><th>Este E (m)</th><th>Norte N (m)</th></tr>");
            var verts = lote.Poligono.Vertices;
            for (int i = 0; i < verts.Count; i++)
                sb.AppendLine($"<tr><td>V-{i + 1}</td><td>{verts[i].X:F4}</td><td>{verts[i].Y:F4}</td></tr>");
            sb.AppendLine("</table>");

            // Cuadro de linderos
            sb.AppendLine("<h2>Cuadro de Linderos</h2><table>");
            sb.AppendLine("<tr><th>Lado</th><th>De</th><th>A</th><th>Distancia</th><th>Rumbo</th></tr>");
            var segs = lote.Poligono.Segmentos.ToList();
            for (int i = 0; i < segs.Count; i++)
                sb.AppendLine($"<tr><td>L-{i + 1}</td><td>V-{i + 1}</td>" +
                    $"<td>V-{(i + 1) % segs.Count + 1}</td>" +
                    $"<td>{segs[i].Longitud:F3} m</td>" +
                    $"<td>{segs[i].Rumbo}</td></tr>");
            sb.AppendLine("</table>");

            // Colindancias
            if (lote.Colindancias.Any())
            {
                sb.AppendLine("<h2>Colindancias</h2><table>");
                sb.AppendLine("<tr><th>Lado</th><th>Colinda con</th><th>Distancia</th></tr>");
                foreach (var c in lote.Colindancias)
                    sb.AppendLine($"<tr><td>{c.Segmento.Rumbo}</td>" +
                        $"<td>{c.Descripcion}</td>" +
                        $"<td>{c.Segmento.Longitud:F3} m</td></tr>");
                sb.AppendLine("</table>");
            }

            // Footer
            sb.AppendLine("<div class='footer'>");
            sb.AppendLine($"<p>Generado por <strong>CatastroTools v2.0</strong> — {DateTime.Now:dd/MM/yyyy HH:mm}</p>");
            sb.AppendLine("<p>Documento de uso técnico referencial. Validar ante SUNARP.</p>");
            sb.AppendLine("</div></body></html>");

            File.WriteAllText(rutaSalida, sb.ToString(), Encoding.UTF8);
        }

        private static void AgregarFila(StringBuilder sb, string campo, string valor) =>
            sb.AppendLine($"<tr><td><strong>{campo}</strong></td><td>{valor ?? "—"}</td></tr>");
    }

    public static class ExportadorCSV
    {
        public static void ExportarVertices(Lote lote, string rutaSalida, string zona)
        {
            var sb = new StringBuilder();
            sb.AppendLine("VERTICE,ESTE_E,NORTE_N,ZONA");
            var verts = lote.Poligono.Vertices;
            for (int i = 0; i < verts.Count; i++)
                sb.AppendLine($"V-{i + 1},{verts[i].X:F4},{verts[i].Y:F4},{zona}");
            File.WriteAllText(rutaSalida, sb.ToString(), Encoding.UTF8);
        }

        public static void ExportarLotes(IEnumerable<Lote> lotes, string rutaSalida)
        {
            var sb = new StringBuilder();
            sb.AppendLine("MANZANA,LOTE,AREA_M2,PERIMETRO_M,CENTROIDE_E,CENTROIDE_N,PROPIETARIO,PARTIDA");
            foreach (var l in lotes)
            {
                var c = l.Centroide;
                sb.AppendLine($"{l.NombreManzana},{l.Numero},{l.Area:F2},{l.Perimetro:F3}," +
                    $"{c.X:F4},{c.Y:F4},{l.Propietario ?? ""},{l.PartidaRegistral ?? ""}");
            }
            File.WriteAllText(rutaSalida, sb.ToString(), Encoding.UTF8);
        }

        public static void ExportarManzanas(IEnumerable<Manzana> manzanas, string rutaSalida)
        {
            var sb = new StringBuilder();
            sb.AppendLine("MANZANA,AREA_M2,PERIMETRO_M,NUM_LOTES,CENTROIDE_E,CENTROIDE_N");
            foreach (var m in manzanas)
            {
                var c = m.Centroide;
                sb.AppendLine($"{m.Nombre},{m.Area:F2},{m.Poligono.Perimetro:F3}," +
                    $"{m.Lotes.Count},{c.X:F4},{c.Y:F4}");
            }
            File.WriteAllText(rutaSalida, sb.ToString(), Encoding.UTF8);
        }
    }
}
