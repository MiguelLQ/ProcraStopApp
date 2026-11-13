# UX por módulo y filosofía visual de ProcraStop

## 1. Filosofía visual (referencias Grial + imagen de guía)

Principios extraídos de las referencias y adoptados como criterio de
composición:

1. **Minimalismo funcional**: cada elemento responde a "¿qué decide el
   usuario aquí?". Sin decoración que no comunique.
2. **Superficies suaves**: fondo `Surface`, cards `SurfaceContainer` sin
   bordes; separación por tonalidad y aire, no por líneas.
3. **Espacio negativo generoso**: `SpacingL` mínimo entre cards; márgenes de
   pantalla 20; la densidad se gana con jerarquía, no con compresión.
4. **Jerarquía tipográfica estricta**: máximo 3 niveles por pantalla
   (Headline/Title + Body + Label).
5. **Cards redondeadas (radius 18)** como unidad de contenido; una card = un
   concepto.
6. **Navegación inferior discreta**: TabBar plano, iconos limpios, sin
   etiquetas ruidosas.
7. **TopBar discreto**: título + una acción; nada de headers saturados.
8. **Indicadores de progreso como firma visual**: barras finas (6 dp) y
   anillos cuando celebra un día completo.
9. **Botones compactos y con propósito**: 1 CTA primario por pantalla;
   acciones secundarias como iconos o texto.
10. **Formularios limpios**: un campo por fila, labels siempre visibles,
    validación en línea.
11. **Sin sobrecarga**: máx. ~2 niveles de scroll por pantalla, máx. 5-7
    cards visibles antes de una sección nueva.
12. **Apariencia premium**: consistencia total de tokens > elementos
    "bonitos" aislados.

## 2. Dashboard (Inicio)

Propósito: **orientar el día en 5 segundos** y llevar al usuario a su
siguiente acción.

Composición (de arriba a abajo):

1. **Saludo contextual** (`HeadlineLarge`): "Buenos días, {nombre}" + fecha.
   Variar por hora (mañana/tarde/noche). Nunca datos en el saludo.
2. **Resumen del día** (hero card): progreso diario (anillo o barra),
   nº tareas pendientes hoy, próximo vencimiento. La única card con
   `PrimaryContainer` — el punto focal.
3. **Check-in de estrés** (card `StressCheckIn`): visible solo si no hay
   registro hoy; escala visual empática (1–5 con caras/etiquetas, no números
   fríos). Tras registrar, colapsa a un mini-estado ("Hoy: nivel bajo") con
   acceso a la analítica.
4. **Próxima tarea**: card de la tarea más próxima (la que el engine
   sugiere), con acción directa "Empezar" (abre Pomodoro/ sesión).
5. **Hábitos pendientes de hoy**: fila horizontal de mini-cards con check.
6. **Acciones rápidas**: fila de 2-4 icon buttons (Nueva tarea, Nuevo hábito,
   Técnicas).
7. **Sugerencias del engine** (opcional): 1 card de consejo contextual —
   **máximo una**, con tono de coach, no de notificación.

Reglas:

- **Máximo 5 secciones** visibles por defecto; lo demás detrás de "Ver todo".
- Sin gráficos complejos: los gráficos viven en las pantallas de analítica.
- Empty state (usuario nuevo): onboarding en cards — "Crea tu primera tarea",
  "Define un hábito", "Registra cómo te sientes" — con progreso de setup.
- El Dashboard **no** es un dashboard administrativo: cero tablas, cero
  grillas de KPIs.

## 3. Tasks (Tareas)

Propósito: **ver el día/semana, priorizar, ejecutar sin fricción.**

- Header: fecha seleccionada (selector de día, chips de "Hoy/Maññana/Semana")
  + acceso a vista analítica (icono).
- **Filtros como chips** (`PSChip`, horizontal scroll): Todas / Hoy /
  Prioridad alta / Atrasadas / Completadas. Filtro activo = `PrimaryContainer`.
- Lista: `PSTaskCard`s. Orden: atrasadas → hoy por prioridad/hora → resto.
  Indicador de atraso: badge `Warning` con días de retraso (empático, no
  alarmista).
- **Zona de apertura vs. check separadas** **[P]**: el check completa; el tap
  en la card abre detalle. Nunca ambas acciones en el mismo gesto.
