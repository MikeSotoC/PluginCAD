// ============================================================
// CatastroTools.CAD — ZwPlatform.cs
// Implementación de ICadPlatform para ZWCAD 2022/2023/2024
//
// Referencias requeridas (agregar manualmente en VS2019):
//   C:\Program Files\ZWSOFT\ZWCAD <version>\ZwCAD.Interop.dll
//   ZwCAD.Interop.Common.dll  (o según versión)
//
// La API de ZWCAD .NET es muy similar a la de AutoCAD ObjectARX
// con namespace ZwCAD.* en lugar de Autodesk.AutoCAD.*
// ============================================================

// NOTA PARA COMPILACIÓN:
// Este archivo usa #if ZWCAD para compilar solo cuando se
// referencia la DLL de ZWCAD. En el .csproj del Plugin se
// define la constante según la plataforma objetivo.

using System;
using System.Collections.Generic;
using System.IO;
using CatastroTools.CAD.Interfaces;
using CatastroTools.Core.Models;

#if ZWCAD
using ZwSoft.ZwCAD.ApplicationServices;
using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.EditorInput;
using ZwSoft.ZwCAD.Geometry;
using ZwSoft.ZwCAD.Runtime;
using AcApp   = ZwSoft.ZwCAD.ApplicationServices.Application;
using AcDb    = ZwSoft.ZwCAD.DatabaseServices;
using AcGe    = ZwSoft.ZwCAD.Geometry;
using AcEd    = ZwSoft.ZwCAD.EditorInput;
using AcColor = ZwSoft.ZwCAD.Colors;
#endif

namespace CatastroTools.CAD.ZwCAD
{
#if ZWCAD
    public class ZwPlatform : ICadPlatform
    {
        // ─── INFO ────────────────────────────────────────────────
        public string NombrePlataforma => "ZWCAD";
        public string Version
        {
            get
            {
                try { return AcApp.Version.ToString(); }
                catch { return "Desconocida"; }
            }
        }

        // ─── ACCESO A OBJETOS PRINCIPALES ────────────────────────
        private Document Doc => AcApp.DocumentManager.MdiActiveDocument;
        private Database Db  => Doc.Database;
        private Editor   Ed  => Doc.Editor;

        // ─── CAPAS ───────────────────────────────────────────────
        public bool ExisteCapa(string nombre)
        {
            using var tr = Db.TransactionManager.StartTransaction();
            var lt = (LayerTable)tr.GetObject(Db.LayerTableId, OpenMode.ForRead);
            return lt.Has(nombre);
        }

        public void CrearCapa(string nombre, int colorAci, string tipLinea = "Continuous")
        {
            using var tr = Db.TransactionManager.StartTransaction();
            var lt = (LayerTable)tr.GetObject(Db.LayerTableId, OpenMode.ForWrite);
            if (!lt.Has(nombre))
            {
                var lr = new LayerTableRecord
                {
                    Name  = nombre,
                    Color = AcColor.Color.FromColorIndex( AcColor.ColorMethod.ByAci, (short)colorAci) 
                };
                // Asignar tipo de línea si no es Continuous
                if (tipLinea != "Continuous")
                {
                    var ltt = (LinetypeTable)tr.GetObject(Db.LinetypeTableId, OpenMode.ForRead);
                    if (!ltt.Has(tipLinea))
                        Db.LoadLineTypeFile(tipLinea, "acad.lin");
                    if (ltt.Has(tipLinea))
                        lr.LinetypeObjectId = ltt[tipLinea];
                }
                lt.Add(lr);
                tr.AddNewlyCreatedDBObject(lr, true);
            }
            tr.Commit();
        }

        public void SetCapaActual(string nombre)
        {
            if (!ExisteCapa(nombre)) CrearCapa(nombre, 7);
            using var tr = Db.TransactionManager.StartTransaction();
            var lt = (LayerTable)tr.GetObject(Db.LayerTableId, OpenMode.ForRead);
            Db.Clayer = lt[nombre];
            tr.Commit();
        }

