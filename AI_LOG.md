# AI_LOG

Registro del uso de IA (Claude Code) en la construcción de Polla Mundialista. Mi rol fue el de
**arquitecto y responsable de las decisiones**: definí el alcance, las reglas de negocio y las
decisiones técnicas, y utilicé la IA como par-programador para acelerar el diseño, la
implementación y —sobre todo— la depuración. A continuación, cuatro casos representativos.

---

## 1. Scaffolding y decisiones de arranque

**Contexto.** Monorepo nuevo, stack fijado por el brief (.NET 10 + Angular 21). Antes de generar
código, era necesario validar que las versiones del brief siguieran siendo las correctas.

**Hallazgo / criterio propio.** Angular **22.0.0 alcanzó GA el 3 de junio de 2026**, un día antes
de iniciar el desarrollo. En lugar de aceptar el valor del brief sin verificar ("la v22 sigue en
RC"), comprobé el estado real en npm (dist-tags de `@angular/core` y fechas de publicación) y
actualicé la justificación: se mantiene **Angular 21 (LTS)** no porque la v22 sea RC —ya no lo
es—, sino porque incorporar una versión mayor con **un día de antigüedad** a producción es
injustificable en un contexto empresarial. La conclusión coincide; el razonamiento es más sólido.

---

## 2. El despliegue lo dirigí yo: CI/CD con los tests como gate

**Contexto.** Había que llevar la aplicación a producción (Azure) y, más importante, dejar un
**proceso** de despliegue, no una publicación manual.

**Cómo lo dirigí.** La decisión fue mía: sin despliegues manuales. Solicité un **pipeline de
CI/CD** en GitHub Actions con dos principios no negociables:

1. **Los tests son el gate** — si la suite no pasa, no se despliega nada.
2. **Sin secretos de larga duración** — autenticación contra Azure mediante **OIDC / federated
   credentials**, no un service principal con contraseña almacenada en los secrets.

Separé los workflows: `ci.yml` se ejecuta en cada Pull Request; `deploy.yml` se ejecuta al hacer
push a `main`, con la cadena `test → deploy-api → deploy-frontend`. La IA implementó la mecánica
siguiendo esa guía (app registration + federated credential, y `az acr build` para construir la
imagen del backend en la nube, sin depender de Docker local).

**Resultado.** Aplicación **en producción** y verificada de extremo a extremo (health, login,
leaderboard, CORS), con el pipeline en verde y un despliegue reproducible y auditable en cada push
a `main`.

---

## 3. El proceso: cómo se valida y se publica cada cambio

Tan importante como que la IA escriba código es **cómo se controla lo que escribe**. Definí un
sistema de salvaguardas que se ejecuta en cada cambio, bajo un principio: **un único gate, tres
puertas** — las mismas comprobaciones se ejecutan en tres lugares para que nada se escape.

**El gate** es un único script (`scripts/verify-full.sh`, la fuente de verdad):

1. `prettier --check` — formato del frontend.
2. `eslint` (`angular-eslint`) — análisis estático del frontend.
3. `dotnet format --verify-no-changes` — formato del backend y analizadores Roslyn
   (`Directory.Build.props`).
4. `dotnet build -c Release && dotnet test` — compila y ejecuta **los 35 tests**.
5. `ng build` — compila el frontend de producción.

(Existe una versión rápida, `verify-fast.sh`, solo formato y análisis estático, para no ralentizar
cada commit.)

**Las tres puertas:**

- **Puerta 1 — Claude Code, durante el desarrollo.** El comando `/verify` ejecuta el gate completo
  a demanda; un **hook `PreToolUse`** intercepta cada comando de shell y **bloquea cualquier intento
  de omitir el control** (`commit`/`push --no-verify`); y un **subagente revisor** (`code-reviewer`)
  audita el diff antes de cada push.
- **Puerta 2 — git hooks, en local** (`core.hooksPath=.githooks`). `pre-commit` ejecuta el gate
  rápido; `pre-push` ejecuta el gate completo. No es posible hacer commit ni push con el árbol de
  trabajo en mal estado.
- **Puerta 3 — CI, en la nube.** Las mismas comprobaciones en `ci.yml` (cada Pull Request) y en
  `deploy.yml` (push a `main`), con **los tests como gate** antes de desplegar — el pipeline del
  caso 2.

El mismo script se ejecuta de forma idéntica en mi equipo y en GitHub Actions: se elimina el
clásico "en mi máquina funciona".

**Los 35 tests, en detalle** — no son triviales; protegen las reglas de negocio que no se pueden
romper:

- **23 unitarios** (puros, sin base de datos):
  - _Puntuación 3/1/0 (11)_ — `ScoreCalculator`, con los ejemplos exactos del brief: 9 casos de
    `Calculate` (3 de marcador exacto → 3 pts; 3 de resultado acertado → 1; 3 de fallo → 0) y 2 de
    `IsExactHit`.
  - _Bloqueo por inicio del partido (4)_ — `Match.IsLocked`: abierto antes del inicio, bloqueado
    **en** el inicio y después, un partido `Finished` bloqueado incluso antes del inicio, y un
    `Scheduled` sin resultado.
  - _Email con TLD real (8)_ — validadores de registro e inicio de sesión: aceptan `a@b.co`,
    `user+tag@sub.domain.io`; rechazan `a@b`, `a@b.`, `abc`, `@b.com`, `a@b@c.com`.
- **4 de servicio** (SQLite en memoria, modelo EF real):
  - _Recálculo (3)_ — `AdminService`: cargar un resultado marca el partido como `Finished` y
    **recalcula los puntos de todas sus predicciones** (exacto→3 / resultado→1 / fallo→0); 404 si el
    partido no existe; **revertir** lo devuelve a `Scheduled` y anula los puntos.
  - _Standings (1)_ — agrega puntos y ordena por grupo derivándolos de `Matches` (un partido sin
    finalizar no cuenta).
- **8 de integración** (`WebApplicationFactory`, el pipeline real con JWT, CORS y EF): registro →
  inicio de sesión (201 + rol `User` + token); `/api/matches` exige autenticación (401); `/health`
  anónimo y `Healthy`; `/health/ready` valida la base de datos; cabeceras de seguridad
  (`X-Content-Type-Options: nosniff`) y de correlación (`X-Correlation-ID`); un usuario no
  administrador no puede cargar un resultado (403); **el flujo completo** (predecir → el
  administrador carga el resultado exacto → la predicción puntúa 3 → leaderboard poblado); y no se
  puede predecir tras el inicio del partido (409 `Conflict`).

**Verificación visual.** Para los cambios de interfaz no me conformo con "parece que funciona": un
script con **`puppeteer-core` sobre el Chrome del sistema** opera la aplicación como un usuario,
**instrumenta el DOM fotograma a fotograma** y graba **GIFs** de cada escenario para revisarlos
antes de publicar.

**No es decorativo.** El subagente revisor ya detectó, antes de llegar a producción, una **fuga de
tweens de GSAP** (sin liberar al destruir el componente) y un error de **animaciones solapadas**
que dejaba filas fijadas en `position:absolute` (ver caso 4).

---

## 4. Depurar la animación del leaderboard: iteración guiada

**Contexto.** El leaderboard se reordena con una animación (GSAP Flip) cuando entra un resultado.
Presentaba defectos sutiles: un "salto" al terminar, un parpadeo en el encabezado de la tabla y el
caso más difícil — cuando un jugador **cruza** entre el podio (3 tarjetas) y la lista inferior
(filas), que son **dos contenedores del DOM distintos** que Flip no puede transicionar como si
fueran uno.

**Cómo lo dirigí.** No me conformé con "parece que funciona". Para no juzgar a simple vista, hice
que la IA **instrumentara** la animación (registrar la posición de cada fila fotograma a fotograma)
y que me **mostrara videos** de cada escenario antes de publicar nada:

> _"¿hay posibilidad de que me muestres un video de cada caso? quiero ver cómo va quedando la
> solución."_

La IA grabó GIFs de cada caso (reordenar dentro del podio, ascender al podio, descender a la lista
y varios cruces simultáneos) que revisé uno por uno, solicitando correcciones concretas: que la
tarjeta que sale **desaparezca**, que la que entra **se construya** en su lugar, que el encabezado
no se mueva. El diagnóstico final salió de los **datos**, no de conjeturas: una fila entrante
quedaba "sola" en el flujo cuando Flip volvía absolutas a las demás, por lo que se posicionaba mal
y se "teletransportaba" al terminar.

**El proceso detectó un error real.** El subagente revisor (caso 3) audita el diff antes de cada
push. En este cambio detectó que, al refrescar la tabla —o al entrar un resultado del
administrador— **a mitad de la animación**, una segunda ejecución dejaba filas fijadas en
`position:absolute`. Lo corregí (abortar y limpiar la animación en curso antes de iniciar otra) y
solo entonces hice el push. La IA no solo **genera** código: lo **revisa con criterio** antes de
que llegue al repositorio.

**Resultado.** Animación fluida y consistente en todos los escenarios —incluidos los cruces
múltiples simultáneos y `prefers-reduced-motion`—, verificada fotograma a fotograma, sin filas
fijadas ni errores de consola.
