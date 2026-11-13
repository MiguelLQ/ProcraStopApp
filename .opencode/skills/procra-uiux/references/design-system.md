# Design System de ProcraStop

Fuente única de verdad para tokens visuales. Los valores provienen de la
paleta existente de ProcraStop (app .NET 10) renombrados a semántica
Material 3. Al crear `Resources/Styles/` en `ProcraStop.Maui11`, estos tokens
son la especificación.

## 1. Principios de identidad

ProcraStop transmite: **productividad, claridad, calma, progreso, autocontrol,
bienestar**. Traducción práctica:

- Superficies suaves con mucho aire (light: gris azulado `#F1F3F8`; dark:
  azul-noche `#222631`) — nunca blanco puro ni negro puro a full screen.
- Un único color de acción (azul `#4075FF`). Los semánticos (success/error)
  solo aparecen cuando comunican estado real, no como decoración.
- Cards con radios 18 y **sin bordes ni sombras por defecto**: la separación
  se logra por tonalidad (superficie elevada sobre fondo).
- Tipografía con jerarquía marcada por tamaño/peso, no por colores extra.
- Animaciones discretas (fade/scale 150–250 ms); nada de rebotes ni spins
  llamativos. El estrés del usuario es el enemigo del producto.

Anti-patrones prohibidos: dashboard administrativo saturado, tablas densas,
bordes en todo, sombras exageradas, gradientes agresivos, rojo/verde
decorativos.

## 2. Tokens de color

### 2.1 Tabla canónica (nuevo nombre → valor → alias legacy en `src/ProcraStop`)

| Token | Light | Dark | Alias legacy / uso |
|---|---|---|---|
| `Primary` | `#4075FF` | `#6590FF` | `primarioS`, `primarioO` |
| `OnPrimary` | `#FFFFFF` | `#FFFFFF` | texto sobre botones primarios |
| `PrimaryContainer` | `#D9E3FF` | `#1F2845` | `secondaryO`, `secondaryS` (fondos de selección, chips activos) |
| `OnPrimaryContainer` | `#1F2845` | `#D9E2FF` | — |
| `Surface` (fondo de página) | `#F1F3F8` | `#222631` | `LightBackground`, `DarkBackground` |
| `SurfaceContainer` (cards) | `#FFFFFF` | `#171B24` | `LightSecondaryBackground`, `DarkSecondaryBackground` |
| `SurfaceContainerHigh` (sheets, dialogs) | `#FFFFFF` | `#1F2430` | — |
| `TabBar` | `#FFFFFF` | `#131821` | `TapBarDarck` |
| `Outline` (bordes de inputs) | `#DBE0ED` | `#333949` | `BackgroundTertiaryColor`, `EntryDark` |
| `TextPrimary` | `#12141B` | `#FEFFFF` | `Titlecl`, `Title` |
| `TextSecondary` | `#6F7A8F` | `#AAB0C0` | `Subtitlecl`, `Subtitle` |
| `TextTertiary` | `#9CA3AF` | `#6B7280` | color de tab inactiva |
| `TextDisabled` | `#B0B7CA` | `#8F94A7` | `PlaceHolderClar`, `PlaceHolderDarck` |
| `Success` | `#46C771` | `#5BD78B` | `saveButton` |
| `SuccessContainer` | `#E4F8EC` | `#1E3328` | — |
| `Warning` | `#F59E0B` | `#FBBF24` | streaks, vencimiento cercano |
| `Error` | `#E5484D` | `#FF6B6E` | `CancelButton` (legacy `#FF2525` se sustituye) |
| `ErrorContainer` | `#FDE8E9` | `#3A1F22` | — |
| `Info` | `#4075FF` | `#6590FF` | `InfoColor`, `InfoTertiaryColor` |

### 2.2 Reglas de implementación

