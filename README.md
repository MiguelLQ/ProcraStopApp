# ProcraStop

Aplicación móvil multiplataforma para ayudar al estudiante universitario a **iniciar, sostener y
cerrar actividades académicas a tiempo**. Construida con **.NET MAUI**, arquitectura en capas,
CQRS ligero y un **motor de recomendaciones basado en reglas** (sin IA).

> **Nota metodológica:** ProcraStop es la intervención (variable independiente). La procrastinación
> académica se mide con la **EPA de 12 ítems** (variable dependiente). Las métricas de la app son
> **indicadores conductuales de uso**, no una puntuación de procrastinación.

---

## Características

- **Tareas**: creación, subtareas/hitos, fecha límite, recordatorio de vencimiento, aviso de inicio
  previsto, primer paso, revisión previa a entrega, archivo y estados.
- **Académico**: cursos con valoración opcional de interés/dificultad (1–5) y horario de clases.
- **Hábitos**: seguimiento diario, metas, rachas, analítica y ranking de disciplina.
- **Estrés**: check-in, historial, analítica y señales para el motor.
- **Técnicas de estudio**: catálogo, recomendador, Pomodoro, sesiones e historial.
- **Ayuda académica**: solicitudes, tiempos hasta pedir/resolver y consultas estancadas.
- **Exámenes**: plan de temas, cobertura, anticipación y simulacros con puntaje.
- **Dashboard**: señales, balance, tendencias e insights.
- **Motor de reglas**: reglas modulares, cooldown y explicaciones (`Message`, `Reason`,
  `SuggestedAction`).
- **UI**: tema claro/oscuro, Shell con flyout y tabs, gráficos SkiaSharp, iconografía Fluent.

---

## Arquitectura

```text
┌──────────────────────────────────────────────────────────┐
│  ProcraStop (MAUI)                                       │
│  XAML · ViewModels · Shell · Servicios de plataforma     │
└───────────────┬───────────────────────┬──────────────────┘
                │                       │
┌───────────────▼──────────┐  ┌─────────▼──────────────────┐
│  ProcraStop.Application  │  │  ProcraStop.Infrastructure │
│  Casos de uso · DTOs     │  │  EF Core · Repositorios    │
│  Métricas · Señales      │  │  Mappers · Recordatorios   │
└───────┬──────────┬───────┘  └─────────┬──────────────────┘
        │          │                    │
┌───────▼──────┐ ┌─▼────────────────────▼──┐
│ ProcraStop.  │ │  ProcraStop.Domain       │
│ Core         │ │  Entidades · Enums       │
│ DecisionEngine│ │  Invariantes            │
└──────────────┘ └──────────────────────────┘
```

| Proyecto | Responsabilidad |
|---|---|
| `src/ProcraStop.Domain` | Entidades, enums e invariantes. Sin dependencias. |
| `src/ProcraStop.Core` | Motor de decisiones, reglas base, contexto y memoria. |
| `src/ProcraStop.Application` | Comandos, queries, handlers, DTOs, métricas y proyecciones. |
| `src/ProcraStop.Infrastructure` | EF Core, repositorios, mappers y recordatorios. |
| `src/ProcraStop` | App MAUI: XAML, ViewModels, Shell y servicios de plataforma. |
| `src/ProcraStop.Shared` | Vacío; pendiente de eliminar o poblar. |
| `tests/ProcraStop.UnitTests` | Pruebas xUnit (47 casos). |

La dirección de dependencias apunta hacia dentro: `Domain` no conoce nada; `Application` conoce
`Domain` y `Core`; `Infrastructure` conoce `Application` y `Domain`; la app MAUI compone todo.

---

## Requisitos

- **.NET SDK 10** (el repositorio fija `10.0.401` en `global.json`).
- **Workloads MAUI**: `android`, `ios`, `maccatalyst`, `maui-windows` y `maui-android`.
- **Android SDK** y un emulador o dispositivo.
- Windows 10/11 para compilar los targets de Windows.
- macOS para compilar iOS/Mac Catalyst.

