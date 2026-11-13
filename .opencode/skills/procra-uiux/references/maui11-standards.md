# Estándares MAUI 11 / XAML / MVVM de ProcraStop

Estándares de código para `ProcraStop.Maui11`. Lecciones de producción del
proyecto marcadas con **[P]** (provienen de bugs reales, son obligatorias).

## 1. Marco técnico (.NET 11)

- **Runtime CoreCLR por defecto** (incluido Android) desde .NET 11: mejor
  startup y rendimiento del JIT que Mono. Implicación práctica: no añadir
  configuración Mono/AOT legacy; si se necesita AOT/trimming en Release,
  seguir la doc oficial de .NET 11, no recetas de .NET 8.
- `UseMaterial3=true` (ya en el csproj para Android): los pickers, dialogs y
  controles nativos heredan Material 3; la UI compuesta usa el Design System
  propio. No duplicar configuración.
- **No inventar APIs de MAUI 11**: antes de usar un control/propiedad nuevo,
  verificar en Microsoft Learn o en el código del proyecto. Las skills
  oficiales (`maui-current-apis`) detectan APIs obsoletas: cargarla ante la
  duda.
- Los `TargetFrameworks` del csproj incluyen windows/ios/maccatalyst, pero el
  foco es **Android**: no introducir código `#if` de otras plataformas sin
  necesidad, y no romper el build multi-target.

## 2. Estándares XAML

### 2.1 Estructura de un archivo XAML

```xml
<?xml version="1.0" encoding="utf-8" ?>
<base:PSPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:vm="clr-namespace:ProcraStop.Maui11.ViewModels.Tasks"
             xmlns:components="clr-namespace:ProcraStop.Maui11.Components.Cards"
             x:Class="ProcraStop.Maui11.Views.Tasks.TasksPage"
             x:DataType="vm:TasksPageViewModel"          <!-- SIEMPRE -->
             x:Name="PageRoot"                            <!-- para x:Reference -->
             Title="Mis tareas"
             BackgroundColor="{DynamicResource Surface}">

    <Grid RowDefinitions="Auto,Auto,*" RowSpacing="{StaticResource SpacingS}">
        ...
    </Grid>
</base:PSPage>
```

Reglas:

1. **`x:DataType` en la raíz de la página y en cada `DataTemplate`**
   (compiled bindings siempre).
2. **`x:Name="PageRoot"` en el elemento raíz** del contenido: es el ancla
   estándar para bindings desde plantillas **[P]**.
3. Elementos ordenados: xmlns → x:Class → x:DataType → x:Name → propiedades
   Shell → propiedades visuales.
4. Comentarios `<!-- -->` solo para explicar *decisión*, no para describir lo
   obvio.
5. Sin estilos inline repetidos: si un bloque de propiedades se repite 2+,
   es un `Style` en `Controls.xaml`.

### 2.2 Layouts — criterio de selección

| Layout | Cuándo | Cuándo NO |
|---|---|---|
| `Grid` | Alineación en filas/columnas, overlays, páginas completas | Como contenedor genérico de un solo hijo |
| `VerticalStackLayout` | Flujos verticales simples | Con muchos hijos que necesitan mezcla horizontal (anidar) |
| `HorizontalStackLayout` | Filas de chips, acciones, avatares | Contenido que necesita wrap → `FlexLayout` |
| `FlexLayout` | Chips dinámicos, tags, wrap | Layout principal de página |
| `AbsoluteLayout` | Capas flotantes declaradas en XAML (snackbar) | Layout general **[P]: solo declarativo, jamás reparenting en runtime** |
| `ScrollView` | Contenido mayor que pantalla | Dentro de `CollectionView` (scroll anidado) |
| `CollectionView` | Listas dinámicas ≥ ~10 items | Listas fijas cortas → `BindableLayout`/estático |
| `BindableLayout` | Listas pequeñas (≤ ~10) dinámicas | Colecciones grandes → CollectionView |

Límite práctico de anidación: **3 niveles** de contenedores de layout. Con
más, se reconsidera la composición (el rendimiento de Android sufre con
jerarquías profundas y el `VisualTreeHelper`-like debugging se hace imposible).

