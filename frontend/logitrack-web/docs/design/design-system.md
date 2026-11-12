# Sistema de diseño de LogiTrack

## De dónde sale

El diseño de referencia se hizo en **Google Stitch**. La pantalla "Despachos y Asignación" está exportada en [`docs/design/stitch/despachos-y-asignacion.html`](stitch/despachos-y-asignacion.html) y se conserva sin cambios como referencia del curso.

El HTML de Stitch no se copia: se traduce a tokens de Tailwind v4 y a componentes Angular.

## Qué se cambió respecto a Stitch, y por qué

| En Stitch                                                                | Problema                                                | Qué se hizo                                                                          |
| ------------------------------------------------------------------------ | ------------------------------------------------------- | ------------------------------------------------------------------------------------ |
| Tailwind v3 por CDK, con la configuración en un `<script>`               | No sirve en producción                                  | Tokens en `src/styles.css` con Tailwind v4                                           |
| `rounded-full` redefinido como `0.75rem`                                 | Rompe los círculos                                      | `rounded-full` conserva su significado; los radios de Stitch van a `sm`, `lg` y `xl` |
| Barra lateral fija de 256 px en todos los anchos                         | No funciona en celular                                  | Panel deslizante en celular; barra fija desde 1024 px, colapsable a 72 px            |
| `::-webkit-scrollbar { display: none }`                                  | Mala accesibilidad y usabilidad                         | Eliminado                                                                            |
| `darkMode: "class"` sin colores oscuros                                  | El modo oscuro no existía                               | Valores oscuros definidos para cada token                                            |
| Selects y buscadores sin `label`; botones de solo ícono sin nombre       | Inaccesibles                                            | `label` obligatorio y `aria-label` en los botones de ícono                           |
| Animaciones permanentes (`pulse`, `ping`, `spin`)                        | Distraen; ignoran la preferencia de movimiento reducido | Solo con `motion-safe:`, y solo para lo que está en vivo                             |
| Fuente de íconos cargada dos veces; imágenes de `googleusercontent`      | Peso innecesario; imágenes que pueden desaparecer       | Íconos y fuentes autoalojados una sola vez; sin imágenes externas                    |
| "LogiSync", Chile (Santiago, patentes `KJ-88-21`, `+56`, licencia A4)    | No es nuestro producto ni nuestro país                  | LogiTrack, Colombia: placas `ABC123`, `+57`, licencias C1, C2 y C3, `es-CO`          |
| Sugerencias de IA, importación SAP, selector de hubs, telemetría en vivo | Prometen algo que la app no hace                        | No se incluyen                                                                       |

**Un valor que no coincide con Stitch:** `--lt-on-tertiary-container` en claro es `#966100`. Stitch usa `#b87500`, que tiene 3,56:1 sobre el fondo claro, por debajo de AA para texto. Se oscureció a 4,97:1.

**Tokens que Stitch define y el tema no tenía:** se añadieron 9 colores (`tertiary`, `on-tertiary`, `tertiary-container`, `on-tertiary-fixed` y su variante, `on-secondary-fixed`, `on-primary-fixed` y su variante, `surface-tint`), los márgenes `margin` y `margin-desktop`, y los títulos `headline-xl-mobile` y `headline-lg-mobile`.

## Tokens

Los valores claros de color son los de Stitch (47 colores verificados uno a uno). Los oscuros son propios, siguiendo los roles de Material 3, y cada par de texto y fondo se revisó contra AA.

### Contraste de texto sobre fondo

Razón de contraste WCAG. Mínimo para texto normal: 4,5:1.

