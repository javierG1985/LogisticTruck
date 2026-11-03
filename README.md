# LogiTrack · Backend

Monolito modular de la plataforma de logística LogiTrack (camiones, conductores, envíos, seguimiento, documentos y facturación).

## Requisitos

- [.NET SDK 10.0.300](https://dotnet.microsoft.com/download/dotnet/10.0) o superior de la línea 10.x (`global.json` fija la versión y permite parches `latestFeature`).
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) para PostgreSQL 18 + PostGIS (se configura en el Paso 3).
- Git.

Comprueba el SDK activo con:

```bash
dotnet --version
```

## Cómo compilar y probar

```bash
dotnet build LogiTrack.slnx
dotnet test LogiTrack.slnx
```

El proyecto usa `TreatWarningsAsErrors`: cualquier warning rompe la compilación.

## Cómo ejecutar

La API se levanta en el Paso 3, cuando exista la base de datos. La documentación del diseño (ADR y glosario) y las convenciones para desarrollo (`CLAUDE.md`) se añaden en el Paso 7.
