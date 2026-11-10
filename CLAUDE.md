# CLAUDE.md · LogiTrack Backend

Monolito (.NET 10, C# 14, ASP.NET Core con controladores, EF Core 10, PostgreSQL 18 + PostGIS). Léelo antes de cambiar código.

## Capas

| Proyecto | Responsabilidad | Referencias de proyecto | Paquetes permitidos |
| --- | --- | --- | --- |
| `LogiTrack.Domain` | Agregados, value objects, eventos de dominio y reglas | Ninguna | Solo `Mediator.Abstractions` (para `INotification`) |
| `LogiTrack.Application` | Casos de uso: comandos, consultas, validadores, handlers | `Domain` | `Mediator.Abstractions`, `Mediator.SourceGenerator`, FluentValidation, `Microsoft.EntityFrameworkCore` (solo `DbSet` y LINQ). **Nunca Npgsql.** |
| `LogiTrack.Infrastructure` | `AppDbContext`, configuraciones EF, migraciones, interceptores | `Application`, `Domain` | EF Core, Npgsql, NetTopologySuite, NamingConventions, HealthChecks.NpgSql |
| `LogiTrack.Api` | Controladores, cuerpos HTTP, ProblemDetails, ETag, OpenAPI, `Program.cs` | `Application`, `Infrastructure` | ASP.NET Core, Serilog, Swagger UI, EF Core Design (solo desarrollo) |

Si una referencia no está en esta tabla, no se agrega.

## Estructura por capa

Una misma palabra identifica una parte del negocio en todas las capas. Para camiones es `Trucks`.

| Capa | Carpeta | Qué contiene |
| --- | --- | --- |
| `Domain` | `Aggregates/Trucks/` | Agregado `Truck`, value object `Plate`, `TruckStatus` y `Events/` |
| `Application` | `Features/Trucks/` | `Commands/`, `Queries/` y `EventHandlers/`, una carpeta por caso de uso |
| `Infrastructure` | `Persistence/Configurations/` | `TruckConfiguration.cs` (un archivo de mapeo por entidad) |
| `Api` | `Controllers/Trucks/` | `TrucksController.cs` y `Requests/` con los cuerpos HTTP |

Features actuales: `Trucks` (camiones), `Drivers` (conductores) y `Assignments` (qué conductor maneja qué camión). `Assignments` tiene su agregado en `Aggregates/Assignments/`, aunque su feature también es `Assignments`.

Los esquemas de PostgreSQL siguen el área de negocio: `Trucks`, `Drivers` y `Assignments` van en `fleet`; más adelante `Shipments` en `shipping` y `Invoices` en `billing`. Hay un solo `AppDbContext`.

## Reglas de diseño

1. **Reglas de negocio solo en `Domain/Aggregates/<Agregado>/`.** Los agregados heredan de `AggregateRoot`, tienen setters privados y se cambian con métodos con intención (`ChangeStatus`, `Delete`). Las reglas violadas lanzan `DomainException` (409).
   - **Excepción documentada (ADR 0003):** `Aggregates/Assignments/TruckAssignmentPolicy.cs` es la única pieza del dominio que referencia `Truck` y `Driver` a la vez. La prueba de arquitectura la exime por nombre, y no hay otra excepción.
2. **CQRS en cada feature:** `Commands/<CasoDeUso>/` implementa `ICommand` o `ICommand<T>`; `Queries/<CasoDeUso>/` implementa `IQuery<T>`. `EventHandlers/` reacciona a domain events.
3. **Un caso de uso = una carpeta** con su comando o consulta, su validador (si hay entrada) y su handler. La respuesta compartida por varios casos de uso de la feature vive en la raíz de la feature (`Features/Trucks/TruckDetailsResponse.cs`). No se comparten DTOs entre features.
4. **Validación ≠ regla de negocio.** FluentValidation revisa la forma (obligatorio, longitud, formato). El agregado protege las invariantes. Si la regla vive en `Plate`, el validador la consulta (`Plate.TryCreate`), no la repite.
5. **Handlers `sealed`** contra `IApplicationDbContext`. Sin Repository genérico ni Unit of Work.
6. **Consultas:** `AsNoTracking()`, proyección con `Select` y paginación siempre.
7. **Una feature no usa las clases de otra.** Se comunican con domain events y, en fases posteriores, con eventos de integración.
8. **DDD proporcional.** Agregados y eventos solo donde hay reglas reales. Si se duda, se pregunta antes de agregar ceremonia.
9. **Lenguaje ubicuo.** Los nombres vienen de `docs/glossary.md`. Un término nuevo se registra antes de usarlo.
10. **Controladores delgados:** `sealed`, solo `ISender` en el constructor, `ActionResult<T>`, rutas por atributo y `[ProducesResponseType]` en cada acción.
11. **Errores en ProblemDetails (RFC 9457)** mediante `GlobalExceptionHandler`. Nunca se devuelve el stack trace.
12. **`CancellationToken`** en acciones, handlers y llamadas a la base de datos.
13. **Eventos de dominio después de guardar.** `DispatchDomainEventsInterceptor` los publica tras un `SaveChanges` exitoso y luego los limpia del agregado. Un handler de evento puede guardar (por ejemplo, liberar asignaciones). Si falla, la petición original responde 500 aunque el cambio ya esté guardado (ADR 0002): los handlers deben ser idempotentes.
14. **Búsqueda sin mayúsculas.** Se usa `TextSearch.ToLowerText` y `TextSearch.ToContainsPattern` (`Common/Search/`), que EF traduce a `lower()` y escapa los comodines. Nunca `ToLower()` ni `ToLowerInvariant()` dentro de una consulta: EF no los traduce.
15. **Orden con `SortMap<T>`.** Cada recurso declara sus campos una sola vez en `Features/<Feature>/<Recurso>Sorts.cs` (por ejemplo, `TruckSorts`). El validador usa `Map.Accepts` y el handler `Map.Apply`. Ni validadores ni handlers repiten la lista de campos.
16. **Concurrencia de asignaciones.** Asignar y liberar usan `If-Match` con la versión del **camión** (`TruckAssignmentLookup`, `Common/Concurrency/VersionCheck`). La unicidad de camión y conductor activos la garantizan los índices únicos parciales (`WHERE released_at IS NULL`), no la comprobación previa.

## Convenciones de nombres

| Elemento | Convención | Ejemplo |
| --- | --- | --- |
| Solución | `LogiTrack.slnx` | `LogiTrack.slnx` |
| Proyectos | `LogiTrack.<Capa>`, y `.UnitTests` / `.IntegrationTests` | `LogiTrack.Domain`, `LogiTrack.Api.IntegrationTests` |
| Namespaces | Igual a la carpeta, con ámbito de archivo | `LogiTrack.Application.Features.Trucks.Commands.CreateTruck` |
| Carpeta de agregado / feature / controladores | Plural, PascalCase, mismo nombre en todas las capas | `Aggregates/Trucks/`, `Features/Trucks/`, `Controllers/Trucks/` |
| Agregados y value objects | Singular | `Truck`, `Plate` |
| Casos de uso | Verbo + sustantivo: `List` para colecciones, `Get` para uno | `CreateTruck`, `ListTrucks`, `GetTruckById` |
| Tipos del caso de uso | Nombre del caso de uso + rol | `CreateTruckCommand`, `ListTrucksQuery`, `CreateTruckValidator`, `CreateTruckHandler` |
| Respuestas | `<Recurso>DetailsResponse` o `<Recurso>ListItem` | `TruckDetailsResponse`, `TruckListItem`, `PagedResult<T>` |
| Cuerpos HTTP | `<CasoDeUso>Request`, en `Api/Controllers/<Feature>/Requests/`, solo si no coincide con el comando | `UpdateTruckRequest` |
| Controladores | Recurso en plural + `Controller` | `TrucksController`, `[Route("api/trucks")]` |
| Acciones | Verbo del caso de uso, sin `Async` | `List`, `GetById`, `Create`, `Update`, `ChangeStatus`, `Delete` |
| Domain events | Hecho en pasado | `TruckRegistered`, `TruckStatusChanged` |
| Handlers de eventos | Qué hace + `On` + evento | `LogOnTruckStatusChanged` |
| Orden por recurso | Recurso + `Sorts` | `TruckSorts`, `DriverSorts`, `AssignmentSorts` |
| Configuraciones EF | Entidad + `Configuration` | `TruckConfiguration` |
| Registro de servicios | `Add<Capa>()` en `DependencyInjection.cs` | `AddApplication()`, `AddInfrastructure(config)`, `AddPresentation()` |
| Tablas | snake_case, plural, esquema por área | `fleet.trucks.capacity_kg` |
| Código | Inglés. Mensajes al usuario en español. | `"La placa es obligatoria."` |
| Commits | Conventional Commits | `feat(trucks): crear camión` |

## Convenciones REST

- Recursos en plural y kebab-case; sin verbos en la URL. Subrecursos de máximo un nivel.
- Operaciones de negocio como subrecurso con sustantivo: `PATCH /api/trucks/{id}/status`, no `POST /activate`.
- Filtros, orden y paginación por query string: `?page=1&pageSize=20&search=abc&status=Available&sort=plate,-createdAt`.

| Operación | Verbo y ruta | Éxito | Errores |
| --- | --- | --- | --- |
| Listar | `GET /api/trucks` | 200 `PagedResult<T>` | 400 |
| Obtener | `GET /api/trucks/{id}` | 200 + `ETag` | 404 |
| Crear | `POST /api/trucks` | 201 + `Location` + `ETag` | 400, 409 |
| Reemplazar | `PUT /api/trucks/{id}` | 204 + `ETag` | 400, 404, 409, 412, 428 |
| Cambiar estado | `PATCH /api/trucks/{id}/status` | 204 + `ETag` | 400, 404, 409, 412, 428 |
| Eliminar (lógico) | `DELETE /api/trucks/{id}` | 204 | 404, 412, 428 |

**Concurrencia:** `GET` y `POST` devuelven `ETag` con la versión (`xmin`). `PUT`, `PATCH` y `DELETE` exigen `If-Match`: sin él, 428; con una versión vieja, 412. El helper está en `Api/Http/ETag.cs`.

**Asignaciones** (feature `Assignments`):

| Operación | Verbo y ruta | Éxito | Errores |
| --- | --- | --- | --- |
| Ver asignación actual de un camión | `GET /api/trucks/{truckId}/assignment` | 200 | 404 |
| Asignar conductor a un camión | `PUT /api/trucks/{truckId}/assignment` | 204 + `ETag` del camión | 400, 404, 409, 412, 428 |
| Liberar la asignación | `DELETE /api/trucks/{truckId}/assignment` | 204 | 404, 409, 412, 428 |
| Listar asignaciones | `GET /api/assignments?truckId=&driverId=&active=` | 200 `PagedResult<T>` | 400 |

`If-Match` de estas operaciones es la versión del **camión**, no de la asignación (ver regla 16).

## Recetas

### Cómo agregar un caso de uso

1. `Application/Features/<Feature>/Commands/<CasoDeUso>/` (o `Queries/`): comando o consulta, validador y handler.
2. Si la respuesta es nueva y la usan varios casos de uso, `Application/Features/<Feature>/<Respuesta>.cs`.
3. Si cambia el dominio, la regla va en el agregado (`Domain/Aggregates/<Agregado>/`) con su prueba en `LogiTrack.Domain.UnitTests`.
4. Si es un endpoint, un método en `Api/Controllers/<Feature>/<Recurso>Controller.cs`. Los cuerpos HTTP que no coinciden con el comando van en `Api/Controllers/<Feature>/Requests/`.
5. Prueba de integración en `LogiTrack.Api.IntegrationTests/Controllers/<Feature>/`.

### Cómo agregar un agregado nuevo

1. `Domain/Aggregates/<Agregado>/`: la clase (heredando de `AggregateRoot`), sus value objects y sus eventos en `Events/`.
2. `Infrastructure/Persistence/Configurations/<Agregado>Configuration.cs`: tabla con su esquema, conversiones y query filter. La prueba de arquitectura `Aggregates_HaveEntityTypeConfiguration` falla si falta.
3. `IApplicationDbContext` y `AppDbContext`: añade el `DbSet<T>`.
4. `dotnet ef migrations add <Nombre>` y revisa el SQL.

### Cómo agregar un evento de dominio

1. `Domain/Aggregates/<Agregado>/Events/<Hecho>.cs`: un `record` que implementa `IDomainEvent`. Nombre en pasado.
2. Lo lanza el agregado con `Raise(...)` dentro del método con intención.
3. Un handler en `Application/Features/<Feature>/EventHandlers/` que implemente `INotificationHandler<T>`. Mediator no compila un evento sin handler, y la prueba `DomainEvents_HaveAtLeastOneHandler` lo vuelve a comprobar.
4. Prueba de integración que confirme la reacción.

## Comandos habituales

```bash
docker compose up -d                                            # PostgreSQL 18 + PostGIS en el puerto 5433
dotnet build LogiTrack.slnx
dotnet test LogiTrack.slnx                                      # incluye Testcontainers: necesita Docker
dotnet run --project src/LogiTrack.Api                          # Swagger en /swagger (Development)
dotnet ef migrations add <Nombre> -p src/LogiTrack.Infrastructure -s src/LogiTrack.Api -o Persistence/Migrations
dotnet ef database update -p src/LogiTrack.Infrastructure -s src/LogiTrack.Api
```

## Lo que no se debe hacer

- No uses MediatR (licencia comercial desde v13). Usa Mediator.
- No agregues FluentAssertions (licencia comercial desde v8). Usa las aserciones de xUnit.
- No pongas reglas de negocio en validadores ni en handlers.
- No expongas agregados en controladores: solo comandos, consultas y respuestas.
- No uses Npgsql en `Application`.
- No agregues Repository genérico, AutoMapper, carpetas `Services/` o `Helpers/` genéricas.
- No hagas referencias entre features ni entre agregados. La única excepción es `TruckAssignmentPolicy` (regla 1 y ADR 0003).
- No uses `ToLower()` ni `ToLowerInvariant()` en consultas EF. Usa `TextSearch`.
- No pongas `HasDefaultValue` en un enum que el dominio siempre rellena: un 0 llegaría a la base de datos y se guardaría en silencio. El default va solo en la migración (ver `AddTruckType`).
- No compartas DTOs entre features.
- No pongas credenciales de producción en el repositorio. `appsettings.Development.json` y `docker-compose.yml` solo tienen credenciales locales.
- No edites migraciones ya aplicadas; crea una nueva.
- No uses versiones preview de paquetes.
