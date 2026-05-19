using System;
using System.Collections.Generic;
using System.Linq;
using CatastroTools.Core.Models;

namespace CatastroTools.Core.Geometry
{
    // ─── SUBDIVISIÓN DE POLÍGONOS ────────────────────────────────
    public static class Subdivision
    {
        /// Subdivide un polígono con una línea de corte p1→p2
        /// Devuelve los dos subpolígonos resultantes o null si no hay corte válido
        public static (Poligono A, Poligono B)? Cortar(Poligono poly, Punto2D p1, Punto2D p2)
        {
            var corte = new Segmento(p1, p2);
            var intersecciones = new List<(Punto2D Pt, int Idx)>();

            var verts = poly.Vertices;
            int n = verts.Count;

            for (int i = 0; i < n; i++)
            {
                var lado = new Segmento(verts[i], verts[(i + 1) % n]);
                var pt = corte.Interseccion(lado);
                if (pt.HasValue)
                    intersecciones.Add((pt.Value, i));
            }

            if (intersecciones.Count < 2) return null;

            // Tomar las dos primeras intersecciones ordenadas por índice
            intersecciones.Sort((a, b) => a.Idx.CompareTo(b.Idx));
            var (ptA, idxA) = intersecciones[0];
            var (ptB, idxB) = intersecciones[1];

            // Polígono A: ptA → verts[idxA+1..idxB] → ptB
            var polyA = new List<Punto2D> { ptA };
            for (int i = idxA + 1; i <= idxB; i++)
                polyA.Add(verts[i]);
            polyA.Add(ptB);

            // Polígono B: ptB → verts[idxB+1..n-1,0..idxA] → ptA
            var polyB = new List<Punto2D> { ptB };
            for (int i = idxB + 1; i < n; i++)
                polyB.Add(verts[i]);
            for (int i = 0; i <= idxA; i++)
                polyB.Add(verts[i]);
            polyB.Add(ptA);

            return (new Poligono(polyA), new Poligono(polyB));
        }

        /// Genera una grilla de lotes dentro de un bounding box
        public static List<Poligono> GenerarGrilla(
            Punto2D origen, double anchoLote, double profLote,
            int nCols, int nFilas)
        {
            var lotes = new List<Poligono>();
            for (int row = 0; row < nFilas; row++)
            for (int col = 0; col < nCols; col++)
            {
                double x0 = origen.X + col * anchoLote;
                double y0 = origen.Y + row * profLote;
                lotes.Add(new Poligono(new[]
                {
                    new Punto2D(x0,               y0),
                    new Punto2D(x0 + anchoLote,   y0),
                    new Punto2D(x0 + anchoLote,   y0 + profLote),
                    new Punto2D(x0,               y0 + profLote)
                }));
            }
            return lotes;
        }

        /// Calcula cuántos lotes caben en un bounding box
        public static (int Cols, int Filas) LotesPosibles(
            Punto2D min, Punto2D max, double anchoLote, double profLote)
        {
            double W = max.X - min.X;
            double H = max.Y - min.Y;
            return (Math.Max(1, (int)(W / anchoLote)),
                    Math.Max(1, (int)(H / profLote)));
        }
    }

    // ─── NUMERACIÓN POR RECORRIDO ─────────────────────────────────
    public static class Recorrido
    {
        /// Ordena una lista de polígonos según el orden en que
        /// los cruza una polilínea de recorrido
        public static List<int> OrdenarPorRecorrido(
            List<Poligono> poligonos,
            List<Punto2D> recorrido,
            double pasoMuestreo = 1.0)
        {
            var muestras = MuestrearPolilinea(recorrido, pasoMuestreo);
            var visitados = new List<int>();
            var orden = new List<int>();

            foreach (var muestra in muestras)
            {
                // Buscar el polígono más cercano al punto de muestreo
                int nearest = -1;
                double minDist = double.MaxValue;

                for (int i = 0; i < poligonos.Count; i++)
                {
                    if (visitados.Contains(i)) continue;
                    double d = muestra.DistanciaA(poligonos[i].Centroide);
                    if (d < minDist)
                    {
                        minDist = d;
                        nearest = i;
                    }
                }

                if (nearest >= 0 && minDist < 100.0 && !visitados.Contains(nearest))
                {
                    visitados.Add(nearest);
                    orden.Add(nearest);
                }
            }

            // Agregar los no visitados al final
            for (int i = 0; i < poligonos.Count; i++)
                if (!orden.Contains(i)) orden.Add(i);

            return orden;
        }

        /// Muestrea una polilínea cada 'paso' metros
        public static List<Punto2D> MuestrearPolilinea(
            List<Punto2D> pts, double paso)
        {
            var result = new List<Punto2D>();
            for (int i = 0; i < pts.Count - 1; i++)
            {
                var p1 = pts[i]; var p2 = pts[i + 1];
                double len = p1.DistanciaA(p2);
                if (len < 1e-6) continue;
                int n = Math.Max(1, (int)(len / paso));
                for (int j = 0; j <= n; j++)
                {
                    double t = (double)j / n;
                    result.Add(new Punto2D(
                        p1.X + t * (p2.X - p1.X),
                        p1.Y + t * (p2.Y - p1.Y)));
                }
            }
            return result;
        }
    }

