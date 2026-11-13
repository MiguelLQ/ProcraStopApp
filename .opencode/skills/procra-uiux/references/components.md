# Catálogo de componentes PS* de ProcraStop

Decisión de creación y especificaciones. Los nombres `PS*` son el espacio de
componentes del Design System de ProcraStop.

## 1. Árbol de decisión: ¿necesito un componente nuevo?

```text
¿Existe un control MAUI nativo que cubra el caso?
├─ Sí → usa el control + Style de Controls.xaml. FIN.
└─ No → ¿Solo cambia apariencia (no estructura)?
        ├─ Sí → crea un Style (Controls.xaml) o plantilla de control. FIN.
        └─ No → ¿Lo usan 2+ módulos con misma estructura y propiedades estables?
                ├─ No (1 módulo) → componente local en Views/<Module>/Controls/. FIN.
                └─ Sí → crea PS* en Components/<Group>/ siguiendo §3. FIN.
```

Reglas transversales (de la misión, adoptadas como criterio duro):

1. No duplicar controles MAUI (`PSButton` no re-implementa `Button`: es un
   `Button` estilizado o un `Border+Label` solo si Button no alcanza).
2. **Composición sobre herencia**: `PSStatCard` compone `Border+Label+Label`;
   no hereda de `Frame` para "reutilizar".
3. Componentes simples: si necesita 15+ BindableProperties, está mal cortado.
4. Sin lógica de negocio: un componente recibe estado ya calculado
   (`Progress`, `IsDone`); no calcula.
5. MVVM: comunicación solo por BindableProperties + comandos expuestos.
6. Documentar en el header de la clase: propósito, propiedades, ejemplo XAML.
7. Accesibilidad incluida (§6 maui11-standards): SemanticProperties, tap target.
8. Cero dependencias nuevas por componente (CommunityToolkit solo si ya está
   en el csproj).

**Regla del segundo uso:** no se crea `Components/PSX` hasta que el segundo
caso de uso existe. Primero: estilo o componente local.

## 2. Catálogo y prioridad de implementación

No se implementa nada "por catálogo": cada entrada entra a producción cuando
su primera pantalla real lo necesita. La tabla define el plano.

| Componente | Grupo | Prioridad | Notas |
|---|---|---|---|
| `PSCard` | Cards | P0 | Base del sistema; estilo implícito `PSCardStyle` (Border, radius 18, `SurfaceContainer`, padding 16). Muchos "cards" son solo esto: usar el estilo antes que el control. |
| `PSButton` | Buttons | P0 | Variantes Primary / Secondary / Destructive / Text via estilos sobre `Button`. Altura 44+, radius 14, `LabelLarge`. |
| `PSIconButton` | Buttons | P0 | `Border` con glyph Fluent, tap target 48×48, `SemanticProperties.Description` obligatoria. |
| `PSEmptyState` | Common | P0 | Icono + título + texto empático + botón de acción opcional. Usado en EmptyView de todas las listas. |
| `PSLoadingState` | Indicators | P0 | Skeleton por defecto; spinner compacto para acciones. |
| `PSErrorState` | Common | P0 | Mensaje claro + botón "Reintentar" (Command bindable). |
| `PSSectionHeader` | Common | P0 | `TitleLarge` + acción opcional ("Ver todo"). |
| `PSChip` | Common | P0 | `Border` radius 14, `LabelMedium`; estados default/selected; `Command`+`CommandParameter` para filtros. |
| `PSBadge` | Common | P1 | Contador/estado pequeño (no interactivo). Colores semánticos con contenedor claro. |
| `PSStatCard` | Cards | P1 | Valor grande (`DisplayLarge`) + label + delta opcional. Para Dashboard/resumen. |
| `PSProgressCard` | Cards | P1 | Título + barra de progreso (6 dp, radius full) + porcentaje; usado en hábitos/tareas. |
| `PSProgressIndicator` | Indicators | P1 | Barra reutilizable de progreso (no circular salvo demanda real). |
| `PSTaskCard` | Cards | P1 | Card de tarea: check, título, meta (fecha/prioridad), indicador de atraso, tap→detalle separado del check **[P]** (evita doble navegación). |
| `PSHabitCard` | Cards | P1 | Hábito: streak, progreso del día, acciones +/- separadas del tap→detalle. |
| `PSListItem` | Common | P1 | Fila estándar: avatar/icono + título + subtítulo + trailing. |
| `PSSearchBar` | Inputs | P2 | Entry estilizada (radius 14, `SurfaceContainer`, icono lupa, botón cancelar). |
| `PSInput` | Inputs | P2 | Entry/Picker estilizados: label flotante simple, `Outline`, radius 8, estado de error con `Error`. |
| `PSAvatar` | Common | P2 | Círculo con iniciales o icono; tamaños S/M/L. |
| `PSBottomSheet` | Navigation | P2 | Página modal transparente + sheet `SurfaceContainerHigh` radius 24 con scrim; slide-up 250 ms. |
| `PSDialog` | Common | P2 | Wrapper de diálogo Material 3 con tipografía/espaciado del sistema (los dialogs nativos ya son M3: usarlos por defecto). |
| `PSDateSelector` | Inputs | P3 | Selector de fecha tipo chips (hoy/mañana/calendario) sobre DatePicker nativo. |
| `PSCalendarItem` | Common | P3 | Celda de calendario (día, estado today/selected/out-of-month) para selectores de fecha y heatmaps de hábitos. |
| `PSBottomNavigation` | Navigation | — | **No como control custom**: TabBar de Shell ya es la bottom nav; solo se estiliza. Evita reimplementar navegación. |
| `PSTopBar` | Navigation | — | **No como control custom**: `Shell.TitleView` + estilos; solo si se necesita composición compleja. |

