using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CatastroTools.CAD.Interfaces;
using CatastroTools.Core.Export;
using CatastroTools.Core.Geometry;
using CatastroTools.Core.Models;
using CatastroTools.Plugin.UI;

#if ZWCAD
using ZwCAD.Runtime;
#elif AUTOCAD
using Autodesk.AutoCAD.Runtime;
#endif

namespace CatastroTools.Plugin.Commands
{
    // ═══════════════════════════════════════════════════════════
    // LOTIZACIÓN
    // ═══════════════════════════════════════════════════════════
    public class ComandosLotizacion
    {
        private static ICadPlatform   Cad => CatastroPlugin.Plataforma;
        private static ServicioDibujo Dib => CatastroPlugin.Dibujo;

        [CommandMethod("CT-LOTIZAR")]
        public void Lotizar()
        {
            try
            {
                var dlg = new VentanaLotizacion();
                if (dlg.ShowDialog() != true) return;

                // 1. Seleccionar lotes
                var ids = Cad.SeleccionarMultiple(
                    "PASO 1/3 — Selecciona todos los LOTES (polilíneas cerradas):",
                    new FiltroSeleccion { TiposPermitidos = new[] { "LWPOLYLINE", "POLYLINE" }.ToList() });
                if (ids.Count == 0) { Cad.MensajeConsola("Sin selección."); return; }

                var polys = ids
                    .Where(id => Cad.EsPolilineaCerrada(id))
                    .Select(id => new Poligono(Cad.ObtenerVerticesPolilinea(id)))
                    .Where(p => p.NumVertices >= 3)
                    .ToList();

                if (polys.Count < 2) { Cad.MensajeError("Se necesitan al menos 2 lotes cerrados."); return; }
                Cad.MensajeConsola($"  {polys.Count} lotes detectados.");

                // 2. Polilínea de recorrido
                Cad.MensajeConsola("PASO 2/3 — Traza la polilínea de RECORRIDO sobre los lotes:");
                var recorrido = Cad.PedirPolilineaInteractiva(
                    "Pasa por encima de los lotes en el orden de numeración deseado:");
                if (recorrido.Count < 2) { Cad.MensajeError("Recorrido inválido."); return; }

                // 3. Ordenar por recorrido
                var orden = Recorrido.OrdenarPorRecorrido(polys, recorrido, 1.0);
                Cad.MensajeConsola("PASO 3/3 — Etiquetando...");

                // 4. Construir lotes con nombres
                var lotes = new List<Lote>();
                for (int i = 0; i < orden.Count; i++)
                {
                    var lote = new Lote
                    {
                        Numero       = Nomenclatura.Lote(i, dlg.NumInicial - 1, ""),
                        NombreManzana = dlg.Prefijo,
                        Poligono     = polys[orden[i]]
                    };
                    lotes.Add(lote);

                    // Etiquetar
                    Dib.DibujarEtiquetaLote(lote, new ConfigTexto
                    {
                        AlturaNumeroLote = dlg.AlturaTexto,
                        AlturaArea       = dlg.AlturaTexto * 0.85,
                        MostrarPartida   = false
                    });
                    Cad.MensajeConsola($"  {dlg.Prefijo} {lote.Numero} → {lote.Area:F2} m²");
                }

                // 5. Colindancias automáticas
                ColindanciasAuto.AsignarColindancias(
                    lotes, CatastroPlugin.ProyectoActual.Vias);

                if (dlg.DibujarColindancias)
                    foreach (var lt in lotes)
                        Dib.DibujarColindancias(lt);

                // Registrar en proyecto
                var mzActual = CatastroPlugin.ProyectoActual.Manzanas.FirstOrDefault()
                    ?? new Manzana { Nombre = dlg.Prefijo };
                mzActual.Lotes.AddRange(lotes);

                Cad.MensajeConsola($"✓ {lotes.Count} lotes etiquetados.");
            }
            catch (OperationCanceledException) { Cad.MensajeConsola("Cancelado."); }
            catch (Exception ex) { Cad.MensajeError(ex.Message); }
        }

