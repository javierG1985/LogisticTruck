# Glosario

Lenguaje ubicuo del sistema: cada término de negocio tiene un nombre en el código. Si un término no está aquí, se añade antes de usarlo.

## Área Fleet (flota)

| Término de negocio | Nombre en el código | Notas |
| --- | --- | --- |
| Camión | `Truck` | Agregado raíz de la feature `Trucks`. Se identifica con un `Guid` UUID v7. |
| Placa | `Plate` | Value object. Formato colombiano `ABC123`: tres letras y tres números. Se guarda en mayúsculas. |
| SOAT | `SoatExpiresOn` | Fecha de vencimiento del seguro obligatorio (`DateOnly`). |
| Revisión técnico-mecánica | `TechReviewExpiresOn` | Fecha de vencimiento de la revisión técnico-mecánica (`DateOnly`). |
| Capacidad | `CapacityKg` | Capacidad de carga en kilogramos. Debe ser mayor que cero. |
| Volumen | `VolumeM3` | Volumen de carga en metros cúbicos. Debe ser mayor que cero. |
| Año modelo | `ModelYear` | Año de fabricación del modelo. Validado entre 1950 y 2100. |
| Marca | `Brand` | Fabricante del camión. Máximo 60 caracteres. |
| Tipo de camión | `TruckType` | Clasificación del vehículo: `Light` (liviano), `Rigid` (rígido) o `Articulated` (articulado). Guardado como texto. Obligatorio al registrar. |
| Estado del camión | `TruckStatus` | Enumeración guardada como texto en la base de datos. |
| Disponible | `TruckStatus.Available` | Estado inicial de un camión recién registrado. |
| En ruta | `TruckStatus.InRoute` | El camión está en un viaje. No puede pasar a ruta desde `Maintenance`. |
| En mantenimiento | `TruckStatus.Maintenance` | El camión está en el taller. |
| Inactivo | `TruckStatus.Inactive` | El camión está fuera de servicio sin darse de baja. |
| Dado de baja | `Truck.DeletedAt` distinto de nulo | Borrado lógico. Un camión dado de baja no se puede modificar y queda fuera de las consultas. |
| Registrar camión | `Truck.Register(...)` | Crea el agregado y registra `TruckRegistered`. |
| Cambio de estado | `Truck.ChangeStatus(...)` | Registra `TruckStatusChanged` con el estado anterior y el nuevo. |
| Versión | `Truck.Version` | Número de cambio del camión para concurrencia optimista (columna `xmin`). |

## Área Fleet: conductores

| Término de negocio | Nombre en el código | Notas |
| --- | --- | --- |
| Conductor | `Driver` | Agregado raíz de la feature `Drivers`. Se identifica con un `Guid` UUID v7. |
| Cédula | `DocumentNumber` | Número de cédula del conductor. Solo dígitos, entre 6 y 10. Único en el sistema. |
| Nombre | `Driver.FirstName` | Nombre(s) del conductor. Obligatorio. |
| Apellidos | `Driver.LastName` | Apellidos del conductor. Obligatorio. |
| Dirección | `Driver.Address` | Dirección de residencia. Obligatoria. |
| Teléfono | `Driver.Phone` | Teléfono de contacto. Obligatorio. |
| Correo | `Driver.Email` | Correo electrónico de contacto. Obligatorio. El formato se valida en la petición, no en el agregado. |
| Foto de perfil | `Driver.ProfilePhotoUrl` | Opcional. Referencia a la imagen; el archivo lo gestionará la feature `Documents` (fuera de esta fase). |
| Fecha de nacimiento | `Driver.BirthDate` | Necesaria para calcular la edad del conductor al expedir la licencia. No cambia. |
| Licencia de conducción | `DriverLicense` | Value object con categoría, expedición y vencimiento. Se crea con `DriverLicense.Create(category, issuedOn, expiresOn, birthDate)`. |
| Fecha de expedición | `DriverLicense.IssuedOn` | Fecha en que se expidió la licencia (`DateOnly`). |
| Fecha de vencimiento | `DriverLicense.ExpiresOn` | Último día en que la licencia es válida (`DateOnly`). |
| Categoría de licencia | `LicenseCategory` | Enumeración ordenada `C1 < C2 < C3`. Solo categorías de servicio público: las categorías B (servicio particular) no se aceptan. Norma: Resolución 1500 de 2005, art. 5. Una categoría superior habilita las inferiores (art. 5, parágrafo 1). |
| Vigencia de la licencia | `DriverLicense.IsValidOn(date)` | Válida entre `IssuedOn` y `ExpiresOn`, ambos incluidos. Plazo máximo: 3 años si el conductor tenía menos de 60 años al expedirla, 1 año si tenía 60 o más. Norma: Ley 769 de 2002, art. 22, modificado por el Decreto 019 de 2012, art. 197. |
| Habilita tipo de camión | `DriverLicense.Allows(LicenseCategory)` | Indica si la categoría de la licencia basta para una categoría mínima exigida. No conoce `TruckType`: las reglas entre agregados viven en `TruckAssignmentPolicy`. |
| Estado del conductor | `DriverStatus` | `Active` (activo), `OnLeave` (vacaciones, incapacidad o licencia) e `Inactive` (retirado de la empresa). |
| Dado de baja (conductor) | `Driver.DeletedAt` distinto de nulo | Borrado lógico. Un conductor dado de baja no se modifica. |
| Registrar conductor | `Driver.Register(...)` | Crea el agregado, con licencia inicial, y registra `DriverRegistered`. |
| Cambio de estado (conductor) | `Driver.ChangeStatus(...)` | Registra `DriverStatusChanged` con el estado anterior y el nuevo. |
| Renovar licencia | `Driver.RenewLicense(...)` | Reemplaza la licencia por una nueva validada con la fecha de nacimiento del conductor. |

