# CLAUDE.md · LogiTrack Web

Frontend de LogiTrack: Angular 22.2 (standalone, zoneless, OnPush), TypeScript estricto, Tailwind CSS v4, Vitest. Léelo antes de cambiar código.

## Requisitos y comandos

Node **22.22.3 o superior** (lo exige la CLI de Angular 22).

```bash
npm start            # servidor de desarrollo en http://localhost:4200; /api va al backend (puerto 5044)
npm run build        # build de producción
npm test             # pruebas unitarias (Vitest)
npm run lint         # ESLint: incluye las reglas de dependencia por carpeta
npm run format:check # Prettier; "npm run format" corrige
```

El backend debe estar levantado para que las pantallas con datos respondan. La Torre de Control y la guía de estilo no lo necesitan.

## Zonas de la app

| Zona                  | Qué contiene                                                                                                                 | Puede usar                 | No puede usar                            |
| --------------------- | ---------------------------------------------------------------------------------------------------------------------------- | -------------------------- | ---------------------------------------- |
| `core/`               | Lo que existe una sola vez: layout, navegación, tema, notificaciones, interceptores, manejo global de errores, configuración | `shared/`, `@env`          | `features/`                              |
| `shared/`             | Kit de UI y modelos genéricos. Sin negocio                                                                                   | Nada de la app             | `core/`, `features/`, `@env`             |
| `features/<feature>/` | Una parte del negocio: rutas, páginas, componentes y acceso a datos                                                          | `core/`, `shared/`, `@env` | Otras features (por alias `@features/*`) |

Las reglas se verifican en `eslint.config.js` con `no-restricted-imports`. Si un import por alias las rompe, `npm run lint` falla. Las rutas relativas que salen de una feature (`../../otra-feature/...`) no están cubiertas por el lint: se revisan en la revisión de código.

Alias de rutas (en `tsconfig.json`): `@core/*`, `@shared/*`, `@features/*`, `@env/*` (entornos) y `@package` (el `package.json`, para leer la versión).

## Estructura de una feature

```text
features/<feature>/
├─ <feature>.routes.ts      # exporta <FEATURE>_ROUTES con loadComponent / loadChildren
├─ data-access/             # (cuando llegue) models/, <feature>-api.ts, <feature>-store.ts
├─ pages/                   # contenedores: uno por ruta; leen datos y pasan inputs
└─ components/              # presentacionales: solo input(), output() y model()
```

Una página (`Page`) conoce las rutas y el estado. Un componente (`components/`) solo recibe datos y emite eventos.

## Reglas de diseño

1. **Componentes** OnPush, signals para el estado local, `computed()` para lo derivado y `effect()` solo para sincronizar con algo externo.
2. **Sin suscripciones manuales** en componentes. Si son inevitables, `takeUntilDestroyed(destroyRef)`.
3. **`shared/ui` es presentacional:** solo `input()`, `output()` y `model()`. No inyecta servicios de la app, `HttpClient` ni `Router`. Sus variantes son tipos cerrados (`variant: 'primary' | 'accent' | …`), nunca clases que el consumidor tenga que conocer.
4. **Tailwind bien usado:** colores, tipografía, espaciados, radios y sombras salen de los tokens. Nada de hexadecimales en plantillas. Nunca se arman clases dinámicamente (`'bg-' + color`): las variantes son mapas con clases completas.
5. **Accesibilidad WCAG 2.1 AA:** HTML semántico, todo usable con teclado, foco visible, `label` en cada control, `aria-label` en botones de solo ícono, íconos decorativos con `aria-hidden`, notificaciones con `aria-live`, contraste AA en claro y oscuro, animaciones solo con `motion-safe:`.
6. **Modo oscuro:** clase `dark` en `<html>`. Claro, oscuro o sistema, guardado en `localStorage` (`lt-theme`) con `try/catch`. El script de `index.html` lo aplica antes del primer pintado.
7. **Formatos colombianos:** `LOCALE_ID` en `es-CO`.
8. **Rendimiento:** cada feature se carga de forma diferida, `track` en todos los `@for`, `@defer` para lo que no se ve al cargar.
9. **Navegación en un solo lugar:** `core/navigation/navigation-items.ts`. Agregar una feature al menú es agregar una línea con `enabled: true` y su `path`.
10. **Nada inventado:** lo que no existe aparece como "Próximamente" (deshabilitado), nunca con datos de muestra. Los datos de ejemplo van marcados como "Ejemplo".
11. **Responsive, mobile first:** 360 px (panel de menú), 640–1023 px (grillas de dos columnas) y 1024 px o más (barra lateral fija). Nada de scroll horizontal a ningún ancho. Áreas táctiles de al menos 44 px; campos con texto de 16 px en celular. Los componentes que cambian según su espacio usan consultas de contenedor (`@container`, `@md:`), no breakpoints de pantalla. Los breakpoints de pantalla solo en el shell.

## Tokens de diseño

Están en `src/styles.css`:

