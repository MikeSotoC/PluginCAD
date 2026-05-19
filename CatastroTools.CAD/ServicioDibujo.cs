using System;
using System.Collections.Generic;
using System.Linq;
using CatastroTools.CAD.Interfaces;
using CatastroTools.Core.Models;

namespace CatastroTools.CAD
{
    /// <summary>
    /// Servicio principal de dibujo catastral.
    /// Toda la lógica de "qué dibujar y dónde" vive aquí.
    /// No sabe si es ZWCAD o AutoCAD — solo habla con ICadPlatform.
    /// </summary>
    public class ServicioDibujo
    {
        private readonly ICadPlatform _cad;
        private readonly ConfigTexto  _cfg;

        public ServicioDibujo(ICadPlatform cad, ConfigTexto cfg = null)
        {
            _cad = cad;
            _cfg = cfg ?? new ConfigTexto();
        }

        // ═══════════════════════════════════════════════════════
        // INICIALIZACIÓN
        // ═══════════════════════════════════════════════════════

        public void InicializarCapas()
        {
            CapasCatastro.InicializarTodas(_cad);
            _cad.MensajeConsola("Capas catastrales inicializadas.");
        }

        // ═══════════════════════════════════════════════════════
        // ETIQUETA DE LOTE (formato SUNARP/Municipalidad)
        // ═══════════════════════════════════════════════════════

        public void DibujarEtiquetaLote(Lote lote, ConfigTexto cfg = null)
        {
            var c   = cfg ?? _cfg;
            var cen = lote.Centroide;
            double h = c.AlturaNumeroLote;
            double li = h * 1.4; // interlineado

            // Número de lote
            _cad.InsertarTexto(
                new Punto2D(cen.X, cen.Y + li * 1.0),
                $"{lote.NombreManzana} Lote {lote.Numero}",
                h * 1.2, 0, CapasCatastro.Labels, TextoJustif.MC);

            // Área
            _cad.InsertarTexto(
                new Punto2D(cen.X, cen.Y),
                $"A = {lote.Area:F2} m²",
                c.AlturaArea, 0, CapasCatastro.Labels, TextoJustif.MC);

            // Propietario
            if (!string.IsNullOrEmpty(lote.Propietario))
                _cad.InsertarTexto(
                    new Punto2D(cen.X, cen.Y - li * 1.0),
                    lote.Propietario,
                    c.AlturaPropietario, 0, CapasCatastro.Labels, TextoJustif.MC);

            // Partida
            if (c.MostrarPartida && !string.IsNullOrEmpty(lote.PartidaRegistral))
                _cad.InsertarTexto(
                    new Punto2D(cen.X, cen.Y - li * 2.1),
                    $"Part. {lote.PartidaRegistral}",
                    c.AlturaPropietario * 0.85, 0, CapasCatastro.Labels, TextoJustif.MC);

            // Zonificación
            if (c.MostrarZonificacion && !string.IsNullOrEmpty(lote.Zonificacion))
                _cad.InsertarTexto(
                    new Punto2D(cen.X, cen.Y - li * 3.1),
                    lote.Zonificacion,
                    c.AlturaPropietario * 0.75, 0, CapasCatastro.Labels, TextoJustif.MC);
        }

        // ═══════════════════════════════════════════════════════
        // LINDEROS (distancia + rumbo por lado)
        // ═══════════════════════════════════════════════════════

        public void DibujarLinderos(Lote lote, ConfigTexto cfg = null)
        {
            var c = cfg ?? _cfg;
            foreach (var seg in lote.Poligono.Segmentos)
                DibujarEtiquetaSegmento(seg, c);
        }

        public void DibujarLinderosManzana(Manzana mz, ConfigTexto cfg = null)
        {
            var c = cfg ?? _cfg;
            foreach (var seg in mz.Poligono.Segmentos)
                DibujarEtiquetaSegmento(seg, c);
        }

