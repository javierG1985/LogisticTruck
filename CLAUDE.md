# CLAUDE.md · LogiTrack Backend

Monolito modular (.NET 10, C# 14, ASP.NET Core con controladores, EF Core 10, PostgreSQL 18 + PostGIS). Léelo antes de cambiar código.

## Capas

| Proyecto | Responsabilidad | Referencias de proyecto | Paquetes permitidos |
| --- | --- | --- | --- |
| `LogiTrack.Domain` | Agregados, value objects, eventos de dominio y reglas | Ninguna | Solo `Mediator.Abstractions` (para `INotification`) |
| `LogiTrack.Application` | Casos de uso: comandos, consultas, validadores, handlers | `Domain` | `Mediator.Abstractions`, `Mediator.SourceGenerator`, FluentValidation, `Microsoft.EntityFrameworkCore` (solo `DbSet` y LINQ). **Nunca Npgsql.** |
| `LogiTrack.Infrastructure` | `AppDbContext`, configuraciones EF, migraciones, interceptores | `Application`, `Domain` | EF Core, Npgsql, NetTopologySuite, NamingConventions, HealthChecks.NpgSql |
| `LogiTrack.Api` | Controladores, ProblemDetails, ETag, OpenAPI, `Program.cs` | `Application`, `Infrastructure` | ASP.NET Core, Serilog, Swagger UI, EF Core Design (solo desarrollo) |

Si una referencia no está en esta tabla, no se agrega.

## Módulos

Un módulo es una parte del negocio con su lenguaje, su modelo y su esquema de PostgreSQL: `Fleet` (esquema `fleet`), y más adelante `Customers`, `Shipping`, `Documents`, `Billing`. Cada módulo tiene `Modules/<Modulo>/` en `Domain`, `Application` y `Api`, y sus configuraciones en `Infrastructure/Persistence/Configurations/<Modulo>/`.

Un módulo nunca usa el dominio ni los slices de otro. Se comunican con eventos de dominio y, en fases posteriores, con eventos de integración.

## Reglas de diseño

1. **Reglas de negocio solo en `Domain`**, en `Modules/<Modulo>/<Agregado>/`. Los agregados heredan de `AggregateRoot`, no tienen setters públicos y se cambian con métodos con intención (`ChangeStatus`, `Delete`). Las reglas violadas lanzan `DomainException` (409).
2. **Un slice = un caso de uso** en `Application/Modules/<Modulo>/<Recurso>/<CasoDeUso>/`: comando o consulta, validador (si hay entrada) y handler. La respuesta compartida por varios slices del mismo recurso vive en la carpeta del recurso.
3. **Validación ≠ regla de negocio.** FluentValidation revisa la forma (obligatorio, longitud, formato). El agregado protege las invariantes. Si la regla vive en `Plate`, el validador la consulta (`Plate.TryCreate`), no la repite.
4. **Handlers `sealed`** contra `IApplicationDbContext`. Sin Repository genérico ni Unit of Work.
5. **Consultas**: `AsNoTracking()`, proyección y paginación siempre.
6. **Ningún módulo usa a otro.** Comunicación por eventos.
7. **DDD proporcional.** Agregados y eventos solo donde hay reglas reales. Si se duda, se pregunta antes de agregar ceremonia.
8. **Lenguaje ubicuo.** Los nombres vienen de `docs/glossary.md`. Un término nuevo se registra antes de usarlo.
9. **Controladores delgados**: `sealed`, solo `ISender` en el constructor, `ActionResult<T>`, rutas por atributo y `[ProducesResponseType]` en cada acción.
10. **Errores en ProblemDetails (RFC 9457)** mediante `GlobalExceptionHandler`. Nunca se devuelve el stack trace.
11. **`CancellationToken`** en acciones, handlers y llamadas a la base de datos.
12. **Un esquema de PostgreSQL por módulo.** Un solo `AppDbContext`, configuraciones separadas por módulo.
13. **Eventos de dominio después de guardar.** `DispatchDomainEventsInterceptor` los publica tras un `SaveChanges` exitoso.

## Convenciones de nombres

| Elemento | Convención | Ejemplo |
| --- | --- | --- |
| Proyectos | `LogiTrack.<Capa>`, y `LogiTrack.<Capa>.UnitTests` / `.IntegrationTests` | `LogiTrack.Domain`, `LogiTrack.Api.IntegrationTests` |
| Namespaces | Igual a la carpeta, con ámbito de archivo | `LogiTrack.Application.Modules.Fleet.Trucks.CreateTruck` |
| Carpeta de recurso | Plural | `Modules/Fleet/Trucks/` |
| Agregados y value objects | Singular | `Truck`, `Plate` |
| Slices | Verbo + sustantivo: `List` para colecciones, `Get` para uno | `CreateTruck`, `ListTrucks`, `GetTruckById` |
| Tipos del slice | Nombre del slice + rol | `CreateTruckCommand`, `CreateTruckValidator`, `CreateTruckHandler` |
| Respuestas | `<Recurso>DetailsResponse` o `<Recurso>ListItem` | `TruckDetailsResponse`, `TruckListItem` |
| Cuerpos HTTP | `<CasoDeUso>Request`, solo si no coincide con el comando | `UpdateTruckRequest` |
| Controladores | Recurso en plural + `Controller` | `TrucksController`, `[Route("api/trucks")]` |
| Acciones | Verbo del caso de uso, sin `Async` | `List`, `GetById`, `Create`, `Update`, `ChangeStatus`, `Delete` |
| Domain events | Hecho en pasado | `TruckRegistered`, `TruckStatusChanged` |
| Handlers de eventos | Qué hace + `On` + evento | `LogOnTruckStatusChanged` |
| Registro de servicios | `Add<Capa>()` en `DependencyInjection.cs` | `AddApplication()`, `AddInfrastructure(config)` |
| Tablas | snake_case, plural, esquema por módulo | `fleet.trucks.capacity_kg` |
| Código | Inglés. Mensajes al usuario en español. | `"La placa es obligatoria."` |
| Commits | Conventional Commits | `feat(fleet): crear camión` |

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

## Recetas

### Cómo agregar un caso de uso

1. `Application/Modules/<Modulo>/<Recurso>/<CasoDeUso>/`: crea el `Command` o `Query`, el `Validator` y el `Handler`.
2. Si la respuesta es nueva, `Application/Modules/<Modulo>/<Recurso>/<Respuesta>.cs`.
3. Si cambia el dominio, la regla va en el agregado (`Domain/Modules/<Modulo>/<Agregado>/`) con su prueba en `LogiTrack.Domain.UnitTests`.
4. Si es un endpoint, un método en el controlador de `Api/Controllers/<Modulo>/<Recurso>/` (los cuerpos HTTP que no coinciden con el comando van en `Api/Requests/<Modulo>/<Recurso>/`) con su `[ProducesResponseType]`.
5. Prueba de integración en `LogiTrack.Api.IntegrationTests`.

### Cómo agregar un agregado a un módulo

1. `Domain/Modules/<Modulo>/<Agregado>/`: la clase (heredando de `AggregateRoot`), sus value objects y sus eventos en `Events/`.
2. `Infrastructure/Persistence/Configurations/<Modulo>/<Agregado>Configuration.cs`: tabla con esquema, conversiones y query filter.
3. `IApplicationDbContext` y `AppDbContext`: añade el `DbSet<T>`.
4. `dotnet ef migrations add <Nombre>` y revisa el SQL.

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
- No hagas referencias entre módulos ni entre slices de distintos módulos.
- No compartas DTOs entre recursos.
- No pongas credenciales de producción en el repositorio. `appsettings.Development.json` y `docker-compose.yml` solo tienen credenciales locales.
- No edites migraciones ya aplicadas; crea una nueva.
- No uses versiones preview de paquetes.
