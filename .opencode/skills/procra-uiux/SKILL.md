---
name: ProcraStop UI/UX
description: >-
  Design system, XAML/MVVM standards and Shell navigation rules for ProcraStop
  (.NET 11 MAUI, Android-first). Use when designing, creating or modifying any
  screen, component, style, resource, theme, navigation or visual element of
  ProcraStop.Maui11 (or reviewing UI quality). Covers Material 3, Light/Dark
  theming, PS* component catalog, accessibility, performance and the mandatory
  ProcraStop navigation/binding safety rules.
license: MIT
compatibility: .NET 11, .NET MAUI 11, Android-first
---

# ProcraStop UI/UX Engineering Skill

Sistema de conocimiento operativo para diseñar e implementar las interfaces de
**ProcraStop**: app de productividad, hábitos y bienestar académico
(gestión de tareas, hábitos, estrés y autorregulación) construida en
**.NET MAUI 11**, **Android-first**, con Shell, XAML, MVVM y Material 3.

Este skill es la **referencia permanente** de presentación del proyecto. Su
objetivo: interfaces consistentes, modernas y profesionales durante todo el
ciclo de desarrollo, sin contaminar Domain/Application/Infrastructure.

---

## 0. Reglas no negociables (leer primero)

1. **Trabaja exclusivamente sobre `src/ProcraStop.Maui11`** cuando te lo pidan.
   Nunca migres ni modifiques `src/ProcraStop` (app .NET 10) a menos que se
   solicite explícitamente.
2. **No cambies Domain ni Application** para resolver un problema visual. Si un
   ViewModel necesita datos que no expone, propón el cambio y pregunta antes.
3. **No inventes APIs de MAUI 11.** Antes de recomendar un control, propiedad o
   handler, verifica que existe (docs oficiales o el código del proyecto).
   Si dudas, busca el API en el código fuente del repo o en Microsoft Learn.
4. **MVVM obligatorio.** Cero lógica de negocio en code-behind. El code-behind
   solo admite: wiring de DI, subscribe/unsubscribe de eventos con
   unsubscribe simétrico, y handlers de gesto triviales que delegan en
   comandos del ViewModel.
5. **Colores y medidas nunca hardcoded** en páginas: usa los tokens del
   Design System (`references/design-system.md`).
6. **Reglas de seguridad de binding/navegación** (provenidas de bugs reales,
   ver `references/maui11-standards.md` §2.4): nunca `RelativeSource
   AncestorType` dentro de `DataTemplate`; nunca reparentar `Content` de una
   página en runtime; toda navegación vía `ShellNavigation` (compuerta de
   serialización).
7. **No declares "implementado y funcional"** nada que no hayas compilado.
   Verifica con `dotnet build src/ProcraStop.Maui11/ProcraStop.Maui11.csproj -f net11.0-windows10.0.19041.0` (Windows) o `-f net11.0-android`.

---

## 1. Cuándo activar este skill

Activa (o se auto-activa) cuando el usuario pida:

