using System.Collections.Generic;
using CatastroTools.Core.Models;

namespace CatastroTools.CAD.Interfaces
{
    /// <summary>
    /// Abstracción completa de la plataforma CAD.
    /// Implementada por ZwPlatform y AcPlatform.
    /// Ningún código de negocio toca la API nativa directamente.
    /// </summary>
    public interface ICadPlatform
    {
        // ─── INFORMACIÓN ────────────────────────────────────────
        string NombrePlataforma { get; }
        string Version { get; }

        // ─── CAPAS ───────────────────────────────────────────────
        void CrearCapa(string nombre, int colorAci, string tipLinea = "Continuous");
        void SetCapaActual(string nombre);
        bool ExisteCapa(string nombre);

        // ─── DIBUJO DE ENTIDADES ─────────────────────────────────
        long DibujarPolilineaCerrada(IList<Punto2D> puntos, string capa);
        long DibujarPolilineaAbierta(IList<Punto2D> puntos, string capa);
        long DibujarLinea(Punto2D p1, Punto2D p2, string capa);
        long DibujarCirculo(Punto2D centro, double radio, string capa);
        long DibujarArco(Punto2D centro, double radio, double angIni, double angFin, string capa);

        // ─── TEXTO ───────────────────────────────────────────────
        long InsertarTexto(Punto2D punto, string texto, double altura,
                           double angulo, string capa, TextoJustif justif = TextoJustif.MC);
        long InsertarMTexto(Punto2D punto, string texto, double altura,
                            double ancho, string capa);

        // ─── ACOTACIONES ─────────────────────────────────────────
        long AcotarLineal(Punto2D p1, Punto2D p2, Punto2D posTexto,
                          string capa, bool horizontal = true);
        long AcotarAlineada(Punto2D p1, Punto2D p2, Punto2D posTexto, string capa);

        // ─── LECTURA DE ENTIDADES ────────────────────────────────
        List<Punto2D> ObtenerVerticesPolilinea(long entityId);
        List<Punto2D> ObtenerVerticesPolilinea(string handle);
        bool EsPolilineaCerrada(long entityId);
        TipoEntidad ObtenerTipo(long entityId);

        // ─── SELECCIÓN ───────────────────────────────────────────
        long SeleccionarEntidad(string prompt);
        List<long> SeleccionarMultiple(string prompt, FiltroSeleccion filtro = null);
        List<Punto2D> PedirPolilineaInteractiva(string prompt);
        Punto2D PedirPunto(string prompt);
        Punto2D PedirPunto(string prompt, Punto2D basePoint);
        double? PedirReal(string prompt, double? defaultVal = null);
        int? PedirEntero(string prompt, int? defaultVal = null);
        string PedirTexto(string prompt, string defaultVal = "");

        // ─── TRANSACCIONES ───────────────────────────────────────
        IDisposableTransaction IniciarTransaccion();

        // ─── UTILIDADES ──────────────────────────────────────────
        void Purgar();
        void Zoom(string opcion = "E");
        void Regen();
        string RutaDWGActual { get; }
        string DirectorioDWG { get; }
        string NombreDWG { get; }
        void MensajeConsola(string texto);
        void MensajeError(string texto);
    }

    public interface IDisposableTransaction : System.IDisposable
    {
        void Commit();
        void Abort();
    }

    public enum TextoJustif
    {
        ML,   // Middle Left
        MC,   // Middle Center
        MR,   // Middle Right
        TL, TC, TR,
        BL, BC, BR
    }

    public enum TipoEntidad
    {
        Polilinea, Linea, Circulo, Arco, Texto, MTexto, Bloque, Desconocido
    }

    public class FiltroSeleccion
    {
        public List<string> TiposPermitidos { get; set; } = new List<string>();
        public string Capa { get; set; }
        public bool SoloCerradas { get; set; }
    }

    // ─── COLORES ACI ESTÁNDAR CATASTRAL ──────────────────────────
    public static class CapasCatastro
    {
        public const string Lotes      = "CT-LOTES";
        public const string Linderos   = "CT-LINDEROS";
        public const string Vertices   = "CT-VERTICES";
        public const string Labels     = "CT-LABELS";
        public const string Tabla      = "CT-TABLA";
        public const string Ejes       = "CT-EJES";
        public const string Vias       = "CT-VIAS";
        public const string Manzanas   = "CT-MANZANAS";
        public const string LabelMz    = "CT-LABEL-MZ";
        public const string AreaVia    = "CT-AREA-VIA";

        public static void InicializarTodas(ICadPlatform cad)
        {
            cad.CrearCapa(Lotes,    3,  "Continuous");   // verde
            cad.CrearCapa(Linderos, 5,  "Continuous");   // azul
            cad.CrearCapa(Vertices, 1,  "Continuous");   // rojo
            cad.CrearCapa(Labels,   7,  "Continuous");   // blanco
            cad.CrearCapa(Tabla,    7,  "Continuous");
            cad.CrearCapa(Ejes,     8,  "CENTER2");      // gris
            cad.CrearCapa(Vias,     2,  "Continuous");   // amarillo
            cad.CrearCapa(Manzanas, 4,  "Continuous");   // cyan
            cad.CrearCapa(LabelMz,  6,  "Continuous");   // magenta
            cad.CrearCapa(AreaVia,  9,  "Continuous");   // gris claro
        }
    }
}
