# Revisión de arquitectura, UX y cobertura EPA — ProcraStop

> Revisión técnica realizada sobre el estado actual del repositorio.
> Enfoque: tesis aplicada, diseño preexperimental de un solo grupo con pretest/postest,
> enfoque cuantitativo, alcance descriptivo-aplicativo.
>
> Fecha de revisión: 2026-11-09

---

## 1. Veredicto general

ProcraStop ya es un **MVP funcional con una base de ingeniería por encima del promedio** de un proyecto de tesis. La separación en capas, el motor de reglas y la cantidad de módulos implementados son un buen punto de partida. Sin embargo, **todavía no es un instrumento de investigación defendible**: le falta el puente entre "app de productividad" y "estudio con datos recolectables, trazables y exportables".

| Dimensión | Nota | Comentario |
|---|---:|---|
| Arquitectura de software | 7/10 | Buenas capas y módulos; hay deuda de validación, migraciones, observabilidad y limpieza. |
| Modelo de dominio | 7/10 | `StudyTask` y `Habit` son ricos; `Course` es anémico y falta el dominio de investigación. |
| Persistencia | 5/10 | EF Core + SQLite con índices, pero `EnsureCreated()`, sin migraciones, sin cifrado y sin multiperfil. |
| Motor de reglas | 6/10 | Buena idea y buena estructura; falta determinismo, persistencia de decisiones y pruebas. |
| UI/UX | 6/10 | Buena base visual; la arquitectura de información, onboarding y accesibilidad están incompletos. |
| Pruebas y CI | 4/10 | 22 pruebas que pasan, pero el motor y las métricas críticas no están cubiertos; no hay CI. |
| Seguridad y privacidad | 3/10 | Sin consentimiento, sin participante, sin cifrado, sin borrado/exportación de datos. |
| Preparación para la tesis | 3/10 | No existe módulo EPA, ni pretest/postest, ni exportación, ni registro de eventos de uso. |

**Conclusión corta:** como producto, el proyecto va bien; como plataforma de investigación, está en una etapa temprana. La prioridad no es agregar más pantallas, sino **cerrar el circuito de investigación** (instrumento, participantes, eventos, exportación) y luego pulir la experiencia.

---

## 2. Lo que ya tienes (y conviene conservar)

### 2.1 Arquitectura

- Solución con 6 proyectos:
  - `ProcraStop.Domain`
  - `ProcraStop.Application`
  - `ProcraStop.Infrastructure`
  - `ProcraStop.Core` (motor de decisiones)
  - `ProcraStop` (MAUI)
  - `ProcraStop.Shared` (vacío)
- Dirección de dependencias correcta en lo esencial:
  - `Domain` no depende de nadie.
  - `Application` depende de `Domain` y `Core`.
  - `Infrastructure` depende de `Application` y `Domain`.
  - La app MAUI compone todo en `MauiProgram.cs`.
- Módulos de aplicación con CQRS "ligero": comandos, consultas, handlers, DTOs, repositorios y fachadas.
- Registro por módulos (`AddTasksModule`, `AddHabitsModule`, etc.), lo cual facilita explicar la arquitectura en la tesis.
- EF Core con SQLite, `IDbContextFactory`, índices explícitos y un interceptor de rendimiento (`DatabasePerformanceInterceptor`).
- Pruebas de dominio y de base de datos: **22 pruebas, todas pasan** (`dotnet test`).
- Trazador de rendimiento de arranque/navegación (`PerformanceTracer`).

### 2.2 Funcionalidad ya implementada

- **Tareas**: creación, edición, subtareas/hitos, fecha límite, recordatorio, inicio previsto, primer paso, revisión previa a entrega, archivo, estados, prioridad, esfuerzo estimado, curso, tipo de actividad (`Task`, `Reading`, `Exam`, `Deliverable`).
- **Académico**: cursos y horario de clases con día, hora, aula, docente y minutos de aviso.
- **Hábitos**: creación, seguimiento diario, metas, reglas de completitud, peso, analítica, ranking de disciplina y reglas de decisión.
- **Estrés**: check-in, historial, analítica y reglas de decisión.
- **Técnicas de estudio**: catálogo, recomendador basado en reglas, Pomodoro, sesiones, historial, solicitudes de ayuda académica y biblioteca de técnicas.
- **Dashboard**: señales, balance, tendencias, snapshots, insights y decisión principal.
- **Motor de reglas**: señales unificadas, reglas por módulo, memoria de decisiones con cooldown, proyección a dashboard y explicaciones (`Message`, `Reason`, `SuggestedAction`).
- **UI**: tema claro/oscuro, estilos centralizados, iconografía Fluent, gráficos SkiaSharp, estados vacíos en varias listas, navegación Shell con flyout y tabs.

### 2.3 Lo que está bien resuelto para la tesis

