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

## 3. Lógica de negocio: puntuación, recálculo y anti-trampa

**Contexto.** El corazón del dominio: puntuación 3/1/0, recálculo al cargar
resultados, bloqueo por kickoff y la regla anti-trampa del historial.

**Cómo lo dirigí (no copy-paste).**
- **Puntuación 3/1/0** la aislé en una función pura de dominio
  (`ScoreCalculator.Calculate`) usando `Math.Sign(predHome-predAway) ==
  Math.Sign(actualHome-actualAway)` para el "mismo resultado". Pura → testeable sin
  base de datos. Escribí tests con los ejemplos exactos del brief (real 2-1: 3-0→1,
  2-1→3, 1-1→0).
- **Bloqueo por kickoff** lo modelé como regla de dominio en la entidad
  (`Match.IsLocked(now)` = `Finished || now >= kickoff`) y lo inyecté con
  `TimeProvider` para poder testear el "ahora" de forma determinista — no
  `DateTime.Now` disperso por el código.
- **Recálculo** (al cargar un resultado, recomputar `PointsAwarded` de todas las
  predicciones del partido) lo verifiqué con un test de integración contra SQLite
  in-memory; ahí descubrí (y arreglé) que faltaba sembrar los `AspNetUsers` por la
  FK de `Predictions` → buen recordatorio de que el modelo relacional sí valida.
- **Anti-trampa (5.5)**: el `UserHistoryService` oculta a quien no es el dueño las
  predicciones de partidos que aún no inician, pero mantiene los agregados
  (puntos/exactos) para no romper la paridad con el leaderboard. Lo verifiqué en vivo:
  el dueño ve 6 predicciones; un tercero ve solo las 4 de partidos ya jugados.

**Criterio propio.** La IA propone fácil meter la lógica en el controlador; insistí en
empujarla al dominio/Application (puro y testeable) y en inyectar el reloj. Resultado:
17 tests verdes que cubren puntuación, bloqueo y recálculo.

---

## 4. Despliegue real en Azure + pipeline (bloqueos sorteados en vivo)

**Contexto.** Llevar la app a producción en Azure (SWA + Container Apps + SQL Serverless)
y dejar un pipeline de GitHub Actions con OIDC y tests como gate.

**Bloqueos reales y cómo los resolví (con criterio, no a ciegas):**
- **RG con `LOCK-READONLY`** (patrón de gobernanza en casi todos los RGs de la
  suscripción). No lo quité unilateralmente: lo señalé, pedí autorización explícita, y
  además advertí que un lock ReadOnly **bloquea al propio pipeline** para actualizar
  recursos → debe quedar desactivado (o ser CanNotDelete) mientras esté desplegado.
- **Sin capacidad de Azure SQL ni Container Apps en East US / East US 2** ese día
  (`RegionDoesNotAllowProvisioning`, `AKSCapacityHeavyUsage`). En vez de quedarme
  trabado, hice un **loop por regiones** y caí en **centralus**, co-ubicando SQL +
  Container Apps para minimizar latencia (ACR/monitoreo quedaron en eastus2).
- **Sin Docker local** → construí la imagen con **`az acr build`** (build en la nube);
  el mismo Dockerfile multistage corre idéntico en el pipeline.
- **OIDC en vez de secreto de larga vida**: app registration + federated credential
  (`repo:sanvarp/PollaMundialista:ref:refs/heads/main`) + Contributor *solo* en el RG.
  El primer `role assignment` falló por propagación del SP; reintenté con
  `--assignee-object-id` + `--assignee-principal-type` y entró.

**Resultado.** App **live** y verificada end-to-end (health, login, leaderboard, CORS),
con pipeline `test → deploy-api → deploy-frontend` en verde a la primera.