        // ─── DIBUJO ──────────────────────────────────────────────
        private ObjectId EspacioModelo(Transaction tr)
        {
            var bt = (BlockTable)tr.GetObject(Db.BlockTableId, OpenMode.ForRead);
            return bt[BlockTableRecord.ModelSpace];
        }

        public long DibujarPolilineaCerrada(IList<Punto2D> puntos, string capa)
            => DibujarPolilinea(puntos, capa, cerrada: true);

        public long DibujarPolilineaAbierta(IList<Punto2D> puntos, string capa)
            => DibujarPolilinea(puntos, capa, cerrada: false);

        private long DibujarPolilinea(IList<Punto2D> puntos, string capa, bool cerrada)
        {
            if (!ExisteCapa(capa)) CrearCapa(capa, 7);
            using var tr = Db.TransactionManager.StartTransaction();
            var ms = (BlockTableRecord)tr.GetObject(EspacioModelo(tr), OpenMode.ForWrite);
            var pl = new Polyline();
            for (int i = 0; i < puntos.Count; i++)
                pl.AddVertexAt(i, new Point2d(puntos[i].X, puntos[i].Y), 0, 0, 0);
            pl.Closed = cerrada;
            pl.Layer  = capa;
            ms.AppendEntity(pl);
            tr.AddNewlyCreatedDBObject(pl, true);
            tr.Commit();
            return pl.ObjectId.OldIdPtr.ToInt64();
        }

        public long DibujarLinea(Punto2D p1, Punto2D p2, string capa)
        {
            if (!ExisteCapa(capa)) CrearCapa(capa, 7);
            using var tr = Db.TransactionManager.StartTransaction();
            var ms = (BlockTableRecord)tr.GetObject(EspacioModelo(tr), OpenMode.ForWrite);
            var ln = new Line(
                new Point3d(p1.X, p1.Y, 0),
                new Point3d(p2.X, p2.Y, 0)) { Layer = capa };
            ms.AppendEntity(ln);
            tr.AddNewlyCreatedDBObject(ln, true);
            tr.Commit();
            return ln.ObjectId.OldIdPtr.ToInt64();
        }

        public long DibujarCirculo(Punto2D centro, double radio, string capa)
        {
            if (!ExisteCapa(capa)) CrearCapa(capa, 7);
            using var tr = Db.TransactionManager.StartTransaction();
            var ms = (BlockTableRecord)tr.GetObject(EspacioModelo(tr), OpenMode.ForWrite);
            var c = new Circle(new Point3d(centro.X, centro.Y, 0),
                               Vector3d.ZAxis, radio) { Layer = capa };
            ms.AppendEntity(c);
            tr.AddNewlyCreatedDBObject(c, true);
            tr.Commit();
            return c.ObjectId.OldIdPtr.ToInt64();
        }

        public long DibujarArco(Punto2D centro, double radio,
                                double angIni, double angFin, string capa)
        {
            if (!ExisteCapa(capa)) CrearCapa(capa, 7);
            using var tr = Db.TransactionManager.StartTransaction();
            var ms = (BlockTableRecord)tr.GetObject(EspacioModelo(tr), OpenMode.ForWrite);
            var a = new Arc(new Point3d(centro.X, centro.Y, 0),
                            radio, angIni, angFin) { Layer = capa };
            ms.AppendEntity(a);
            tr.AddNewlyCreatedDBObject(a, true);
            tr.Commit();
            return a.ObjectId.OldIdPtr.ToInt64();
        }