```xml
<!-- Colors.xaml: definición por tema con AppThemeBinding -->
<Color x:Key="SurfaceLight">#F1F3F8</Color>
<Color x:Key="SurfaceDark">#222631</Color>
<Color x:Key="Surface">  <!-- consumidores usan {DynamicResource Surface} -->
  <AppThemeBinding Light="{StaticResource SurfaceLight}"
                   Dark="{StaticResource SurfaceDark}" />
</Color>
```

- Páginas y controles consumen con `{DynamicResource Token}` (responde a
  cambio de tema en caliente) o `{AppThemeBinding}` cuando el valor es local
  a ese elemento.
- `{StaticResource}` solo para colores que NO cambian con el tema
  (`Primary` base, iconos blancos sobre botón primario).
- Los brushes (`PrimaryBrush`, etc.) se derivan de los colores, no al revés.
- **Prohibido**: `BackgroundColor="#4075FF"` en una página. Si el color no
  existe como token, primero se añade el token.
- Color dinámico de Android (Material You): los controles **nativos** bajo
  Material 3 lo toman del sistema automáticamente; no intentar mapearlo a los
  tokens MAUI. El Design System propio manda en toda la UI compuesta.

### 2.3 Contraste mínimo (obligatorio)

- Texto primario sobre Surface: ≥ 12:1 (cumplido por diseño).
- Texto secundario sobre Surface: ≥ 4.5:1.
- Texto blanco sobre `Primary`: ≥ 4.5:1 (`#4075FF` pasa con texto bold ≥ 14).
- `TextSecondary` nunca como único texto de acción crítica.

## 3. Tipografía

Fuentes del proyecto (declarar en `MauiProgram.ConfigureFonts`):

| Fuente | Uso |
|---|---|
| `MontserratSemiBold` / `MontserratRegular` | Títulos, headers de sección |
| `InterRegular` | Cuerpo, botones, inputs |
| `ManropeRegular` / `ManropeSemiBold` | Datos numéricos, labels, chips, timestamps |
| `FluentSystemIcons` | Iconografía (glyphs `FluentUI.cs`) |

Escala (tamaño en unidades MAUI — como escala en sp en Android):

| Estilo | Tamaño | Fuente | LineHeight aprox. | Uso |
|---|---|---|---|---|
| `DisplayLarge` | 34 | MontserratSemiBold | — | Números grandes de progreso, greeting hero |
| `HeadlineLarge` | 24 | MontserratSemiBold | 32 | Título de pantalla |
| `TitleLarge` | 21 | MontserratSemiBold | 28 | Título de sección |
| `TitleMedium` | 18 | MontserratSemiBold | 24 | Título de card, header de grupo |
| `BodyLarge` | 16 | InterRegular | 24 | Párrafos principales |
| `BodyMedium` | 14 | InterRegular | 20 | Cuerpo estándar, items de lista |
| `BodySmall` | 13 | InterRegular | 18 | Descripciones secundarias |
| `LabelLarge` | 14 | InterRegular | — | Botones (nada menor en CTAs) |
| `LabelMedium` | 12 | ManropeSemiBold | — | Chips, badges, tabs |
| `LabelSmall` | 11 | ManropeRegular | — | Timestamps, metadatos |

Reglas:

- Un solo peso semibold para toda la jerarquía de títulos: **prohibido**
  `FontAttributes="Bold"` en fuentes que ya son SemiBold (engorda feo).
- El tamaño de botón nunca baja de 14. Los metadatos nunca bajan de 11.
- Los estilos se definen como `Style TargetType="Label"` implícitos por
  nombre (`TitleLargeStyle`, `BodyMediumStyle`, ...) en `Typography.xaml`.
- Accesibilidad: los tamaños escalan con la fuente del sistema por defecto;
  no fijar `FontSize` con `OnIdiom` para "compactar" — usar layouts que
  toleren crecimiento (wrapping, no clips).

## 4. Espaciado

Escala 4-pt base; definir como `x:Double` en `Spacing.xaml`:

