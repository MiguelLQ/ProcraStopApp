# Estrategia de navegación Shell para ProcraStop

MAUI Shell como **infraestructura** de navegación; la interfaz de navegación
es un TabBar inferior propio del Design System. Este documento codifica
decisiones ya validadas en producción de la app (.NET 10) y las extiende a
`ProcraStop.Maui11`.

## 1. Dirección visual: Bottom Navigation, sin Flyout principal

```text
┌─────────────────────────────────────────┐
│  TopBar discreto (título + acción)      │
│                                         │
│  Contenido de la pestaña activa         │
│                                         │
│  [ ● Dashboard ] [ Tareas ] [ Hábitos ] [ Ajustes ]  ← TabBar inferior
└─────────────────────────────────────────┘
```

- **4 tabs**: Dashboard (Inicio), Tareas, Hábitos, Ajustes.
- **Stress** vive dentro de Dashboard (check-in diario) y expone su analítica
  como ruta global/stage propia (no tab). Justificación UX: el registro de
  estrés es una acción contextual de 15 s, no un destino recurrente.
- Módulos Académico (Cursos/Horario) y Técnicas de estudio: accesibles desde
  Dashboard como secciones/tarjetas y rutas globales; si crecen, se evalúa
  un 5º tab — decisión explícita del usuario, nunca por defecto.
- **El Flyout no se usa como interfaz principal** (no encaja con la filosofía
  minimalista y de navegación inferior de la imagen de referencia). Shell
  sigue siendo el motor: rutas, pila, deep links, back.
- TabBar: fondo `TabBar`, iconos Fluent glyphs, activa `Primary`,
  inactiva `TextTertiary`. `Shell.TabBarForegroundColor`, `UnselectedColor`,
  `TitleColor` via AppThemeBinding en `AppShell.xaml`.

## 2. Jerarquía Shell en AppShell.xaml

```xml
<TabBar>
    <Tab Title="Inicio" Route="dashboard" Icon="{StaticResource IconDashboard}">
        <ShellContent ContentTemplate="{DataTemplate DashboardPage}" />
    </Tab>
    <Tab Title="Tareas" Route="tasks" Icon="{StaticResource IconTasks}">
        <ShellContent ContentTemplate="{DataTemplate TasksPage}" Route="TasksPage" />
    </Tab>
    <Tab Title="Hábitos" Route="habits" ... />
    <Tab Title="Ajustes" Route="settings" ... />
</TabBar>
```

Reglas:

1. **Rutas de ShellContent (jerarquía) jamás se registran de nuevo** en
   `Routing.RegisterRoute` — rutas duplicadas dejan Shell en estado roto
   **[P]** (bug real del proyecto).
2. `Routing.RegisterRoute` **solo** para páginas apilables que NO viven en el
   TabBar: `create-task`, `edit-task`, `task-detail`, `create-habit`,
   `habit-track`, `stress-analytics`, `courses`, `schedule`,
   `technique-library`, etc. Registro idempotente (guard si ya registrado).
3. Nombres de ruta: kebab-case, únicos, estables (contratos entre módulos).
4. Las páginas de tab se resuelven con `ContentTemplate` (lazy) + DI.
5. `Shell.CurrentItem` solo se manipula desde código de navegación
   centralizado, nunca desde ViewModels.

## 3. Navegación desde ViewModels: ShellNavigation (compuerta) **[P]**

Toda navegación pasa por un `ShellNavigation` (servicio de presentación con
la lógica ya probada en la app .NET 10):

```csharp
public sealed class ShellNavigation
{
    private static readonly SemaphoreSlim Gate = new(1, 1);

    public static async Task<bool> GoToAsync(string route)
    {
        // Descarta peticiones concurrentes (doble toque) en vez de encolarlas:
        if (!await Gate.WaitAsync(0)) return false;
        try
        {
            if (Shell.Current is null) return false;
            await Shell.Current.GoToAsync(route);
            return true;
        }
        catch (Exception ex) { AppCrashLogger.Log(ex, $"GoToAsync({route})"); return false; }
        finally { Gate.Release(); }
    }

    public static async Task<bool> GoBackAsync()
    {
        // No desapila a ciegas: verifica pila/modal antes de ".."
        var shell = Shell.Current;
        if (shell is null) return false;
        var nav = shell.Navigation;
        if (nav.NavigationStack.Count > 1 || nav.ModalStack.Count > 0)
            return await GoToAsync("..");
        return false;
    }
}
```

Motivación (bug real): navegaciones simultáneas (doble tap rápido, tap + back
rápido) dejan el pipeline de Shell en estado del que no se recupera. La
compuerta serializa y descarta.

Reglas:

- ViewModels llaman `ShellNavigation.GoToAsync` / `GoBackAsync` — **nunca**
  `Shell.Current.GoToAsync` directo.
- Los VM exponen comandos (`OpenDetailCommand`), no métodos de navegación
  cruzada entre VMs.