        private void DibujarEtiquetaSegmento(Segmento seg, ConfigTexto c)
        {
            double h   = c.AlturaLindero;
            double ang = seg.Angulo;
            // Texto legible: normalizar ángulo
            double angDeg = ang * 180.0 / Math.PI;
            while (angDeg < 0)   angDeg += 360;
            while (angDeg >= 360) angDeg -= 360;
            double angTexto = angDeg;
            if (angTexto > 90 && angTexto <= 270) angTexto += 180;
            while (angTexto >= 360) angTexto -= 360;

            // Punto offset perpendicular al segmento
            double perp = ang + Math.PI / 2.0;
            var mid = seg.PuntoMedio;
            var offsetPt = new Punto2D(
                mid.X + c.OffsetLindero * Math.Cos(perp),
                mid.Y + c.OffsetLindero * Math.Sin(perp));

            // Texto distancia
            string dist = $"{seg.Longitud:F{c.DecimalesDistancia}} m";
            string label = c.MostrarRumbo ? $"{dist}  {seg.Rumbo}" : dist;

            _cad.InsertarTexto(offsetPt, label, h,
                angTexto * Math.PI / 180.0,
                CapasCatastro.Linderos, TextoJustif.MC);
        }

        // ═══════════════════════════════════════════════════════
        // ACOTACIÓN COMPLETA (etiqueta + linderos)
        // ═══════════════════════════════════════════════════════

        public void AcotarLoteCompleto(Lote lote, ConfigTexto cfg = null)
        {
            DibujarEtiquetaLote(lote, cfg);
            DibujarLinderos(lote, cfg);
        }

        // ═══════════════════════════════════════════════════════
        // ACOTACIONES DE MANZANA
        // ═══════════════════════════════════════════════════════

        public void AcotarManzana(Manzana mz, ConfigTexto cfg = null)
        {
            var c   = cfg ?? _cfg;
            var cen = mz.Centroide;
            double h = c.AlturaManzana;

            // Nombre de manzana (grande, centrado)
            _cad.InsertarTexto(
                new Punto2D(cen.X, cen.Y + h * 0.6),
                mz.Nombre, h * 1.1, 0, CapasCatastro.LabelMz, TextoJustif.MC);

            // Área
            _cad.InsertarTexto(
                new Punto2D(cen.X, cen.Y - h * 0.2),
                $"A = {mz.Area:F2} m²",
                h * 0.75, 0, CapasCatastro.LabelMz, TextoJustif.MC);

            // Linderos exteriores de la manzana
            DibujarLinderosManzana(mz, c);

            // Acotaciones dimensionales (AlignedDimension) en los 4 lados
            AcotarDimensionesManzana(mz, c);
        }

        private void AcotarDimensionesManzana(Manzana mz, ConfigTexto c)
        {
            var segs = mz.Poligono.Segmentos.ToList();
            double offset = c.AlturaLindero * 4.0; // separación de la cota al borde

            foreach (var seg in segs)
            {
                // Desplazar la línea de cota hacia afuera del polígono
                double perp = seg.Angulo + Math.PI / 2.0;
                var cen = mz.Centroide;
                var mid = seg.PuntoMedio;

                // Determinar si el offset va hacia adentro o afuera
                // (afuera = alejarse del centroide)
                double dx = mid.X - cen.X;
                double dy = mid.Y - cen.Y;
                double dot = dx * Math.Cos(perp) + dy * Math.Sin(perp);
                double sign = dot > 0 ? 1.0 : -1.0;

                var posTexto = new Punto2D(
                    mid.X + sign * offset * Math.Cos(perp),
                    mid.Y + sign * offset * Math.Sin(perp));

                _cad.AcotarAlineada(seg.P1, seg.P2, posTexto, CapasCatastro.Linderos);
            }
        }

        // ═══════════════════════════════════════════════════════
        // COLINDANCIAS SOBRE EL PLANO
        // ═══════════════════════════════════════════════════════

