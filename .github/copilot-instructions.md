# Coalesce Framework Development Instructions

Always follow these instructions first and fallback to additional search and context gathering only when the information here is incomplete or found to be in error.

## Project Overview

Coalesce is a framework for rapid development of ASP.NET Core + Vue.js web applications. It generates DTOs, API controllers, and TypeScript from Entity Framework Core models and other C# code.

## Prerequisites & Dependencies

The required tools and dependencies are automatically installed via the GitHub Actions workflow at `.github/workflows/copilot-setup-steps.yml`.

## Instructions

- Always update the documentation when making changes or adding features that will affect developers who use Coalesce.
- Always add an entry to CHANGELOG.md when adding new features or fixing non-trivial bugs.
- Avoid making breaking changes if not necessary. A less obvious example of a breaking change would be changing an existing CSS class name.
- Consider adding or updating example files in `playground\Coalesce.Web.Vue3\src\examples` when making changes to coalesce-vue-vuetify.

## Validation Checklist

After making changes, ALWAYS run this validation sequence:

1. **Build verification**:
   From the repo root:

```bash
npm ci
dotnet build
cd src/coalesce-vue && npm run build
cd ../coalesce-vue-vuetify3 && npm run build
```

2. **Test verification**:
   From the repo root:

```bash
dotnet test
cd src/coalesce-vue && npm run test
cd ../coalesce-vue-vuetify3 && npm run test
```

3. **Template verification**:
   If you make changes to the template in the `templates` directory, validate the changes by running `TestLocal.ps1 -- "--FeatureOne --FeatureTwo"` where the FeatureOne, FeatureTwo parameters are replaced with each flag that might affect the changes you made. Run it multiple times if there are different combinations of feature flags that might interact in different ways. The flags are the variables checked by the `#if` in the template code.

4. **Documentation verification**:
   If the documentation was updated:

```bash
cd docs
npm run build
```

REMEMBER: NEVER CANCEL long-running build operations.