        [CommandMethod("CT-COLIN-AUTO")]
        public void ColindanciasAutomaticas()
        {
            try
            {
                var ids = Cad.SeleccionarMultiple("Selecciona los lotes:",
                    new FiltroSeleccion { TiposPermitidos = new[] { "LWPOLYLINE", "POLYLINE" }.ToList() });
                if (ids.Count == 0) return;

                int numBase = Cad.PedirEntero("Número inicial de lote <1>:", 1) ?? 1;
                string pref = Cad.PedirTexto("Prefijo (ej: Lote):", "Lote ");

                var lotes = ids
                    .Where(id => Cad.EsPolilineaCerrada(id))
                    .Select((id, i) => new Lote
                    {
                        Numero        = (numBase + i).ToString(),
                        NombreManzana = pref,
                        Poligono      = new Poligono(Cad.ObtenerVerticesPolilinea(id))
                    })
                    .Where(l => l.Poligono.NumVertices >= 3)
                    .ToList();

                ColindanciasAuto.AsignarColindancias(
                    lotes, CatastroPlugin.ProyectoActual.Vias);

                foreach (var lt in lotes)
                    Dib.DibujarColindancias(lt);

                Cad.MensajeConsola($"✓ Colindancias etiquetadas en {lotes.Count} lotes.");
            }
            catch (Exception ex) { Cad.MensajeError(ex.Message); }
        }

        [CommandMethod("CT-HABILITACION")]
        public void Habilitacion()
        {
            try
            {
                var dlg = new VentanaHabilitacion();
                if (dlg.ShowDialog() != true) return;

                long entId = Cad.SeleccionarEntidad("Selecciona el polígono de la manzana:");
                if (entId < 0) return;

                var verts = Cad.ObtenerVerticesPolilinea(entId);
                if (verts.Count < 3) { Cad.MensajeError("Polilínea inválida."); return; }

                var poly = new Poligono(verts);
                var (min, _) = poly.BoundingBox;
                var (nCols, nFilas) = Subdivision.LotesPosibles(
                    min, poly.BoundingBox.Max, dlg.AnchoLote, dlg.ProfLote);

                var lotePolys = Subdivision.GenerarGrilla(min, dlg.AnchoLote, dlg.ProfLote, nCols, nFilas);

                int idx = 0;
                foreach (var lp in lotePolys)
                {
                    Cad.DibujarPolilineaCerrada(lp.Vertices, CapasCatastro.Lotes);
                    if (dlg.Numerar)
                    {
                        var lt = new Lote
                        {
                            Numero   = (dlg.NumInicial + idx).ToString(),
                            Poligono = lp
                        };
                        Dib.DibujarEtiquetaLote(lt, new ConfigTexto
                        {
                            AlturaNumeroLote = dlg.AlturaTexto,
                            AlturaArea       = dlg.AlturaTexto * 0.85,
                            MostrarPartida   = false
                        });
                    }
                    idx++;
                }
                Cad.MensajeConsola($"✓ {idx} lotes generados ({nCols}×{nFilas}).");
            }
            catch (OperationCanceledException) { Cad.MensajeConsola("Cancelado."); }
            catch (Exception ex) { Cad.MensajeError(ex.Message); }
        }

