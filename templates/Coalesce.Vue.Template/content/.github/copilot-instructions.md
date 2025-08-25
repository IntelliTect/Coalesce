---
applyTo: '**'
---

# Coalesce Copilot instructions

## Overview

An `IntelliTect Coalesce`–based project.

- Backend: ASP.NET Core with EF Core (migrations)
- Frontend: Vue 3 with Coalesce for data binding and API access
- Coalesce generates DTOs, controllers, and TypeScript clients from EF Core models — no manual endpoints. Trigger generation after EF model/migration changes with the `dotnet coalesce` command from the Web project.

## Docs (canonical)

Use the Context7 server to fetch official docs. Flow: `#resolve-library-id` → `#get-library-docs`.

- Coalesce: `/IntelliTect/Coalesce`
- Vue 3: `/vuejs/docs`
- `Vuetify`: `/vuetifyjs/vuetify`

## Standard workflow

- After EF model or migration changes, run `dotnet coalesce` to sync DTOs, controllers, and TypeScript.
- Build the solution to validate server-side changes.
- If generated TypeScript changed, rebuild the frontend.
- Run the app end-to-end with `dotnet run` in the web project.

## Troubleshooting

- DTO/API mismatch or missing endpoints: rerun `dotnet coalesce`, then rebuild.
- TypeScript compile errors after model changes: rerun `dotnet coalesce`, rebuild the frontend, verify imports.
- API shape surprises: confirm EF model and migrations are current; regenerate with `dotnet coalesce`.
- Build errors: clean, restore packages, rebuild; regenerate if models changed.
