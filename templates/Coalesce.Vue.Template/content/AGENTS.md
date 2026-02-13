---
applyTo: "**"
---

## Overview

An `IntelliTect Coalesce`–based project. Projects:

- AppHost: Aspire host project. Always launch this, never launch `Web` directly.
- Data: EF Core entity models, services, business logic, etc
- Data.Test: xUnit automated tests for the Data project.
- Migrations: EF Core Migrations assembly. Create and manage migrations here, e.g. `dotnet ef migrations add MigrationName`. Run `dotnet ef` in this project's working directory, don't pass extra args like startup-project to `dotnet ef` calls.
- Web: ASP.NET Core server. Serves both API endpoints and frontend Vue assets. Automatically launches Vite dev server - don't do `npm run dev` manually.
- Web/src: Vite, Vue 3, Vuetify 3, and Coalesce for data binding and API access.

- Coalesce generates DTOs, controllers, and TypeScript from your models. After making changes to your models, invoke code generation with the `coalesce_generate` tool. The `coalesce.json` file is in the workspace root.
- If you run tests with `npm test`, pass `--run` to avoid running in interactive mode. E.g. `npm test -- --run`.
- In Vue files, you don't need to add import statements for components nor composables from `vue`, `vue-router`, or those defined by the project's source code - they will be auto-imported by Vite.

## Docs

Use the Context7 server to fetch official docs. Flow: `#resolve-library-id` → `#get-library-docs`. The following are existing known library IDs that may be useful:

- Coalesce: `/IntelliTect/Coalesce`
- `Vuetify`: `/vuetifyjs/vuetify`