### 2.3 Estilos y recursos

- `Resources/Styles/`: `Colors.xaml` (tokens), `Typography.xaml`, `Spacing.xaml`,
  `Shapes.xaml`, `Controls.xaml` (estilos implícitos por control:
  `PSButtonPrimaryStyle`, `PSCardStyle`, `PSInputStyle`...), `Styles.xaml`
  (agregador, merged en `App.xaml`).
- Estilos **implícitos** (sin `x:Key`, TargetType) para look por defecto
  (Label→BodyMedium, Button→Primary); estilos **con key** para variantes.
- `BasedOn` para derivar variantes (`PSButtonSecondaryStyle BasedOn
  PSButtonPrimaryStyle`).
- `DynamicResource` para tokens que cambian con el tema; `StaticResource`
  para el resto.
- Prohibido: duplicar un estilo en dos diccionarios; estilos dentro de la
  página salvo que sean exclusivos de ella y no reutilizables.

### 2.4 Bindings — reglas de seguridad **[P]**

Estas reglas vienen de la depuración de un `NullReferenceException` en
producción (`CreateTypedBindingFrom_bindingExtension…` sobre
`OpenDetailCommand`):

1. **Prohibido `Source={RelativeSource AncestorType={x:Type ...}}` dentro de
   `DataTemplate`.** MAUI resuelve `RelativeSource` recorriendo el árbol
   visual en vivo; si el árbol se desconecta momentáneamente (navegación,
   reparent, theme change), el binding se aplica con `source == null` y el
   getter compilado lanza NRE. Patrón correcto:

```xml
<!-- En la raíz del contenido: x:Name="PageRoot" -->
<!-- Dentro de DataTemplate: -->
<TapGestureRecognizer
    Command="{Binding Path=BindingContext.OpenDetailCommand, Source={x:Reference PageRoot}}"
    CommandParameter="{Binding Id}" />
```

   `x:Reference` se resuelve en tiempo de parseo → nunca es null.

2. **Prohibido reparentar `Content` de una página en runtime** (envolver,
   mover, `Content = null` en `OnHandlerChanged`). Destruye handlers de
   CollectionViews y reinfla plantillas con `BindingContext` heredado nulo.
   Toda capa flotante (snackbar, scrim) se declara en el XAML de la página.

3. `CommandParameter` siempre con tipo claro; el `CanExecute` del comando
   tolera item null.

4. Bindings TwoWay solo cuando el usuario edita el valor; default `OneWay`.

5. `Mode=OneTime` para datos que no cambian en vida del template.

6. Converters: tolerantes a null (`value is null → Binding.DoNothing` o
   fallback), sin estado, registrados como `StaticResource` en `Styles.xaml`.
   ConvertBack no lanza `NotImplementedException`: devuelve
   `Binding.DoNothing`.

7. `BindableLayout`/`CollectionView` ItemTemplate: `x:DataType` del modelo de
   fila (no del ViewModel de página).

## 3. MVVM

### 3.1 ViewModel

```csharp
public partial class TasksPageViewModel : ObservableObject
{
    private readonly ITasksFacade _tasks;          // solo fachadas de Application
    private readonly INavigationService _nav;      // abstracción propia

    [ObservableProperty]
    private partial ObservableCollection<TaskItemModel> tasks { get; set; } = [];

    [RelayCommand]
    private async Task OpenDetailAsync(Guid id) =>
        await _nav.GoToAsync($"task-detail?id={id}");
}
```

Reglas:

1. `[ObservableProperty]` + `[RelayCommand]` (CommunityToolkit.Mvvm) — el
   proyecto no escribe `INotifyPropertyChanged` a mano.
2. El ViewModel **no referencia** `Microsoft.Maui.Controls` (ni `Page`,
   `Shell.Current`, `Application.Current`). Navegación y diálogos por
   abstracción (`INavigationService`, `IAlertService` de Infrastructure de
   presentación).
3. Async commands: sufijo `Async`, `CanExecute` cuando corresponda, y
   protección de excepción (el runner de `RelayCommand` captura, pero el
   ViewModel registra en `AppCrashLogger` los fallos esperables).