| Texto                     | Fondo                    | Claro | Oscuro |
| ------------------------- | ------------------------ | ----- | ------ |
| on-surface                | surface                  | 16,23 | 14,36  |
| on-surface-variant        | surface-container        | 8,07  | 9,65   |
| on-surface-variant        | surface-container-lowest | 9,39  | 11,33  |
| on-primary                | primary                  | 21,00 | 7,73   |
| on-primary-container      | primary-container        | 4,56  | 9,76   |
| on-secondary              | secondary                | 6,69  | 7,74   |
| on-secondary-container    | secondary-container      | 4,55  | 7,01   |
| on-error                  | error                    | 6,46  | 7,72   |
| on-error-container        | error-container          | 7,24  | 7,24   |
| on-tertiary-container     | surface (advertencias)   | 4,97  | 10,89  |
| inverse-on-surface        | inverse-surface          | 11,60 | 10,19  |
| on-tertiary               | tertiary                 | 21,00 | 13,34  |
| on-tertiary-fixed         | tertiary-fixed           | 13,34 | 13,34  |
| on-secondary-fixed        | secondary-fixed          | 13,25 | 13,25  |
| on-primary-fixed          | primary-fixed            | 13,30 | 13,30  |
| on-primary-fixed-variant  | primary-fixed            | 7,26  | 7,26   |
| on-tertiary-fixed-variant | tertiary-fixed           | 7,26  | 7,26   |

La misma tabla está en la guía de estilo (`/style-guide`).

### Errores que encontró la revisión en navegador

- En oscuro, `on-secondary-fixed-variant` era igual a su fondo: las insignias `info` quedaban sin texto. Ahora usa `#003ea8`.
- Los ítems deshabilitados del menú usaban `text-outline` (4,05:1 en claro). Ahora usan `on-surface-variant`.

## Escala tipográfica

| Token                | Tamaño        | Uso                                                                                |
| -------------------- | ------------- | ---------------------------------------------------------------------------------- |
| `headline-xl`        | 36 px / 44 px | Título de página en escritorio                                                     |
| `headline-xl-mobile` | 28 px / 36 px | Título de página en celular                                                        |
| `headline-lg`        | 28 px / 36 px | Título de sección grande                                                           |
| `headline-lg-mobile` | 22 px / 28 px | Título de sección grande en celular                                                |
| `headline-md`        | 20 px / 28 px | Título de tarjeta                                                                  |
| `headline-sm`        | 16 px / 24 px | Título de sección                                                                  |
| `body-lg`            | 16 px / 24 px | Texto de formularios en celular                                                    |
| `body-md`            | 14 px / 20 px | Texto general                                                                      |
| `body-sm`            | 12 px / 16 px | Texto de apoyo                                                                     |
| `label-lg`           | 14 px / 20 px | Etiquetas de botón y de campo                                                      |
| `label-md`           | 12 px / 16 px | Etiquetas de menú y de chips                                                       |
| `label-sm`           | 11 px / 14 px | Solo etiquetas secundarias; nunca información que el usuario necesite para decidir |
| `telemetry-lg`       | 24 px / 28 px | Cifras de indicadores                                                              |
| `telemetry-md`       | 16 px / 20 px | Cifras pequeñas: peso, placa                                                       |

Títulos en Plus Jakarta Sans; el resto en Inter. Ambas están autoalojadas (`@fontsource`).

## Espaciado, radios y sombras

- **Espaciado:** `space-xs` 4 px, `space-sm` 8 px, `space-md` 16 px, `space-lg` 24 px, `space-xl` 32 px. `gutter` 16 px en celular y `gutter-desktop` 24 px en escritorio. `margin` 16 px y `margin-desktop` 32 px.
- **Radios:** `rounded-sm` 2 px, `rounded-lg` 4 px, `rounded-xl` 8 px. `rounded-full` conserva su valor de Tailwind.
- **Sombras:** `shadow-soft` (`0 1px 8px` al 4 %) y `shadow-soft-md` (al 8 %).

## Componentes

La lista y el uso de cada uno están en [`CLAUDE.md`](../../CLAUDE.md#componentes-de-sharedui). Cada componente se ve con todas sus variantes y estados en `/style-guide`, solo fuera de producción.

## Accesibilidad

- Foco visible global con `:focus-visible` en el color secundario.
- `prefers-reduced-motion`: las transiciones y animaciones se reducen a 0,01 ms.
- `color-scheme` sigue al tema, para que los controles nativos también cambien.
- Los campos usan 16 px en celular para evitar el zoom automático de iOS.
- Las notificaciones se anuncian con `aria-live`; el panel móvil atrapa el foco y se cierra con Esc.
