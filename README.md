# GeoSuite - Suite de Ingeniería para AutoCAD/ZwCAD

## Arquitectura Modular

GeoSuite es un sistema de plugins modulares para ingeniería civil, topografía y catastro, compatible con AutoCAD y ZwCAD.

### Estructura del Proyecto

```
GeoSuite/
├── src/
│   ├── GeoSuite.Core/              # Lógica matemática y modelos (independiente de CAD)
│   ├── GeoSuite.Platform/          # Abstracción de plataforma CAD (AutoCAD/ZwCAD)
│   ├── GeoSuite.Survey.Module/     # Módulo de Topografía (DLL independiente)
│   ├── GeoSuite.Catastro.Module/   # Módulo de Catastro (DLL independiente)
│   ├── GeoSuite.Roadway.Module/    # Módulo de Vías/Carreteras (DLL independiente)
│   └── GeoSuite.Hydraulics.Module/ # Módulo de Hidráulica/Drenaje (DLL independiente)
```

## Módulos Disponibles

### 1. Topografía (`GeoSuite.Survey.Module.dll`)

| Comando | Descripción |
|---------|-------------|
| `GS-T-IMP` | Importar puntos desde CSV (N,E,Z,Código,Descripción) |
| `GS-T-TIN` | Generar malla triangular (TIN) con Delaunay |
| `GS-T-CN` | Generar curvas de nivel con intervalo configurable |

**Capas creadas:**
- `TOPO-PUNTOS` (Rojo) - Puntos topográficos
- `TOPO-DATOS` (Verde) - Etiquetas de cotas
- `TOPO-TIN` (Cyan) - Malla triangular
- `TOPO-CURVAS-PRINC` (Amarillo) - Curvas maestras
- `TOPO-CURVAS-SEC` (Gris) - Curvas secundarias

### 2. Catastro (`GeoSuite.Catastro.Module.dll`)

| Comando | Descripción |
|---------|-------------|
| `GS-C-POLY` | Dibujar polígono catastral con cálculo automático de área |

**Capas creadas:**
- `CATASTRO-LINDEROS` (Rojo) - Líneas de linderos
- `CATASTRO-TEXTO` (Blanco) - Etiquetas de área/perímetro

### 3. Vías/Carreteras (`GeoSuite.Roadway.Module.dll`)

| Comando | Descripción |
|---------|-------------|
| `GS-R-ALIGN` | Crear alineamiento horizontal con PIs etiquetados |

**Capas creadas:**
- `VIAS-EJE` (Rojo) - Eje de vía
- `VIAS-PI` (Verde) - Puntos de intersección

### 4. Hidráulica/Drenaje (`GeoSuite.Hydraulics.Module.dll`)

| Comando | Descripción |
|---------|-------------|
| `GS-H-DRAIN` | Crear red de drenaje (pozos y tuberías) |

**Capas creadas:**
- `DRENAJE-POZOS` (Rojo) - Pozos de visita
- `DRENAJE-TUBERIA` (Cyan) - Tuberías
- `DRENAJE-TEXTO` (Blanco) - Etiquetas

## Formato de Archivo de Puntos (CSV)

```csv
ID,Este,Norte,Cota,Codigo,Descripcion
1,5000,10000,120.5,BM,Banco de Nivel
2,5020,10050,121.3,PI,Punto Intermedio
3,5040,9980,119.8,PT,Punto Termino
```

## Compilación

### Requisitos
- .NET 6.0 SDK o superior
- Visual Studio 2022 (opcional)
- Referencias a AutoCAD o ZwCAD (ajustar paths en `.csproj`)

### Pasos

1. **Restaurar paquetes:**
   ```bash
   dotnet restore
   ```

2. **Compilar todos los módulos:**
   ```bash
   dotnet build --configuration Release
   ```

