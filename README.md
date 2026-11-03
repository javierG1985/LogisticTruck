# LogiTrack · Backend

Monolito modular de la plataforma de logística LogiTrack (camiones, conductores, envíos, seguimiento, documentos y facturación).

## Requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download/dotnet/10.0). `global.json` fija `10.0.300` y permite parches de la banda `latestFeature`.
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) para PostgreSQL 18 + PostGIS.
- Git.

Comprueba el SDK activo con:

```bash
dotnet --version
```

## Levantar la base de datos

```bash
docker compose up -d
```

Crea un contenedor `logitrack-postgres` (imagen `postgis/postgis:18-3.6`) en el puerto 5433 del host, con la base `logitrack` y el usuario `logitrack`. Las credenciales son solo de desarrollo local. Si el puerto 5433 también está ocupado, cambia el mapeo en `docker-compose.yml` y `ConnectionStrings:LogiTrack`.

## Migraciones

Proyecto de migraciones y proyecto de arranque van separados: `-p` apunta a `Infrastructure` (donde viven las migraciones) y `-s` a `Api` (donde está la cadena de conexión).

```bash
dotnet ef migrations add <Nombre> -p src/LogiTrack.Infrastructure -s src/LogiTrack.Api -o Persistence/Migrations
dotnet ef database update -p src/LogiTrack.Infrastructure -s src/LogiTrack.Api
dotnet ef migrations script -p src/LogiTrack.Infrastructure -s src/LogiTrack.Api
```

Instala la herramienta una vez con `dotnet tool install --global dotnet-ef`.

## Compilar y probar

```bash
dotnet build LogiTrack.slnx
dotnet test LogiTrack.slnx
```

El proyecto usa `TreatWarningsAsErrors`: cualquier warning rompe la compilación. Las pruebas usan Microsoft Testing Platform (opt-in en `global.json`).

## Ejecutar la API

```bash
dotnet run --project src/LogiTrack.Api
```

En Development:

- Swagger UI: `https://localhost:<puerto>/swagger`
- Documento OpenAPI: `/openapi/v1.json`
- Salud: `/health` (devuelve `Healthy` cuando PostgreSQL responde)

La documentación del diseño (ADR y glosario) y las convenciones para desarrollo (`CLAUDE.md`) se añaden en el Paso 7.