        [CommandMethod("CT-SUBDIV")]
        public void Subdividir()
        {
            try
            {
                long entId = Cad.SeleccionarEntidad("Selecciona la polilínea a subdividir:");
                if (entId < 0) return;
                if (!Cad.EsPolilineaCerrada(entId)) { Cad.MensajeError("La polilínea debe estar cerrada."); return; }

                var verts = Cad.ObtenerVerticesPolilinea(entId);
                var poly = new Poligono(verts);

                var p1 = Cad.PedirPunto("Primer punto del corte:");
                var p2 = Cad.PedirPunto("Segundo punto del corte:", p1);

                var resultado = Subdivision.Cortar(poly, p1, p2);
                if (!resultado.HasValue) { Cad.MensajeError("La línea de corte no cruza el lote."); return; }

                var (polyA, polyB) = resultado.Value;
                Cad.DibujarPolilineaCerrada(polyA.Vertices, CapasCatastro.Lotes);
                Cad.DibujarPolilineaCerrada(polyB.Vertices, CapasCatastro.Lotes);
                Cad.DibujarLinea(p1, p2, CapasCatastro.Linderos);

                Cad.MensajeConsola($"✓ Subdivisión completada:");
                Cad.MensajeConsola($"  Lote A: {polyA.Area:F2} m²");
                Cad.MensajeConsola($"  Lote B: {polyB.Area:F2} m²");
            }
            catch (OperationCanceledException) { Cad.MensajeConsola("Cancelado."); }
            catch (Exception ex) { Cad.MensajeError(ex.Message); }
        }
    }

    // ═══════════════════════════════════════════════════════════
    // ETIQUETADO
    // ═══════════════════════════════════════════════════════════
    public class ComandosEtiquetado
    {
        private static ICadPlatform   Cad => CatastroPlugin.Plataforma;
        private static ServicioDibujo Dib => CatastroPlugin.Dibujo;

        [CommandMethod("CT-ETIQUETA")]
        public void EtiquetarLote()
        {
            try
            {
                long entId = Cad.SeleccionarEntidad("Selecciona el lote:");
                if (entId < 0) return;

                var verts = Cad.ObtenerVerticesPolilinea(entId);
                var poly  = new Poligono(verts);

                var dlg = new VentanaEtiquetaLote(poly.Area, poly.Perimetro, poly.NumVertices);
                if (dlg.ShowDialog() != true) return;

                var lote = dlg.ResultadoLote;
                lote.Poligono = poly;
                Dib.DibujarEtiquetaLote(lote, dlg.ConfigTexto);
                Cad.MensajeConsola($"✓ Lote {lote.Numero} etiquetado — {lote.Area:F2} m²");
            }
            catch (OperationCanceledException) { Cad.MensajeConsola("Cancelado."); }
            catch (Exception ex) { Cad.MensajeError(ex.Message); }
        }

        [CommandMethod("CT-ACOTAR")]
        public void AcotarLote()
        {
            try
            {
                long entId = Cad.SeleccionarEntidad("Selecciona el lote a acotar:");
                if (entId < 0) return;

                var verts = Cad.ObtenerVerticesPolilinea(entId);
                var poly  = new Poligono(verts);

                var dlg = new VentanaAcotar(poly.Area, poly.Perimetro, verts.Count);
                if (dlg.ShowDialog() != true) return;

                var lote = dlg.ResultadoLote;
                lote.Poligono = poly;
                Dib.AcotarLoteCompleto(lote, dlg.ConfigTexto);
                Cad.MensajeConsola($"✓ Lote {lote.Numero} acotado — {lote.Area:F2} m²");
            }
            catch (OperationCanceledException) { Cad.MensajeConsola("Cancelado."); }
            catch (Exception ex) { Cad.MensajeError(ex.Message); }
        }

        [CommandMethod("CT-LINDEROS")]
        public void Linderos()
        {
            try
            {
                long entId = Cad.SeleccionarEntidad("Selecciona el lote:");
                if (entId < 0) return;

                var cfg = new ConfigTexto
                {
                    AlturaLindero = Cad.PedirReal("Altura texto <1.8>:", 1.8) ?? 1.8,
                    MostrarRumbo  = true
                };
                var lote = new Lote { Poligono = new Poligono(Cad.ObtenerVerticesPolilinea(entId)) };
                Dib.DibujarLinderos(lote, cfg);
                Cad.MensajeConsola($"✓ {lote.Poligono.NumVertices} linderos etiquetados.");
            }
            catch (Exception ex) { Cad.MensajeError(ex.Message); }
        }