        public void DibujarColindancias(Lote lote, ConfigTexto cfg = null)
        {
            var c = cfg ?? _cfg;
            double h = c.AlturaLindero * 0.9;

            foreach (var colin in lote.Colindancias)
            {
                var seg = colin.Segmento;
                double ang = seg.Angulo;
                double angDeg = ang * 180.0 / Math.PI;
                while (angDeg < 0) angDeg += 360;
                if (angDeg > 90 && angDeg <= 270) angDeg += 180;
                while (angDeg >= 360) angDeg -= 360;

                double perp = ang + Math.PI / 2.0;
                var mid = seg.PuntoMedio;

                // Texto en dos líneas: descripción + distancia
                var pt1 = new Punto2D(
                    mid.X + c.OffsetLindero * 0.7 * Math.Cos(perp),
                    mid.Y + c.OffsetLindero * 0.7 * Math.Sin(perp));
                var pt2 = new Punto2D(
                    mid.X - h * 0.3 * Math.Cos(perp),
                    mid.Y - h * 0.3 * Math.Sin(perp));

                _cad.InsertarTexto(pt1, colin.Descripcion,
                    h, angDeg * Math.PI / 180.0,
                    CapasCatastro.Linderos, TextoJustif.MC);

                _cad.InsertarTexto(pt2, $"{seg.Longitud:F3} m",
                    h * 0.85, angDeg * Math.PI / 180.0,
                    CapasCatastro.Linderos, TextoJustif.MC);
            }
        }

        // ═══════════════════════════════════════════════════════
        // VÉRTICES Y MOJONES
        // ═══════════════════════════════════════════════════════

        public void DibujarVertices(Lote lote, ConfigTexto cfg = null, string tipoSimbolo = "CRUZ")
        {
            var c   = cfg ?? _cfg;
            var vts = lote.Poligono.Vertices;
            for (int i = 0; i < vts.Count; i++)
                DibujarVerticeIndividual(vts[i], c.NumVerticeInicial + i,
                    c, tipoSimbolo);
        }

        public void DibujarVerticeIndividual(Punto2D pt, int numero,
            ConfigTexto cfg, string tipoSimbolo = "CRUZ")
        {
            var c = cfg ?? _cfg;
            double s = 1.5; // tamaño símbolo

            // Símbolo
            switch (tipoSimbolo.ToUpper())
            {
                case "CIRCULO":
                    _cad.DibujarCirculo(pt, s / 2.0, CapasCatastro.Vertices);
                    break;
                case "TRIANGULO":
                    _cad.DibujarPolilineaCerrada(new[]
                    {
                        new Punto2D(pt.X,       pt.Y + s),
                        new Punto2D(pt.X - s,   pt.Y - s),
                        new Punto2D(pt.X + s,   pt.Y - s)
                    }, CapasCatastro.Vertices);
                    break;
                default: // CRUZ
                    _cad.DibujarLinea(
                        new Punto2D(pt.X - s, pt.Y),
                        new Punto2D(pt.X + s, pt.Y), CapasCatastro.Vertices);
                    _cad.DibujarLinea(
                        new Punto2D(pt.X, pt.Y - s),
                        new Punto2D(pt.X, pt.Y + s), CapasCatastro.Vertices);
                    break;
            }

            // Etiqueta: V-N
            double h  = c.AlturaVertice;
            double ox = c.OffsetVerticeX;
            double oy = c.OffsetVerticeY;

            _cad.InsertarTexto(
                new Punto2D(pt.X + ox, pt.Y + oy + h * 1.4),
                $"{c.PrefijoVertice}{numero}",
                h, 0, CapasCatastro.Vertices, TextoJustif.ML);

            _cad.InsertarTexto(
                new Punto2D(pt.X + ox, pt.Y + oy),
                $"E={pt.X:F{c.DecimalesUTM}}",
                h * 0.85, 0, CapasCatastro.Vertices, TextoJustif.ML);

            _cad.InsertarTexto(
                new Punto2D(pt.X + ox, pt.Y + oy - h * 1.2),
                $"N={pt.Y:F{c.DecimalesUTM}}",
                h * 0.85, 0, CapasCatastro.Vertices, TextoJustif.ML);
        }

        // ═══════════════════════════════════════════════════════
        // TABLA DE DATOS TÉCNICOS (formato SUNARP)
        // ═══════════════════════════════════════════════════════

