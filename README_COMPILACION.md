# CatastroTools v2.0 — Guía de Compilación
## Visual Studio 2019 + .NET 4.8 | ZWCAD / AutoCAD

---

## 1. Abrir la solución

1. Abre `CatastroTools.sln` en Visual Studio 2019
2. Verifica que la plataforma sea **x64** (no Any CPU)
3. Target Framework: **.NET Framework 4.8**

---

## 2. Agregar referencias nativas (paso crítico)

Las DLLs de ZWCAD/AutoCAD **no están en NuGet** — se agregan manualmente.

### Para ZWCAD:

```
Click derecho en CatastroTools.Plugin → Agregar → Referencia...
→ Examinar → navegar a:
   C:\Program Files\ZWSOFT\ZWCAD 2024\

Seleccionar:
  ✓ ZwCAD.Interop.dll
  ✓ ZwCAD.Interop.Common.dll
  (o según versión: ZWCAD.dll, ZwCAD.Core.dll)
```

Para cada DLL agregada:
```
Click derecho en la referencia → Propiedades
→ Copia local = False   ← MUY IMPORTANTE
```

### Para AutoCAD:

```
C:\Program Files\Autodesk\AutoCAD 2024\
  ✓ acdbmgd.dll
  ✓ acmgd.dll
  ✓ accoremgd.dll
```

También con `Copia local = False`.

---

## 3. Configurar la constante de compilación

En `CatastroTools.Plugin.csproj`, la configuración Debug/Release
incluye `ZWCAD` por defecto.

Para AutoCAD: cambiar `ZWCAD` por `AUTOCAD`:

```xml
<DefineConstants>RELEASE;AUTOCAD</DefineConstants>
```

O crear una nueva configuración en VS2019:
- Build → Configuration Manager → New
- Nombre: `Release-AutoCAD`
- Copiar desde `Release`
- Luego editar el .csproj para esa configuración con `AUTOCAD`

---

## 4. Proyecto CatastroTools.UI (WPF)

Necesitas crear el proyecto WPF con las ventanas de diálogo.
Los comandos ya están listos — solo necesitan las ventanas:

```
VentanaVia.xaml              → CT-VIA-EJE
VentanaViasGrilla.xaml       → CT-VIAS-GRILLA
VentanaManzaneo.xaml         → CT-MANZANEO
VentanaManzaneoGrilla.xaml   → CT-MANZANEO-GRILLA
VentanaSeccionVia.xaml       → CT-SECCION-VIA
VentanaLotizacion.xaml       → CT-LOTIZAR
VentanaHabilitacion.xaml     → CT-HABILITACION
VentanaEtiquetaLote.xaml     → CT-ETIQUETA
VentanaAcotar.xaml           → CT-ACOTAR
VentanaVertices.xaml         → CT-VERTICES
VentanaImportarCoords.xaml   → CT-IMPORTAR-COORDS
VentanaTabla.xaml            → CT-TABLA
VentanaColindancias.xaml     → CT-TABLA-COLIN
VentanaExportHTML.xaml       → CT-EXPORT-HTML
VentanaConfiguracion.xaml    → CT-CONFIG
```

En VS2019:
```
Click derecho en CatastroTools.UI → Agregar → Ventana (WPF)
```

---

## 5. Compilar

```
Build → Build Solution  (Ctrl+Shift+B)
```

El DLL resultante estará en:
```
CatastroTools.Plugin\bin\x64\Release\CatastroTools.dll
```

---

## 6. Instalar en ZWCAD

### Opción A — NETLOAD (por sesión):
```
En ZWCAD, línea de comandos:
NETLOAD
→ Seleccionar CatastroTools.dll
```

### Opción B — Carga automática (recomendado):
```
ZWCAD → Herramientas → Opciones → Archivos
→ Rutas de carga automática de aplicaciones
→ Agregar CatastroTools.dll
```

### Opción C — Startup Suite:
```
APPLOAD → Startup Suite → Agregar → CatastroTools.dll
```

---

## 7. Instalar en AutoCAD

```
NETLOAD → Seleccionar CatastroTools.dll
(compilado con la constante AUTOCAD)
```

---

## 8. Verificar instalación

En la línea de comandos del CAD:
```
CT
```
Debe aparecer el menú de comandos con todos los disponibles.

---

## Estructura de archivos del proyecto

```
CatastroTools.sln
├── CatastroTools.Core/
│   ├── Models/Modelos.cs          ← Punto2D, Lote, Manzana, Via...
│   ├── Geometry/Geometria.cs      ← Subdivisión, Recorrido, Colindancias
│   └── Export/Exportadores.cs     ← HTML, CSV
│
├── CatastroTools.CAD/
│   ├── Interfaces/ICadPlatform.cs ← Abstracción de plataforma
│   ├── ZwCAD/ZwPlatform.cs        ← Implementación ZWCAD
│   ├── AutoCAD/AcPlatform.cs      ← Implementación AutoCAD
│   └── ServicioDibujo.cs          ← Lógica de dibujo catastral
│
├── CatastroTools.UI/
│   └── Views/*.xaml               ← Diálogos WPF (crear en VS2019)
│
└── CatastroTools.Plugin/
    ├── EntryPoint.cs              ← IExtensionApplication
    └── Commands/
        ├── ComandosManzaneo.cs    ← CT-VIA-EJE, CT-MANZANEO...
        └── Comandos.cs            ← CT-LOTIZAR, CT-ETIQUETA...
```

---

## Notas importantes

- **Windows 7 SP1**: requiere .NET 4.8 instalado (descargable de Microsoft)
- **x64 obligatorio**: ZWCAD y AutoCAD son 64-bit; no compilar en Any CPU
- **CopyLocal = False**: las DLLs del CAD no deben ir en el output (ya existen en el programa)
- **Una DLL para todo**: el plugin compilado es un solo `CatastroTools.dll`

---

*CatastroTools v2.0 — Sistema Catastral Perú — SUNARP / RNE GH.020*
