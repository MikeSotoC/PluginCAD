// ============================================================
// CatastroTools.CAD — AcPlatform.cs
// Implementación de ICadPlatform para AutoCAD 2019+
//
// Referencias requeridas (agregar manualmente en VS2019):
//   C:\Program Files\Autodesk\AutoCAD <version>\acdbmgd.dll
//   acmgd.dll
//   accoremgd.dll
// ============================================================

using System;
using System.Collections.Generic;
using System.IO;
using CatastroTools.CAD.Interfaces;
using CatastroTools.Core.Models;

#if AUTOCAD
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using AcApp = Autodesk.AutoCAD.ApplicationServices.Application;
using AcDb  = Autodesk.AutoCAD.DatabaseServices;
using AcGe  = Autodesk.AutoCAD.Geometry;
using AcEd  = Autodesk.AutoCAD.EditorInput;
#endif

namespace CatastroTools.CAD.AutoCAD
{
#if AUTOCAD
    /// <summary>
    /// La API de AutoCAD ObjectARX .NET es prácticamente idéntica
    /// a la de ZWCAD — solo cambian los namespaces.
    /// Esta clase es un espejo de ZwPlatform con Autodesk.* en lugar de ZwCAD.*
    /// </summary>
    public class AcPlatform : ICadPlatform
    {
        public string NombrePlataforma => "AutoCAD";
        public string Version
        {
            get { try { return AcApp.Version.ToString(); } catch { return "Desconocida"; } }
        }

        private Document Doc => AcApp.DocumentManager.MdiActiveDocument;
        private Database Db  => Doc.Database;
        private Editor   Ed  => Doc.Editor;

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
                    Color = AcDb.Color.FromColorIndex(ColorMethod.ByAci, (short)colorAci)
                };
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

        private ObjectId EspacioModelo(Transaction tr)
        {
            var bt = (BlockTable)tr.GetObject(Db.BlockTableId, OpenMode.ForRead);
            return bt[BlockTableRecord.ModelSpace];
        }

        public long DibujarPolilineaCerrada(IList<Punto2D> puntos, string capa)
            => DibujarPolilinea(puntos, capa, true);

