# ADR 0003: Asignaciones como agregado propio

- Estado: aceptada
- Fecha: 2026-11-10

## Contexto

Un camión lo maneja como mucho un conductor a la vez, y un conductor conduce como mucho un camión a la vez. Hay que guardar ese vínculo, conservar su historial y reaccionar cuando cambia el estado del camión o del conductor (por ejemplo, un camión que pasa a mantenimiento o un conductor de vacaciones).

Hay dos formas de modelarlo:

1. Guardar el conductor dentro del agregado `Truck` (un campo `AssignedDriverId`).
2. Crear un agregado `TruckAssignment` propio, que referencia a camión y conductor por id.

## Decisiones

### 1. `TruckAssignment` es un agregado propio

La asignación es un agregado con su propia identidad, su historial y su ciclo de vida: se inicia, se libera con un motivo y no se modifica después. Un registro por vínculo, con `ReleasedAt` nulo mientras esté activa.

Si el conductor viviera dentro de `Truck`, cada asignación cambiaría el camión, y la concurrencia del camión se mezclaría con la del vínculo. Además, el historial dejaría de ser una colección simple de liberaciones. Con un agregado propio, el camión sigue siendo el mismo aunque cambie de conductor.

### 2. Referencias por id; la regla que combina agregados vive en la política

`TruckAssignment` guarda `TruckId` y `DriverId`, nunca los agregados. Las reglas que necesitan leer camión y conductor están en `TruckAssignmentPolicy.EnsureCanAssign(truck, driver, today)`, en `Domain/Aggregates/Assignments/`.

La política es la única pieza que referencia `Truck` y `Driver`. Eso choca con la regla de arquitectura "un agregado no depende de otro", así que `ArchitectureTests` la exime por nombre (`CrossAggregatePolicy`). La excepción es explícita y no se generaliza: cualquier otra referencia cruzada sigue prohibida.

La aplicación llama a la política antes de `TruckAssignment.Start`. El agregado de asignación no puede comprobar la regla por sí mismo, porque necesitaría referenciar camión y conductor.

Se descartó mover la política a `Application`: sus reglas son de negocio (qué camión puede llevar qué conductor) y tienen que probarse en el dominio sin base de datos.

### 3. El camión define la versión del vínculo

`PUT` y `DELETE /api/trucks/{truckId}/assignment` exigen `If-Match` con la versión del **camión**. Asignar no cambia el camión, así que dos peticiones con la misma versión compiten de verdad en la base de datos: los índices únicos parciales (`WHERE released_at IS NULL`) dejan pasar una y rechazan la otra con 409.

La alternativa era una versión propia en `TruckAssignment`. Se descartó porque una asignación nueva no tiene versión que el cliente pueda haber leído.

### 4. Unicidad en la base de datos, comprobación previa en la aplicación

Hay dos índices únicos parciales: uno por `truck_id` y otro por `driver_id`, ambos con `WHERE released_at IS NULL`. La comprobación previa del caso de uso da un mensaje claro ("El camión ya tiene un conductor asignado"); si dos peticiones pasan esa comprobación a la vez, el índice responde 23505 y `AppDbContext` lo convierte en 409.

Un check constraint exige que `released_at` y `release_reason` sean ambos nulos o ambos con valor.

### 5. Los motivos de liberación los fijan los eventos

`AssignmentReleaseReason` tiene cinco valores: `Manual`, `TruckStatusChanged`, `TruckDeleted`, `DriverStatusChanged` y `DriverDeleted`. Los cuatro últimos los fijan handlers de eventos en `Application/Features/Assignments/EventHandlers/`:

- Un camión que pasa a `Maintenance` o `Inactive` libera su asignación. Pasar a ruta no la libera.
- Un camión dado de baja libera su asignación.
- Un conductor que pasa a `OnLeave` o `Inactive` libera su asignación. Volver a `Active` no la restaura.
- Un conductor dado de baja libera su asignación.

Los handlers son idempotentes: si no hay asignación activa, no hacen nada.

### 6. Categoría mínima por tipo de camión

`TruckAssignmentPolicy.RequiredLicenseFor` asigna `Light` → C1, `Rigid` → C2 y `Articulated` → C3. El tipo vive en `Truck` y la categoría en `Driver`, así que la relación solo puede estar en la política.

## Consecuencias

- Positivas:
  - El historial de asignaciones es completo y consultable (`GET /api/assignments?active=false`).
  - Camión y conductor se pueden cambiar sin tocar el vínculo, y viceversa.
  - La unicidad no depende de que la aplicación compruebe bien: la base de datos la garantiza incluso con peticiones paralelas.
- Negativas:
  - La política es una excepción a una regla de arquitectura. Si aparece otra regla que combine agregados, habrá que decidir si entra en la política o si se rediseña.
  - `ETag` del camión no cambia cuando cambia su conductor. Un cliente que solo mira el camión no ve el cambio de asignación; tiene que consultar `GET /api/trucks/{id}/assignment`.
  - Los handlers de liberación se ejecutan tras guardar el cambio de estado. Si fallan, la petición responde 500 aunque el camión o el conductor ya estén guardados (ver ADR 0002). Por eso son idempotentes.
  - La categoría mínima por tipo (`Light` → C1, `Rigid` → C2, `Articulated` → C3) está pendiente de confirmar con la normativa colombiana. El mapeo es una suposición de la fase, no un dato verificado.