        public void DibujarTablaDatosTecnicos(Lote lote, Punto2D insercion,
            double anchoTotal = 120.0, double altoFila = 7.0, double altTexto = 2.0)
        {
            double x = insercion.X;
            double y = insercion.Y;
            double h = altTexto;
            double rH = altoFila;
            double W = anchoTotal;

            // Zona UTM detectada
            string zona = CoordUtils.NombreZona(
                CoordUtils.DetectarZona(lote.Poligono.Vertices));

            // Encabezado
            DibujarCelda(x, y + rH * 8, W, rH, "CUADRO DE DATOS TÉCNICOS DEL PREDIO", h * 1.1, true);

            // Fila 1: Propietario / DNI
            DibujarCelda(x,             y + rH * 7, W * 0.7, rH, $"PROPIETARIO: {lote.Propietario}", h);
            DibujarCelda(x + W * 0.7,   y + rH * 7, W * 0.3, rH, $"DNI/RUC: {lote.Dni}", h);

            // Fila 2: Dirección
            DibujarCelda(x, y + rH * 6, W, rH, $"DIRECCIÓN: {lote.Direccion}", h);

            // Fila 3: Distrito / Provincia / Departamento
            DibujarCelda(x,             y + rH * 5, W * 0.33, rH, $"DISTRITO: {lote.Distrito}", h);
            DibujarCelda(x + W * 0.33,  y + rH * 5, W * 0.34, rH, $"PROVINCIA: {lote.Provincia}", h);
            DibujarCelda(x + W * 0.67,  y + rH * 5, W * 0.33, rH, $"DEPTO: {lote.Departamento}", h);

            // Fila 4: Lote / Manzana / Habilitación
            DibujarCelda(x,             y + rH * 4, W * 0.15, rH, $"LOTE: {lote.Numero}", h);
            DibujarCelda(x + W * 0.15,  y + rH * 4, W * 0.15, rH, $"MZA: {lote.NombreManzana}", h);
            DibujarCelda(x + W * 0.30,  y + rH * 4, W * 0.70, rH, $"HABILITACIÓN: {lote.HabilitacionUrbana}", h);

            // Fila 5: Partida / Zonificación
            DibujarCelda(x,             y + rH * 3, W * 0.5, rH, $"PARTIDA REGISTRAL: {lote.PartidaRegistral}", h);
            DibujarCelda(x + W * 0.5,   y + rH * 3, W * 0.5, rH, $"ZONIFICACIÓN: {lote.Zonificacion}", h);

            // Fila 6: Área / Perímetro (resaltado)
            DibujarCelda(x,             y + rH * 2, W * 0.5, rH, $"ÁREA TOTAL: {lote.Area:F2} m²", h * 1.05);
            DibujarCelda(x + W * 0.5,   y + rH * 2, W * 0.5, rH, $"PERÍMETRO: {lote.Perimetro:F3} m", h * 1.05);

            // Fila 7: Escala / Sistema
            DibujarCelda(x,             y + rH, W * 0.5, rH, "ESCALA: 1/200", h);
            DibujarCelda(x + W * 0.5,   y + rH, W * 0.5, rH, $"SISTEMA: UTM WGS84 Z{zona}", h);

            // Fila 8: Firma / Fecha
            DibujarCelda(x,             y, W * 0.6, rH, "ELABORADO POR:", h);
            DibujarCelda(x + W * 0.6,   y, W * 0.4, rH, $"FECHA: {DateTime.Now:dd/MM/yyyy}", h);
        }

        // ═══════════════════════════════════════════════════════
        // TABLA DE COORDENADAS UTM
        // ═══════════════════════════════════════════════════════

        public void DibujarTablaCoordenadas(Lote lote, Punto2D insercion,
            double anchoTotal = 80.0, double altoFila = 7.0, double altTexto = 2.0)
        {
            var verts = lote.Poligono.Vertices;
            int n = verts.Count;
            double x = insercion.X, y = insercion.Y;
            double rH = altoFila, h = altTexto, W = anchoTotal;
            double c1 = 12.0, c2 = (W - c1) / 2.0, c3 = (W - c1) / 2.0;

            // Encabezado
            DibujarCelda(x, y + rH * (n + 1), W, rH,
                $"CUADRO DE VÉRTICES UTM WGS84 — Z{CoordUtils.NombreZona(CoordUtils.DetectarZona(verts))}",
                h, true);

            // Cabecera columnas
            DibujarCelda(x,          y + rH * n, c1, rH, "VÉR.", h);
            DibujarCelda(x + c1,     y + rH * n, c2, rH, "ESTE E (m)", h);
            DibujarCelda(x + c1 + c2, y + rH * n, c3, rH, "NORTE N (m)", h);

            // Datos
            for (int i = 0; i < n; i++)
            {
                double fy = y + rH * (n - 1 - i);
                DibujarCelda(x,          fy, c1, rH, $"V-{_cfg.NumVerticeInicial + i}", h);
                DibujarCelda(x + c1,     fy, c2, rH, $"{verts[i].X:F{_cfg.DecimalesUTM}}", h * 0.9);
                DibujarCelda(x + c1 + c2, fy, c3, rH, $"{verts[i].Y:F{_cfg.DecimalesUTM}}", h * 0.9);
            }
        }