- La documentación `docs/alcance-academico.md` ya aclara algo fundamental: **los registros de la app no son la EPA ni una puntuación de procrastinación**. Eso es correcto y hay que mantenerlo.
- El motor es **basado en reglas**, no un modelo de IA. Eso evita introducir una tercera variable y es defendible metodológicamente si se documenta como "sistema experto basado en reglas".
- `StudyTask` ya guarda fechas clave para medir comportamiento: `CreatedAtUtc`, `PlannedStartDate`, `StartedAtUtc`, `PlannedReviewDate`, `ReviewedAtUtc`, `CompletedAtUtc`, `DueDate`.
- `AcademicHelpRequest` ya guarda estados y fechas (`Planned`, `Requested`, `Resolved`), lo que permite medir tiempo de búsqueda de ayuda.
- `HabitLog` y `StudyTechniqueSession` ya permiten construir métricas de constancia y dedicación.

---

## 3. La brecha más importante: la tesis necesita un "modo investigación"

### 3.1 Aclaración conceptual

En tu diseño hay dos variables:

- **Variable independiente (VI):** ProcraStop como intervención (uso de la app y sus módulos).
- **Variable dependiente (VD):** procrastinación académica, medida con la EPA.
- **No hay una tercera variable de IA.** El motor de reglas es una característica de la intervención, no una variable del estudio.

La app **no mide la EPA**. La EPA se aplica antes y después con su redacción, escala y puntuación originales. Lo que la app debe hacer es:

1. Aplicar la EPA (o al menos vincular sus resultados) en pretest y postest.
2. Registrar el uso real de la intervención (dosis, adherencia, fidelidad).
3. Exportar ambos conjuntos de datos de forma anónima y trazable.

### 3.2 Lo que falta hoy

| Necesidad | Estado | Consecuencia |
|---|---|---|
| Consentimiento informado | 🔴 Ausente | No hay evidencia ética del participation. |
| Código de participante | 🔴 Ausente | No se pueden vincular pretest, postest y uso de la app. |
| Cuestionario EPA (12 ítems) | 🔴 Ausente | La VD se mide fuera de la app, sin control de calidad. |
| Pretest / postest con fecha | 🔴 Ausente | No hay registro estructurado de los dos momentos. |
| Exportación CSV/JSON | 🔴 Ausente | Los datos quedan atrapados en SQLite; difícil análisis en SPSS/R. |
| Registro de eventos de uso | 🔴 Ausente | No se puede demostrar adherencia ni dosis de intervención. |
| Multiperfil / participantes | 🔴 Ausente | Una app por dispositivo o un solo usuario; no escala a una muestra. |
| Borrado / portabilidad de datos | 🔴 Ausente | Riesgo ético y legal. |
| Migraciones de base de datos | 🔴 Ausente | `EnsureCreated()` no actualiza esquemas; se cambia el nombre del `.db` a mano. |
| Pruebas del motor y métricas | 🟡 Parcial | No hay evidencia técnica de que las recomendaciones sean estables. |

### 3.3 Dos caminos válidos

**Opción A — Modo investigación dentro de la app (recomendado si tienes tiempo):**

- Pantalla de bienvenida con consentimiento y código de participante.
- Pretest EPA de 12 ítems (Likert 1–5) con instrucciones neutras.
- Uso normal de la app durante N semanas.
- Postest EPA al finalizar.
- Exportación CSV/JSON por participante y global.
- Panel del investigador protegido por PIN para ver avance y exportar.

**Opción B — Instrumento externo + vinculación por código (recomendado si el tiempo aprieta):**

- La EPA se aplica en Google Forms o papel.
- La app solo pide un **código de participante** al inicio.
- Toda la actividad se etiqueta con ese código.
- La app exporta CSV con ese código.
- Tú unes pretest/postest con uso en Excel/SPSS/R.

En ambos casos, **sin código de participante y sin exportación no hay tesis cuantitativa defendible**.

### 3.4 Modelo de datos mínimo para investigación

```text
ResearchParticipant
  Id, ParticipantCode, ConsentVersion, ConsentAcceptedAtUtc,
  PretestStartedAtUtc, PretestCompletedAtUtc,
  PosttestStartedAtUtc, PosttestCompletedAtUtc,
  Group, CreatedAtUtc

EpaResponse
  Id, ParticipantId, Timepoint (Pre/Post), ItemNumber (1..12),
  Dimension (D1/D2), RawScore (1..5), ReverseScored, CreatedAtUtc

AppEvent
  Id, ParticipantId, EventType, EntityType, EntityId,
  OccurredAtUtc, AppVersion, MetadataJson

AttendanceRecord
  Id, ParticipantId, ClassScheduleId, Date, Status (Present/Absent/Late/Justified),
  Reason, RegisteredAtUtc

NotificationLog
  Id, ParticipantId, NotificationType, EntityId,
  ScheduledAtUtc, DeliveredAtUtc, OpenedAtUtc

StudyGoal
  Id, ParticipantId, CourseId, WeekStart, TargetMinutes, ActualMinutes

ReviewRecord
  Id, ParticipantId, TaskId, ReviewedAtUtc,
  ChecklistJson, NextReviewAtUtc

Reward
  Id, ParticipantId, BadgeCode, EarnedAtUtc, RedeemedAtUtc
```

