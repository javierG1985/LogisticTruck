# ADR 0004: Autenticación con JWT en cookie HttpOnly, roles y permisos en base de datos

## Estado

Aceptado.

## Contexto

La API no tenía autenticación: cualquiera que llegara al puerto podía leer y modificar camiones, conductores y asignaciones. Hace falta identificar a quien opera, decidir qué puede hacer, proteger el token en el navegador frente a XSS y poder cambiar permisos sin recompilar.

## Decisión

- **JWT (HS256)** emitido por `POST /api/auth/login`. El token lleva `sub`, `email`, `role` (código del rol), un claim `permission` por cada permiso del rol y `jti`. La vigencia la fija y la valida el **backend**: `Jwt:ExpiresMinutes` (60 por defecto), `ValidateLifetime` y una tolerancia de reloj de 30 segundos.
- **El token viaja en la cookie `lt_session`** con `HttpOnly`, `Secure` y `SameSite=Lax` (en desarrollo; `Strict` antes de producción):
  - `HttpOnly`: el JavaScript de la página no puede leer el token.
  - `Secure`: solo se envía por HTTPS (los navegadores aceptan `localhost`).
  - El cuerpo del login devuelve `email`, `role` y `expiresIn`, nunca el token.
  - `POST /api/auth/logout` borra la cookie.
- **Lectura del token:** `JwtBearer` lo toma de la cookie. Si viene la cabecera `Authorization: Bearer` (clientes de API y pruebas), esa manda.
- **Clave de firma** en `Jwt:SigningKey`. En desarrollo está en `appsettings.Development.json` (solo local). En producción va por variable de entorno (`Jwt__SigningKey`). Si falta o tiene menos de 32 caracteres, la aplicación no arranca.
- **HTTPS solo fuera de desarrollo:** redirección, HSTS y `RequireHttpsMetadata` se aplican cuando el entorno no es `Development`.
- **Contraseñas con PBKDF2-SHA256** (600 000 iteraciones, sal de 16 bytes, comparación en tiempo constante). Mínimo 8 caracteres.
- **Roles y permisos en base de datos** (esquema `security`):
  - `roles` (`code`, `name`): `Administrator` (Administrador), `Operator` (Operador), `Driver` (Conductor).
  - `permissions` (`code`, `description`): `trucks.read`, `trucks.write`, `drivers.read`, `drivers.write`, `assignments.read`, `assignments.write`, `users.manage`.
  - `role_permissions` (`role_code`, `permission_code`): qué permisos tiene cada rol.
  - `users.role_code` es clave foránea a `roles.code`.
  - Las políticas de la API comprueban **permisos**, no roles: `[Authorize(Policy = Permissions.TrucksWrite)]`. Los códigos de permiso están en `Application/Common/Security/Permissions.cs`; la asignación a roles está en la base.
  - Para cambiar lo que puede hacer un rol (por ejemplo, el conductor) basta con modificar `role_permissions` en una migración; no hace falta tocar los controladores.
  - Asignación inicial: Administrator, todos; Operator, todos salvo `users.manage`; Driver, solo lectura de camiones, conductores y asignaciones. **Los permisos del conductor están por definir** y se ajustan con esa tabla.
- **Mensaje genérico en el login:** "Correo o contraseña no válidos."
- **Primer administrador:** `dotnet run --project src/LogiTrack.Api -- --bootstrap-admin` con `Bootstrap__AdminEmail` y `Bootstrap__AdminPassword` en el entorno. Es idempotente.

## Consecuencias

- Crear un usuario exige que el rol exista: si no, responde 400 con `role` como error de validación.
- Los catálogos de parametrización para el frontend (tipos de licencia, documentos, etc.) serán tablas aparte, de solo lectura para el front. No cambian las reglas de los agregados.
- El logout solo borra la cookie. Un JWT es stateless: un token copiado sigue válido hasta que expire (máximo 60 minutos).
- El frontend envía las peticiones con la cookie (mismo origen o `credentials: 'include'`) y no puede leer el token.

## Riesgos residuales y trabajo pendiente

- **CSRF:** `SameSite=Lax` cubre las escrituras desde otros sitios en navegadores modernos. Si se permite un frontend en otro origen, añadir token anti-falsificación.
- **Fuerza bruta en el login:** añadir `AddRateLimiter` a `api/auth/login` (por ejemplo 5 intentos por minuto por IP).
- **Permisos del conductor:** definir qué ve y qué puede modificar.
- Rotación de la clave de firma y revocación de tokens: fases posteriores.