        // ═══════════════════════════════════════════════════════
        // TABLA DE ÁREAS (distribución de lotes)
        // ═══════════════════════════════════════════════════════

        public void DibujarTablaAreas(IList<Lote> lotes, Punto2D insercion,
            double anchoTotal = 80.0, double altoFila = 7.0, double altTexto = 2.0)
        {
            int n = lotes.Count;
            double x = insercion.X, y = insercion.Y;
            double rH = altoFila, h = altTexto, W = anchoTotal;
            double totalArea = lotes.Sum(l => l.Area);

            // Encabezado
            DibujarCelda(x, y + rH * (n + 2), W, rH, "CUADRO DE ÁREAS", h * 1.05, true);

            // Cabecera
            DibujarCelda(x,           y + rH * (n + 1), W * 0.15, rH, "LOTE", h);
            DibujarCelda(x + W * 0.15, y + rH * (n + 1), W * 0.30, rH, "ÁREA (m²)", h);
            DibujarCelda(x + W * 0.45, y + rH * (n + 1), W * 0.55, rH, "DESCRIPCIÓN", h);

            // Datos
            for (int i = 0; i < n; i++)
            {
                double fy = y + rH * (n - i);
                var    lt = lotes[i];
                DibujarCelda(x,           fy, W * 0.15, rH, lt.Numero, h);
                DibujarCelda(x + W * 0.15, fy, W * 0.30, rH, $"{lt.Area:F2}", h);
                DibujarCelda(x + W * 0.45, fy, W * 0.55, rH, lt.Uso ?? "", h * 0.85);
            }

            // Total
            DibujarCelda(x,           y, W * 0.15, rH, "TOTAL", h * 1.05);
            DibujarCelda(x + W * 0.15, y, W * 0.30, rH, $"{totalArea:F2}", h * 1.05);
            DibujarCelda(x + W * 0.45, y, W * 0.55, rH, "", h);
        }

        // ═══════════════════════════════════════════════════════
        // TABLA DE COLINDANCIAS
        // ═══════════════════════════════════════════════════════

        public void DibujarTablaColindancias(
            string norte, string sur, string este, string oeste,
            Punto2D insercion,
            double anchoTotal = 120.0, double altoFila = 7.0, double altTexto = 2.0)
        {
            double x = insercion.X, y = insercion.Y;
            double rH = altoFila, h = altTexto, W = anchoTotal;

            DibujarCelda(x, y + rH * 4, W, rH, "COLINDANCIAS", h * 1.05, true);

            var filas = new[] {
                ("NORTE", norte), ("SUR", sur),
                ("ESTE",  este),  ("OESTE", oeste)
            };
            for (int i = 0; i < 4; i++)
            {
                double fy = y + rH * (3 - i);
                DibujarCelda(x,           fy, W * 0.15, rH, filas[i].Item1, h);
                DibujarCelda(x + W * 0.15, fy, W * 0.85, rH, filas[i].Item2, h);
            }
        }

        // ═══════════════════════════════════════════════════════
        // SECCIÓN DE VÍA (en planta)
        // ═══════════════════════════════════════════════════════