## Área Fleet: asignaciones

| Término | Código | Definición |
| --- | --- | --- |
| Asignación | `TruckAssignment` | Vínculo entre un camión y un conductor. Solo guarda los ids de ambos. Las liberadas se conservan como historial. |
| Asignación activa | `TruckAssignment.ReleasedAt` nulo | Una asignación sin liberar. Un camión y un conductor tienen como mucho una activa, garantizado por índices únicos parciales en PostgreSQL. |
| Iniciar asignación | `TruckAssignment.Start(...)` | Crea la asignación y registra `DriverAssigned`. Requiere antes `TruckAssignmentPolicy.EnsureCanAssign`. |
| Liberar asignación | `TruckAssignment.Release(reason, utcNow)` | Cierra la asignación con un motivo (`AssignmentReleaseReason`) y registra `AssignmentReleased`. |
| Motivo de liberación | `AssignmentReleaseReason` | `Manual`, `TruckStatusChanged`, `TruckDeleted`, `DriverStatusChanged`, `DriverDeleted`. |
| Política de asignación | `TruckAssignmentPolicy.EnsureCanAssign(truck, driver, today)` | Regla que combina camión y conductor: camión disponible, conductor activo, licencia vigente y categoría suficiente. Es la única excepción a la regla de no referenciar agregados entre sí. |
| Categoría mínima por tipo | `TruckAssignmentPolicy.RequiredLicenseFor(TruckType)` | `Light` → C1, `Rigid` → C2, `Articulated` → C3. Pendiente de confirmar con la normativa. |

## Eventos de dominio

| Evento | Cuándo ocurre | Quién reacciona |
| --- | --- | --- |
| `TruckRegistered` | Se registra un camión nuevo. | `LogOnTruckRegistered` (registra el alta en el log). |
| `TruckStatusChanged` | Cambia el estado de un camión. | `LogOnTruckStatusChanged` (registra el cambio en el log). `ReleaseOnTruckStatusChanged` libera la asignación si el camión pasa a `Maintenance` o `Inactive`. |
| `TruckDeleted` | Se da de baja un camión. | `LogOnTruckDeleted` (registra la baja en el log). `ReleaseOnTruckDeleted` libera la asignación. |
| `DriverRegistered` | Se registra un conductor nuevo. | Pendiente (se añade con la feature `Drivers`). |
| `DriverStatusChanged` | Cambia el estado de un conductor. | `ReleaseOnDriverStatusChanged` libera la asignación si el conductor pasa a `OnLeave` o `Inactive`. Volver a `Active` no restaura la asignación. |
| `DriverDeleted` | Se da de baja un conductor (borrado lógico). | `ReleaseOnDriverDeleted` libera la asignación. |
| `DriverAssigned` | Se inicia una asignación. | `LogOnDriverAssigned` (registra la asignación en el log). |
| `AssignmentReleased` | Se libera una asignación, con su motivo. | `LogOnAssignmentReleased` (registra la liberación en el log). |