> **Estado del toolchain:** `dotnet workload list` puede no mostrar `maui-android` en este equipo;
> aun así `dotnet restore` resuelve los packs MAUI vía NuGet y el build Android funciona con el
> SDK fijado en `global.json` (`10.0.401`). Si aparece `NETSDK1147`, ejecutar
> `dotnet workload restore` o `dotnet workload repair` con permisos de administrador.

---

## Inicio rápido

```powershell
# Restaurar dependencias
dotnet restore

# Compilar Android
dotnet build src/ProcraStop/ProcraStopApp.csproj -f net10.0-android

# Ejecutar pruebas
dotnet test tests/ProcraStop.UnitTests/ProcraStop.UnitTests.csproj

# Comprobar vulnerabilidades
dotnet list package --vulnerable

# Ver workloads instalados
dotnet workload list
```

Si `dotnet build` falla por workloads:

```powershell
dotnet workload restore
# o, si persiste:
dotnet workload repair
```

---

## Documentación

| Documento | Contenido |
|---|---|
| [`docs/arquitectura-procrastop-maui11.md`](docs/arquitectura-procrastop-maui11.md) | Arquitectura, UI, UX, patrones, matriz EPA, deuda, migración a .NET 11 y guía para crear skills. |
| [`docs/revision-arquitectura-procrastop.md`](docs/revision-arquitectura-procrastop.md) | Revisión técnica, hallazgos, roadmap y recomendaciones. |
| [`docs/alcance-academico.md`](docs/alcance-academico.md) | Alcance de los flujos y reglas de la primera versión. |

---

## Estado del proyecto

| Área | Estado |
|---|---|
| Arquitectura | ✅ Capas separadas, DI modular, CQRS ligero |
| Motor de reglas | ✅ Funcional; faltan pruebas y determinismo documentado |
| Cobertura D1 (postergación) | ✅ Tareas, cursos y lecturas |
| Cobertura D2 (autorregulación) | ✅ Ayuda académica, exámenes y simulacros; 🟡 asistencia, motivación, revisión |
| Persistencia | 🟡 `EnsureCreated` + upgrades manuales; sin migraciones EF |
| UI/UX | 🟡 Base visual sólida; navegación, accesibilidad y tokens pendientes |
| Pruebas | 🟡 47/47 pasan; faltan motor, ViewModels y CI |
| Investigación | 🔴 Sin consentimiento, participante, eventos ni exportación |
| Migración .NET 11 | 🟡 Planificada; bloqueada por workloads |

### Deuda conocida

- `TasksFacade` y `ProgressToWidthConverter.ConvertBack` lanzan `NotImplementedException`.
- `GetFocusTasksHandler` está completamente comentado.
- `UnitTest1` vacío.
- `SQLitePCLRaw.lib.e_sqlite3` 2.1.11 con `NU1903`.
- 493 colores hexadecimales y 10 familias tipográficas en XAML.
- `MainActivity` no implementa `OnNewIntent` (deep link de notificación).
- `Console.WriteLine` en lugar de `ILogger`.

---

## Próximos pasos

1. Cerrar el circuito de investigación: consentimiento, código de participante, eventos y
   exportación CSV/JSON.
2. Migrar de `EnsureCreated()` a migraciones EF Core.
3. Completar la cobertura EPA: asistencia, motivación, lectura ampliada y revisión.
4. Limpiar código muerto y sustituir `Console.WriteLine` por `ILogger`.
5. Reorganizar la navegación a 5 destinos e implementar onboarding.
6. Migrar a .NET 11 cuando los workloads estén completos.

El plan detallado, los criterios de aceptación y la guía para crear skills están en
[`docs/arquitectura-procrastop-maui11.md`](docs/arquitectura-procrastop-maui11.md).

---

## Licencia

Proyecto académico. Consultar con el autor antes de reutilizar datos o código.