    // ─── COLINDANCIAS AUTOMÁTICAS ─────────────────────────────────
    public static class ColindanciasAuto
    {
        /// Detecta todas las colindancias entre lotes de una lista
        public static List<(int IdxA, int IdxB, Segmento Seg)> DetectarAdyacentes(
            List<Poligono> poligonos, double tol = 0.05)
        {
            var result = new List<(int, int, Segmento)>();
            for (int i = 0; i < poligonos.Count - 1; i++)
            for (int j = i + 1; j < poligonos.Count; j++)
            {
                var seg = poligonos[i].SegmentoCompartido(poligonos[j], tol);
                if (seg != null) result.Add((i, j, seg));
            }
            return result;
        }

        /// Para cada lote, construye su lista de colindancias completa
        public static void AsignarColindancias(
            List<Lote> lotes,
            List<Via> vias,
            double tolAdyacencia = 0.05,
            double tolVia = 2.0)
        {
            var polys = lotes.Select(l => l.Poligono).ToList();
            var adyacentes = DetectarAdyacentes(polys, tolAdyacencia);

            // Limpiar colindancias previas
            foreach (var lote in lotes)
                lote.Colindancias.Clear();

            // Colindancias lote-lote
            foreach (var (idxA, idxB, seg) in adyacentes)
            {
                var loteA = lotes[idxA];
                var loteB = lotes[idxB];

                loteA.Colindancias.Add(new Colindancia
                {
                    Segmento = seg,
                    Descripcion = $"Con {loteB.NombreManzana} {loteB.Numero}",
                    Tipo = ColindanciaTipo.OtroLote
                });
                loteB.Colindancias.Add(new Colindancia
                {
                    Segmento = seg,
                    Descripcion = $"Con {loteA.NombreManzana} {loteA.Numero}",
                    Tipo = ColindanciaTipo.OtroLote
                });
            }

            // Colindancias lote-vía
            if (vias == null) return;
            foreach (var lote in lotes)
            {
                foreach (var lado in lote.Poligono.Segmentos)
                {
                    // Verificar si algún lado ya tiene colindancia asignada
                    bool yaAsignado = lote.Colindancias.Any(c =>
                        c.Segmento.P1.DistanciaA(lado.P1) < tolAdyacencia &&
                        c.Segmento.P2.DistanciaA(lado.P2) < tolAdyacencia);
                    if (yaAsignado) continue;

                    // Buscar vía más cercana al punto medio del lado
                    Via viaProxima = null;
                    double minD = double.MaxValue;

                    foreach (var via in vias)
                    {
                        if (via.Eje == null || via.Eje.Count < 2) continue;
                        for (int k = 0; k < via.Eje.Count - 1; k++)
                        {
                            var segVia = new Segmento(via.Eje[k], via.Eje[k + 1]);
                            double d = segVia.DistanciaPunto(lado.PuntoMedio);
                            if (d < minD) { minD = d; viaProxima = via; }
                        }
                    }

                    if (viaProxima != null && minD < tolVia)
                    {
                        lote.Colindancias.Add(new Colindancia
                        {
                            Segmento = lado,
                            Descripcion = viaProxima.Nombre ?? "Vía Pública",
                            NombreVia = viaProxima.Nombre,
                            Tipo = ColindanciaTipo.Via
                        });
                    }
                }
            }
        }

        /// Genera texto de resumen de colindancias por cuadrante (N/S/E/O)
        public static string ResumenColindancias(Lote lote)
        {
            if (!lote.Colindancias.Any())
                return "Sin colindancias detectadas";
            return string.Join("\n",
                lote.Colindancias.Select(c =>
                    $"  {c.Segmento.Rumbo}: {c.Descripcion} ({c.Segmento.Longitud:F3} m)"));
        }
    }

    // ─── MANZANEO ────────────────────────────────────────────────
    public static class ManzaneoHelper
    {
        /// Genera la grilla de manzanas dado el predio, anchos de vía y separaciones
        public static List<Manzana> GenerarManzanasGrilla(
            Poligono predio,
            double anchoViaH, double anchoViaV,
            double separacionH, double separacionV,
            SistemaNomenclatura sistema, int inicio = 1)
        {
            var (min, max) = predio.BoundingBox;
            double W = max.X - min.X;
            double H = max.Y - min.Y;

            double mzW = separacionV - anchoViaV;
            double mzH = separacionH - anchoViaH;
            int nCols = Math.Max(1, (int)(W / separacionV));
            int nFilas = Math.Max(1, (int)(H / separacionH));

            var manzanas = new List<Manzana>();
            int idx = 0;

            for (int row = 0; row < nFilas; row++)
            for (int col = 0; col < nCols; col++)
            {
                double x0 = min.X + col * separacionV + anchoViaV;
                double y0 = min.Y + row * separacionH + anchoViaH;

                var poly = new Poligono(new[]
                {
                    new Punto2D(x0,        y0),
                    new Punto2D(x0 + mzW,  y0),
                    new Punto2D(x0 + mzW,  y0 + mzH),
                    new Punto2D(x0,        y0 + mzH)
                });

                string nombre = sistema == SistemaNomenclatura.Alfabetico
                    ? Nomenclatura.ManzanaAlfa(idx)
                    : Nomenclatura.ManzanaNum(idx, inicio);

                manzanas.Add(new Manzana
                {
                    Nombre = nombre,
                    Poligono = poly
                });
                idx++;
            }
            return manzanas;
        }
    }
}
