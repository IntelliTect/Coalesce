---
applyTo: "**"
---

# Coalesce Copilot instructions

## Overview

An `IntelliTect Coalesce`–based project.

- Backend: ASP.NET Core with EF Core (migrations)
- Frontend: Vue 3, Vuetify 3, and Coalesce for data binding and API access
- Coalesce generates DTOs, controllers, and TypeScript from your models. After making changes to your models, invoke code generation with the `coalesce_generate` tool.
- If you run tests with `npm test`, pass `--run` to avoid running in interactive mode. E.g. `npm test -- --run`.
- You don't need to add import statements for components or composables from `vue`, `vue-router`, or those defined by the project's source code. This project is configured to auto-import these.

## Docs

Use the Context7 server to fetch official docs. Flow: `#resolve-library-id` → `#get-library-docs`. The following are existing known library IDs that may be useful:

- Coalesce: `/IntelliTect/Coalesce`
- `Vuetify`: `/vuetifyjs/vuetify`
