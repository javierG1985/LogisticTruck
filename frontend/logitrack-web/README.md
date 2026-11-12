# LogiTrack Web

Frontend de LogiTrack, la plataforma de logística para transportadoras colombianas. Esta fase construye la base: arquitectura, sistema de diseño, kit de componentes y estructura de la app. Las pantallas de negocio llegan en fases posteriores.

## Requisitos

- **Node.js 22.22.3 o superior.** La CLI de Angular 22 no arranca con versiones anteriores.
- **npm** (incluido con Node).
- El **backend** (`workspace/backend/LogiTrack`) para las pantallas con datos.

## Cómo levantar la app

Desde `workspace/frontend/logitrack-web`:

```bash
npm install
```

```bash
npm start
```

Abre `http://localhost:4200`. La Torre de Control y la guía de estilo funcionan sin el backend.

Para las pantallas con datos, levanta primero el backend (PostgreSQL con `docker compose up -d` y `dotnet run --project src/LogiTrack.Api` desde `workspace/backend/LogiTrack`).

## Scripts

| Comando                | Qué hace                                                   |
| ---------------------- | ---------------------------------------------------------- |
| `npm start`            | Servidor de desarrollo con recarga automática              |
| `npm run build`        | Build de producción en `dist/`                             |
| `npm test`             | Pruebas unitarias con Vitest                               |
| `npm run lint`         | ESLint, incluidas las reglas de dependencia entre carpetas |
| `npm run format`       | Prettier: da formato a todo el proyecto                    |
| `npm run format:check` | Prettier en modo verificación, sin cambiar archivos        |

## Proxy hacia el backend

En desarrollo, `proxy.conf.json` redirige todas las peticiones a `/api` hacia `http://localhost:5044`, el puerto del perfil `http` del backend.

Así el navegador habla con el mismo origen que la app (`localhost:4200`) y no hace falta configurar CORS en desarrollo. Cuando exista el gateway, el proxy apuntará a él y el código de la app no cambia: la URL base sigue siendo `/api`, y lo que cambia es a dónde se reenvía.

## Guía de estilo

Con la app en marcha, abre `http://localhost:4200/style-guide`. Solo existe fuera de producción y muestra:

- Los colores con su nombre y la tabla de contraste AA en claro y en oscuro.
- La escala tipográfica, espaciados, radios y sombras.
- Todos los componentes de `shared/ui` con sus variantes y estados.
- La composición de ejemplo de la pantalla "Despachos y Asignación", con datos ficticios marcados como "Ejemplo".

El diseño de referencia (Stitch) está en `docs/design/stitch/despachos-y-asignacion.html`. Las decisiones de diseño están en `docs/design/design-system.md`.

## Estructura

```text
src/app/
├─ core/       layout, navegación, tema, notificaciones, interceptores, configuración
├─ shared/     kit de UI (shared/ui) y modelos genéricos
└─ features/   dashboard (Torre de Control) y style-guide
```

Las reglas de dependencia entre estas carpetas están en [CLAUDE.md](CLAUDE.md) y en `eslint.config.js`.

## Documentación

- [CLAUDE.md](CLAUDE.md): convenciones y reglas, para cualquier persona o sesión que cambie el código.
- [docs/design/design-system.md](docs/design/design-system.md): sistema de diseño, tokens y decisiones respecto a Stitch.
- [docs/adr/0001-arquitectura-por-features.md](docs/adr/0001-arquitectura-por-features.md): por qué la app está organizada por features.
