# Coalesce Framework Development Guide

Always reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the info here.

## Overview
Coalesce is a framework for rapid development of ASP.NET Core + Vue.js web applications. It generates DTOs, API controllers, and TypeScript from Entity Framework Core data models. The framework supports both Vue 2 and Vue 3 with Vuetify UI components.

## Prerequisites and Setup

### Required SDKs and Runtimes
Install .NET 9.0 SDK and both .NET 8.0 + 9.0 runtimes:
```bash
# Install .NET 9.0 SDK
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 9.0

# Install .NET 8.0 runtime
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 8.0 --runtime dotnet

# Install ASP.NET Core 8.0 runtime  
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 8.0 --runtime aspnetcore

# Update PATH
export PATH="$HOME/.dotnet:$PATH"
```

Node.js and npm are required (version 20+ recommended).

## Building and Testing

### Initial Setup and Build
NEVER CANCEL builds or tests - they can take significant time but will complete successfully.

```bash
# 1. Restore NuGet packages (takes ~85 seconds)
dotnet restore
# NEVER CANCEL: Wait for completion, estimated 90 seconds. Set timeout to 120+ seconds.

# 2. Build the solution (takes ~50 seconds)  
dotnet build --no-restore --configuration Release
# NEVER CANCEL: Wait for completion, estimated 60 seconds. Set timeout to 90+ seconds.

# 3. Run .NET tests (takes ~30 seconds, 563/568 pass)
dotnet test --no-build --configuration Release --framework net9.0
# NEVER CANCEL: Wait for completion, estimated 45 seconds. Set timeout to 60+ seconds.
# Note: 1 TypeScript compilation test failure is expected and acceptable.
```

### NPM Package Testing
Install and test npm packages for Vue components:

```bash
# Install and test core Vue package (takes ~40 seconds install, ~6 seconds tests)
cd src/coalesce-vue
npm ci
npm run test

# Install Vue3 Vuetify package (takes ~25 seconds)
cd ../coalesce-vue-vuetify3  
npm ci

# Install Vue2 Vuetify package (takes ~70 seconds)
cd ../coalesce-vue-vuetify2
npm ci
```

## Playground Applications

### Vue3 Playground (Recommended for Testing)
```bash
cd playground/Coalesce.Web.Vue3

# Install dependencies (takes ~30 seconds)
npm ci

# Run code generation (takes ~18 seconds)  
npm run coalesce
# NEVER CANCEL: Code generation builds .NET tools first, estimated 20 seconds.

# Build frontend (takes ~6 seconds)
npm run build

# Build backend (takes ~3 seconds)
dotnet build --configuration Release
```

### Vue2 Playground
```bash
cd playground/Coalesce.Web.Vue2

# Install dependencies (takes ~23 seconds)
npm ci

# Run code generation (takes ~13 seconds)
npm run coalesce

# Build frontend (takes ~11 seconds) 
npm run build

# Build backend
dotnet build --configuration Release
```

### Database Limitation
**CRITICAL**: Playground applications are configured for SQL Server LocalDB and will NOT run on Linux/macOS. They require Windows for database functionality. Use the applications only for build testing, not runtime testing on non-Windows platforms.

## Documentation
```bash
cd docs

# Install dependencies (takes ~50 seconds)
npm ci

# Run development server
npm run dev
# Serves at http://localhost:5173/Coalesce/ (or next available port)
```

## Validation Workflow
After making changes, always run this complete validation sequence:

```bash
# 1. Build and test .NET (total ~3 minutes)
dotnet restore
dotnet build --no-restore --configuration Release  
dotnet test --no-build --configuration Release --framework net9.0

# 2. Test Vue packages (~1 minute)
cd src/coalesce-vue && npm ci && npm run test

# 3. Test playground build (~1 minute)
cd ../../playground/Coalesce.Web.Vue3
npm ci
npm run coalesce
npm run build
dotnet build --configuration Release
```

## Key Project Structure
- `src/IntelliTect.Coalesce*` - Core C# framework libraries
- `src/coalesce-vue*` - TypeScript/Vue npm packages  
- `playground/` - Example applications (Vue2 and Vue3)
- `templates/` - Project template for new Coalesce apps
- `docs/` - VitePress documentation site

## Code Generation
Coalesce generates TypeScript, DTOs, and API controllers from C# models:
- Run `npm run coalesce` in playground projects to test generation
- Uses `dotnet run --project ../../src/IntelliTect.Coalesce.DotnetTool`
- Configuration files: `coalesce-vue2.json` and `coalesce-vue3.json`

## Common Issues
- **"LocalDB not supported"**: Expected on Linux/macOS, playground requires Windows SQL Server
- **Missing .NET runtime**: Install both 8.0 and 9.0 runtimes as shown above
- **TypeScript compilation test failure**: 1 test failure in VueOutputCompiles is expected
- **Long build times**: This is normal, do not cancel builds

## Testing Strategy
1. **Build validation**: Ensure `dotnet build` succeeds 
2. **Test validation**: Run `dotnet test` (allow 1 failure)
3. **Generation validation**: Test `npm run coalesce` in playgrounds
4. **Frontend validation**: Test `npm run build` in playgrounds
5. **Package validation**: Test `npm run test` in Vue packages

Always use appropriate timeouts (90+ seconds for builds, 60+ seconds for tests) and never cancel long-running operations.