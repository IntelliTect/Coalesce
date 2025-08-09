# Coalesce Framework Development Instructions

Always follow these instructions first and fallback to additional search and context gathering only when the information here is incomplete or found to be in error.

## Project Overview

Coalesce is a framework for rapid development of ASP.NET Core + Vue.js web applications. It generates DTOs, API controllers, and TypeScript from Entity Framework Core models.

## Prerequisites & Dependencies

**CRITICAL**: Install these exact versions and runtimes before working with the codebase:

- **.NET 9.0 SDK**: Required for building projects. Install via: `curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 9.0 --install-dir ~/.dotnet`
- **.NET 8.0 runtime and ASP.NET Core runtime**: Required for tests. Install via: 
  ```bash
  curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 8.0 --runtime dotnet --install-dir ~/.dotnet
  curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 8.0 --runtime aspnetcore --install-dir ~/.dotnet
  ```
- **Node.js v20+**: Tested with v20.19.4
- **NPM v10+**: Tested with v10.8.2
- **Add .NET to PATH**: `export PATH="$HOME/.dotnet:$PATH"`

## Build & Development Workflow

**ALWAYS run these commands in order with the specified timeouts:**

### 1. Install Dependencies
```bash
# From repository root - NEVER run npm ci in subdirectories
npm ci
```
- **Timing**: 2-3 minutes. NEVER CANCEL. Set timeout to 300+ seconds.
- **Purpose**: Installs all NPM workspace dependencies

### 2. Restore .NET Packages
```bash
export PATH="$HOME/.dotnet:$PATH"  # Always set PATH first
dotnet restore
```
- **Timing**: 1-2 minutes. NEVER CANCEL. Set timeout to 180+ seconds.
- **Purpose**: Restores NuGet packages for all projects

### 3. Build Solution
```bash
export PATH="$HOME/.dotnet:$PATH"
dotnet build --no-restore --configuration Release
```
- **Timing**: 45-60 seconds. NEVER CANCEL. Set timeout to 120+ seconds.
- **Purpose**: Builds all .NET projects

### 4. Run Tests
```bash
export PATH="$HOME/.dotnet:$PATH"
# Run .NET 9.0 tests (reliable)
dotnet test --no-build --configuration Release --framework net9.0
```
- **Timing**: 35-40 seconds. NEVER CANCEL. Set timeout to 120+ seconds.
- **Results**: 704 tests should pass, 4 may be skipped
- **Note**: .NET 8.0 tests may fail due to runtime dependencies

## Code Generation Workflow

Test code generation using the playground project:

```bash
cd playground/Coalesce.Web.Vue3
export PATH="$HOME/.dotnet:$PATH"
npm run coalesce
```
- **Timing**: 15-20 seconds. Set timeout to 60+ seconds.
- **Purpose**: Generates TypeScript and API code from .NET models

## Frontend Build & Test

### Vue Package Testing
```bash
# Test core Vue package
cd src/coalesce-vue
npm run test
```
- **Timing**: 7-10 seconds. 568 tests should pass.

```bash
# Test Vuetify package  
cd src/coalesce-vue-vuetify3
npm run test
```
- **Timing**: 15-20 seconds. 98 tests should pass.

### Playground Frontend Build
```bash
cd playground/Coalesce.Web.Vue3
npm run build
```
- **Timing**: 5-10 seconds. Builds Vite frontend.

## Project Structure

- **`src/`**: Core Coalesce framework (.NET projects)
  - `IntelliTect.Coalesce/`: Main framework
  - `IntelliTect.Coalesce.CodeGeneration/`: Code generation engine
  - `IntelliTect.Coalesce.Vue/`: ASP.NET Core Vue integration
  - `coalesce-vue/`: Core Vue.js TypeScript package
  - `coalesce-vue-vuetify3/`: Vuetify 3 component library
  - `IntelliTect.Coalesce.DotnetTool/`: CLI tool for code generation

- **`playground/`**: Example application
  - `Coalesce.Domain/`: Entity Framework models
  - `Coalesce.Web.Vue3/`: Full-stack Vue 3 + ASP.NET Core app

- **`docs/`**: VitePress documentation website
- **`templates/`**: Project templates for `dotnet new`

## Known Issues & Workarounds

### Documentation Build
```bash
cd docs
npm run build  # FAILS due to spelling errors in markdown files
```
- **Issue**: CSpell validation fails on technical terms
- **Workaround**: Skip docs build for now

### ESLint (Playground)
```bash
cd playground/Coalesce.Web.Vue3  
npm run lint  # FAILS due to old ESLint config format
```
- **Issue**: Uses legacy .eslintrc.js with new ESLint v9
- **Workaround**: ESLint needs migration to flat config format

### Playground Application Runtime
The playground application cannot run locally without configuration:
- **Database**: Requires SQL Server LocalDB or SQLite configuration
- **Azure OpenAI**: Requires Azure OpenAI endpoint configuration
- **Workaround**: Use these environment variables for basic testing:
  ```bash
  export ConnectionStrings__DefaultConnection="Data Source=app.db"
  export Azure__OpenAI__Deployment="dummy"
  export Azure__OpenAI__Endpoint="https://dummy.openai.azure.com/"
  export Azure__OpenAI__Key="dummy"
  ```

## Validation Checklist

After making changes, ALWAYS run this validation sequence:

1. **Build verification**:
   ```bash
   npm ci  # 2-3 minutes, NEVER CANCEL
   export PATH="$HOME/.dotnet:$PATH"
   dotnet restore  # 1-2 minutes, NEVER CANCEL  
   dotnet build --no-restore --configuration Release  # 45-60 seconds
   ```

2. **Test verification**:
   ```bash
   dotnet test --no-build --configuration Release --framework net9.0  # 35-40 seconds
   cd src/coalesce-vue && npm run test  # 7-10 seconds
   cd ../coalesce-vue-vuetify3 && npm run test  # 15-20 seconds
   ```

3. **Code generation verification**:
   ```bash
   cd playground/Coalesce.Web.Vue3
   npm run coalesce  # 15-20 seconds
   npm run build  # 5-10 seconds
   ```

## Key Configuration Files

- **`coalesce-vue3.json`**: Code generation configuration for playground
- **`package.json`** (root): NPM workspace configuration
- **`Coalesce.sln`**: Main solution file with all projects
- **`Directory.Packages.props`**: Centralized NuGet package versions

## Debugging Code Generation

To debug the CLI tool against playground:
```bash
cd src/IntelliTect.Coalesce.DotnetTool
dotnet run --framework net8.0 -- ../../coalesce-vue3.json --debug
```

## Common Commands Summary

| Task | Command | Timing | Critical Notes |
|------|---------|---------|----------------|
| Install deps | `npm ci` | 2-3 min | NEVER CANCEL, timeout 300s |
| Restore .NET | `dotnet restore` | 1-2 min | NEVER CANCEL, timeout 180s |
| Build | `dotnet build --no-restore --configuration Release` | 45-60s | timeout 120s |
| Test | `dotnet test --no-build --configuration Release --framework net9.0` | 35-40s | 704 tests pass |
| Generate | `npm run coalesce` | 15-20s | From playground dir |
| Vue build | `npm run build` | 5-10s | From playground dir |

REMEMBER: Always set `export PATH="$HOME/.dotnet:$PATH"` before running .NET commands, and NEVER CANCEL long-running build operations.