Eventos mínimos a registrar desde el primer día:

- `app_opened`, `app_closed`
- `task_created`, `task_started`, `task_completed`, `task_reviewed`, `task_archived`
- `subtask_completed`
- `reminder_scheduled`, `reminder_delivered`, `reminder_opened`
- `habit_created`, `habit_logged`
- `study_session_started`, `study_session_completed`, `pomodoro_completed`
- `help_requested`, `help_resolved`
- `exam_plan_created`, `exam_simulation_completed`
- `class_attended`, `class_missed`
- `recommendation_shown`, `recommendation_followed`
- `pretest_started`, `pretest_completed`, `posttest_started`, `posttest_completed`
- `data_exported`

Sin `recommendation_shown` / `recommendation_followed` no puedes evaluar si el motor sirvió de algo.

---

## 4. Cobertura de las dimensiones e indicadores EPA

Leyenda: ✅ cubierto · 🟡 parcial · 🔴 ausente.

### D1 — Postergación de actividades

| Indicador | Módulo actual | Cobertura | Qué falta |
|---|---|---|---|
| Postergación de tareas al último momento | Tareas + analítica + motor | 🟡 | Métrica de "tiempo hasta el primer avance": `StartedAtUtc - CreatedAtUtc`, `StartedAtUtc - PlannedStartDate`, `CompletedAtUtc - DueDate`. Aviso cuando llega `PlannedStartDate` y no se inició. Razón de postergación. |
| Postergación de trabajos de cursos no preferidos | Tareas + Cursos + motor | 🟡 | Preferencia/aversión por curso. Hoy `Course` solo tiene nombre, descripción y color. Agregar `PreferenceLevel` (1–5) o `AversionLevel`. Regla "estás evitando el curso X". |
| Postergación de lecturas de cursos no preferidos | `ActivityType.Reading` + Tareas | 🔴 | Flujo de lectura: páginas/minutos, sesión de lectura, verificación de comprensión, recordatorio de lectura, vínculo con curso no preferido. |

### D2 — Autorregulación académica

| Indicador | Módulo actual | Cobertura | Qué falta |
|---|---|---|---|
| Preparación para exámenes | Examen + `ExamPlanBuilder` + técnicas | 🟡 | Vínculo entre la tarea de examen y las sesiones de estudio. Hoy `StudyTechniqueSession` solo guarda `CourseName` y `Topic`, no `TaskId`. Falta puntaje de simulacro, cobertura por tema y repaso espaciado. |
| Búsqueda de ayuda académica | `AcademicHelpRequest` + pantalla de ayuda | ✅/🟡 | Ya hay estados y fechas. Falta recordatorio de seguimiento, tiempo hasta pedir ayuda, tiempo hasta resolver y vínculo con la recomendación que la originó. |
| Asistencia a clases | Horario (`ClassSchedule`) | 🔴 | No hay registro de asistencia. `ReminderMinutesBefore` se guarda pero **no se usa para programar ninguna notificación**. Falta marcar presente/ausente/tarde/justificado y calcular tasa de asistencia. |
| Realización oportuna de tareas | Tareas + analítica | 🟡 | Hay `Completed`, `Overdue` y tasa de finalización, pero no una métrica explícita de "a tiempo" (`CompletedAtUtc <= DueDate`), ni tendencia de puntualidad, ni motivo de entrega tardía. |
| Mejora de hábitos de estudio | Hábitos + analítica + ranking | ✅ | Bien cubierto. Falta plantillas de hábitos de estudio y vínculo explícito con metas académicas. |
| Dedicación al estudio | Pomodoro + sesiones + dashboard | 🟡 | El Pomodoro **no persiste la sesión**, no se vincula a una tarea/curso y se pierde si se cierra la app. Falta meta semanal de minutos por curso y registro de interrupciones. |
| Motivación para estudiar | Ranking de hábitos | 🔴 | No existe módulo de metas, recompensas, insignias, rachas canjeables o celebraciones. El ranking es lo más cercano, pero no es motivación explícita. |
| Entrega anticipada de trabajos | Tareas + revisión | 🟡 | Se puede calcular con `CompletedAtUtc < DueDate`, pero no hay un estado/timestamp explícito de "entregado", ni indicador de entrega anticipada, ni celebración/refuerzo. |
| Revisión de tareas | Revisión de entregables | 🟡 | Solo los `Deliverable` exigen revisión. No hay checklist de revisión, ni repaso espaciado de tareas/notas ya completadas, ni recordatorio de repaso. |

### Prioridad de módulos nuevos

1. **Asistencia a clases** (indicador hoy ausente y con datos ya modelados).
2. **Motivación y recompensas** (indicador completamente ausente).
3. **Flujo de lectura** (indicador ausente).
4. **Preferencia/aversión por curso** (habilita D1 completo).
5. **Revisión/repaso espaciado** (indicador parcial).
6. **Pomodoro persistente + metas de estudio** (indicador parcial).
7. **Examen vinculado a sesiones y simulacros** (indicador parcial).