        public long DibujarPolilineaAbierta(IList<Punto2D> puntos, string capa)
            => DibujarPolilinea(puntos, capa, false);

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
            return pl.ObjectId.Handle.Value;
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
            return ln.ObjectId.Handle.Value;
        }

        public long DibujarCirculo(Punto2D centro, double radio, string capa)
        {
            if (!ExisteCapa(capa)) CrearCapa(capa, 7);
            using var tr = Db.TransactionManager.StartTransaction();
            var ms = (BlockTableRecord)tr.GetObject(EspacioModelo(tr), OpenMode.ForWrite);
            var c = new Circle(
                new Point3d(centro.X, centro.Y, 0), Vector3d.ZAxis, radio) { Layer = capa };
            ms.AppendEntity(c);
            tr.AddNewlyCreatedDBObject(c, true);
            tr.Commit();
            return c.ObjectId.Handle.Value;
        }

        public long DibujarArco(Punto2D centro, double radio,
                                double angIni, double angFin, string capa)
        {
            if (!ExisteCapa(capa)) CrearCapa(capa, 7);
            using var tr = Db.TransactionManager.StartTransaction();
            var ms = (BlockTableRecord)tr.GetObject(EspacioModelo(tr), OpenMode.ForWrite);
            var a = new Arc(
                new Point3d(centro.X, centro.Y, 0), radio, angIni, angFin) { Layer = capa };
            ms.AppendEntity(a);
            tr.AddNewlyCreatedDBObject(a, true);
            tr.Commit();
            return a.ObjectId.Handle.Value;
        }

        public long InsertarTexto(Punto2D punto, string texto, double altura,
                                  double angulo, string capa, TextoJustif justif = TextoJustif.MC)
        {
            if (!ExisteCapa(capa)) CrearCapa(capa, 7);
            using var tr = Db.TransactionManager.StartTransaction();
            var ms = (BlockTableRecord)tr.GetObject(EspacioModelo(tr), OpenMode.ForWrite);
            var txt = new DBText
            {
                TextString     = texto,
                Height         = altura,
                Rotation       = angulo,
                Layer          = capa,
                Position       = new Point3d(punto.X, punto.Y, 0),
                Justify        = MapJustif(justif),
                AlignmentPoint = new Point3d(punto.X, punto.Y, 0)
            };
            ms.AppendEntity(txt);
            tr.AddNewlyCreatedDBObject(txt, true);
            tr.Commit();
            return txt.ObjectId.Handle.Value;
        }

        public long InsertarMTexto(Punto2D punto, string texto,
                                   double altura, double ancho, string capa)
        {
            if (!ExisteCapa(capa)) CrearCapa(capa, 7);
            using var tr = Db.TransactionManager.StartTransaction();
            var ms = (BlockTableRecord)tr.GetObject(EspacioModelo(tr), OpenMode.ForWrite);
            var mt = new MText
            {
                Contents   = texto,
                TextHeight = altura,
                Width      = ancho,
                Layer      = capa,
                Location   = new Point3d(punto.X, punto.Y, 0)
            };
            ms.AppendEntity(mt);
            tr.AddNewlyCreatedDBObject(mt, true);
            tr.Commit();
            return mt.ObjectId.Handle.Value;
        }

        private AttachmentPoint MapJustif(TextoJustif j)
        {
            switch (j)
            {
                case TextoJustif.ML: return AttachmentPoint.MiddleLeft;
                case TextoJustif.MR: return AttachmentPoint.MiddleRight;
                case TextoJustif.TL: return AttachmentPoint.TopLeft;
                case TextoJustif.TC: return AttachmentPoint.TopCenter;
                case TextoJustif.BL: return AttachmentPoint.BottomLeft;
                case TextoJustif.BC: return AttachmentPoint.BottomCenter;
                default:             return AttachmentPoint.MiddleCenter;
            }
        }

        public long AcotarAlineada(Punto2D p1, Punto2D p2, Punto2D pos, string capa)
        {
            if (!ExisteCapa(capa)) CrearCapa(capa, 7);
            using var tr = Db.TransactionManager.StartTransaction();
            var ms = (BlockTableRecord)tr.GetObject(EspacioModelo(tr), OpenMode.ForWrite);
            var dim = new AlignedDimension(
                new Point3d(p1.X, p1.Y, 0),
                new Point3d(p2.X, p2.Y, 0),
                new Point3d(pos.X, pos.Y, 0),
                null, ObjectId.Null) { Layer = capa };
            ms.AppendEntity(dim);
            tr.AddNewlyCreatedDBObject(dim, true);
            tr.Commit();
            return dim.ObjectId.Handle.Value;
        }

        public long AcotarLineal(Punto2D p1, Punto2D p2, Punto2D pos,
                                 string capa, bool horizontal = true)
        {
            if (!ExisteCapa(capa)) CrearCapa(capa, 7);
            using var tr = Db.TransactionManager.StartTransaction();
            var ms = (BlockTableRecord)tr.GetObject(EspacioModelo(tr), OpenMode.ForWrite);
            var dim = new RotatedDimension(
                horizontal ? 0 : Math.PI / 2,
                new Point3d(p1.X, p1.Y, 0),
                new Point3d(p2.X, p2.Y, 0),
                new Point3d(pos.X, pos.Y, 0),
                null, ObjectId.Null) { Layer = capa };
            ms.AppendEntity(dim);
            tr.AddNewlyCreatedDBObject(dim, true);
            tr.Commit();
            return dim.ObjectId.Handle.Value;
        }

        public List<Punto2D> ObtenerVerticesPolilinea(long entityId)
        {
            var result = new List<Punto2D>();
            using var tr = Db.TransactionManager.StartTransaction();
            try
            {
                var oid = Db.GetObjectId(false, new Handle(entityId), 0);
                var pl  = tr.GetObject(oid, OpenMode.ForRead) as Polyline;
                if (pl == null) return result;
                for (int i = 0; i < pl.NumberOfVertices; i++)
                {
                    var p = pl.GetPoint2dAt(i);
                    result.Add(new Punto2D(p.X, p.Y));
                }
            }
            catch { }
            return result;
        }

        public List<Punto2D> ObtenerVerticesPolilinea(string handle)
        {
            var oid = Db.GetObjectId(false,
                new Handle(Convert.ToInt64(handle, 16)), 0);
            return ObtenerVerticesPolilinea(oid.Handle.Value);
        }

        public bool EsPolilineaCerrada(long entityId)
        {
            using var tr = Db.TransactionManager.StartTransaction();
            try
            {
                var oid = Db.GetObjectId(false, new Handle(entityId), 0);
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
                var oid = Db.GetObjectId(false, new Handle(entityId), 0);
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

        public long SeleccionarEntidad(string prompt)
        {
            var res = Ed.GetEntity(new PromptEntityOptions($"\n{prompt}"));
            if (res.Status != PromptStatus.OK) return -1;
            return res.ObjectId.Handle.Value;
        }

        public List<long> SeleccionarMultiple(string prompt, FiltroSeleccion filtro = null)
        {
            var result = new List<long>();
            var opts   = new PromptSelectionOptions { MessageForAdding = $"\n{prompt}" };
            SelectionFilter sf = null;
            if (filtro?.TiposPermitidos?.Count > 0)
            {
                sf = new SelectionFilter(new[] {
                    new TypedValue((int)DxfCode.Start,
                        string.Join(",", filtro.TiposPermitidos))
                });
            }
            var res = sf != null ? Ed.GetSelection(opts, sf) : Ed.GetSelection(opts);
            if (res.Status != PromptStatus.OK) return result;
            foreach (SelectedObject so in res.Value)
                if (so != null) result.Add(so.ObjectId.Handle.Value);
            return result;
        }

        public List<Punto2D> PedirPolilineaInteractiva(string prompt)
        {
            var puntos = new List<Punto2D>();
            Ed.WriteMessage($"\n{prompt}\n  (Enter para terminar)");
            var optPt = new PromptPointOptions("\n  >> Punto: ");
            while (true)
            {
                if (puntos.Count > 0)
                {
                    var last = puntos[puntos.Count - 1];
                    optPt.BasePoint    = new Point3d(last.X, last.Y, 0);
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
            if (res.Status != PromptStatus.OK) throw new OperationCanceledException();
            return new Punto2D(res.Value.X, res.Value.Y);
        }

        public Punto2D PedirPunto(string prompt, Punto2D basePoint)
        {
            var opts = new PromptPointOptions($"\n{prompt}")
            {
                BasePoint = new Point3d(basePoint.X, basePoint.Y, 0),
                UseBasePoint = true
            };
            var res = Ed.GetPoint(opts);
            if (res.Status != PromptStatus.OK) throw new OperationCanceledException();
            return new Punto2D(res.Value.X, res.Value.Y);
        }

        public double? PedirReal(string prompt, double? def = null)
        {
            var opts = new PromptDoubleOptions($"\n{prompt}");
            if (def.HasValue) { opts.DefaultValue = def.Value; opts.UseDefaultValue = true; }
            var res = Ed.GetDouble(opts);
            return res.Status == PromptStatus.OK ? res.Value : (double?)null;
        }

        public int? PedirEntero(string prompt, int? def = null)
        {
            var opts = new PromptIntegerOptions($"\n{prompt}");
            if (def.HasValue) { opts.DefaultValue = def.Value; opts.UseDefaultValue = true; }
            var res = Ed.GetInteger(opts);
            return res.Status == PromptStatus.OK ? res.Value : (int?)null;
        }

        public string PedirTexto(string prompt, string def = "")
        {
            var opts = new PromptStringOptions($"\n{prompt}")
            {
                DefaultValue    = def,
                UseDefaultValue = !string.IsNullOrEmpty(def),
                AllowSpaces     = true
            };
            var res = Ed.GetString(opts);
            return res.Status == PromptStatus.OK ? res.StringResult : def;
        }

        public IDisposableTransaction IniciarTransaccion()
            => new AcTransaction(Db.TransactionManager.StartTransaction());

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

        private class AcTransaction : IDisposableTransaction
        {
            private readonly Transaction _tr;
            public AcTransaction(Transaction tr) { _tr = tr; }
            public void Commit()  => _tr.Commit();
            public void Abort()   => _tr.Abort();
            public void Dispose() => _tr.Dispose();
        }
    }
#endif
}
