# Checklist de calidad UI/UX de ProcraStop

Ejecutar antes de declarar terminada cualquier pantalla, componente o cambio
visual. Marco la casilla solo con evidencia (compilación, ejecución, o
inspección real del código). Al entregar, reportar: qué pasó, qué quedó
pendiente y por qué.

## A. Arquitectura y cumplimiento del skill

- [ ] El cambio vive en `ProcraStop.Maui11` (no en la app .NET 10) y no toca
      Domain/Application/Infrastructure.
- [ ] Archivos nuevos en las ubicaciones del Design System (Resources/Styles,
      Components/, Views/, ViewModels/); nada "temporales" fuera de sitio.
- [ ] No se reescribió una pantalla existente sin necesidad; el cambio es
      incremental.
- [ ] Ninguna API usada sin verificar (MAUI 11 real, no Xamarin ni APIs
      removidas).

## B. Design System

- [ ] Cero colores hardcoded en XAML nuevo (todo `DynamicResource`/
      `AppThemeBinding` con tokens); los 2-3 valores permitidos por
      excepción están justificados en comentario.
- [ ] Cero medidas mágicas: espaciados de la escala (4/8/12/16/20/24/32) y
      radios de la escala (8/14/18/24/full).
- [ ] Tipografía solo de la escala (Display→LabelSmall) y fuentes
      registradas.
- [ ] Componente reutilizado en vez de duplicar estructura; si es nuevo
      PS*, cumple el contrato de `components.md` §3.
- [ ] Elevación conforme (sin sombras en items de lista; sombras solo
      flotantes).
- [ ] Light y Dark verificados visualmente (o razonados token a token si no
      hay dispositivo).

## C. XAML

- [ ] `x:DataType` en raíz de página y en cada `DataTemplate`.
- [ ] **Ninguna `RelativeSource AncestorType` dentro de `DataTemplate`**
      (usar `{x:Reference PageRoot}` + `Path=BindingContext.X`) **[P]**.
- [ ] Nada reparenta `Content` en runtime **[P]**.
- [ ] Anidación de layouts ≤ 3 niveles; no hay `Grid` de un hijo, ni
      `StackLayout` con wrap implícito.
- [ ] Estilos repetidos extraídos a `Controls.xaml`; sin estilos duplicados
      entre páginas.
- [ ] Bindings con `Mode` explícito cuando no es OneWay; `OneTime` donde
      aplica.
- [ ] Converters tolerantes a null; ConvertBack sin excepciones.

## D. MVVM y code-behind

- [ ] Cero lógica de negocio en code-behind.
- [ ] ViewModel sin referencias a `Microsoft.Maui.Controls` ni `Shell.Current`
      (navegación vía `INavigationService`/`ShellNavigation`).
- [ ] Comandos async con sufijo Async y protección de excepciones.
- [ ] `OnAppearing` protegido con `SafeUi.RunAsync` **[P]**.
- [ ] Suscripciones deshechas en `OnDisappearing` (simetría total,
      idempotencia al re-appearing) **[P]**.

## E. Estados de datos

- [ ] Loading (skeleton o spinner) al cargar.
- [ ] `EmptyView`/`PSEmptyState` con acción sugerida.
- [ ] Error en línea con "Reintentar".
- [ ] Pull-to-refresh con reset de `IsRefreshing` en `finally`.
- [ ] Doble-click protegido (`CanExecute` o compuerta).

## F. Navegación

- [ ] Rutas nuevas registradas idempotentemente y no duplican rutas de
      ShellContent **[P]**.
- [ ] Navegación vía `ShellNavigation` (compuerta serializada) **[P]**.
- [ ] Parámetros por query string (ids/primitivas); el destino recarga datos
      desde su facade.
- [ ] Back funciona (pila correcta); `GoBackAsync` con guarda.
- [ ] Smoke test de navegación actualizado y ejecutado si tocó rutas
      (`PROCRASTOP_NAV_SMOKE=1`) — ver `shell-navigation.md` §9.

## G. Accesibilidad

- [ ] Áreas táctiles ≥ 48×48 dp (visual puede ser menor; tappable no).
- [ ] `SemanticProperties.Description` en iconos/controles sin texto.
- [ ] `HeadingLevel` en títulos de sección.
- [ ] Contraste ≥ 4.5:1 para texto de lectura en ambos temas.
- [ ] Textos con `LineBreakMode` consciente; nada recortado crítico.
- [ ] Estados de foco/orden lógico sin trampas (`IsTabStop` coherente).

## H. Rendimiento

- [ ] Compiled bindings en todas las plantillas.
- [ ] Celdas de lista ligeras (sin sombras, imágenes de tamaño fijo).
- [ ] `CollectionView` (no `BindableLayout`) para listas grandes.
- [ ] Animaciones solo `Opacity`/`Translation`/`Scale`, con restauración
      garantizada (`try/finally`) **[P]**.
- [ ] Nada bloqueante en UI thread (I/O await; CPU pesada en background).
- [ ] Sin suscripciones sin desuscripción (leak audit incluido).

## I. Compilación y verificación

- [ ] `dotnet build src/ProcraStop.Maui11/ProcraStop.Maui11.csproj -f
      <target>` → 0 errores.
- [ ] Ejecutado en el target principal (Android si hay dispositivo; Windows
      como mínimo) sin crash.
- [ ] Revisado el log (`AppCrashLogger`) tras la ejecución: 0 excepciones
      nuevas.
- [ ] No se declaró "funcional" nada no compilado.

## J. Consistencia final (prueba de humo de diseño)

Mira la pantalla resultante y pregúntate:

- [ ] ¿Pertenece a la misma familia visual que el resto de la app?
- [ ] ¿Hay un solo CTA primario claro?
- [ ] ¿Se entiende el propósito en 5 segundos sin explicación?
- [ ] ¿Los textos hablan en español natural y empático?
- [ ] ¿La pantalla funciona con una mano (CTA alcanzable)?
- [ ] ¿Eliminaría algún elemento sin pérdida de función? (si sí, elimínalo)