---

## 5. Hallazgos de arquitectura

### 5.1 Fortalezas

- Capas separadas y dependencias mayormente correctas.
- Un solo motor de decisiones en `ProcraStop.Core`, alimentado por reglas de cada módulo (`EngineTask`, `EngineHabits`, `EngineStress`). La idea es coherente: **no hay dos motores**, hay un motor con reglas modulares.
- Persistencia con `IDbContextFactory`, lo cual es adecuado para MAUI.
- Índices declarados y probados (`DatabaseIndexPlanTests`).
- Uso de `record`/DTOs y mapeos explícitos.
- Módulos de UI registrados por `DependencyInjection` y `*Module`.

### 5.2 Deuda técnica prioritaria

| Hallazgo | Evidencia | Riesgo | Recomendación |
|---|---|---|---|
| `EnsureCreated()` y cambio manual de nombre de BD (`procra-v3.db`) | `PersistenceDependencyInjection.cs`, `MauiProgram.cs` | Pérdida de datos y esquemas inconsistentes | Migraciones EF Core versionadas + `Migrate()` + pruebas de actualización. |
| `ProcraStop.Shared` vacío y sin uso | `src/ProcraStop.Shared` | Ruido arquitectónico | Eliminarlo o mover allí contratos realmente compartidos. |
| `TasksFacade` con `NotImplementedException` | `Application/Tasks/Facade/TasksFacade.cs` | Fachada engañosa; riesgo de fallo en runtime | Implementar o eliminar; si no se usa, borrar. |
| `GetFocusTasksHandler` completamente comentado | `Application/Tasks/Queries/GetFocusTasks` | Código muerto | Eliminar o implementar. |
| `ProgressToWidthConverter` lanza `NotImplementedException` | `Modules/Dashboard/Presentation/Converters` | Posible crash si se usa | Implementar o eliminar. |
| `UnitTest1` vacío | `tests/ProcraStop.UnitTests/UnitTest1.cs` | Ruido | Eliminar. |
| Dos implementaciones de recordatorios | `Infrastructure/Tasks/Services/TaskReminderService.cs` y `Platforms/Android/Services/AndroidTaskReminderService.cs` | Comportamiento distinto por plataforma; deuda | Definir una sola abstracción y una estrategia por plataforma documentada. |
| `Console.WriteLine` en producción | 20+ apariciones en ViewModels, repositorios y servicios | Ruido, PII en logs, no estructurado | Sustituir por `ILogger<T>` y logs estructurados. |
| Sin validación de comandos | Handlers reciben comandos sin `FluentValidation` ni `Result<T>` | Errores de negocio cruzan capas como excepciones | Añadir validadores y un `Result`/`Error` uniforme. |
| Sin Unit of Work | Cada repositorio crea su `DbContext` | Operaciones compuestas no transaccionales | Evaluar `IUnitOfWork` o transacciones explícitas para flujos críticos. |
| Recordatorio fuera de la transacción | `CreateTaskCommandHandler` guarda y luego programa | Tarea guardada sin recordatorio si falla la notificación | Patrón outbox o reintento con log. |
| El motor ignora `Priority` | `IDecisionRule.Priority` no se usa en `DecisionEngine` | Reglas no compiten como se espera | Usar prioridad como desempate o eliminar la propiedad. |
| Memoria del motor en RAM | `InMemoryDecisionMemoryStore` | Cooldowns se pierden al reiniciar | Persistir memoria/decisiones o documentar que es efímera. |
| `GetRecentAsync(int days)` y nombres "ThisWeek" | `StudyDashboardService` | Métrica semanal engañosa | Renombrar a "últimos 7 días" o calcular semana calendario. |
| `CalculateHelpNeedScore(3, 3, sessions)` | `StudyDashboardService` | Ignora el check-in real del estudiante | Usar la última sesión/estado real. |
| `TaskReminderService` usa `Guid.GetHashCode()` | `Infrastructure/Tasks/Services/TaskReminderService.cs` | IDs de notificación inestables/colisiones | Usar un ID estable derivado del GUID (por ejemplo, hash determinista). |
| `Task.Delay(500)` y logs con emojis | `TaskReminderService` | Código de depuración en producción | Eliminar esperas artificiales y logs de desarrollo. |

### 5.3 Seguridad y privacidad

- No hay cifrado de SQLite. Para datos de estudiantes y respuestas EPA, evaluar SQLCipher o, como mínimo, una política de anonimización y borrado.
- No hay autenticación ni perfiles. Para un estudio con varios participantes, se necesita al menos un código de participante y separación lógica de datos.
- No hay consentimiento, política de privacidad ni opción de eliminar datos.
- La app declara `POST_NOTIFICATIONS` en el manifest, pero **no hay una solicitud explícita del permiso en el código de la app** para Android 13+. El servicio Android verifica el permiso y, si no está concedido, no muestra la notificación. El plugin de notificaciones podría solicitarlo en algunos flujos, pero el servicio personalizado no lo garantiza; conviene añadir una solicitud explícita con explicación.
- El manifest tiene `android:allowBackup="true"`. Para datos de investigación, conviene desactivarlo o documentar el riesgo.
- No hay `OnNewIntent` ni manejo del deep link `procrastop://task/{id}`: al tocar la notificación se abre la app, pero no la tarea.