3. **DLLs generadas en:**
   ```
   src/GeoSuite.Survey.Module/bin/Release/net6.0-windows/GeoSuite.Survey.Module.dll
   src/GeoSuite.Catastro.Module/bin/Release/net6.0-windows/GeoSuite.Catastro.Module.dll
   src/GeoSuite.Roadway.Module/bin/Release/net6.0-windows/GeoSuite.Roadway.Module.dll
   src/GeoSuite.Hydraulics.Module/bin/Release/net6.0-windows/GeoSuite.Hydraulics.Module.dll
   ```

## Instalación en AutoCAD/ZwCAD

1. Copiar las DLLs del módulo deseado a una carpeta segura
2. En CAD, ejecutar `NETLOAD` y seleccionar la DLL
3. Los comandos estarán disponibles inmediatamente

## Nomenclatura Técnica

- **Prefijo de comandos:** `GS-` (GeoSuite)
- **Segundo nivel:** `T` (Topografía), `C` (Catastro), `R` (Roadway), `H` (Hydraulics)
- **Tercer nivel:** Función específica (ej: `IMP`, `TIN`, `CN`, `POLY`)

## Extensibilidad

Para agregar nuevos módulos:

1. Crear nueva carpeta `src/GeoSuite.[Nombre].Module/`
2. Agregar referencia a `GeoSuite.Core` y `GeoSuite.Platform`
3. Implementar comandos con `[CommandMethod]`
4. Compilar como DLL independiente

## Licencia

Desarrollo interno - Todos los derechos reservados

---

## 🚀 Plan de Desarrollo por Fases

### ✅ Fase 1: Cimientos (COMPLETADO)
- [x] Definición de estructura de solución y proyectos
- [x] Implementación de `GeoSuite.Core`:
  - [x] `Coordinate3`: Estructura básica XYZ
  - [x] `SurveyPoint`: Punto topográfico con metadatos
  - [x] `Polygon2D`: Polígono cerrado para catastro
  - [x] `Triangulation`: Esqueleto para algoritmo Delaunay
- [x] Implementación de `GeoSuite.Platform`:
  - [x] Interfaz `ICadHost`
  - [x] `CadServiceFactory` para detección de plataforma
  - [x] `MockCadHost` para desarrollo sin CAD instalado
- [x] Creación de esqueletos de módulos (Survey, Catastro, Roadway, Hydraulics)
- [x] Documentación inicial (README.md)

### 🔨 Fase 2: Conectores CAD (PENDIENTE)
- [ ] Implementar `AcadHost.cs` (AutoCAD):
  - [ ] Referenciar `acdbmgd.dll`, `acmgd.dll`
  - [ ] Implementar dibujo de entidades (Línea, Círculo, Polilínea, Texto, Bloque)
  - [ ] Gestión de capas y bloques
  - [ ] Diálogos de selección de archivos
- [ ] Implementar `ZwHost.cs` (ZwCAD):
  - [ ] Referenciar `zwtxbrx.dll` o equivalentes .NET
  - [ ] Mapeo de métodos a API ZwCAD
- [ ] Configurar condiciones de compilación (`#if ACAD`, `#if ZWCAD`) o proyectos separados

### 📈 Fase 3: Módulo de Topografía (Survey) - COMANDOS BASE
- [ ] **Comando `GS-T-IMP` (Importar Puntos)**:
  - [ ] Diálogo de selección de archivo (CSV, TXT, DAT)
  - [ ] Parser de formatos comunes (N,E,Z,Código,Descripción)
  - [ ] Inserción de bloques "Punto Topográfico" con atributos
  - [ ] Opción de importar solo algunos puntos por filtro
- [ ] **Comando `GS-T-TIN` (Superficie TIN)**:
  - [ ] Integrar librería de triangulación (Triangle.NET o NetTopologySuite)
  - [ ] Dibujar malla de triángulos en capa "TOPO-TIN"
  - [ ] Opción de línea de rotura (breaklines)
  - [ ] Excluir triángulos con longitud mayor a umbral