        // ─── TEXTO ───────────────────────────────────────────────
        public long InsertarTexto(Punto2D punto, string texto, double altura,
                                  double angulo, string capa, TextoJustif justif = TextoJustif.MC)
        {
            if (!ExisteCapa(capa)) CrearCapa(capa, 7);
            using var tr = Db.TransactionManager.StartTransaction();
            var ms = (BlockTableRecord)tr.GetObject(EspacioModelo(tr), OpenMode.ForWrite);
            var txt = new DBText
            {
                TextString  = texto,
                Height      = altura,
                Rotation    = angulo,
                Layer       = capa,
                Position    = new Point3d(punto.X, punto.Y, 0),
                Justify     = MapJustif(justif),
                AlignmentPoint = new Point3d(punto.X, punto.Y, 0)
            };
            ms.AppendEntity(txt);
            tr.AddNewlyCreatedDBObject(txt, true);
            tr.Commit();
            return txt.ObjectId.OldIdPtr.ToInt64();
        }

        public long InsertarMTexto(Punto2D punto, string texto,
                                   double altura, double ancho, string capa)
        {
            if (!ExisteCapa(capa)) CrearCapa(capa, 7);
            using var tr = Db.TransactionManager.StartTransaction();
            var ms = (BlockTableRecord)tr.GetObject(EspacioModelo(tr), OpenMode.ForWrite);
            var mt = new MText
            {
                Contents  = texto,
                TextHeight = altura,
                Width     = ancho,
                Layer     = capa,
                Location  = new Point3d(punto.X, punto.Y, 0)
            };
            ms.AppendEntity(mt);
            tr.AddNewlyCreatedDBObject(mt, true);
            tr.Commit();
            return mt.ObjectId.OldIdPtr.ToInt64();
        }

        private AttachmentPoint MapJustif(TextoJustif j)
        {
            if (j == TextoJustif.ML) return AttachmentPoint.MiddleLeft;
            if (j == TextoJustif.MR) return AttachmentPoint.MiddleRight;
            if (j == TextoJustif.TL) return AttachmentPoint.TopLeft;
            if (j == TextoJustif.TC) return AttachmentPoint.TopCenter;
            if (j == TextoJustif.BL) return AttachmentPoint.BottomLeft;
            if (j == TextoJustif.BC) return AttachmentPoint.BottomCenter;
            return AttachmentPoint.MiddleCenter;
        }
        // ─── ACOTACIONES ─────────────────────────────────────────
        public long AcotarAlineada(Punto2D p1, Punto2D p2, Punto2D posTexto, string capa)
        {
            if (!ExisteCapa(capa)) CrearCapa(capa, 7);
            using var tr = Db.TransactionManager.StartTransaction();
            var ms = (BlockTableRecord)tr.GetObject(EspacioModelo(tr), OpenMode.ForWrite);
            var dim = new AlignedDimension(
                new Point3d(p1.X, p1.Y, 0),
                new Point3d(p2.X, p2.Y, 0),
                new Point3d(posTexto.X, posTexto.Y, 0),
                null, ObjectId.Null)
            { Layer = capa };
            ms.AppendEntity(dim);
            tr.AddNewlyCreatedDBObject(dim, true);
            tr.Commit();
            return dim.ObjectId.OldIdPtr.ToInt64();
        }

        public long AcotarLineal(Punto2D p1, Punto2D p2, Punto2D posTexto,
                                 string capa, bool horizontal = true)
        {
            if (!ExisteCapa(capa)) CrearCapa(capa, 7);
            using var tr = Db.TransactionManager.StartTransaction();
            var ms = (BlockTableRecord)tr.GetObject(EspacioModelo(tr), OpenMode.ForWrite);
            var dim = new RotatedDimension(
                horizontal ? 0 : Math.PI / 2,
                new Point3d(p1.X, p1.Y, 0),
                new Point3d(p2.X, p2.Y, 0),
                new Point3d(posTexto.X, posTexto.Y, 0),
                null, ObjectId.Null)
            { Layer = capa };
            ms.AppendEntity(dim);
            tr.AddNewlyCreatedDBObject(dim, true);
            tr.Commit();
            return dim.ObjectId.OldIdPtr.ToInt64();
        }