### 5.4 Pruebas y CI

- 22 pruebas que pasan, pero se concentran en dominio y base de datos.
- Sin pruebas de:
  - `DecisionEngine` y sus reglas.
  - `StudyRecommendationEngine`.
  - `HabitMetricsService`, `StressMetricsService`, `TrendEngine`.
  - `TaskAnalyticsService` y métricas de puntualidad.
  - Exportación y serialización.
  - ViewModels críticos.
- No hay pipeline de CI (`.github/workflows` ausente).
- `dotnet test` reporta una vulnerabilidad de severidad alta en `SQLitePCLRaw.lib.e_sqlite3` 2.1.11 (`GHSA-2m69-gcr7-jv3q`). Actualizar el paquete.
- No hay `Directory.Build.props`, `.editorconfig`, análisis estático ni cobertura mínima.

### 5.5 Documentación

- `README.md` está vacío.
- Solo existe `docs/alcance-academico.md`.
- Faltan:
  - Diagrama de arquitectura.
  - ADRs (decisiones de arquitectura).
  - Diccionario de datos.
  - Matriz indicador EPA → módulo → dato → métrica.
  - Manual de usuario.
  - Protocolo de investigación.
  - Guía de despliegue y recolección de datos.

---

## 6. Hallazgos de UI/UX

### 6.1 Fortalezas

- Tema claro/oscuro y colores de marca.
- Uso de `Shell`, flyout, tabs y rutas.
- Tarjetas, gráficos y jerarquía visual en dashboard.
- Algunos estados vacíos y `RefreshView`.
- Iconografía consistente con Fluent UI.
- Copy en español y orientado al estudiante.

### 6.2 Problemas de arquitectura de información

La navegación actual tiene **6 grupos en el flyout y 12 tabs**, con etiquetas duplicadas y una organización que no responde a las tareas del estudiante:

- "Tareas" contiene tabs "Tareas", "Dashboard" y "Stress".
- "Dashboard" dentro de Tareas apunta a `TaskAnalyticsPage`, no al dashboard general.
- "Inicio" es otro flyout item separado.
- "Hábitos" y "Técnicas de estudio" son módulos separados, aunque conceptualmente pertenecen al mismo flujo de autorregulación.
- "Configuración" es un placeholder "Próximamente".

**Recomendación de arquitectura de información:**

```text
Inicio (Hoy)
  - Próxima acción
  - Check-in rápido
  - Próxima clase
  - Racha / progreso
Tareas
  - Hoy / Semana / Todas
  - Calendario
  - Revisión
Horario
  - Horario semanal
  - Asistencia
Estudio
  - Técnicas
  - Pomodoro
  - Exámenes
  - Ayuda académica
Progreso
  - Métricas de tareas
  - Hábitos
  - Bienestar
  - Insights
Perfil / Configuración
  - Participante
  - Privacidad
  - Exportar datos
  - Notificaciones
```

### 6.3 Onboarding y primera impresión

Hoy no hay onboarding. Para una tesis, la primera sesión debe:

1. Explicar qué es ProcraStop y qué no es (no es terapia, no es diagnóstico).
2. Pedir consentimiento informado (si aplica).
3. Pedir código de participante.
4. Solicitar permisos de notificaciones con explicación.
5. Configurar cursos y horario mínimos.
6. Crear la primera tarea con "primer paso pequeño".
7. Mostrar un "Hoy" con una acción concreta.

Sin onboarding, la adherencia inicial y la calidad de los datos caen.

### 6.4 Accesibilidad

- Solo **2** referencias a `SemanticProperties` y **0** a `AutomationProperties` en 32 archivos XAML.
- Falta:
  - Etiquetas semánticas en botones, iconos y gráficos.
  - Descripciones de estado para lectores de pantalla.
  - Contraste verificado en modo claro/oscuro.
  - Tamaños táctiles mínimos (48 dp).
  - Soporte de escalado de fuente.
  - Navegación por teclado en Windows.
- Los gráficos SkiaSharp no tienen alternativa textual.

### 6.5 Sistema de diseño

- **303** colores hexadecimales hardcodeados en XAML.
- **249** atributos `FontFamily` en XAML.
- **7+ familias tipográficas**: Montserrat, OpenSans, Inter, Manrope, Lato, Segoe UI, Fluent.
- Hay un typo: `FontFamily="Monts erratSemiBold"`.
- No hay tokens semánticos consistentes (`Surface`, `OnSurface`, `Primary`, `Danger`, etc.).
- Recomendación:
  - 2 familias máximo (por ejemplo, Manrope para títulos e Inter/OpenSans para cuerpo).
  - Tokens de color semánticos en `Colors.xaml`.
  - Estilos por rol, no por página.
  - Eliminar fuentes no usadas.
  - Revisar contraste y modo oscuro.