4. Constructor: solo inyección de dependencias (DI de Shell resuelve las
   páginas; los parámetros de ruta se inyectan con `IQueryAttributable`).
5. Colecciones: `ObservableCollection` creada una vez, actualizada con
   `Clear/Add` — no reasignar la referencia salvo reset completo (menos
   reseteo de layout en CollectionView).

### 3.2 Code-behind

Permitido: `InitializeComponent`, asignación inicial de `BindingContext`
(solo si no viene por Shell DI), `InitializeSnackbar(...)`, subscribe/unsubscribe
simétrico de eventos, handlers de gesto que delegan en comando, focus/scroll
imperativo puntual.

Prohibido: lógica de negocio, llamadas a servicios de datos, `DisplayAlert`
con texto de lógica, creación de ViewModels a mano.

**[P]** Todo `OnAppearing` async (`async void`) va protegido:
`await SafeUi.RunAsync(async () => { ... }, "Page.OnAppearing")`. Toda
suscripción (`Messenger`, eventos de servicio) se desuscribe en
`OnDisappearing` — sin dobles suscripciones al re-appearing.

## 4. Estados de UI

- **Carga**: `IsBusy` del ViewModel (o flag específico); skeleton preferido
  sobre spinner para listas; spinner solo para acciones cortas.
- **Vacío**: `CollectionView.EmptyView` obligatorio con `PSEmptyState`
  (icono, título, texto empático, botón de acción).
- **Error**: banner/card de error en línea (no toast volador) con botón
  "Reintentar"; el toast solo confirma acciones exitosas.
- **Pull-to-refresh**: `RefreshView` + `IsRefreshing` OneWay con reset
  garantizado en `finally`.

## 5. Accesibilidad

1. `SemanticProperties.Description` en todo elemento visual sin texto propio
   (iconos, FAB, avatares). `SemanticProperties.HeadingLevel` en títulos.
2. Áreas táctiles ≥ 48×48 dp: el elemento visual puede ser más pequeño, pero
   el tappable (`Border`, `Grid` con recognizer) no. Botones: altura ≥ 44.
3. Contraste según §2.3 del design system, verificado en Light **y** Dark.
4. `IsTabStop`/orden lógico; sin foco en decoración.
5. Texto no se corta (`LineBreakMode` consciente); nada escondido solo por
   `IsVisible=false` que el lector deba anunciar (`AutomationProperties.IsInAccessibleTree`).
6. Feedback de acciones: toda acción async larga muestra progreso y deshabilita
   el botón (`CanExecute`), no deja doble-click.

## 6. Rendimiento (reglas con evidencia práctica del proyecto)

1. Compiled bindings siempre (§2.1) — binding reflejo en listas es el mayor
   coste típico en Android.
2. Celdas ligeras: `Grid` plano, imágenes con `Aspect` y tamaño fijo,
   sin sombras dentro de item templates.
3. `CollectionView` con `ItemsLayout` declarado y `ItemSizingStrategy`
   explícito cuando el template es heterogéneo.
4. Imágenes: solo recursos que existen; sin cargar PNG grandes para glyphs
   (usar fuentes de iconos).
5. No suscribir a `PropertyChanged` global sin desuscripción; leak típico:
   servicio singleton escuchando a ViewModel transitorio.
6. Animaciones solo sobre `Opacity/Translation/Scale` (§8 design system).
7. Nada bloqueante en UI thread: I/O y BD siempre await; CPU pesada a
   `Task.Run` con resultado entregado al UI thread.
8. `OnDisappearing` cancela trabajos largos pendientes (`CancellationToken`
   por página cuando aplique).

## 7. Convenciones de nombres