        [CommandMethod("CT-VERTICES")]
        public void Vertices()
        {
            try
            {
                long entId = Cad.SeleccionarEntidad("Selecciona el lote:");
                if (entId < 0) return;

                var dlg = new VentanaVertices();
                if (dlg.ShowDialog() != true) return;

                var lote = new Lote { Poligono = new Poligono(Cad.ObtenerVerticesPolilinea(entId)) };
                Dib.DibujarVertices(lote, dlg.ConfigTexto, dlg.TipoSimbolo);
                Cad.MensajeConsola($"✓ {lote.Poligono.NumVertices} vértices marcados.");
            }
            catch (OperationCanceledException) { Cad.MensajeConsola("Cancelado."); }
            catch (Exception ex) { Cad.MensajeError(ex.Message); }
        }

        [CommandMethod("CT-IMPORTAR-COORDS")]
        public void ImportarCoords()
        {
            try
            {
                var dlg = new VentanaImportarCoords();
                if (dlg.ShowDialog() != true) return;

                var puntos = dlg.Puntos;
                if (puntos.Count < 3) { Cad.MensajeError("Se necesitan al menos 3 puntos."); return; }

                Cad.DibujarPolilineaCerrada(puntos, CapasCatastro.Lotes);

                if (dlg.MarcarVertices)
                {
                    var lote = new Lote { Poligono = new Poligono(puntos) };
                    Dib.DibujarVertices(lote, dlg.ConfigTexto);
                }

                var poly = new Poligono(puntos);
                Cad.MensajeConsola($"✓ Polilínea creada — {puntos.Count} vértices — {poly.Area:F2} m²");
            }
            catch (Exception ex) { Cad.MensajeError(ex.Message); }
        }
    }

    // ═══════════════════════════════════════════════════════════
    // TABLAS
    // ═══════════════════════════════════════════════════════════
    public class ComandosTablas
    {
        private static ICadPlatform   Cad => CatastroPlugin.Plataforma;
        private static ServicioDibujo Dib => CatastroPlugin.Dibujo;

        [CommandMethod("CT-TABLA")]
        public void TablaDatosTecnicos()
        {
            try
            {
                long entId = Cad.SeleccionarEntidad("Selecciona el lote (o Esc para ingresar datos manualmente):");
                Poligono poly = entId >= 0
                    ? new Poligono(Cad.ObtenerVerticesPolilinea(entId))
                    : null;

                var dlg = new VentanaTabla(poly?.Area ?? 0, poly?.Perimetro ?? 0);
                if (dlg.ShowDialog() != true) return;

                var lote  = dlg.ResultadoLote;
                if (poly != null) lote.Poligono = poly;
                var ins = Cad.PedirPunto("Punto de inserción de la tabla:");
                Dib.DibujarTablaDatosTecnicos(lote, ins, dlg.AnchoTabla, dlg.AltoFila, dlg.AlturaTexto);
                Cad.MensajeConsola("✓ Tabla de datos técnicos insertada.");
            }
            catch (OperationCanceledException) { Cad.MensajeConsola("Cancelado."); }
            catch (Exception ex) { Cad.MensajeError(ex.Message); }
        }

        [CommandMethod("CT-TABLA-COORDS")]
        public void TablaCoordenadas()
        {
            try
            {
                long entId = Cad.SeleccionarEntidad("Selecciona el lote:");
                if (entId < 0) return;
                var lote = new Lote { Poligono = new Poligono(Cad.ObtenerVerticesPolilinea(entId)) };
                var ins  = Cad.PedirPunto("Punto de inserción:");
                Dib.DibujarTablaCoordenadas(lote, ins);
                Cad.MensajeConsola("✓ Tabla de coordenadas insertada.");
            }
            catch (OperationCanceledException) { Cad.MensajeConsola("Cancelado."); }
            catch (Exception ex) { Cad.MensajeError(ex.Message); }
        }