- El resultado `bool` permite deshabilitar/rehabilitar el origen del comando
  si hace falta.

## 4. Tipos de navegación y cuándo usar cada uno

| Tipo | Cuándo | Ejemplo |
|---|---|---|
| **Tab (absoluta `//TabRoute`)** | Cambiar de sección principal | `//TasksPage`, `//DashboardView` |
| **Jerárquica (ruta global)** | Detalle/creación dentro del contexto de una tab | `task-detail?id=X`, `create-task` |
| **Modal** (`GoToAsync` a una ruta global registrada — Shell la apila como modal) | Flujos de foco único con descarte explícito | Confirmación destructiva, wizard corto |
| **Back** | Siempre `GoBackAsync` con guarda | — |

Decisión: ¿la página es un *destino* con estado propio que el usuario
revisa varias veces → jerárquica apilada. ¿Es una *acción* que se completa o
cancela → modal. ¿Es un *destino principal* → tab.

## 5. Parámetros de navegación

- Query string: `task-detail?id={id}`; el ViewModel implementa
  `IQueryAttributable` (o propiedad `[QueryProperty]`) — tipado en el VM, no
  parseo manual en la página.
- Objetos completos **no** viajan en la query (solo ids + primitivas
  serializables); el destino recarga desde su facade (fuente única de verdad,
  evita desincronización tras editar).
- Estado transitorio de UI (scroll, filtro activo) vive en el ViewModel de la
  tab; al volver de un detalle, la tab recarga datos sutiles en `OnAppearing`
  **sin** perder scroll ni filtro.

## 6. Ciclo de vida de páginas

- `OnAppearing`: carga de datos vía `SafeUi.RunAsync` (protegida) + re-*
  subscripción idempotente de eventos **[P]** (p.ej. `TaskCreated` se
  re-suscribe en cada appearing, no solo en el constructor).
- `OnDisappearing`: `HideNow()` de snackbar + unsubscribe simétrico +
  cancelación de trabajos largos.
- No confiar en el orden OnNavigated/OnAppearing para lógica de datos:
  usar OnAppearing como único punto de carga.
- **[P]** Ningún handler de página (`OnHandlerChanged`, etc.) toca
  `Content`/árbol visual (ver `maui11-standards.md` §2.4).

## 7. Deep links

- `procrastop://task/{id}` (Android App Links + custom scheme): declarado en
  Platforms/Android; el intent resuelve a `task-detail?id={id}`.
  La app .NET 10 tenía pendiente `OnNewIntent` para links en caliente: en
  Maui11, implementarlo desde el inicio (Activity + `OnNewIntent` →
  `ShellNavigation.GoToAsync`).
- Deep links siempre a rutas globales registradas; nunca a tabs con parámetros.

## 8. Estados de navegación y UX

- Transiciones: las de Shell por defecto (ya Material); no desactivar
  animación global.
- Back de Android (gesto/botón): gestionado por Shell; solo interceptar con
  `Shell.SetBackButtonBehavior` cuando un flujo pueda perder datos
  (confirmar descarte).
- Pestaña activa al abrir un detalle: mantener (el usuario vuelve con back
  al mismo contexto).
- Cuando una navegación es descartada por la compuerta (doble toque), la UX
  ya es correcta por definición (la primera navegación gana); no mostrar
  error.

## 9. Smoke test de navegación (verificación)

El proyecto (.NET 10) incluye `NavigationSmokeTest` opt-in
(`PROCRASTOP_NAV_SMOKE=1`): recorre todas las rutas, valida página final,
handler vivo, opacidad y que ningún `TapGestureRecognizer` de plantilla
quede con `Command == null`. En `Maui11` se traslada el mismo patrón
(`Diagnostics/NavigationSmokeTest.cs`). Al añadir/modificar rutas:

1. Añadir la ruta al smoke test.
2. Ejecutarlo (Windows target para velocidad).
3. Confirmar `ok=True`, `match=True`, `sinComando=0` (salvo gestos con
   `Tapped` event y el snackbar).

## 10. Prohibiciones de navegación

1. Navegar por eventos improvisados entre páginas (page-to-page references).
2. `Shell.Current` fuera de `ShellNavigation` y clases de Navigation/.
3. Acoplar ViewModels entre sí para navegar (A conoce el VM de B).
4. `PushAsync`/`PopAsync`/`PushModalAsync` de `INavigation` crudo (rompe el
   modelo de rutas de Shell): en Shell, los modales se abren con
   `GoToAsync` a una ruta global registrada — **nunca** con rutas absolutas
   `//` (esas reemplazan la pila de navegación, no apilan). Excepción real:
   solo cuando la doc oficial de la versión vigente lo soporte para el
   escenario concreto, y centralizado en `ShellNavigation`.
5. Rutas dinámicas/strings mágicos dispersos: centralizar constantes de rutas
   en `Navigation/Routes.cs`.
6. Duplicar registro de rutas (§2).