| Elemento | Patrón | Ejemplo |
|---|---|---|
| Página | `<Feature>Page` en `Views/<Module>/` | `TasksPage`, `CreateHabitPage` |
| ViewModel | `<Feature>PageViewModel` en `ViewModels/<Module>/` | `TasksPageViewModel` |
| Componente | `PS<Purpose>` en `Components/<Group>/` | `PSStatCard`, `PSChip` |
| Estilo | `PS<Group><Variant>Style` | `PSButtonPrimaryStyle`, `PSCardStyle` |
| Token color | PascalCase Material-like | `SurfaceContainer`, `TextSecondary` |
| Token spacing | `Spacing<Size>` | `SpacingL` |
| Ruta Shell | kebab-case, sin espacios | `task-detail`, `create-habit` |
| Event handler code-behind | `On<Event>` | `OnSearchTapped` |

## 8. Checklist de compilación y verificación

1. `dotnet build src/ProcraStop.Maui11/ProcraStop.Maui11.csproj -f
   net11.0-android` (o el target de trabajo) — **0 errores, 0
   advertencias propias** (el ruido WMC1509 de WinUI en Windows no cuenta;
   ver §9: puede compilarse con VS abierto gracias a `Directory.Build.props`).
2. Probar en Light y Dark.
3. Recorrer la pantalla con la checklist de `quality-checklist.md`.
4. Si tocó navegación: smoke de rutas (ver `shell-navigation.md` §6).
5. No declarar terminado lo no compilado/probado.

## 9. Workflow CLI / Visual Studio (artifacts layout) **[P]**

**Problema histórico:** VS y la CLI compartían `obj\`/`bin\`; el build Android
de CLI fallaba con `XALNS7019 / UnauthorizedAccessException` (archivos
bloqueados por devenv) siempre que VS estuviera abierto. Un intento con
`-p:BaseIntermediateOutputPath=obj_cli\` produjo 40× CS0579: al mover el
intermedio, los `DefaultItemExcludes` dejan de excluir el `obj\` viejo y los
generados por VS (`.AssemblyInfo.cs` de otros TFMs) se compilan como fuentes.

**Solución definitiva (activa):** `src/ProcraStop.Maui11/Directory.Build.props`
fija `ArtifactsPath=...\artifacts_cli` cuando `BuildingInsideVisualStudio !=
'true'`. Con eso el SDK .NET 11 activa el *artifacts output layout*
(`UseArtifactsOutput` + `UseArtifactsIntermediateOutput`): bin y obj viven en
`artifacts_cli\` y el SDK **excluye automáticamente `bin/**` y `obj/**`** de los
globs (validado: build Android CLI con devenv abierto → 0 errores).

**CRÍTICO — exclusión simétrica:** en el MISMO `Directory.Build.props`,
`artifacts_cli\**` se añade a `DefaultItemExcludes` SIEMPRE (también en VS).
Sin esto, los generados de los builds CLI (`App.g.cs`, `XamlTypeInfo.g.cs`,
`AssemblyInfo.cs`, resizetizer `FluentUI.cs`) se filtran como fuentes en el
build de VS y producen ~70 errores CS0101/CS0234/CS0579/CS0759/MAUIX2000
(bug real ocurrido, reproducido con MSBuild.exe + `BuildingInsideVisualStudio`
y corregido). Es seguro definirlo ahí: el SDK **concatena** este valor sobre
sus defaults en `Microsoft.NET.Sdk.DefaultItems.targets`.

Reglas:
- **VS**: usa siempre `obj\`/`bin\` del proyecto — no tocar. F5 y despliegue
  al móvil desde VS funcionan igual que siempre.
- **CLI**: build directo sin flags extra — `Directory.Build.props` lo enruta a
  `artifacts_cli\` solo. NUNCA pasar `-p:BaseIntermediateOutputPath` ni
  `-p:OutputPath` a mano (causa los CS0579).
- Salidas CLI: `src/ProcraStop.Maui11/artifacts_cli/bin/<Project>/debug_<tfm>/`
  (exe de Windows y APK incluidos). El exe de prueba de Windows ya NO está en
  `bin\Debug\...win-x64\` sino en `artifacts_cli\...\ProcraStop.Maui11.exe`.
- Si `artifacts_cli\` queda en estado corrupto: borrarlo y reconstruir; no
  afecta a VS.
- NO agregar un `Directory.Build.props` a nivel de repo (afectaría a la app
  .NET 10 de `src/ProcraStop`); el de Maui11 solo aplica a Maui11.
