# ADR 0001: Arquitectura por features

- Estado: aceptada
- Fecha: 2026-11-12

## Contexto

LogiTrack tendrá muchas pantallas de negocio: camiones, conductores, asignaciones, despachos, seguimiento, documentos y facturación. Hay que decidir cómo organizar el código del frontend para que crezca sin que las partes se enreden, y que un alumno nuevo pueda encontrar cada cosa.

El backend ya usa una organización por partes del negocio (`Features/Trucks`, `Features/Drivers`, `Features/Assignments`). El frontend debe ser coherente con ella.

## Decisión

El código se organiza en tres zonas con reglas de dependencia explícitas:

- **`core/`:** lo que existe una sola vez en la app (layout, navegación, tema, notificaciones, interceptores, manejo global de errores). Puede usar `shared/`.
- **`shared/`:** piezas reutilizables sin negocio (el kit de UI y los modelos genéricos como `PagedResult<T>` y `ProblemDetails`). No puede usar `core/` ni `features/`.
- **`features/<feature>/`:** todo lo de una parte del negocio. Puede usar `core/` y `shared/`. **Una feature no importa de otra.** Si dos features necesitan lo mismo y no tiene negocio, va a `shared/`.

Las reglas se verifican con `no-restricted-imports` por carpeta en ESLint: si alguien las rompe, `npm run lint` falla. Se comprobó con dos imports prohibidos de prueba.

Cada feature sigue la misma forma: rutas, `data-access/` (modelos, API y store), `pages/` (contenedores) y `components/` (presentacionales).

## Alternativas descartadas

- **Organización por tipo técnico** (`components/`, `services/`, `pages/` en la raíz). Con pocas pantallas parece sencilla, pero con muchas features cada carpeta se llena de cosas no relacionadas y no se sabe dónde vive una pantalla. Además, no coincide con el backend.
- **Una librería de componentes** (Angular Material, PrimeNG). Impone su estilo visual, pesa más y obliga a sobrescribir el diseño de Stitch. El kit propio se construye con Tailwind y tokens.
- **Copiar el HTML de Stitch tal cual.** Trae Tailwind v3 por CDN, imágenes externas, una barra lateral fija que no funciona en celular y accesibilidad incompleta. Se traduce a tokens y componentes.
- **Nx desde el inicio.** Aporta herramientas de monorepo que hoy no hacen falta. Si el frontend crece hasta necesitar varias aplicaciones, se evalúa entonces.
- **Eslint-plugin-boundaries para las reglas de dependencia.** Se probó, pero no resolvía los alias de `tsconfig` sin un resolver adicional, así que no detectaba las violaciones. Se usó `no-restricted-imports`, que viene con ESLint.

## Consecuencias

- **Positivas:**
  - Cada parte del negocio vive en una carpeta, igual que en el backend.
  - El kit de `shared/ui` se reutiliza en todas las features sin acoplarse a ninguna.
  - Las reglas quedan escritas en el lint: no dependen de que alguien las recuerde.
  - Agregar una feature es seguir una receta corta (ver `CLAUDE.md`).
- **Negativas:**
  - Mientras una feature necesite algo de otra, hay que moverlo a `shared/` o a `core/`. Eso añade un paso al principio, pero evita dependencias cruzadas que luego son caras de deshacer.
  - Las reglas de `no-restricted-imports` cubren los imports por alias (`@features/*`, `@core/*`, `@shared/*`). Un import relativo que salga de una feature (`../../otra-feature/...`) no lo detecta el lint; se revisa en la revisión de código. Es el hueco que hay que cerrar con una regla de rutas si aparece.
  - Los alias `@env` y `@package` se añadieron fuera de los tres que pedía el prompt, para importar el entorno y la versión sin rutas relativas que salgan de la carpeta.
