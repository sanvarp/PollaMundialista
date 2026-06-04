# AI_LOG

Registro de cómo se usó IA (Claude Code) en la construcción de Polla Mundialista.
Para cada entrada: **contexto → prompt → qué entregó la IA → qué ajusté yo** (criterio propio).

> Se completa con 2–3 casos representativos para la sesión en vivo:
> 1. Arquitectura/scaffolding del monorepo y las capas.
> 2. Un bloqueo técnico concreto (p.ej. animación Flip del leaderboard, shader del hero, o detalle de EF/Identity/CORS).
> 3. Lógica de negocio (puntuación 3/1/0 + recálculo, o el bloqueo por kickoff).

---

## 1. Scaffolding y decisiones de arranque

**Contexto.** Monorepo nuevo, stack pinneado (.NET 10 + Angular 21). Antes de generar
nada, había que validar que las versiones del brief siguieran siendo las correctas.

**Hallazgo / criterio propio.** Angular **22.0.0 salió GA el 3-jun-2026**, un día antes
de empezar el build. En vez de aceptar ciegamente el pin del brief ("v22 sigue en RC"),
verifiqué el estado real en npm (`@angular/core` dist-tags + fechas de release) y
actualicé la justificación: se mantiene **Angular 21 (LTS)** no porque v22 sea RC —ya no lo es—
sino porque meter un major con **1 día de vida** a producción es injustificable en
contexto enterprise. La conclusión coincide; el razonamiento es más sólido.

_(prompt y diff detallados se añaden en la sesión)_

---

## 2. Bloqueo: OpenAPI/Swagger en .NET 10 (breaking change de namespaces)

**Contexto.** Quería Swagger UI con botón "Authorize" para la demo. Añadí
`Swashbuckle.AspNetCore` y el build falló: `Microsoft.OpenApi.Models` no existe.

**Diagnóstico.** El template de .NET 10 ya referencia `Microsoft.AspNetCore.OpenApi`,
que arrastra **Microsoft.OpenApi 2.0**, donde los tipos (`OpenApiInfo`,
`OpenApiSecurityScheme`, …) se movieron del namespace `Microsoft.OpenApi.Models` al
namespace raíz `Microsoft.OpenApi`, y Swashbuckle 10 además convive mal con esa versión.

**Decisión / criterio propio.** En vez de pelear con Swashbuckle (ya no es first-party
desde .NET 9), migré al stack idiomático actual: **OpenAPI integrado (`AddOpenApi`) +
Scalar** para la UI. Para el "Authorize" implementé un `IOpenApiDocumentTransformer`
que inyecta el esquema Bearer. Verifiqué el patrón exacto contra la doc oficial de
Microsoft Learn (no lo escribí de memoria) y lo adapté al proyecto.

**Resultado.** API documentada en `/scalar/v1`, con auth JWT funcional para probar
endpoints protegidos en vivo. Defensa: "Swashbuckle dejó de ser first-party; usé el
generador OpenAPI nativo + Scalar, que es la recomendación actual de ASP.NET Core".

### Nota de decisión: base de datos provider-agnóstica

La máquina de build no tenía SQL Server / LocalDB / Docker. En vez de bloquear el
desarrollo, hice el `DbContext` agnóstico al proveedor: **SQLite en local** (cero
instalación, `EnsureCreated`) y **Azure SQL en producción** (migraciones EF). Esto
también beneficia a quien clone el repo público: `dotnet run` y listo.

---

## 3. _(pendiente — lógica de negocio: puntuación 3/1/0 + recálculo / bloqueo por kickoff)_