        // ─── LECTURA ─────────────────────────────────────────────
        public List<Punto2D> ObtenerVerticesPolilinea(long entityId)
        {
            var result = new List<Punto2D>();
            using var tr = Db.TransactionManager.StartTransaction();
            try
            {
                var oid = new ObjectId(new IntPtr(entityId));
                var pl  = tr.GetObject(oid, OpenMode.ForRead) as Polyline;
                if (pl == null) return result;
                for (int i = 0; i < pl.NumberOfVertices; i++)
                {
                    var p = pl.GetPoint2dAt(i);
                    result.Add(new Punto2D(p.X, p.Y));
                }
            }
            catch { /* entidad no encontrada */ }
            return result;
        }

        public List<Punto2D> ObtenerVerticesPolilinea(string handle)
        {
            using var tr = Db.TransactionManager.StartTransaction();
            var oid = Db.GetObjectId(false, new Handle(Convert.ToInt64(handle, 16)), 0);
            return ObtenerVerticesPolilinea(oid.OldIdPtr.ToInt64());
        }

        public bool EsPolilineaCerrada(long entityId)
        {
            using var tr = Db.TransactionManager.StartTransaction();
            try
            {
                var oid = new ObjectId(new IntPtr(entityId));
                var pl  = tr.GetObject(oid, OpenMode.ForRead) as Polyline;
                return pl?.Closed ?? false;
            }
            catch { return false; }
        }

        public TipoEntidad ObtenerTipo(long entityId)
        {
            using var tr = Db.TransactionManager.StartTransaction();
            try
            {
                var oid = new ObjectId(new IntPtr(entityId));
                var ent = tr.GetObject(oid, OpenMode.ForRead) as Entity;
                if (ent is Polyline) return TipoEntidad.Polilinea;
                if (ent is Line)     return TipoEntidad.Linea;
                if (ent is Circle)   return TipoEntidad.Circulo;
                if (ent is Arc)      return TipoEntidad.Arco;
                if (ent is DBText)   return TipoEntidad.Texto;
                if (ent is MText)    return TipoEntidad.MTexto;
                return TipoEntidad.Desconocido;
            }
            catch { return TipoEntidad.Desconocido; }
        }

        // ─── SELECCIÓN ───────────────────────────────────────────
        public long SeleccionarEntidad(string prompt)
        {
            var opts = new PromptEntityOptions($"\n{prompt}");
            var res  = Ed.GetEntity(opts);
            if (res.Status != PromptStatus.OK) return -1;
            return res.ObjectId.OldIdPtr.ToInt64();
        }

        public List<long> SeleccionarMultiple(string prompt, FiltroSeleccion filtro = null)
        {
            var result = new List<long>();
            var opts   = new PromptSelectionOptions { MessageForAdding = $"\n{prompt}" };
            SelectionFilter sf = null;

            if (filtro != null && filtro.TiposPermitidos.Count > 0)
            {
                var tipos = string.Join(",", filtro.TiposPermitidos);
                sf = new SelectionFilter(new[] {
                    new TypedValue((int)DxfCode.Start, tipos)
                });
            }

            var res = sf != null ? Ed.GetSelection(opts, sf) : Ed.GetSelection(opts);
            if (res.Status != PromptStatus.OK) return result;

            foreach (SelectedObject so in res.Value)
                if (so != null)
                    result.Add(so.ObjectId.OldIdPtr.ToInt64());
            return result;
        }

        public List<Punto2D> PedirPolilineaInteractiva(string prompt)
        {
            // Solicitar una polilínea dibujándola en tiempo real
            var puntos = new List<Punto2D>();
            Ed.WriteMessage($"\n{prompt}");
            Ed.WriteMessage("\n  (Enter para terminar el trazado)");

            var optPt = new PromptPointOptions("\n  >> Punto: ");
            while (true)
            {
                if (puntos.Count > 0)
                {
                    optPt.BasePoint = new Point3d(
                        puntos[puntos.Count - 1].X,
                        puntos[puntos.Count - 1].Y, 0);
                    optPt.UseBasePoint = true;
                }
                var res = Ed.GetPoint(optPt);
                if (res.Status != PromptStatus.OK) break;
                puntos.Add(new Punto2D(res.Value.X, res.Value.Y));
                optPt.Message = $"\n  >> Punto {puntos.Count + 1} (Enter=terminar): ";
            }
            return puntos;
        }

