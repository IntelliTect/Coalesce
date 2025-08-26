---
applyTo: '**'
---

# Coalesce Copilot instructions

## Overview

An `IntelliTect Coalesce`–based project.

- Backend: ASP.NET Core with EF Core (migrations)
- Frontend: Vue 3, Vuetify 3, and Coalesce for data binding and API access
- Coalesce generates DTOs, controllers, and TypeScript from your models. After making changes to your models, invoke code generation with the `coalesce_generate` tool.

## Docs (canonical)

Use the Context7 server to fetch official docs. Flow: `#resolve-library-id` → `#get-library-docs`.

- Coalesce: `/IntelliTect/Coalesce`
- `Vuetify`: `/vuetifyjs/vuetify`

## Standard workflow

- After EF model or migration changes, run `coalesce_generate` to sync DTOs, controllers, and TypeScript.
- Build the solution to validate server-side changes.

## Troubleshooting

- DTO/API mismatch or missing endpoints: rerun `coalesce_generate`, then rebuild.
- TypeScript compile errors after model changes: rerun `coalesce_generate`, rebuild the frontend, verify imports.
- API shape surprises: confirm EF model and migrations are current; regenerate with `coalesce_generate`.
- Build errors: clean, restore packages, rebuild; regenerate if models changed.