        [CommandMethod("CT-TABLA-COLIN")]
        public void TablaColindancias()
        {
            try
            {
                var dlg = new VentanaColindancias();
                if (dlg.ShowDialog() != true) return;
                var ins = Cad.PedirPunto("Punto de inserción:");
                Dib.DibujarTablaColindancias(dlg.Norte, dlg.Sur, dlg.Este, dlg.Oeste, ins);
                Cad.MensajeConsola("✓ Tabla de colindancias insertada.");
            }
            catch (OperationCanceledException) { Cad.MensajeConsola("Cancelado."); }
            catch (Exception ex) { Cad.MensajeError(ex.Message); }
        }
    }

    // ═══════════════════════════════════════════════════════════
    // EXPORTACIÓN
    // ═══════════════════════════════════════════════════════════
    public class ComandosExport
    {
        private static ICadPlatform Cad => CatastroPlugin.Plataforma;

        [CommandMethod("CT-EXPORT-HTML")]
        public void ExportHTML()
        {
            try
            {
                long entId = Cad.SeleccionarEntidad("Selecciona el lote:");
                if (entId < 0) return;

                var dlg = new VentanaExportHTML();
                if (dlg.ShowDialog() != true) return;

                var lote = dlg.ResultadoLote;
                lote.Poligono = new Poligono(Cad.ObtenerVerticesPolilinea(entId));

                string ruta = Path.Combine(Cad.DirectorioDWG, $"{Cad.NombreDWG}_catastro.html");
                ExportadorHTML.GenerarReporte(lote, ruta);
                Cad.MensajeConsola($"✓ Reporte HTML: {ruta}");

                // Abrir en navegador
                try { System.Diagnostics.Process.Start(ruta); } catch { }
            }
            catch (OperationCanceledException) { Cad.MensajeConsola("Cancelado."); }
            catch (Exception ex) { Cad.MensajeError(ex.Message); }
        }

        [CommandMethod("CT-EXPORT-CSV")]
        public void ExportCSV()
        {
            try
            {
                long entId = Cad.SeleccionarEntidad("Selecciona el lote:");
                if (entId < 0) return;

                var lote = new Lote { Poligono = new Poligono(Cad.ObtenerVerticesPolilinea(entId)) };
                var zona = CoordUtils.NombreZona(CoordUtils.DetectarZona(lote.Poligono.Vertices));
                string ruta = Path.Combine(Cad.DirectorioDWG, $"{Cad.NombreDWG}_vertices.csv");
                ExportadorCSV.ExportarVertices(lote, ruta, zona);
                Cad.MensajeConsola($"✓ CSV exportado: {ruta}");
            }
            catch (Exception ex) { Cad.MensajeError(ex.Message); }
        }

        [CommandMethod("CT-LIMPIAR")]
        public void Limpiar()
        {
            try
            {
                CatastroPlugin.Plataforma.Purgar();
                Cad.MensajeConsola("✓ Dibujo purgado y auditado.");
            }
            catch (Exception ex) { Cad.MensajeError(ex.Message); }
        }
    }

    // ═══════════════════════════════════════════════════════════
    // SISTEMA
    // ═══════════════════════════════════════════════════════════
    public class ComandosSistema
    {
        private static ICadPlatform Cad => CatastroPlugin.Plataforma;