- Diseñar o implementar una pantalla, vista o componente ("crea la pantalla de
  X", "rediseña el Dashboard", "haz un card de progreso").
- Crear/modificar estilos, colores, temas, ResourceDictionaries, fuentes.
- Configurar o depurar navegación Shell, tabs, rutas, deep links.
- Revisar calidad de UI (accesibilidad, rendimiento, consistencia).
- Migrar/adaptar una pantalla desde `src/ProcraStop` (.NET 10) a
  `ProcraStop.Maui11`.

Cuando el usuario pida **solo lógica** (ViewModel sin UI, servicios, datos),
este skill aplica solo en lo relativo a bindings y navegación.

---

## 2. Composición con la librería oficial de skills de MAUI

Este skill es **específico de ProcraStop** (tokens, componentes, reglas del
proyecto). Se complementa con las **41 skills oficiales de MAUI**
(`davidortinau/maui-skills`, equipo .NET) para detalle técnico profundo:

| Tema | Skill oficial a cargar |
|---|---|
| Theming Light/Dark, AppThemeBinding | `maui-theming` |
| Shell, tabs, rutas, GoToAsync | `maui-shell-navigation` |
| CollectionView, EmptyView, RefreshView | `maui-collectionview` |
| Compiled bindings, converters, MultiBinding | `maui-data-binding` |
| Safe area / edge-to-edge (.NET 10+) | `maui-safe-area` |
| Accesibilidad, SemanticProperties | `maui-accessibility` |
| Rendimiento, trimming, AOT | `maui-performance` |
| Razonamiento de diseño para móvil | `ux-first-principles` + `ux-mobile` |
| Guardia anti-APIs obsoletas | `maui-current-apis` |
| Gestos (tap, swipe, drag) | `maui-gestures` |
| DI y resolución de ViewModels por Shell | `maui-dependency-injection` |

Instalación (una vez, por herramienta):

```sh
# Copilot CLI / Claude Code
/plugin marketplace add davidortinau/maui-skills
/plugin install maui-skills@maui-skills

# OpenCode / otros: copiar los directorios de skills que necesites a
# .opencode/skills/ o .claude/skills/ del proyecto.
```

**Regla de resolución:** este skill define *qué* y *con qué identidad*;
las skills oficiales definen *cómo* en detalle técnico. Si ambas contradicen
en algo específico del proyecto (tokens, rutas, componentes PS*), gana este
skill.

---

## 3. Workflow obligatorio para cualquier pantalla o componente

Sigue estos pasos en orden; no salte pasos sin justificarlo:

1. **Comprende el propósito funcional.** ¿Qué decide o hace el usuario aquí?
   ¿Qué módulo es (Dashboard/Tasks/Habits/Stress/Settings)? Lee
   `references/ux-guidelines.md` para los principios del módulo.
2. **Inspecciona el repo antes de escribir.** Lista `Components/`,
   `Resources/Styles/` y las vistas del módulo en `ProcraStop.Maui11`.
   **No inventes archivos existentes.** Si una pantalla ya existe, propón
   mejora incremental, no reescritura.
3. **Consulta el Design System** (`references/design-system.md`): usa tokens
   (colores, tipografía, espaciado, shapes, elevación). Prohibido definir
   valores ad-hoc fuera del sistema salvo justificación explícita.
4. **Reutiliza antes de crear.** Antes de un componente nuevo, revisa el
   catálogo (`references/components.md`). Aplica el árbol de decisión:
   ¿control nativo MAUI sirve? → usa el control. ¿Estilo XAML basta? → crea
   `Style` en `Controls.xaml`. ¿Solo un módulo lo usa? → componente local del
   módulo. ¿2+ módulos, misma apariencia y propiedades estables? → componente
   `PS*` en `Components/`.
5. **Define la navegación** (`references/shell-navigation.md`): ¿página de tab,
   global route, modal o back? Registra la ruta idempotente y navega por
   ViewModel con `ShellNavigation`.
6. **Implementa XAML + ViewModel** con los estándares de
   `references/maui11-standards.md` (compiled bindings, x:DataType, estilos
   implícitos, estados vacíos/carga/error).
7. **Verifica la compilación** del target correspondiente. Un cambio no
   compilado no está terminado.
8. **Pasa la checklist de calidad** (`references/quality-checklist.md`) y
   corrige lo que falle antes de entregar.

---

## 4. Resumen del Design System (detalles en `references/design-system.md`)

**Identidad:** productividad + calma + progreso. No dashboard empresarial.

- **Primario:** azul `#4075FF` (acción, foco, progreso).
- **Fondos:** Light `#F1F3F8` / Dark `#222631`; superficies elevadas
  Light `#FFFFFF` / Dark `#171B24`.
- **Texto:** `#12141B`/`#FFFFFF` (primario), `#6F7A8F`/`#AAB0C0` (secundario).
- **Semánticos:** Success `#46C771`, Warning `#F59E0B`, Error `#E5484D`,
  Info `#4075FF`.
- **Tipografía:** Montserrat (títulos), Inter (cuerpo), Manrope (datos/labels).
- **Radios:** inputs 8 · botones/chips 14 · cards 18 · FAB/avatares circulares.
- **Elevación:** separación por tonalidad primero; sombra discreta solo en
  elementos flotantes (FAB, snackbar, bottom sheet).

Theming: `AppThemeBinding` + `DynamicResource` para tokens; nunca colores
literales. Material 3 activo en Android vía `<UseMaterial3>` (ya configurado
en el csproj): los controles nativos (pickers, dialogs) heredan el tema del
sistema; los controles MAUI usan los recursos propios del Design System.

---

## 5. Navegación: decisión única y estable

**Shell con TabBar inferior (sin Flyout como interfaz principal):**

```text
TabBar: Dashboard · Tareas · Hábitos · Ajustes
Stress: integrado en Dashboard (check-in) + rutas globales de analítica.
Rutas jerárquicas: cada tab gestiona su pila (detalle → back).
Rutas globales: solo páginas que NO viven en la jerarquía del Shell
                (CreateTask, EditTask, TaskDetail, CreateHabit, ...).
Modal: solo flujos de foco único (confirmación destructiva, wizard corto).
```

Reglas críticas (detalladas en `references/shell-navigation.md`):

- Rutas de `Routing.RegisterRoute` **jamás duplican** rutas de `ShellContent`.
- Navegación desde ViewModel **siempre** por `ShellNavigation.GoToAsync`
  (compuerta que serializa navegaciones y descarta dobles toques).
- Pestañas existentes: rutas absolutas `//TabName`.
- Parámetros por query string (`?id=`); nada de estado global compartido
  como transporte de navegación.

---

## 6. Estados obligatorios en toda pantalla de datos

Toda vista que muestre datos del usuario define los 4 estados:

| Estado | Requisito |
|---|---|
| **Loading** | Indicador discreto o skeleton; nunca pantalla en blanco. |
| **Empty** | `EmptyView` con icono + título + texto empático + acción sugerida. |
| **Error** | Mensaje claro + acción de reintento; nunca un toast que desaparece. |
| **Loaded** | Contenido con jerarquía clara y acciones rápidas al alcance del pulgar. |

Los tres primeros se resuelven con `PSEmptyState` / `PSLoadingState` /
`PSErrorState` (ver `references/components.md`) — mismo idioma visual en toda
la app.

---

## 7. Criterios de calidad de entrega (resumen)

Antes de entregar, confirma (checklist completa en
`references/quality-checklist.md`):

- [ ] Compila el target solicitado (Android/Windows).
- [ ] Cero colores/medidas hardcoded fuera del Design System.
- [ ] Compiled bindings con `x:DataType` en plantillas; ninguna
      `RelativeSource AncestorType` dentro de `DataTemplate`.
- [ ] Estados loading/empty/error implementados.
- [ ] Áreas táctiles ≥ 48×48 dp; `SemanticProperties` en elementos icónicos.
- [ ] Contraste de texto ≥ 4.5:1 en ambos temas.
- [ ] Navegación por comandos + `ShellNavigation`; sin acoplamiento entre
      ViewModels ni service locator.
- [ ] Suscripciones a eventos con unsubscribe simétrico en `OnDisappearing`.
- [ ] Dark theme revisado de verdad (no solo "asumo que se ve bien").

---

## 8. Estructura objetivo del proyecto de presentación

```text
ProcraStop.Maui11/
├── Resources/Styles/        # Colors, Typography, Spacing, Shapes,
│                            # Controls, Styles (Design System)
├── Resources/Fonts/         # Montserrat, Inter, Manrope, Fluent icons
├── Components/              # Catálogo PS* (solo componentes probados)
│   ├── Buttons/  Cards/  Inputs/  Indicators/  Navigation/  Common/
├── Views/                   # Por módulo: Dashboard, Tasks, Habits,
│                            # Stress, Settings (páginas = Views)
├── ViewModels/              # Por módulo, junto a su vista
├── Navigation/              # ShellNavigation.cs, mapeo de rutas
├── Services/                # Servicios de presentación (snackbar, tema)
└── AppShell.xaml            # TabBar + rutas globales
```

No crees carpetas ni archivos vacíos "por si acaso": el catálogo `Components/`
crece cuando el segundo uso justifica la abstracción (regla del segundo uso).

---

## 9. Referencias internas (leer según la tarea)

| Archivo | Cuándo |
|---|---|
| `references/design-system.md` | Siempre: tokens, theming, tipografía, shapes, elevación. |
| `references/maui11-standards.md` | Al escribir XAML/C#: estándares, binding seguro, rendimiento, MAUI 11. |
| `references/shell-navigation.md` | Al tocar navegación, rutas, tabs, deep links o ciclo de vida de páginas. |
| `references/components.md` | Al crear/usar componentes visuales: catálogo PS*, árbol de decisión, specs. |
| `references/ux-guidelines.md` | Al diseñar una pantalla: principios por módulo, filosofía visual, accesibilidad UX. |
| `references/quality-checklist.md` | Antes de entregar: checklist completa de diseño, XAML, MVVM, a11y, rendimiento. |

## 10. Fuentes técnicas de referencia

- Microsoft Learn: .NET MAUI docs (User interface, Shell, Theming,
  CollectionView, Accessibility, Performance).
- Anuncio .NET 11 (.NET Blog): MAUI 11 adopta CoreCLR como runtime por
  defecto (Android incluido) y mejora la iteración de XAML y controles
  nativos; .NET MAUI DevFlow (experimental) permite inspeccionar el árbol
  visual de una app en ejecución.
- `davidortinau/maui-skills` (MIT): 41 skills del equipo .NET MAUI; las de
  este proyecto se listan en §2.
- Grial UI Kit: referencia de calidad de composición (principios, no copia).
- Material Design 3: foundation de tokens y semántica de color/elevación.

**Prohibido** usar como fuentes: API de Xamarin.Forms sin verificar vigencia,
tutoriales no oficiales desactualizados, o APIs de librerías no presentes en
el proyecto (CommunityToolkit solo si está referenciado en el csproj).
