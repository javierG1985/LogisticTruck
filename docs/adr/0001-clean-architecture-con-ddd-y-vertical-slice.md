# ADR 0001: Clean Architecture con DDD y Vertical Slice

- Estado: aceptada
- Fecha: 2026-11-03

## Contexto

LogiTrack es una plataforma de logística con varias áreas de negocio (flota, clientes, envíos, documentos, facturación) que se usará en un curso para llevar a los alumnos de junior a senior. El código debe servir de ejemplo: claro, consistente y sin complejidad innecesaria.

Hacen falta tres cosas a la vez:

- Separar las **capas** técnicas para que el dominio no dependa de la base de datos ni de HTTP.
- Dividir el negocio en **partes** (bounded contexts) para que cada área tenga su propio lenguaje y su propio esquema.
- Organizar cada **caso de uso** de forma que sea fácil encontrarlo, leerlo y cambiarlo sin afectar a los demás.

## Decisión

Usamos **Clean Architecture** con **DDD** (estratégico y táctico) y **Vertical Slice** dentro de la capa de aplicación. Cada idea resuelve un problema distinto:

| Idea | Qué decide | Cómo se ve |
| --- | --- | --- |
| Clean Architecture | En qué capa va cada pieza y quién puede depender de quién | Cuatro proyectos: `Domain`, `Application`, `Infrastructure`, `Api` |
| DDD estratégico | En qué partes del negocio se divide el sistema | Una carpeta `Modules/<Modulo>/` por módulo en cada capa |
| DDD táctico | Dónde viven las reglas | Agregados, value objects y domain events en `LogiTrack.Domain` |
| Vertical Slice | Cómo se organiza cada caso de uso | Una carpeta por caso de uso en `Application`: `Modules/Fleet/Trucks/CreateTruck/` |

Las dependencias entre proyectos son la regla de arquitectura: `Application → Domain`, `Infrastructure → Application, Domain`, `Api → Application, Infrastructure`. Ninguna otra. Un proyecto que no referencia EF Core no puede usarlo, aunque alguien quiera.

Las pruebas de arquitectura (`LogiTrack.ArchitectureTests`) cubren lo que las referencias no pueden expresar: que un módulo no use otro, que los controladores solo dependan de `ISender` y de su recurso, y que los agregados no tengan setters públicos.

## Alternativas descartadas

- **N-capas tradicional** (`Controllers → Services → Repositories → Entities`): los servicios terminan siendo un cajón de sastre y la lógica de negocio se reparte entre servicios y entidades anémicas. No protege las invariantes.
- **Capas como carpetas dentro de un solo proyecto**: nada impide que una carpeta importe otra. La dependencia se rompe en silencio y el compilador no lo advierte.
- **Proyectos por capa dentro de cada módulo** (`Fleet.Domain`, `Fleet.Application`…): multiplica los proyectos por cada módulo sin ganar aislamiento real, porque el monolito comparte una única base de datos y una única transacción.
- **Microservicios desde el inicio**: añade red, despliegue y consistencia eventual antes de que el dominio esté entendido. Se reserva para cuando un módulo lo necesite (fase de Tracking y Notifications).

## Consecuencias

- Positivas:
  - Las reglas de negocio viven en un único lugar (`Domain`) y se prueban sin infraestructura.
  - Cambiar la base de datos o el framework HTTP no toca el dominio.
  - Cada caso de uso es una carpeta autocontenida: comando o consulta, validador y handler.
  - Los módulos quedan preparados para separarse más adelante, porque ya no comparten tipos.
- Negativas:
  - Más archivos para una operación sencilla (un CRUD pequeño genera varias clases por caso de uso).
  - Hay que explicar la estructura a cada alumno nuevo; por eso existe `CLAUDE.md`.
  - Algunos mapeos se repiten entre capas (por ejemplo, la respuesta y el modelo), a cambio de que cada capa no conozca la forma de la otra.