- [ ] **Comando `GS-T-CN` (Curvas de Nivel)**:
  - [ ] Algoritmo de interpolación lineal en aristas
  - [ ] Suavizado de curvas (Splines)
  - [ ] Etiquetado automático de cotas
  - [ ] Curvas maestras vs secundarias (intervalo configurable)
- [ ] **Comando `GS-T-PERF` (Perfiles)**:
  - [ ] Definición de línea de sección (dos puntos o polilínea existente)
  - [ ] Generación de gráfico de perfil longitudinal
  - [ ] Escalas horizontal y vertical configurables

### 📐 Fase 4: Módulo de Catastro - COMANDOS BASE
- [ ] **Comando `GS-C-POLY` (Polígonos)**:
  - [ ] Dibujo asistido de polígonos cerrados
  - [ ] Cálculo automático de área y perímetro
  - [ ] Verificación de cierre (tolerancia)
- [ ] **Comando `GS-C-LBL` (Etiquetado)**:
  - [ ] Etiquetas dinámicas de área/lote
  - [ ] Tablas de áreas automáticas (formato HTML/DWG)
  - [ ] Numeración automática de lotes
- [ ] **Comando `GS-C-SUB` (Subdivisión)**:
  - [ ] Herramientas de división proporcional
  - [ ] División por área específica
  - [ ] Líneas de división paralelas a un lado

### 🛣️ Fase 5: Módulo de Vías (Roadway) - COMANDOS BASE
- [ ] **Comando `GS-R-ALINE` (Alineamiento Horizontal)**:
  - [ ] Diseño de ejes (Tangentes, Curvas circulares, Transiciones/Espirales)
  - [ ] Cálculo de elementos de curva (T, L, E, Δ)
  - [ ] Estacionamiento automático (PKs)
- [ ] **Comando `GS-R-PERF` (Perfil Longitudinal)**:
  - [ ] Muestreo de superficie TIN a lo largo del eje
  - [ ] Diseño de rasante (líneas y curvas verticales)
  - [ ] Cálculo de pendientes
- [ ] **Comando `GS-R-SECC` (Secciones Transversales)**:
  - [ ] Generación de secciones típicas cada X metros
  - [ ] Plantillas personalizables (carretera, berma, taludes)
  - [ ] Cálculo de volúmenes (Método de áreas medias)

### 💧 Fase 6: Módulo de Hidráulica (Hydraulics) - COMANDOS BASE
- [ ] **Comando `GS-H-RED` (Redes de Drenaje)**:
  - [ ] Dibujo de tuberías y pozos de visita
  - [ ] Cálculo hidráulico (fórmula de Manning)
  - [ ] Verificación de velocidad y tirante
- [ ] **Comando `GS-H-CUN` (Cunetas y Canales)**:
  - [ ] Diseño de secciones abiertas (trapecial, rectangular, triangular)
  - [ ] Cálculo de pendiente y caudal
- [ ] **Comando `GS-H-PERF` (Perfiles de Tubería)**:
  - [ ] Perfil longitudinal de red de drenaje
  - [ ] Cobertura mínima sobre tubería

### 🧪 Fase 7: Pruebas y Distribución
- [ ] Crear suite de pruebas unitarias (`tests/GeoSuite.Tests/`)
- [ ] Pruebas de integración con AutoCAD real
- [ ] Pruebas de integración con ZwCAD real
- [ ] Scripts de instalación (MSI o loader LISP)
- [ ] Documentación de usuario final (PDF/Markdown)
- [ ] Sistema de licenciamiento (opcional)

---

## 📋 Registro de Cambios

| Versión | Fecha | Descripción |
|---------|-------|-------------|
| 0.1.0 | 2024-01 | Fase 1 completada: Core, Platform y esqueletos de módulos |
| 0.2.0 | TBD | Fase 2: Implementación de conectores AutoCAD/ZwCAD |
| 0.3.0 | TBD | Fase 3: Módulo de Topografía funcional |
| 1.0.0 | TBD | Lanzamiento estable con todos los módulos base |
