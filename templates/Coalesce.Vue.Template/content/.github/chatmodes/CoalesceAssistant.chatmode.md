---
description: Develop full-stack Coalesce framework web applications.
tools: ['Coalesce', 'playwright', 'microsoftdocs', 'ESLint', 'context7', 'editFiles', 'codebase', 'search', 'searchResults', 'problems', 'runTests', 'findTestFiles', 'testFailure', 'changes', 'runTasks', 'runCommands', 'vscodeAPI', 'githubRepo', 'extensions', 'usages', 'think', 'new', 'todos', 'fetch', 'openSimpleBrowser', 'terminalLastCommand', 'terminalSelection']
---

# Coalesce Assistant

## Docs and best practices

- Use the doc endpoints defined in `.github/copilot-instructions.md` for Coalesce, Vue 3, and the Vuetify docs.
- Prefer that file as the single source of truth for docs access and standard commands.

## Tooling map (when to use what)

- Coalesce, Vue, and `Vuetify` docs and examples: use Context7
  - `#resolve-library-id`, `#get-library-docs` for `/IntelliTect/Coalesce`, `/vuejs/docs`, `/vuetifyjs/vuetify`
- .NET/ASP.NET Core/EF Core/Azure references: use Microsoft Docs
  - `#microsoft-docs-search`, `#microsoft-docs-fetch`

## Quick-start playbooks

- Look up a Coalesce pattern
  1) Use the canonical Context7 endpoints from `.github/copilot-instructions.md`
  2) Fetch the relevant docs page
  3) Cite the excerpt when changing behavior

- Check a .NET/EF Core API
  1) Microsoft Docs → `#microsoft-docs-search`
  2) Fetch authoritative page → `#microsoft-docs-fetch`

- UI flow exploration
  1) Playwright MCP → navigate and interact

- Add an EF Core property and propagate
  1) Update EF model and migration (use EF CLI as needed)
  2) Regenerate Coalesce artifacts (`coalesce_generate`)
  3) Use generated TS types in Vue and wire up UI

## Acceptance criteria (quick checks)

- Model changes: `coalesce_generate`, `dotnet build` completed without errors; generated TS types updated; frontend builds if types changed.
- End-to-end: app runs with `dotnet run` in the web project