        public void DibujarSeccionVia(Via via, Punto2D insercion)
        {
            double x = insercion.X, y = insercion.Y;
            double aw = via.AnchoCalzada > 0 ? via.AnchoCalzada : via.Ancho * 0.675;
            double av = via.AnchoVereda  > 0 ? via.AnchoVereda  : via.Ancho * 0.1625;
            double ab = via.AnchoBerma   > 0 ? via.AnchoBerma   : 0;
            double profSeccion = via.Ancho * 0.6; // profundidad visual de la sección

            double xCurr = x;

            // Vereda izquierda
            DibujarBandaVia(xCurr, y, av, profSeccion, CapasCatastro.Vias, "VEREDA");
            xCurr += av;

            // Berma izquierda (si hay)
            if (ab > 0)
            {
                DibujarBandaVia(xCurr, y, ab, profSeccion, CapasCatastro.Ejes, "BERMA");
                xCurr += ab;
            }

            // Calzada
            DibujarBandaVia(xCurr, y, aw, profSeccion, CapasCatastro.Vias, "CALZADA");
            xCurr += aw;

            // Berma derecha
            if (ab > 0)
            {
                DibujarBandaVia(xCurr, y, ab, profSeccion, CapasCatastro.Ejes, "BERMA");
                xCurr += ab;
            }

            // Vereda derecha
            DibujarBandaVia(xCurr, y, av, profSeccion, CapasCatastro.Vias, "VEREDA");
            xCurr += av;

            // Eje de simetría
            _cad.DibujarLinea(
                new Punto2D(x + via.Ancho / 2.0, y - profSeccion * 0.1),
                new Punto2D(x + via.Ancho / 2.0, y + profSeccion * 1.1),
                CapasCatastro.Ejes);

            // Acotaciones totales
            double hAcot = via.Ancho * 0.08;
            _cad.AcotarAlineada(
                new Punto2D(x, y - hAcot * 4),
                new Punto2D(x + via.Ancho, y - hAcot * 4),
                new Punto2D(x + via.Ancho / 2.0, y - hAcot * 5),
                CapasCatastro.Linderos);

            // Etiqueta nombre y tipo
            _cad.InsertarTexto(
                new Punto2D(x + via.Ancho / 2.0, y + profSeccion + hAcot * 2),
                $"{via.Nombre ?? "Vía"}  —  {Via.NombreTipo(via.Tipo)}",
                hAcot * 1.2, 0, CapasCatastro.LabelMz, TextoJustif.MC);

            _cad.InsertarTexto(
                new Punto2D(x + via.Ancho / 2.0, y + profSeccion + hAcot * 0.5),
                $"Ancho total = {via.Ancho:F2} m  |  Calzada = {aw:F2} m  |  Vereda = {av:F2} m",
                hAcot * 0.85, 0, CapasCatastro.LabelMz, TextoJustif.MC);
        }

        private void DibujarBandaVia(double x, double y, double ancho,
            double prof, string capa, string etiqueta)
        {
            _cad.DibujarPolilineaCerrada(new[]
            {
                new Punto2D(x,        y),
                new Punto2D(x + ancho, y),
                new Punto2D(x + ancho, y + prof),
                new Punto2D(x,        y + prof)
            }, capa);

            _cad.InsertarTexto(
                new Punto2D(x + ancho / 2.0, y + prof / 2.0),
                etiqueta, ancho * 0.18, 0, CapasCatastro.Labels, TextoJustif.MC);

            // Acotación del ancho de la banda
            _cad.AcotarAlineada(
                new Punto2D(x, y),
                new Punto2D(x + ancho, y),
                new Punto2D(x + ancho / 2.0, y - ancho * 0.5),
                CapasCatastro.Linderos);
        }

        // ═══════════════════════════════════════════════════════
        // HELPER: DIBUJAR CELDA DE TABLA
        // ═══════════════════════════════════════════════════════

        private void DibujarCelda(double x, double y, double w, double h,
            string texto, double altTexto, bool negrita = false)
        {
            // Borde de la celda
            _cad.DibujarPolilineaCerrada(new[]
            {
                new Punto2D(x,     y),
                new Punto2D(x + w, y),
                new Punto2D(x + w, y + h),
                new Punto2D(x,     y + h)
            }, CapasCatastro.Tabla);

            // Texto centrado verticalmente
            _cad.InsertarTexto(
                new Punto2D(x + 1.0, y + h / 2.0 - altTexto / 2.0),
                texto ?? "",
                altTexto, 0, CapasCatastro.Tabla, TextoJustif.ML);
        }
    }
}