- **Color:** variables `--lt-*` en `:root` (claro, valores de Stitch) y `.dark` (oscuro). `@theme inline` las expone a Tailwind como `bg-surface`, `text-on-surface`, etc. Para cambiar el modo basta con cambiar los valores, no las clases.
- **Tipografía:** `text-headline-xl`, `text-headline-lg`, `text-headline-md`, `text-headline-sm`, `text-body-lg/md/sm`, `text-label-lg/md/sm`, `text-telemetry-lg/md`. Los títulos usan `font-headline`; el resto hereda Inter.
- **Espaciado:** `space-xs/sm/md/lg/xl`, `gutter`, `gutter-desktop`, `margin`, `margin-desktop`.
- **Radios:** `rounded-sm`, `rounded-lg`, `rounded-xl`. `rounded-full` conserva su significado.
- **Sombras:** `shadow-soft`, `shadow-soft-md`.

Si necesitas un color que no está, **agrégalo como token** en claro y oscuro, y verifica contraste AA antes de usarlo. Nunca escribas un hexadecimal en una plantilla.

**Excepción documentada:** `--lt-on-tertiary-container` en claro es `#966100` (Stitch tenía `#b87500`, con 3,56:1 sobre fondo claro). Ver `docs/design/design-system.md`.

## Componentes de `shared/ui`

Cada carpeta tiene `.ts`, `.html` y `.spec.ts`. Usa el selector `lt-*`.

| Componente             | Cuándo usarlo                                                                                     |
| ---------------------- | ------------------------------------------------------------------------------------------------- |
| `lt-icon`              | Ícono de Material Symbols. Decorativo por defecto; con `label` se anuncia                         |
| `lt-button`            | Acción de texto. Variantes `primary`, `accent`, `secondary`, `ghost`, `danger`; `loading`, `icon` |
| `lt-icon-button`       | Acción solo con ícono. `label` obligatorio                                                        |
| `lt-badge`             | Estado corto. Variantes `neutral`, `info`, `success`, `warning`, `danger`; `dot`, `live`          |
| `lt-card`              | Contenedor blanco con sombra suave. `padding` `md` o `lg`                                         |
| `lt-section-header`    | Título de una sección, con ícono y acción a la derecha (`[actions]`)                              |
| `lt-page-header`       | Título de página con migas de pan (`breadcrumbs`) y acciones (`[actions]`)                        |
| `lt-kpi-card`          | Indicador. Sin `value` muestra "Sin datos"; `tone` `positive`, `neutral`, `negative`              |
| `lt-progress-bar`      | Porcentaje (`value`, `max`), con `role="progressbar"`                                             |
| `lt-search-input`      | Búsqueda. `label` obligatorio aunque esté oculto; `disabled` para lo que no existe                |
| `lt-select-field`      | Lista desplegable con etiqueta en línea                                                           |
| `lt-filter-bar`        | Buscador siempre visible con filtros (`[filters]`) que se despliegan en celular                   |
| `lt-segmented-control` | Opciones excluyentes con radios nativos                                                           |
| `lt-empty-state`       | Estado vacío con acción opcional. `dashed` para zonas donde soltar algo                           |
| `lt-skeleton`          | Carga. Se anuncia como estado ocupado                                                             |
| `lt-pagination`        | Páginas. En celular muestra "Página X de Y"                                                       |
| `lt-confirm-dialog`    | Confirmación sobre CDK Dialog. `[(open)]`, `confirmed`, `cancelled`                               |

## Recetas

### Cómo agregar una feature nueva

1. Crea `src/app/features/<feature>/<feature>.routes.ts` que exporte `<FEATURE>_ROUTES`, con `loadComponent` a cada página.
2. Crea las páginas en `pages/<pagina>/<pagina>-page.ts` (clase `<Pagina>Page`). Usa solo `shared/ui` y, si hace falta, `core/`.
3. Registra la ruta en `src/app/app.routes.ts` con `loadChildren` dentro del `Shell`.
4. Agrega la entrada en `core/navigation/navigation-items.ts` con `enabled: true` y `path`.
5. Si necesitas un componente que se repita, va a `shared/ui` (nunca a otra feature).
6. Pruebas: una por página y una por componente de la feature.

### Cómo agregar un componente al kit

1. Crea `src/app/shared/ui/<nombre>/` con `<nombre>.ts`, `<nombre>.html` y `<nombre>.spec.ts`.
2. Selector `lt-<nombre>`, OnPush, solo `input()`, `output()` y `model()`.
3. Las variantes son un tipo cerrado y un mapa de clases completas.
4. Textos de negocio por inputs o contenido proyectado. Nada fijo.
5. Accesibilidad: nombre accesible, roles y `aria-*` cuando corresponda.
6. Agrégalo a la galería de `features/style-guide/components/component-gallery/` con sus variantes y estados.
7. `npm test`, `npm run lint` y `npm run build` en verde.

## Lo que no se debe hacer

- No importes una feature desde otra, ni por alias ni por ruta relativa que salga de la feature.
- No importes `core/` ni `features/` desde `shared/`.
- No uses hexadecimales ni clases armadas con variables en plantillas.
- No uses `any`; usa `unknown` y estrechamiento.
- No agregues librerías de componentes (Angular Material, PrimeNG, etc.).
- No pongas `NgModule`s, CDN de Tailwind ni carpetas genéricas `helpers/` o `services/` en la raíz.
- No muestres datos inventados: lo que no existe va como "Próximamente".
- No dejes textos en inglés al usuario ni menciones "LogiSync" o el escenario chileno: el producto es LogiTrack, en Colombia.
- No reformatees `docs/design/stitch/`: es la referencia exportada de Stitch.
