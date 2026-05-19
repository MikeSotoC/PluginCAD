using System;
using System.Collections.Generic;
using System.Linq;
using CatastroTools.CAD.Interfaces;
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
    public class ComandosManzaneo
    {
        private static ICadPlatform Cad  => CatastroPlugin.Plataforma;
        private static ServicioDibujo Dib => CatastroPlugin.Dibujo;

        // ─── CT-VIA-EJE ────────────────────────────────────────
        [CommandMethod("CT-VIA-EJE")]
        public void ViaEje()
        {
            try
            {
                var dlg = new VentanaVia();
                if (dlg.ShowDialog() != true) return;

                var via = dlg.ResultadoVia;
                Cad.MensajeConsola($"Trazando eje: {via.Nombre} ({via.Ancho:F2} m)");

                var puntos = Cad.PedirPolilineaInteractiva(
                    $"Traza el EJE de '{via.Nombre}' — Enter para terminar:");
                if (puntos.Count < 2)
                {
                    Cad.MensajeError("Se necesitan al menos 2 puntos."); return;
                }

                via.Eje = puntos;
                double mitad = via.Ancho / 2.0;

                // Dibujar eje
                Cad.DibujarPolilineaAbierta(puntos, CapasCatastro.Ejes);

                // Offset de bordes mediante puntos paralelos aproximados
                var bordeIzq = OffsetPolyline(puntos, -mitad);
                var bordeDer = OffsetPolyline(puntos,  mitad);
                Cad.DibujarPolilineaAbierta(bordeIzq, CapasCatastro.Vias);
                Cad.DibujarPolilineaAbierta(bordeDer, CapasCatastro.Vias);

                // Etiqueta en punto medio del eje
                var mid = puntos[0].PuntoMedio(puntos[puntos.Count - 1]);
                double ang = puntos[0].AnguloA(puntos[puntos.Count - 1]);
                Cad.InsertarTexto(mid, via.Nombre,
                    via.Ancho * 0.12, ang, CapasCatastro.LabelMz, TextoJustif.MC);

                // Registrar en proyecto
                CatastroPlugin.ProyectoActual.Vias.Add(via);
                Cad.MensajeConsola($"✓ Vía '{via.Nombre}' trazada. Ancho: {via.Ancho:F2} m");
            }
            catch (OperationCanceledException) { Cad.MensajeConsola("Cancelado."); }
            catch (Exception ex) { Cad.MensajeError(ex.Message); }
        }

        // ─── CT-VIAS-GRILLA ────────────────────────────────────
        [CommandMethod("CT-VIAS-GRILLA")]
        public void ViasGrilla()
        {
            try
            {
                var dlg = new VentanaViasGrilla();
                if (dlg.ShowDialog() != true) return;

                var p = dlg.Parametros;
                long entId = Cad.SeleccionarEntidad("Selecciona el predio matriz:");
                if (entId < 0) return;

                var verts = Cad.ObtenerVerticesPolilinea(entId);
                if (verts.Count < 3) { Cad.MensajeError("Polilínea inválida."); return; }

                var poly = new Poligono(verts);
                var (min, max) = poly.BoundingBox;

                // Calles horizontales
                double y = min.Y + p.SepH;
                int numH = 1;
                while (y < max.Y)
                {
                    double mitH = p.AnchoH / 2.0;
                    var b1 = new[] { new Punto2D(min.X, y - mitH), new Punto2D(max.X, y - mitH) };
                    var b2 = new[] { new Punto2D(min.X, y + mitH), new Punto2D(max.X, y + mitH) };
                    Cad.DibujarPolilineaAbierta(b1, CapasCatastro.Vias);
                    Cad.DibujarPolilineaAbierta(b2, CapasCatastro.Vias);
                    Cad.DibujarLinea(new Punto2D(min.X, y), new Punto2D(max.X, y), CapasCatastro.Ejes);
                    Cad.InsertarTexto(
                        new Punto2D((min.X + max.X) / 2.0, y),
                        $"{p.NombreH} {numH}", p.AnchoH * 0.25, 0,
                        CapasCatastro.LabelMz, TextoJustif.MC);
                    y += p.SepH; numH++;
                }

                // Calles verticales
                double x = min.X + p.SepV;
                int numV = 1;
                while (x < max.X)
                {
                    double mitV = p.AnchoV / 2.0;
                    var b1 = new[] { new Punto2D(x - mitV, min.Y), new Punto2D(x - mitV, max.Y) };
                    var b2 = new[] { new Punto2D(x + mitV, min.Y), new Punto2D(x + mitV, max.Y) };
                    Cad.DibujarPolilineaAbierta(b1, CapasCatastro.Vias);
                    Cad.DibujarPolilineaAbierta(b2, CapasCatastro.Vias);
                    Cad.DibujarLinea(new Punto2D(x, min.Y), new Punto2D(x, max.Y), CapasCatastro.Ejes);
                    Cad.InsertarTexto(
                        new Punto2D(x, (min.Y + max.Y) / 2.0),
                        $"{p.NombreV} {numV}", p.AnchoV * 0.25, Math.PI / 2.0,
                        CapasCatastro.LabelMz, TextoJustif.MC);
                    x += p.SepV; numV++;
                }

                Cad.MensajeConsola($"✓ Grilla: {numH - 1} calles H × {numV - 1} calles V.");
                Cad.MensajeConsola("  Siguiente: CT-MANZANEO-GRILLA");
            }
            catch (OperationCanceledException) { Cad.MensajeConsola("Cancelado."); }
            catch (Exception ex) { Cad.MensajeError(ex.Message); }
        }

        // ─── CT-MANZANEO ───────────────────────────────────────
        [CommandMethod("CT-MANZANEO")]
        public void Manzaneo()
        {
            try
            {
                var dlg = new VentanaManzaneo();
                if (dlg.ShowDialog() != true) return;

                var ids = Cad.SeleccionarMultiple(
                    "Selecciona las polilíneas de MANZANAS:",
                    new FiltroSeleccion { TiposPermitidos = new List<string> { "LWPOLYLINE", "POLYLINE" } });

                if (ids.Count == 0) { Cad.MensajeConsola("Sin selección."); return; }

                var manzanas = new List<Manzana>();
                for (int i = 0; i < ids.Count; i++)
                {
                    var verts = Cad.ObtenerVerticesPolilinea(ids[i]);
                    if (verts.Count < 3) continue;

                    string nombre = dlg.Sistema == SistemaNomenclatura.Alfabetico
                        ? Nomenclatura.ManzanaAlfa(i + dlg.Inicio - 1)
                        : dlg.Sistema == SistemaNomenclatura.Numerico
                            ? Nomenclatura.ManzanaNum(i, dlg.Inicio)
                            : Cad.PedirTexto($"Nombre manzana {i + 1}: ", $"MZ {i + 1}");

                    var mz = new Manzana
                    {
                        Nombre   = nombre,
                        Poligono = new Poligono(verts)
                    };
                    manzanas.Add(mz);

                    // Dibujar etiqueta
                    Dib.AcotarManzana(mz, new ConfigTexto { AlturaManzana = dlg.AlturaTexto });

                    Cad.MensajeConsola($"  {nombre} → {mz.Area:F2} m²");
                }

                CatastroPlugin.ProyectoActual.Manzanas.AddRange(manzanas);
                Cad.MensajeConsola($"✓ {manzanas.Count} manzanas registradas.");
                Cad.MensajeConsola("  Siguiente: CT-LOTIZAR sobre cada manzana.");
            }
            catch (OperationCanceledException) { Cad.MensajeConsola("Cancelado."); }
            catch (Exception ex) { Cad.MensajeError(ex.Message); }
        }

        // ─── CT-MANZANEO-GRILLA ────────────────────────────────
        [CommandMethod("CT-MANZANEO-GRILLA")]
        public void ManzaneoGrilla()
        {
            try
            {
                var dlg = new VentanaManzaneoGrilla();
                if (dlg.ShowDialog() != true) return;

                long entId = Cad.SeleccionarEntidad("Selecciona el predio matriz:");
                if (entId < 0) return;

                var verts = Cad.ObtenerVerticesPolilinea(entId);
                if (verts.Count < 3) { Cad.MensajeError("Polilínea inválida."); return; }

                var predio = new Poligono(verts);
                var manzanas = ManzaneoHelper.GenerarManzanasGrilla(
                    predio,
                    dlg.AnchoViaH, dlg.AnchoViaV,
                    dlg.SepH, dlg.SepV,
                    dlg.Sistema, dlg.Inicio);

                foreach (var mz in manzanas)
                {
                    Cad.DibujarPolilineaCerrada(mz.Poligono.Vertices, CapasCatastro.Manzanas);
                    Dib.AcotarManzana(mz, new ConfigTexto { AlturaManzana = dlg.AlturaTexto });
                    Cad.MensajeConsola($"  {mz.Nombre} → {mz.Area:F2} m²");
                }

                CatastroPlugin.ProyectoActual.Manzanas.AddRange(manzanas);
                Cad.MensajeConsola($"✓ {manzanas.Count} manzanas generadas.");
            }
            catch (OperationCanceledException) { Cad.MensajeConsola("Cancelado."); }
            catch (Exception ex) { Cad.MensajeError(ex.Message); }
        }

        // ─── CT-SECCION-VIA ────────────────────────────────────
        [CommandMethod("CT-SECCION-VIA")]
        public void SeccionVia()
        {
            try
            {
                var dlg = new VentanaSeccionVia();
                if (dlg.ShowDialog() != true) return;

                var via = dlg.ResultadoVia;
                var ins = Cad.PedirPunto("Punto de inserción de la sección:");
                Dib.DibujarSeccionVia(via, ins);
                Cad.MensajeConsola($"✓ Sección de '{via.Nombre}' insertada.");
            }
            catch (OperationCanceledException) { Cad.MensajeConsola("Cancelado."); }
            catch (Exception ex) { Cad.MensajeError(ex.Message); }
        }

        // ─── UTILIDAD: OFFSET DE POLILÍNEA ────────────────────
        private static List<Punto2D> OffsetPolyline(List<Punto2D> pts, double dist)
        {
            var result = new List<Punto2D>();
            for (int i = 0; i < pts.Count; i++)
            {
                // Calcular normal promedio en cada vértice
                double nx = 0, ny = 0;
                if (i > 0)
                {
                    double ang = pts[i - 1].AnguloA(pts[i]) + Math.PI / 2.0;
                    nx += Math.Cos(ang); ny += Math.Sin(ang);
                }
                if (i < pts.Count - 1)
                {
                    double ang = pts[i].AnguloA(pts[i + 1]) + Math.PI / 2.0;
                    nx += Math.Cos(ang); ny += Math.Sin(ang);
                }
                double len = Math.Sqrt(nx * nx + ny * ny);
                if (len > 1e-6) { nx /= len; ny /= len; }
                result.Add(new Punto2D(pts[i].X + nx * dist, pts[i].Y + ny * dist));
            }
            return result;
        }
    }
}