### 6.6 Estados y retroalimentación

- Hay `EmptyView` en varias listas, pero no en todas.
- No hay estados de error consistentes ni mensajes accionables.
- No hay indicador de offline.
- No hay skeletons de carga; algunas pantallas muestran contenido vacío mientras cargan.
- El artefacto `artifacts/procrastop-current.png` muestra una pantalla vacía con tabs; conviene verificar si es un estado de carga, un fallo de render en Android o una captura antigua.
- Los `Snackbar` y `Toast` existen, pero hay mucho `Console.WriteLine` y poca telemetría de errores.

### 6.7 Otras oportunidades de UX

- Deep link de notificación a la tarea concreta.
- Acción rápida "Empezar ahora" desde la notificación.
- Widget de próxima tarea/clase.
- "Modo enfoque" que bloquee distracciones (fuera del alcance inicial).
- Celebración de entrega anticipada y rachas.
- Resumen semanal ("Esta semana avanzaste en 3 cursos, pero evitastes Cálculo").
- Pregunta de cierre: "¿Qué te impidió avanzar hoy?" con opciones (cansancio, ansiedad, distracción, no sé por dónde empezar).
- Recordatorio de revisión de tareas completadas (repaso espaciado).

---

## 7. Recomendaciones para el motor de reglas

El motor es una buena decisión. Para que sea defendible en la tesis:

1. **Nómbralo correctamente**: "sistema experto basado en reglas" o "motor de recomendaciones basado en reglas", no "IA".
2. **Documenta el catálogo de reglas** en una tabla:

   | Regla | Condición | Mensaje | Acción sugerida | Fundamento |
   |---|---|---|---|---|
   | `OverdueOverloadRule` | Tareas vencidas > N | "Tienes carga vencida" | "Elige una tarea de 10 min" | Carga cognitiva |
   | `AvoidingHardTasksRule` | Tareas difíciles postergadas | "Estás evitando lo difícil" | "Empieza por 2 minutos" | Activación conductual |
   | ... | ... | ... | ... | ... |

3. **Haz el scoring determinista**: usa `Priority`, define desempates y prueba cada regla con casos límite.
4. **Persiste las decisiones mostradas**: `recommendation_shown`, `recommendation_followed`, timestamp, regla que ganó, score.
5. **Explica siempre**: mensaje + razón + acción + señal que la originó. Ya lo haces parcialmente; consolídalo.
6. **Evita métricas compuestas presentadas como validadas**: "health score", "balance" y "riesgo" son heurísticas. Etiquétalas como "indicador interno de la app", no como constructo psicológico.
7. **Versiona las reglas**: `RulesVersion`. Si cambias una regla a mitad del estudio, los datos cambian de significado.
8. **Prueba el motor**: tests de reglas, de selección, de cooldown y de explicaciones.
9. **No satures**: limita a 1 recomendación principal + 2 secundarias, con cooldown. Ya existe, pero documéntalo.
10. **No uses el motor para medir la VD**. Solo para recomendar.

---

## 8. Recomendaciones metodológicas para la tesis

### 8.1 Diseño

- Un solo grupo, pretest y postest, N semanas de intervención (4–6 semanas es razonable).
- Define claramente:
  - Población y muestra.
  - Criterios de inclusión/exclusión.
  - Tiempo de exposición mínima (por ejemplo, ≥3 tareas y ≥2 sesiones de estudio por semana).
  - Defines "uso activo" de la app.
- Amenazas a la validez interna: historia, maduración, testing, regresión a la media, mortalidad experimental. Documéntalas y mitiga con:
  - Grupo de comparación si es posible (aunque sea cuasi-experimental).
  - Registro de eventos y adherencia.
  - Entrevista breve de cierre.
  - Triangulación con EPA.

### 8.2 Instrumento

- Usa la versión validada de la EPA (12 ítems) con su redacción y escala originales.
- Dimensiones:
  - D1 Postergación de actividades: ítems 1, 6 y 7.
  - D2 Autorregulación académica: ítems 2, 3, 4, 5, 8, 9, 10, 11 y 12.
- Verifica en la fuente si hay ítems inversos y cómo se puntúa. No lo improvises.
- Calcula:
  - Puntaje total.
  - Puntaje por dimensión.
  - Alfa de Cronbach en tu muestra.
  - Normalidad (Shapiro-Wilk).
- Análisis:
  - Wilcoxon si no hay normalidad o si tratas los ítems como ordinales.
  - t de Student pareada si hay normalidad y suma puntajes.
  - Tamaño del efecto (r o d de Cohen).
  - Estadísticos descriptivos y gráficos pre/post.
- No presentes las métricas de la app como "nivel de procrastinación". Son **indicadores conductuales de uso**, no la VD.

### 8.3 Ética

- Consentimiento informado.
- Anonimato mediante código de participante.
- Derecho a retirarse y a eliminar sus datos.
- Protección de datos según la normativa de tu país.
- Autorización/cita de los autores de la EPA.
- Aprobación del comité de ética de tu universidad, si aplica.