        [CommandMethod("CT")]
        public void MenuAyuda()
        {
            Cad.MensajeConsola("══════════════════════════════════════════════════");
            Cad.MensajeConsola("  CatastroTools v2.0 — Comandos disponibles");
            Cad.MensajeConsola("══════════════════════════════════════════════════");
            Cad.MensajeConsola("  ★ CT-PANEL     Abrir panel visual (recomendado)");
            Cad.MensajeConsola("  FLUJO: CT-PROYECTO (ver guía completa)");
            Cad.MensajeConsola("  VÍAS:     CT-VIA-EJE | CT-VIAS-GRILLA | CT-SECCION-VIA");
            Cad.MensajeConsola("  MANZANAS: CT-MANZANEO | CT-MANZANEO-GRILLA");
            Cad.MensajeConsola("  LOTES:    CT-LOTIZAR | CT-SUBDIV | CT-HABILITACION");
            Cad.MensajeConsola("  ETIQUETAS:CT-ETIQUETA | CT-ACOTAR | CT-LINDEROS");
            Cad.MensajeConsola("  VÉRTICES: CT-VERTICES | CT-IMPORTAR-COORDS");
            Cad.MensajeConsola("  TABLAS:   CT-TABLA | CT-TABLA-COORDS | CT-TABLA-COLIN");
            Cad.MensajeConsola("  EXPORT:   CT-EXPORT-HTML | CT-EXPORT-CSV");
            Cad.MensajeConsola("  SISTEMA:  CT-CAPAS | CT-CONFIG | CT-LIMPIAR");
            Cad.MensajeConsola("══════════════════════════════════════════════════");
        }

        [CommandMethod("CT-CAPAS")]
        public void InicializarCapas()
        {
            CatastroPlugin.Dibujo.InicializarCapas();
        }

        [CommandMethod("CT-CONFIG")]
        public void Configuracion()
        {
            try
            {
                var dlg = new VentanaConfiguracion(CatastroPlugin.Config);
                if (dlg.ShowDialog() == true)
                {
                    CatastroPlugin.Dibujo.GetType()
                        .GetField("_cfg", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                        ?.SetValue(CatastroPlugin.Dibujo, dlg.ConfigResultado);
                    Cad.MensajeConsola("✓ Configuración actualizada.");
                }
            }
            catch (Exception ex) { Cad.MensajeError(ex.Message); }
        }

        [CommandMethod("CT-PROYECTO")]
        public void FlujoProyecto()
        {
            Cad.MensajeConsola("══════════════════════════════════════════════════");
            Cad.MensajeConsola("  FLUJO COMPLETO DE HABILITACIÓN URBANA");
            Cad.MensajeConsola("══════════════════════════════════════════════════");
            Cad.MensajeConsola("  1. CT-IMPORTAR-COORDS  → Predio matriz desde UTM");
            Cad.MensajeConsola("  2. CT-VIAS-GRILLA      → Generar red vial");
            Cad.MensajeConsola("     CT-VIA-EJE          → Vía individual por eje");
            Cad.MensajeConsola("  3. CT-MANZANEO-GRILLA  → Generar manzanas");
            Cad.MensajeConsola("     CT-MANZANEO         → Manzanas ya dibujadas");
            Cad.MensajeConsola("  4. CT-LOTIZAR          → Numerar+etiquetar+colindancias");
            Cad.MensajeConsola("     CT-HABILITACION     → Grilla de lotes");
            Cad.MensajeConsola("  5. CT-VERTICES         → Marcar mojones UTM");
            Cad.MensajeConsola("  6. CT-TABLA            → Cuadro datos técnicos");
            Cad.MensajeConsola("     CT-TABLA-COORDS     → Cuadro UTM");
            Cad.MensajeConsola("     CT-TABLA-COLIN      → Cuadro colindancias");
            Cad.MensajeConsola("  7. CT-EXPORT-HTML      → Reporte para SUNARP");
            Cad.MensajeConsola("  8. CT-SECCION-VIA      → Sección vial en planta");
            Cad.MensajeConsola("  9. CT-LIMPIAR          → Purgar antes de entregar");
            Cad.MensajeConsola("══════════════════════════════════════════════════");
        }
    }
}
