using CatastroTools.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CatastroTools.Core.Models
{
    // ─── PUNTO 2D ────────────────────────────────────────────────
    public struct Punto2D
    {
        public double X { get; }
        public double Y { get; }

        public Punto2D(double x, double y) { X = x; Y = y; }

        public double DistanciaA(Punto2D otro) =>
            Math.Sqrt(Math.Pow(otro.X - X, 2) + Math.Pow(otro.Y - Y, 2));

        public Punto2D PuntoMedio(Punto2D otro) =>
            new Punto2D((X + otro.X) / 2.0, (Y + otro.Y) / 2.0);

        public double AnguloA(Punto2D otro) =>
            Math.Atan2(otro.Y - Y, otro.X - X);

        public override string ToString() => $"({X:F4}, {Y:F4})";

        public static Punto2D operator +(Punto2D a, Punto2D b) =>
            new Punto2D(a.X + b.X, a.Y + b.Y);
        public static Punto2D operator -(Punto2D a, Punto2D b) =>
            new Punto2D(a.X - b.X, a.Y - b.Y);
        public static Punto2D operator *(Punto2D p, double s) =>
            new Punto2D(p.X * s, p.Y * s);
    }

    // ─── SEGMENTO ────────────────────────────────────────────────
    public class Segmento
    {
        public Punto2D P1 { get; }
        public Punto2D P2 { get; }

        public Segmento(Punto2D p1, Punto2D p2) { P1 = p1; P2 = p2; }

        public double Longitud => P1.DistanciaA(P2);
        public Punto2D PuntoMedio => P1.PuntoMedio(P2);
        public double Angulo => P1.AnguloA(P2);

        /// Rumbo geográfico: "N 45°30'20\" E"
        public string Rumbo
        {
            get
            {
                double deg = Angulo * 180.0 / Math.PI;
                while (deg < 0) deg += 360;
                while (deg >= 360) deg -= 360;

                int d = (int)deg;
                int m = (int)((deg - d) * 60);
                int s = (int)(((deg - d) * 60 - m) * 60);

                if (deg < 90)  return $"N {d}°{m:D2}'{s:D2}\" E";
                if (deg < 180) return $"S {180 - d}°{m:D2}'{s:D2}\" E";
                if (deg < 270) return $"S {d - 180}°{m:D2}'{s:D2}\" O";
                return $"N {360 - d}°{m:D2}'{s:D2}\" O";
            }
        }

        /// Distancia de un punto al segmento
        public double DistanciaPunto(Punto2D pt)
        {
            double dx = P2.X - P1.X, dy = P2.Y - P1.Y;
            double len2 = dx * dx + dy * dy;
            if (len2 < 1e-10) return pt.DistanciaA(P1);
            double t = Math.Max(0, Math.Min(1,
                ((pt.X - P1.X) * dx + (pt.Y - P1.Y) * dy) / len2));
            return pt.DistanciaA(new Punto2D(P1.X + t * dx, P1.Y + t * dy));
        }

        /// Intersección con otro segmento — devuelve null si no hay
        public Punto2D? Interseccion(Segmento otro)
        {
            double x1 = P1.X, y1 = P1.Y, x2 = P2.X, y2 = P2.Y;
            double x3 = otro.P1.X, y3 = otro.P1.Y, x4 = otro.P2.X, y4 = otro.P2.Y;
            double denom = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
            if (Math.Abs(denom) < 1e-10) return null;
            double t = ((x1 - x3) * (y3 - y4) - (y1 - y3) * (x3 - x4)) / denom;
            double u = -((x1 - x2) * (y1 - y3) - (y1 - y2) * (x1 - x3)) / denom;
            if (t >= -1e-10 && t <= 1 + 1e-10 && u >= -1e-10 && u <= 1 + 1e-10)
                return new Punto2D(x1 + t * (x2 - x1), y1 + t * (y2 - y1));
            return null;
        }
    }

    // ─── POLÍGONO ────────────────────────────────────────────────
    public class Poligono
    {
        public List<Punto2D> Vertices { get; }

        public Poligono(IEnumerable<Punto2D> vertices)
        {
            Vertices = new List<Punto2D>(vertices);
        }

        public int NumVertices => Vertices.Count;

        /// Área por fórmula de Gauss (Shoelace)
        public double Area
        {
            get
            {
                int n = Vertices.Count;
                double area = 0;
                for (int i = 0; i < n; i++)
                {
                    var p1 = Vertices[i];
                    var p2 = Vertices[(i + 1) % n];
                    area += (p1.X * p2.Y) - (p2.X * p1.Y);
                }
                return Math.Abs(area) / 2.0;
            }
        }

        /// Perímetro
        public double Perimetro
        {
            get
            {
                int n = Vertices.Count;
                double p = 0;
                for (int i = 0; i < n; i++)
                    p += Vertices[i].DistanciaA(Vertices[(i + 1) % n]);
                return p;
            }
        }

        /// Centroide
        public Punto2D Centroide
        {
            get
            {
                double sx = Vertices.Sum(v => v.X);
                double sy = Vertices.Sum(v => v.Y);
                return new Punto2D(sx / Vertices.Count, sy / Vertices.Count);
            }
        }

        /// Segmentos del polígono
        public IEnumerable<Segmento> Segmentos
        {
            get
            {
                int n = Vertices.Count;
                for (int i = 0; i < n; i++)
                    yield return new Segmento(Vertices[i], Vertices[(i + 1) % n]);
            }
        }

        /// Ray-casting: ¿punto dentro del polígono?
        public bool ContienePunto(Punto2D pt)
        {
            int n = Vertices.Count;
            bool inside = false;
            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                var vi = Vertices[i]; var vj = Vertices[j];
                if (vi.Y != vj.Y &&
                    (vi.Y > pt.Y) != (vj.Y > pt.Y) &&
                    pt.X < (vj.X - vi.X) * (pt.Y - vi.Y) / (vj.Y - vi.Y) + vi.X)
                    inside = !inside;
            }
            return inside;
        }

        /// Bounding box
        public (Punto2D Min, Punto2D Max) BoundingBox
        {
            get
            {
                double minX = Vertices.Min(v => v.X), minY = Vertices.Min(v => v.Y);
                double maxX = Vertices.Max(v => v.X), maxY = Vertices.Max(v => v.Y);
                return (new Punto2D(minX, minY), new Punto2D(maxX, maxY));
            }
        }

        /// Verifica si dos polígonos comparten un segmento (adyacentes)
        public Segmento SegmentoCompartido(Poligono otro, double tol = 0.05)
        {
            foreach (var sa in Segmentos)
            foreach (var sb in otro.Segmentos)
            {
                bool match1 = sa.P1.DistanciaA(sb.P1) < tol && sa.P2.DistanciaA(sb.P2) < tol;
                bool match2 = sa.P1.DistanciaA(sb.P2) < tol && sa.P2.DistanciaA(sb.P1) < tol;
                if (match1 || match2) return sa;
            }
            return null;
        }
    }

    // ─── SISTEMA DE COORDENADAS ──────────────────────────────────
    public enum ZonaUTM { Auto, Zona17S, Zona18S, Local }

    public static class CoordUtils
    {
        public static ZonaUTM DetectarZona(IEnumerable<Punto2D> puntos)
        {
            var lista = puntos.ToList();
            if (!lista.Any()) return ZonaUTM.Local;
            double x = lista.Average(p => p.X);
            if (x < 100000) return ZonaUTM.Local;
            return x < 500000 ? ZonaUTM.Zona17S : ZonaUTM.Zona18S;
        }

        public static string NombreZona(ZonaUTM zona)
        {
            if (zona == ZonaUTM.Zona17S) return "17S";
            if (zona == ZonaUTM.Zona18S) return "18S";
            return "LOCAL";
        }
    }

    // ─── ENTIDADES CATASTRALES ───────────────────────────────────

    public enum TipoVia
    {
        AvenidaPrincipal,
        AvenidaSecundaria,
        Calle,
        PasajeVehicular,
        PasajePeatonal,
        ViaColectora,
        ViaLocal,
        Personalizado
    }

    public class Via
    {
        public string Nombre { get; set; }
        public TipoVia Tipo { get; set; }
        public double Ancho { get; set; }
        public double AnchoCalzada { get; set; }
        public double AnchoVereda { get; set; }
        public double AnchoBerma { get; set; }
        public List<Punto2D> Eje { get; set; } = new List<Punto2D>();

        /// Anchos estándar RNE GH.020
        public static double AnchoEstandar(TipoVia tipo)
        {
            if (tipo == TipoVia.AvenidaPrincipal)  return 22.0;
            if (tipo == TipoVia.AvenidaSecundaria) return 18.0;
            if (tipo == TipoVia.ViaColectora)      return 15.0;
            if (tipo == TipoVia.Calle)             return 8.0;
            if (tipo == TipoVia.ViaLocal)          return 8.0;
            if (tipo == TipoVia.PasajeVehicular)   return 6.0;
            if (tipo == TipoVia.PasajePeatonal)    return 3.0;
            return 8.0;
        }

        public static string NombreTipo(TipoVia tipo)
        {
            if (tipo == TipoVia.AvenidaPrincipal)  return "Avenida Principal";
            if (tipo == TipoVia.AvenidaSecundaria) return "Avenida Secundaria";
            if (tipo == TipoVia.ViaColectora)      return "Vía Colectora";
            if (tipo == TipoVia.Calle)             return "Calle";
            if (tipo == TipoVia.ViaLocal)          return "Vía Local";
            if (tipo == TipoVia.PasajeVehicular)   return "Pasaje Vehicular";
            if (tipo == TipoVia.PasajePeatonal)    return "Pasaje Peatonal";
            return "Vía";
        }
    };
    

    public class Manzana
    {
        public string Nombre { get; set; }         // "MZ A", "MZ 1"
        public Poligono Poligono { get; set; }
        public List<Lote> Lotes { get; set; } = new List<Lote>();
        public string Habilitacion { get; set; }

        public double Area => Poligono?.Area ?? 0;
        public Punto2D Centroide => Poligono?.Centroide ?? new Punto2D();
    }

    public class Lote
    {
        public string Numero { get; set; }         // "1", "2A"
        public string NombreManzana { get; set; }  // "MZ A"
        public Poligono Poligono { get; set; }

        // Datos registrales SUNARP
        public string Propietario { get; set; }
        public string Dni { get; set; }
        public string Direccion { get; set; }
        public string Distrito { get; set; }
        public string Provincia { get; set; }
        public string Departamento { get; set; }
        public string PartidaRegistral { get; set; }
        public string Zonificacion { get; set; }
        public string Uso { get; set; }
        public string HabilitacionUrbana { get; set; }

        public double Area => Poligono?.Area ?? 0;
        public double Perimetro => Poligono?.Perimetro ?? 0;
        public Punto2D Centroide => Poligono?.Centroide ?? new Punto2D();

        // Colindancias detectadas por lado
        public List<Colindancia> Colindancias { get; set; } = new List<Colindancia>();
    }

    public class Colindancia
    {
        public Segmento Segmento { get; set; }
        public string Descripcion { get; set; }   // "Con Lote 2", "Vía Pública"
        public string NombreVia { get; set; }
        public ColindanciaTipo Tipo { get; set; }
    }

    public enum ColindanciaTipo { OtroLote, Via, LimiteExterno, OtroPrediO }

    public class PredioMatriz
    {
        public string Nombre { get; set; }
        public Poligono Poligono { get; set; }
        public List<Punto2D> VerticesUTM { get; set; } = new List<Punto2D>();
        public ZonaUTM Zona { get; set; }
        public List<Manzana> Manzanas { get; set; } = new List<Manzana>();
        public List<Via> Vias { get; set; } = new List<Via>();

        public double AreaTotal => Poligono?.Area ?? 0;
        public double AreaVias => Vias.Sum(v => 0.0); // calcular por geometría
        public double AreaManzanas => Manzanas.Sum(m => m.Area);
    }

    // ─── CONFIGURACIÓN DE TEXTO CATASTRAL ────────────────────────
    public class ConfigTexto
    {
        public double AlturaNumeroLote { get; set; } = 3.0;
        public double AlturaArea { get; set; } = 2.5;
        public double AlturaPropietario { get; set; } = 2.0;
        public double AlturaLindero { get; set; } = 1.8;
        public double AlturaVertice { get; set; } = 1.8;
        public double AlturaManzana { get; set; } = 4.0;
        public double OffsetLindero { get; set; } = 2.5;
        public double OffsetVerticeX { get; set; } = 2.0;
        public double OffsetVerticeY { get; set; } = 1.0;
        public bool MostrarRumbo { get; set; } = true;
        public bool MostrarPartida { get; set; } = true;
        public bool MostrarZonificacion { get; set; } = false;
        public string PrefijoVertice { get; set; } = "V-";
        public int NumVerticeInicial { get; set; } = 1;
        public int DecimalesUTM { get; set; } = 4;
        public int DecimalesArea { get; set; } = 2;
        public int DecimalesDistancia { get; set; } = 3;
    }

    // ─── NOMENCLATURA ────────────────────────────────────────────
    public enum SistemaNomenclatura { Alfabetico, Numerico, Personalizado }

    public static class Nomenclatura
    {
        public static string ManzanaAlfa(int index)
        {
            string result = "";
            index++;
            while (index > 0)
            {
                int r = (index - 1) % 26;
                result = (char)('A' + r) + result;
                index = (index - 1) / 26;
            }
            return "MZ " + result;
        }

        public static string ManzanaNum(int index, int inicio = 1) =>
            $"MZ {index + inicio}";

        public static string Lote(int index, int inicio, string prefijo = "Lote ") =>
            $"{prefijo}{index + inicio}";
    }
}