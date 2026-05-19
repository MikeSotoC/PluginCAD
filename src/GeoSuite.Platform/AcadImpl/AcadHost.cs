using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using GeoSuite.Core.Models;

namespace GeoSuite.Platform.AcadImpl;

public class AcadHost : ICadHost
{
    private readonly Document _doc;
    private readonly Editor _ed;
    private readonly Database _db;

    public AcadHost()
    {
        _doc = Application.DocumentManager.MdiActiveDocument ?? throw new InvalidOperationException("No document active");
        _db = _doc.Database;
        _ed = _doc.Editor;
    }

    public string ActiveDocumentName => _doc.Name;

    public void DrawPoint(Coordinate3 pt, string layer = "0")
    {
        using var tr = _db.TransactionManager.StartTransaction();
        EnsureLayerExists(layer, tr);
        _doc.CurrentLayer = GetLayerTableRecord(layer, tr);

        var dbPt = new Point3d(pt.X, pt.Y, pt.Z);
        var point = new DBPoint(dbPt);
        point.LayerId = GetLayerId(layer, tr);

        var bt = tr.GetObject(_db.BlockTableId, OpenMode.ForRead) as BlockTable;
        var btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
        btr.AppendEntity(point);
        tr.AddNewlyCreatedDBObject(point, true);
        tr.Commit();
    }

    public void DrawLine(Coordinate3 start, Coordinate3 end, string layer = "0")
    {
        using var tr = _db.TransactionManager.StartTransaction();
        EnsureLayerExists(layer, tr);

        var line = new Line(new Point3d(start.X, start.Y, start.Z), new Point3d(end.X, end.Y, end.Z));
        line.LayerId = GetLayerId(layer, tr);

        var bt = tr.GetObject(_db.BlockTableId, OpenMode.ForRead) as BlockTable;
        var btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
        btr.AppendEntity(line);
        tr.AddNewlyCreatedDBObject(line, true);
        tr.Commit();
    }

    public void DrawPolyline(List<Coordinate3> vertices, bool closed, string layer = "0")
    {
        if (vertices.Count < 2) return;

        using var tr = _db.TransactionManager.StartTransaction();
        EnsureLayerExists(layer, tr);

        var pline = new Polyline();
        for (int i = 0; i < vertices.Count; i++)
        {
            var v = vertices[i];
            pline.AddVertexAt(i, new Point2d(v.X, v.Y), 0, 0, 0);
        }
        pline.Closed = closed;
        pline.LayerId = GetLayerId(layer, tr);

        var bt = tr.GetObject(_db.BlockTableId, OpenMode.ForRead) as BlockTable;
        var btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
        btr.AppendEntity(pline);
        tr.AddNewlyCreatedDBObject(pline, true);
        tr.Commit();
    }

    public void DrawCircle(Coordinate3 center, double radius, string layer = "0")
    {
        using var tr = _db.TransactionManager.StartTransaction();
        EnsureLayerExists(layer, tr);

        var circle = new Circle(new Point3d(center.X, center.Y, center.Z), Vector3d.ZAxis, radius);
        circle.LayerId = GetLayerId(layer, tr);

        var bt = tr.GetObject(_db.BlockTableId, OpenMode.ForRead) as BlockTable;
        var btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
        btr.AppendEntity(circle);
        tr.AddNewlyCreatedDBObject(circle, true);
        tr.Commit();
    }

    public void AddText(string content, Coordinate3 location, double height, string layer = "TEXTOS")
    {
        using var tr = _db.TransactionManager.StartTransaction();
        EnsureLayerExists(layer, tr);

        var text = new DBText
        {
            Position = new Point3d(location.X, location.Y, location.Z),
            Height = height,
            TextString = content,
            LayerId = GetLayerId(layer, tr)
        };

        var bt = tr.GetObject(_db.BlockTableId, OpenMode.ForRead) as BlockTable;
        var btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
        btr.AppendEntity(text);
        tr.AddNewlyCreatedDBObject(text, true);
        tr.Commit();
    }

    public void SetCurrentLayer(string layerName)
    {
        using var tr = _db.TransactionManager.StartTransaction();
        _doc.CurrentLayer = GetLayerTableRecord(layerName, tr);
        tr.Commit();
    }

    public void CreateLayerIfNotExists(string name, string colorIndex = "7")
    {
        using var tr = _db.TransactionManager.StartTransaction();
        EnsureLayerExists(name, tr, colorIndex);
        tr.Commit();
    }

    public void ShowMessage(string title, string message)
    {
        Application.ShowAlertDialog($"{title}\n\n{message}");
    }

    public void SendCommand(string commandStr)
    {
        _ed.SendCommand(commandStr + "\n");
    }

    private void EnsureLayerExists(string name, Transaction tr, string colorIndex = "7")
    {
        var lt = tr.GetObject(_db.LayerTableId, OpenMode.ForRead) as LayerTable;
        if (!lt.Has(name))
        {
            lt.UpgradeOpen();
            var ltr = new LayerTableRecord { Name = name };
            var color = short.TryParse(colorIndex, out var c) ? Autodesk.AutoCAD.Colors.Color.FromColorIndex(Autodesk.AutoCAD.Colors.ColorMethod.ByAci, c) : Autodesk.AutoCAD.Colors.Color.White;
            ltr.Color = color;
            lt.Add(ltr);
            tr.AddNewlyCreatedDBObject(ltr, true);
        }
    }

    private ObjectId GetLayerId(string name, Transaction tr)
    {
        var lt = tr.GetObject(_db.LayerTableId, OpenMode.ForRead) as LayerTable;
        return lt[name];
    }

    private LayerTableRecord GetLayerTableRecord(string name, Transaction tr)
    {
        var lt = tr.GetObject(_db.LayerTableId, OpenMode.ForRead) as LayerTable;
        return tr.GetObject(lt[name], OpenMode.ForRead) as LayerTableRecord;
    }
}
