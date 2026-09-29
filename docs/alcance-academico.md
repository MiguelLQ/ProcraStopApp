# Alcance de ProcraStop para la tesis

## Objetivo

Ayudar al estudiante universitario a iniciar, sostener y cerrar actividades academicas a tiempo. La Escala de Procrastinacion Academica (EPA) se aplica por separado para evaluar cambios; los registros de la app describen el uso, no reemplazan las respuestas del cuestionario.

La EPA de 12 items contempla postergacion de actividades (1, 6 y 7) y autorregulacion academica (2, 3, 4, 5, 8, 9, 10, 11 y 12). Referencia: Dominguez Lara, Villegas Garcia y Centeno Leyva (2014), *Procrastinacion academica: validacion de una escala en una muestra de estudiantes de una universidad privada*.

## Flujos de la primera version

### 1. Iniciar y avanzar una actividad

- **Entrada:** curso opcional, titulo, tipo (tarea o lectura), fecha limite, fecha prevista de inicio, primer paso pequeno e hitos opcionales.
- **Accion:** el estudiante inicia el primer paso, registra avances y puede replanificar un hito sin perder el historial.
- **Salida:** estado pendiente, en curso o completado; aviso cuando llega la fecha prevista y aun no se ha iniciado.
- **EPA:** 1, 5, 6, 7 y 11. La lectura debe poder identificarse para observar especificamente el item 7.
- **Aceptacion:** una tarea ordinaria creada antes de este flujo sigue abriendose y completandose; las fechas de inicio e hitos son opcionales.

### 2. Preparar un examen

- **Entrada:** curso, fecha del examen y temas.
- **Accion:** crear sesiones distribuidas antes del examen y permitir practica de recuperacion (preguntas o ejercicios sin consultar apuntes), con registro de temas pendientes.
- **Salida:** plan de sesiones, avance por tema y proximo repaso.
- **EPA:** 2, 8, 9 y 10.
- **Aceptacion:** se pueden programar varias sesiones anteriores a la fecha del examen; una sesion omitida se puede reprogramar sin marcarla como completada.

### 3. Revisar antes de entregar

- **Entrada:** actividad entregable y fecha limite.
- **Accion:** reservar un bloque de revision y comprobar criterios breves antes de marcar la entrega.
- **Salida:** estados de trabajo terminado, revisado y entregado, con sus fechas.
- **EPA:** 5, 11 y 12.
- **Aceptacion:** revisar es visible y medible; las tareas que no son entregables conservan el cierre simple.

### 4. Pedir ayuda a tiempo

- **Entrada:** duda o bloqueo surgido en una actividad o sesion de estudio.
- **Accion:** redactar una pregunta concreta, elegir un recurso o persona de apoyo y registrar la solicitud de ayuda.
- **Salida:** estado sugerida, solicitada o resuelta; posibilidad de continuar trabajando mientras se espera respuesta.
- **EPA:** 3 y 4.
- **Aceptacion:** una recomendacion de ayuda conduce a una accion real, pero nunca bloquea el estudio.

## Ampliacion posterior

Los proyectos con compañeros registrados requieren identidad, busqueda de usuarios, invitaciones y sincronizacion. Se diseñaran despues de estabilizar los cuatro flujos individuales; una subtarea local no equivale a una asignacion compartida.

## Registros para la evaluacion

Registrar por separado fecha planificada y real de inicio, avance de hitos, sesiones de preparacion, solicitudes de ayuda y revision previa a la entrega. La EPA debe conservar su redaccion y puntuacion aprobadas para la tesis; las metricas de uso no se presentan como puntuacion EPA ni como prueba causal por si mismas.

## Base de datos durante desarrollo

`AppDbContext` define el esquema y `InitializeDatabase` crea una base nueva con `EnsureCreated()`. Cuando cambie el modelo, crear una base de desarrollo nueva o borrar los datos de la instalacion de prueba antes de ejecutarla; `EnsureCreated()` no actualiza tablas existentes. Nunca borrar automaticamente la base en cada inicio. Antes de distribuir la app a usuarios con datos que deban conservarse, sustituir este procedimiento por migraciones versionadas y probar la actualizacion.

Los flujos de inicio, examen, revision y ayuda usan `procra-v3.db` durante esta fase para no abrir una base de desarrollo con el esquema anterior. Los datos guardados en `procra.db` o `procra-v2.db` no se trasladan. La siguiente alteracion de esquema requiere otra base nueva o migraciones.

La ayuda academica guarda una pregunta y la via elegida. "Preparar consulta" no envia mensajes: el estudiante registra por separado que ya consulto y que la duda se resolvio. La recomendacion y el detalle de tareas abren este flujo con el tema precargado.