La filosofía del catálogo es de Grial: composición de primitivas bien
espaciadas, no widgets monolíticos.

## 3. Contrato de implementación de un PS*

```csharp
// Components/Cards/PSStatCard.xaml.cs
/// <summary>
/// Card de estadística: valor destacado + descripción.
/// Ejemplo:
/// <components:PSStatCard Value="{Binding FocusMinutes}"
///                        Label="Minutos de foco" />
/// </summary>
public partial class PSStatCard : Border
{
    public static readonly BindableProperty ValueProperty = ...;   // string
    public static readonly BindableProperty LabelProperty = ...;   // string
    public PSStatCard() => InitializeComponent();
}
```

Checklist de un componente nuevo:

1. `ContentView`/`Border` raíz — el tipo raíz comunica el contrato (una card
   es `Border`, no `ContentView` genérico).
2. `x:Class` en namespace `ProcraStop.Maui11.Components.<Group>`.
3. BindableProperties públicas con `BindingMode` correcto (OneWay default;
   TwoWay solo edición).
4. Estilos desde tokens: cero valores literales en el XAML del componente.
5. `ControlTemplate`/`IsTemplateBindingScoped` solo cuando se necesite
   re-skinning; preferir composición directa.
6. Sin `BindingContext` fijo interno: hereda el del consumidor o recibe por
   propiedades.
7. Regla **[P]**: el componente no manipula el árbol de sus padres ni se
   reparenta a sí mismo.
8. Accesibilidad: descripciones cuando el contenido es icónico.
9. Compilado y probado en Light + Dark antes de declararse disponible.

## 4. Componentes explícitamente vetados

- **`PSBottomNavigation` custom**: duplicaría Shell TabBar y rompería el
  modelo de navegación. Prohibido.
- Controles que envuelven `CollectionView` "mágicos": el consumo directo de
  CollectionView con estilos es más simple y configurable.
- Cualquier componente que resuelva servicios por constructor (los XAML los
  instancia el framework): los componentes son *view-only*; los servicios se
  inyectan en páginas/VMs.

## 5. Estado de componentes (interacción)

Estados a cubrir por todo componente interactivo:

| Estado | Tratamiento visual |
|---|---|
| Default | Token base |
| Pressed | Opacidad 0.85 (rápido, sin animación compleja) o fondo `PrimaryContainer` |
| Disabled | `TextDisabled` + `Opacity` 0.6 |
| Selected | `PrimaryContainer` + `OnPrimaryContainer` |
| Error (inputs) | borde `Error` + mensaje debajo |

Implementación: `VisualStateManager` o triggers en el estilo; consistente en
todos los componentes (elegir VSM como estándar del sistema).
