# Glosario

Lenguaje ubicuo del sistema: cada término de negocio tiene un nombre en el código. Si un término no está aquí, se añade antes de usarlo.

## Módulo Fleet (flota)

| Término de negocio | Nombre en el código | Notas |
| --- | --- | --- |
| Camión | `Truck` | Agregado raíz del módulo. Se identifica con un `Guid` UUID v7. |
| Placa | `Plate` | Value object. Formato colombiano `ABC123`: tres letras y tres números. Se guarda en mayúsculas. |
| SOAT | `SoatExpiresOn` | Fecha de vencimiento del seguro obligatorio (`DateOnly`). |
| Revisión técnico-mecánica | `TechReviewExpiresOn` | Fecha de vencimiento de la revisión técnico-mecánica (`DateOnly`). |
| Capacidad | `CapacityKg` | Capacidad de carga en kilogramos. Debe ser mayor que cero. |
| Volumen | `VolumeM3` | Volumen de carga en metros cúbicos. Debe ser mayor que cero. |
| Año modelo | `ModelYear` | Año de fabricación del modelo. Validado entre 1950 y 2100. |
| Marca | `Brand` | Fabricante del camión. Máximo 60 caracteres. |
| Estado del camión | `TruckStatus` | Enumeración guardada como texto en la base de datos. |
| Disponible | `TruckStatus.Available` | Estado inicial de un camión recién registrado. |
| En ruta | `TruckStatus.InRoute` | El camión está en un viaje. No puede pasar a ruta desde `Maintenance`. |
| En mantenimiento | `TruckStatus.Maintenance` | El camión está en el taller. |
| Inactivo | `TruckStatus.Inactive` | El camión está fuera de servicio sin darse de baja. |
| Dado de baja | `Truck.DeletedAt` distinto de nulo | Borrado lógico. Un camión dado de baja no se puede modificar y queda fuera de las consultas. |
| Registrar camión | `Truck.Register(...)` | Crea el agregado y registra `TruckRegistered`. |
| Cambio de estado | `Truck.ChangeStatus(...)` | Registra `TruckStatusChanged` con el estado anterior y el nuevo. |
| Versión | `Truck.Version` | Número de cambio del camión para concurrencia optimista (columna `xmin`). |

## Eventos de dominio

| Evento | Cuándo ocurre | Quién reacciona |
| --- | --- | --- |
| `TruckRegistered` | Se registra un camión nuevo. | `LogOnTruckRegistered` (registra el alta en el log). |
| `TruckStatusChanged` | Cambia el estado de un camión. | `LogOnTruckStatusChanged` (registra el cambio en el log). |