### 8.4 Tamaño muestral

- Para un pretest/postest con Wilcoxon o t pareada, apunta a **≥30 participantes** si es posible.
- Un piloto de 10–15 usuarios sirve para validar usabilidad y bugs, no para inferencia.
- Si el tiempo aprieta, reporta el estudio como piloto y ajusta las conclusiones.

### 8.5 Evaluación de usabilidad

- Aplica SUS (System Usability Scale) o un cuestionario UX breve, **separado de la EPA**.
- Mide:
  - Tiempo para crear la primera tarea.
  - Tareas completadas en la primera sesión.
  - Errores reportados.
  - Abandono por pantalla.
- Esto fortalece la discusión y la carta de presentación profesional.

---

## 9. Roadmap sugerido

### Fase 0 — Cerrar el circuito de investigación (1–2 semanas)

1. Escribir `docs/protocolo-investigacion.md`: objetivos, hipótesis, variables, muestra, instrumento, procedimiento, análisis y ética.
2. Definir el código de participante y el flujo pretest/postest.
3. Implementar `ResearchParticipant`, `EpaResponse` y `AppEvent` (o al menos el evento y el código).
4. Implementar exportación CSV/JSON con diccionario de datos.
5. Añadir consentimiento y borrado de datos.
6. Migrar de `EnsureCreated()` a migraciones EF Core.
7. Actualizar `SQLitePCLRaw` por la vulnerabilidad alta.
8. Crear CI mínimo: `dotnet restore`, `dotnet build`, `dotnet test`.

### Fase 1 — Completar la cobertura EPA (2–4 semanas)

9. Asistencia a clases: registro y notificaciones usando `ReminderMinutesBefore`.
10. Motivación y recompensas: metas semanales, insignias, rachas, celebración de logros.
11. Pomodoro persistente + vínculo con tarea/curso + metas de minutos.
12. Preferencia/aversión por curso + regla de evitación.
13. Flujo de lectura: sesiones, páginas/minutos, comprensión.
14. Examen vinculado a sesiones + simulacro con puntaje + repaso espaciado.
15. Revisión/repaso de tareas completadas + checklist.
16. Entrega anticipada: `SubmittedAtUtc`, clasificación on-time/early/late.
17. Intenciones de implementación: "Si… entonces…", cuándo/dónde/cómo.

### Fase 2 — Pulido profesional (2–4 semanas)

18. Reorganizar navegación y crear pantalla "Hoy".
19. Onboarding + permisos + consentimiento.
20. Sistema de diseño: tokens, 2 tipografías, accesibilidad, contraste.
21. Estados de carga/error/offline/empty consistentes.
22. Deep link de notificación a la tarea.
23. Sustituir `Console.WriteLine` por `ILogger`, añadir manejo global de excepciones y crash reporting.
24. Eliminar código muerto y `NotImplementedException`.
25. Pruebas del motor, métricas, exportación y ViewModels críticos.
26. Documentación: README, diagrama, ADRs, diccionario de datos, manual.
27. Evaluación de usabilidad (SUS/UX) y análisis de resultados.

---

## 10. Quick wins (1–2 días)

- Actualizar `SQLitePCLRaw` y verificar que `dotnet test` siga pasando.
- Eliminar `UnitTest1.cs`, `GetFocusTasksHandler` comentado, `TasksFacade` no implementado y `ProgressToWidthConverter` no implementado.
- Reemplazar `Console.WriteLine` por `ILogger` en los flujos principales.
- Corregir `FontFamily="Monts erratSemiBold"`.
- Reducir a 2 familias tipográficas y mover colores a tokens.
- Añadir `SemanticProperties.Description` a botones e iconos principales.
- Solicitar `POST_NOTIFICATIONS` en runtime en Android 13+.
- Manejar `OnNewIntent` para abrir la tarea desde la notificación.
- Persistir la sesión de Pomodoro al completar.
- Añadir un `README.md` con arquitectura, cómo ejecutar y cómo probar.
- Añadir un registro mínimo de eventos (`AppEvent`) y exportación CSV.
- Añadir un campo de código de participante en la configuración.

---

## 11. Matriz de trazabilidad sugerida para la tesis