        public Punto2D PedirPunto(string prompt)
        {
            var res = Ed.GetPoint(new PromptPointOptions($"\n{prompt}"));
            if (res.Status != PromptStatus.OK)
                throw new OperationCanceledException("Operación cancelada por el usuario.");
            return new Punto2D(res.Value.X, res.Value.Y);
        }

        public Punto2D PedirPunto(string prompt, Punto2D basePoint)
        {
            var opts = new PromptPointOptions($"\n{prompt}")
            {
                BasePoint    = new Point3d(basePoint.X, basePoint.Y, 0),
                UseBasePoint = true
            };
            var res = Ed.GetPoint(opts);
            if (res.Status != PromptStatus.OK)
                throw new OperationCanceledException();
            return new Punto2D(res.Value.X, res.Value.Y);
        }

        public double? PedirReal(string prompt, double? defaultVal = null)
        {
            var opts = new PromptDoubleOptions($"\n{prompt}");
            if (defaultVal.HasValue)
            {
                opts.DefaultValue = defaultVal.Value;
                opts.UseDefaultValue = true;
            }
            var res = Ed.GetDouble(opts);
            return res.Status == PromptStatus.OK ? res.Value : (double?)null;
        }

        public int? PedirEntero(string prompt, int? defaultVal = null)
        {
            var opts = new PromptIntegerOptions($"\n{prompt}");
            if (defaultVal.HasValue)
            {
                opts.DefaultValue = defaultVal.Value;
                opts.UseDefaultValue = true;
            }
            var res = Ed.GetInteger(opts);
            return res.Status == PromptStatus.OK ? res.Value : (int?)null;
        }

        public string PedirTexto(string prompt, string defaultVal = "")
        {
            var opts = new PromptStringOptions($"\n{prompt}")
            {
                DefaultValue   = defaultVal,
                UseDefaultValue = !string.IsNullOrEmpty(defaultVal),
                AllowSpaces    = true
            };
            var res = Ed.GetString(opts);
            return res.Status == PromptStatus.OK ? res.StringResult : defaultVal;
        }

        // ─── TRANSACCIÓN ─────────────────────────────────────────
        public IDisposableTransaction IniciarTransaccion()
            => new ZwTransaction(Db.TransactionManager.StartTransaction());

        // ─── UTILIDADES ──────────────────────────────────────────
        public void Purgar()
            => Doc.SendStringToExecute("._PURGE _ALL * _No ", true, false, false);

        public void Zoom(string opcion = "E")
            => Doc.SendStringToExecute($"._ZOOM _{opcion} ", true, false, false);

        public void Regen()
            => Doc.SendStringToExecute("._REGEN ", true, false, false);

        public string RutaDWGActual => Db.Filename ?? "";
        public string DirectorioDWG => Path.GetDirectoryName(RutaDWGActual) ?? "";
        public string NombreDWG     => Path.GetFileNameWithoutExtension(RutaDWGActual) ?? "catastro";

        public void MensajeConsola(string texto) => Ed.WriteMessage($"\n[CT] {texto}");
        public void MensajeError(string texto)   => Ed.WriteMessage($"\n[CT] ⚠ {texto}");

        // ─── TRANSACCIÓN INTERNA ─────────────────────────────────
        private class ZwTransaction : IDisposableTransaction
        {
            private readonly Transaction _tr;
            public ZwTransaction(Transaction tr) { _tr = tr; }
            public void Commit() => _tr.Commit();
            public void Abort()  => _tr.Abort();
            public void Dispose() => _tr.Dispose();
        }
    }
#endif
                }
