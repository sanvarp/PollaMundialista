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

## 2. _(pendiente)_

---

## 3. _(pendiente)_