| Dimensión | Indicador | Módulo(s) | Dato que se registra | Métrica conductual | Instrumento |
|---|---|---|---|---|---|
| D1 | Postergación al último momento | Tareas | `CreatedAtUtc`, `PlannedStartDate`, `StartedAtUtc`, `CompletedAtUtc`, `DueDate` | ✅ Implementado: tiempo medio hasta el primer avance, retraso medio vs inicio previsto, % de inicios puntuales, % de inicios en las últimas 24 h y aviso de tareas con inicio vencido sin empezar | EPA ítems 1, 6, 7 |
| D1 | Cursos no preferidos | Tareas + Cursos | `CourseId`, `PreferenceLevel`, tiempos | Brecha de retraso por curso, tareas evitadas | EPA ítems 1, 6, 7 |
| D1 | Lecturas no preferidas | Tareas + Lecturas | `ActivityType.Reading`, sesiones, páginas | % de lecturas iniciadas a tiempo | EPA ítems 1, 6, 7 |
| D2 | Preparación de exámenes | Exámenes + Estudio | Plan, sesiones, simulacros | Cobertura de temas, puntaje de simulacro, días de anticipación | EPA ítems 2, 8, 9, 10 |
| D2 | Búsqueda de ayuda | Ayuda académica | `CreatedAtUtc`, `RequestedAtUtc`, `ResolvedAtUtc`, canal | Tiempo hasta pedir ayuda, tasa de resolución | EPA ítems 3, 4 |
| D2 | Asistencia a clases | Horario + Asistencia | `AttendanceRecord` | % de asistencia, tardanzas, faltas justificadas | EPA ítem 4 |
| D2 | Tareas oportunas | Tareas | `CompletedAtUtc`, `DueDate` | % on-time, días de anticipación/retraso | EPA ítems 5, 11 |
| D2 | Hábitos de estudio | Hábitos | `HabitLog` | Consistencia, racha, tasa de fallo | EPA ítems 8, 9 |
| D2 | Dedicación al estudio | Pomodoro + Sesiones | `StudyTechniqueSession` | Minutos/semana, sesiones/semana, foco promedio | EPA ítems 9, 10 |
| D2 | Motivación | Motivación/Recompensas | `Reward`, metas, rachas | Logros obtenidos, metas cumplidas | EPA ítem 10 |
| D2 | Entrega anticipada | Tareas | `SubmittedAtUtc`, `DueDate` | % de entregas anticipadas, días de anticipación | EPA ítems 11, 12 |
| D2 | Revisión de tareas | Revisión/Repaso | `ReviewRecord` | % de tareas revisadas, repasos completados | EPA ítem 12 |

### 11.1. Cobertura D1 implementada

- **Primer avance:** se registra con `StartedAtUtc` y se calcula el tiempo desde `CreatedAtUtc` (media en horas/días en la pantalla de análisis).
- **Retraso al iniciar:** diferencia entre la fecha local de `StartedAtUtc` y `PlannedStartDate`; se muestra la media y el % de inicios puntuales.
- **Inicio al último momento:** % de tareas cuyo primer avance ocurre en las últimas 24 h antes de `DueDate` (o después del vencimiento).
- **Aviso de inicio:** cada tarea puede tener dos notificaciones independientes (vencimiento e inicio previsto). El aviso de inicio se programa a las 08:00 de `PlannedStartDate` o, si esa fecha ya pasó, unos minutos después de guardar la tarea. Se cancela al iniciar, completar o archivar.
- **Señal en la lista:** las tareas con fecha de inicio vencida y sin empezar muestran «Debías empezar hace X día(s)».


---

## 12. Archivos clave revisados

- `docs/alcance-academico.md`
- `src/ProcraStop/MauiProgram.cs`
- `src/ProcraStop/AppShell.xaml`
- `src/ProcraStop.Core/Decisions/Engine/DecisionEngine.cs`
- `src/ProcraStop.Application/Decisions/DecisionContextBuilder.cs`
- `src/ProcraStop.Application/Dashboard/Services/DashboardProjectionService.cs`
- `src/ProcraStop.Domain/Tasks/Entities/StudyTask.cs`
- `src/ProcraStop.Domain/Habits/Entities/Habit.cs`
- `src/ProcraStop.Domain/StudyTechniques/Entities/StudyTechniqueSession.cs`
- `src/ProcraStop.Infrastructure/DBContext/AppDbContext.cs`
- `src/ProcraStop.Infrastructure/DBContext/PersistenceDependencyInjection.cs`
- `src/ProcraStop.Infrastructure/Tasks/Services/TaskReminderService.cs`
- `src/ProcraStop/Platforms/Android/Services/AndroidTaskReminderService.cs`
- `src/ProcraStop.Application/StudyTechniques/Services/StudyRecommendationEngine.cs`
- `src/ProcraStop.Application/StudyTechniques/Services/StudyDashboardService.cs`
- `src/ProcraStop/Modules/StudyTechniques/Presentation/ViewModels/PomodoroViewModel.cs`
- `tests/ProcraStop.UnitTests/*`

---

## 13. Recomendación final

No agregues más módulos antes de cerrar el circuito de investigación. El orden correcto es:

1. **Instrumento + participantes + eventos + exportación** (sin esto no hay tesis cuantitativa).
2. **Cobertura EPA faltante** (asistencia, motivación, lectura, revisión, entrega anticipada).
3. **Pulido de arquitectura y UX** (migraciones, accesibilidad, navegación, diseño, pruebas, CI).
4. **Documentación y evaluación de usabilidad**.

El motor de reglas es un diferenciador valioso, pero su valor para la tesis no está en "parecer inteligente", sino en **ser transparente, reproducible y medible**. Si documentas las reglas, registras cuándo se mostraron y si el estudiante las siguió, puedes defenderlo como parte de la intervención y no como una caja negra.