- Acciones rápidas por card (segunda interacción o swipe): editar, posponer.
  Swipe para completar solo si también existe el check visual.
- FAB "+" (único, esquina inferior derecha): `create-task` (ruta global).
- Creación/edición: formulario de una columna, campos agrupados (Qué / Cuándo
  / Detalle), validación en línea, guardado con snackbar de confirmación.
- Empty states: día sin tareas ("Día libre — disfrútalo" con tono positivo)
  vs. sin resultados de filtro (sugerir otro filtro).
- Vencimiento: recordatorio visible como `PSBadge` (Warning/Error por
  urgencia); **no** bloquear la UI con modales de vencimiento.

## 4. Habits (Hábitos)

Propósito: **ritual diario satisfactorio con evidencia de progreso.**

- Header: fecha de hoy + streak global (icono llama `Warning`, número).
- Lista de hábitos: `PSHabitCard` con nombre, streak del hábito, progreso del
  día (si cuantitativo), acciones +/- (numéricos) o check (binario). Estados
  de cumplimiento: pendiente (neutral), hecho hoy (Success, check animado
  sutil), fallado (apagado, **sin** rojo agresivo — el fallo no se castiga
  visualmente).
- Calendario/semana: fila de `PSCalendarItem` (últimos 7 días) con heatmap
  sutil de cumplimiento; tap en día cambia la fecha consultada.
- Progreso semanal: `PSProgressCard` por hábito o resumen general.
- Creación: wizard corto en `create-habit` (ruta global): nombre → frecuencia
  → meta opcional. Preview del hábito antes de guardar.
- Feedback: snackbar al completar; al completar la meta del día, celebración
  breve (anillo que se completa, **no** confeti industrial).
- Disciplina/ranking: vista de analítica accesible desde el header, no como
  tab.

## 5. Stress (módulo, sin tab propia)

Propósito: **conciencia emocional sin ansiedad**. Este módulo exige el mayor
cuidado UX del producto.

- **Registro (check-in)**: dentro de Dashboard (§2). Escala 1–5 con etiquetas
  ("Muy tranquilo → Muy estresado"), controles grandes, opción "Prefiero no
  decirlo". Confirmación empática inmediata ("Gracias. Registrado.").
- **Técnicas de regulación**: cuando el nivel es alto (4–5), la app sugiere —
  no impone — una técnica (respiración, pausa, Pomodoro corto) con botón
  directo. Nunca un modal invasivo ni alertas rojas.
- **Analítica**: tendencias simples (últimos 7/30 días), correlación con
  días de estudio siempre explicada en lenguaje claro ("En tus días con
  menos estrés estudiaste un 40 % más"), sin diagnósticos ni lenguaje
  clínico.
- **Prohibido**: rojo dominante en niveles altos (usar `Warning` con contorno
  suave), comparaciones sociales, gamificación del estrés.
- Visualizaciones: área/línea suave con `Primary`; valores discretos
  etiquetados; sin eje y agresivo con decimales.

## 6. Settings (Ajustes)

- Lista de secciones (`PSListItem`): Apariencia (tema Light/Dark/Sistema),
  Recordatorios, Cursos y horario, Datos (exportar/limpiar), Acerca de.
- Controles: switches nativos estilizados, selectores con `PSBottomSheet`.
- Confirmación destructiva (limpiar datos): diálogo nativo M3 con botón
  `Error` + doble confirmación explícita.

## 7. Cross-cutting UX

- **Una mano**: CTAs primarios en el tercio inferior de la pantalla; acciones
  destructivas lejos del pulgar.
- **Feedback inmediato**: toda acción exitosa → snackbar breve (≤ 2 s) o
  cambio visual de estado; toda acción lenta (> 400 ms) → indicador.
- **Errores**: en línea y accionables, nunca solo toasts que desaparecen.
- **Consistencia de patrones**: lo que es swipe en una lista, es swipe en
  todas; lo que abre detalle con tap en Tareas, lo hace en Hábitos.
- **Onboarding**: primera vez = 3 pasos máx.; las pantallas vacías enseñan
  (empty states con acción), no tutoriales bloqueantes.
- **Textos**: español natural, tú informal, frases cortas, sin jerga
  técnica. Ej.: "Aún no tienes tareas para hoy" ≠ "Lista vacía".
