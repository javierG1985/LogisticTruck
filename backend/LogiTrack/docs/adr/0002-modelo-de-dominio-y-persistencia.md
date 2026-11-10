# ADR 0002: Modelo de dominio y persistencia

- Estado: aceptada
- Fecha: 2026-11-03

## Contexto

El dominio tiene agregados con invariantes (un camión dado de baja no se modifica; un camión en mantenimiento no pasa a ruta) y necesita persistencia en PostgreSQL con PostGIS. Hay que decidir cómo accede la aplicación a los datos, cómo se organizan las tablas entre áreas de negocio y cómo se publican los eventos de dominio.

## Decisiones

### 1. `IApplicationDbContext` en lugar de repositorios

Los handlers de comandos trabajan contra `IApplicationDbContext`, una interfaz de `Application` que expone los `DbSet<T>` y `SaveChangesAsync`. `AppDbContext` en `Infrastructure` la implementa.

No hay Repository genérico ni Unit of Work. `DbContext` ya es una unidad de trabajo y `DbSet<T>` ya es una colección de agregados. Un repositorio genérico añadiría una capa que solo reenvía llamadas, y en la práctica suele filtrar `IQueryable` o perder consultas específicas.

Las consultas (`Queries`) usan `DbSet` con `AsNoTracking()` y proyecciones; los comandos cargan el agregado, llaman a un método con intención y guardan.

### 2. Un solo `AppDbContext` con un esquema de PostgreSQL por área de negocio

Todas las áreas comparten `AppDbContext`, pero cada una es dueña de su esquema (`fleet`, `customers`, `shipping`…). Las configuraciones EF viven en `Persistence/Configurations/`, un archivo por entidad.

Un contexto por área impediría una transacción que toque dos agregados del monolito (por ejemplo, asignar un camión y un conductor). El aislamiento se consigue por esquema y por reglas de dependencia, no por separar contextos.

Ningún handler consulta tablas de otra feature. Si lo necesita, debe recibir un evento de dominio.

### 3. Los domain events implementan `INotification` de Mediator

`IDomainEvent` hereda de `INotification` (paquete `Mediator.Abstractions`). Es un compromiso documentado: `Domain` referencia ese paquete solo para el marcador `INotification`, y no conoce el mecanismo de despacho.

Los eventos se despachan con `DispatchDomainEventsInterceptor`, un `SaveChangesInterceptor` de EF Core que publica con `IPublisher` **después** de un `SaveChanges` exitoso y luego limpia los eventos del agregado. Así no se notifica algo que no llegó a guardarse.

### 4. Mediator en `Application`, con el generador en el mismo proyecto

Mediator (licencia MIT, a diferencia de MediatR desde la v13) genera el código de despacho en compilación. El paquete `Mediator.SourceGenerator` va en `Application`, porque todos los handlers viven ahí y `AddApplication()` es quien registra Mediator con `ServiceLifetime.Scoped`.

La documentación de Mediator sugiere poner el generador en el proyecto más externo. Lo descartamos para este monolito porque la regla del curso es que `AddApplication()` registre los casos de uso. Si un handler se mueve a otro ensamblado, se usa la opción `MediatorOptions.Assemblies`.

### 5. Concurrencia optimista con `xmin`

El agregado tiene `Version` (`uint`) mapeada a la columna de sistema `xmin` de PostgreSQL, con `IsRowVersion()`. Cada `GET` devuelve la versión como `ETag` y cada escritura exige `If-Match`. Si la versión no coincide, la respuesta es 412.

## Consecuencias

- Positivas:
  - Los handlers son pequeños y se leen de arriba abajo.
  - Las consultas pueden proyectar columnas concretas sin pasar por el agregado.
  - Los eventos de dominio se publican solo cuando los datos ya están guardados.
  - La concurrencia no depende de bloqueos: no hay que gestionar transacciones manuales.
- Negativas:
  - Un solo `AppDbContext` crece a medida que se añaden áreas de negocio. Se mitiga con una configuración por entidad, en archivos separados.
  - `Plate` es un value object convertido a texto. EF Core no traduce `Plate.Value` en SQL, así que las consultas proyectan la columna y convierten en memoria. Está comentado en `ListTrucksHandler`.
  - El interceptor publica dentro del mismo hilo tras guardar. Si un handler de evento falla, la excepción llega al llamador aunque los datos ya estén guardados. Por eso los handlers de eventos deben ser idempotentes y, cuando haga falta, delegar en colas (fase de RabbitMQ).
