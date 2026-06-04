# Modelo de datos (ERD)

```mermaid
erDiagram
    AspNetUsers ||--o{ Predictions : makes
    Teams ||--o{ Matches : "home/away"
    Matches ||--o{ Predictions : "predicted in"

    AspNetUsers {
        string Id PK
        string Email
        string DisplayName
        string PasswordHash
    }
    Teams {
        int Id PK
        string Code "3 letras (ISO)"
        string Name
        string GroupName "A, B..."
    }
    Matches {
        int Id PK
        string GroupName
        int HomeTeamId FK
        int AwayTeamId FK
        datetime KickoffUtc
        string Stage "Group"
        int HomeGoals "nullable"
        int AwayGoals "nullable"
        int Status "Scheduled=0, Finished=1"
    }
    Predictions {
        int Id PK
        string UserId FK
        int MatchId FK
        int PredHomeGoals
        int PredAwayGoals
        int PointsAwarded "nullable"
        datetime CreatedAtUtc
        datetime UpdatedAtUtc
    }
```

## Constraints e índices

- Roles vía Identity (`AspNetRoles`): `Admin` y `User`, sembrados.
- `Predictions`: índice **único** `(UserId, MatchId)` → una predicción por usuario/partido.
- `Teams.Code`: índice único (3 letras).
- `Matches.Status`: enum (`Scheduled=0` / `Finished=1`), persistido como `int`.
- FKs `Matches.HomeTeamId` / `AwayTeamId` → `Teams.Id` con `DeleteBehavior.Restrict`
  (no se puede borrar un equipo con partidos).
- FK `Predictions.MatchId` → `Matches.Id` y `Predictions.UserId` → `AspNetUsers.Id`,
  ambas en cascada.
- Todas las fechas se guardan en **UTC**; el front formatea a hora local.

## Datos semilla

- 8 equipos reales en 2 grupos (A y B) → round-robin de 6 partidos por grupo = **12 partidos**.
- Kickoffs entre el 11 y 27 de junio de 2026 (UTC).
- La **jornada 1** de cada grupo viene precargada como jugada (4 partidos `Finished`)
  para poblar el leaderboard en la demo; los 8 restantes quedan abiertos para predecir.
- 5 usuarios demo con predicciones de ejemplo (ver credenciales en el README).

> Nota: el sorteo de grupos es **ilustrativo** para la demo; los equipos son reales y
> las fechas coherentes. Se pueden sustituir por las fixtures oficiales cuando estén.