| Token | Valor | Uso |
|---|---|---|
| `SpacingXS` | 4 | Icono↔texto dentro de un elemento compacto |
| `SpacingS` | 8 | Entre elementos íntimamente relacionados |
| `SpacingM` | 12 | Padding interno de chips/inputs/list items |
| `SpacingL` | 16 | Padding de card (default), separación entre elementos de lista |
| `SpacingXL` | 20 | Entre secciones dentro de una pantalla |
| `SpacingXXL` | 24 | Separación entre bloques mayores |
| `SpacingScreen` | 20 | **Margen lateral estándar de toda pantalla** |
| `SpacingScreenBottom` | 32 | Reserva inferior antes de la TabBar |

Reglas:

- Toda página usa `Padding="{StaticResource ...}"` o margen lateral 20
  (excepción: listas full-bleed con cards que llevan su propio margen).
- Espaciado vertical entre cards de una lista: 12. Entre secciones: 20–24.
- No usar `Padding="14"`-style magic numbers: si 16 no encaja, el problema es
  la composición, no el número.

## 5. Shapes (radios)

| Token | Radio | Aplica a |
|---|---|---|
| `RadiusS` | 8 | Inputs, tooltips, progress bars grandes |
| `RadiusM` | 14 | Botones, chips, search bar, imágenes pequeñas |
| `RadiusL` | 18 | Cards (radio por defecto del sistema), snackbars |
| `RadiusXL` | 24 | Bottom sheets, dialogs |
| `RadiusFull` | mitad del tamaño | Avatares, FAB, toggles |

Implementación con `RoundRectangle` en `StrokeShape` de `Border`; el radio de
card (18) vive en el estilo implícito `PSCardStyle`. Un radio nunca menor que
la mitad del padding del elemento.

## 6. Elevación y sombras

Regla de decisión, en orden de preferencia:

1. **Separación por tonalidad** (default): card = `SurfaceContainer` sobre
   `Surface`. Sin sombra, sin borde.
2. **Sombra ligera** (opcional para el FAB y cards hero): `Shadow` brush
   `#14000000` (light) / `#40000000` (dark), `Offset=0,4`, `Radius=10`.
3. **Elevación real** (flotantes): FAB (nivel 3), snackbar (nivel 3), bottom
   sheet y dialogs (nivel 3) — capas por encima del contenido con scrim en
   modales (`#52000000`).
4. **Prohibido**: sombras en elementos dentro de listas (matan el scrolling),
   doble sombra, sombra + borde juntos.

## 7. Theming Light/Dark

- App declara `AppTheme` soportado; el usuario puede forzar tema en Ajustes
  (Light/Dark/System), persistido en Preferences.
- **Toda** página debe verse correcta en ambos temas: los tokens cubren el
  100 % de la UI; si algo "se ve mal en dark", es un bug de tokens, no se
  parchea con un color literal.
- Imágenes/illustraciones: proveer variante dark cuando el asset tiene fondo
  propio (`AppThemeBinding` en `Source` o `Aspect` tolerante).
- Scrim de modales y estados de presionado (`Pressed` visual style) se definen
  por tema, no compartidos.

## 8. Motion (animación)

| Contexto | Animación | Duración | Easing |
|---|---|---|---|
| Aparición de card/sección | Fade + sutil translateY(8) | 200 ms | Sin |
| Cambio de tab | Nada extra (Shell ya hace el swipe/switch) | — | — |
| Snackbar | Slide up + fade in; fade out | 250 ms | Cubic out |
| Check de hábito/tarea | Scale 1.0→1.06→1.0 del check | 220 ms | Cubic in-out |
| Bottom sheet | Slide up con scrim fade | 250 ms | — |
| Progreso | Animar el valor, no la barra entera | 400 ms | Linear |

Reglas: toda animación en `try/finally` que garantice restaurar el estado
final (`Opacity=1`, `Scale=1`) — lección aprendida del bug de Dashboard.
Animar solo propiedades baratas (`Opacity`, `Translation`, `Scale`); nunca
animar `Layout`/`Size` en listas.
