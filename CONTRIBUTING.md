# Contributing to Coalesce

## Standards and Quality

- Pull requests should solve a single problem and not include any changes that weren't necessary to solve that problem.
- Pull request titles should follow [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/#summary).
- Unless your change is trivial (e.g. a minor documentation update, or fixing an obvious bug), it is unlikely to be accepted if you haven't discussed the work with project maintainers in a Github issue.
- Changes should be accompanied by appropriate automated tests.
- For TypeScript/Vue code, Eslint is used to enforce style and formatting.
- For C#, you are expected to write well-formatted, well-organized code.

## Installing dependencies

Install NPM dependencies in the following projects by running `npm ci` in each directory:

- `src/coalesce-vue`
- `src/coalesce-vue-vuetify3`
- `playground/Coalesce.Web.Vue3` (if you plan to run the Vue3 playground)
- `docs` (if you plan to run the docs)

## Debugging Code Generation

The following can be used to debug code generation against your own Coalesce project, or against the playground project in this repository.

### From Visual Studio:

1. Navigate to `src/IntelliTect.Coalesce.DotnetTool/Properties` (on disk) or `cli/IntelliTect.Coalesce.DotnetTool/Properties` (Visual Studio Solution Explorer).
1. Create a launchSettings.json file.
1. Add a launch profile similar to the following, updating the workingDirectory to point at the directory that contains your coalesce.json. [Additional CLI parameters](https://intellitect.github.io/Coalesce/stacks/agnostic/generation.html#cli-options) may be passed as desired.

```json
{
  "profiles": {
    "MyProject": {
      "commandName": "Project",
      "commandLineArgs": "C:\\src\\MyProject\\coalesce.json"
    }
  }
}
```

### From the command line:

Run the following command from the `src/IntelliTect.Coalesce.DotnetTool` project directory. [Additional CLI parameters](https://intellitect.github.io/Coalesce/stacks/agnostic/generation.html#cli-options) may be passed as desired.

```bash
dotnet run -- <path-to-coalesce.json> --debug
```

## Running the Documentation

```bash
cd docs
npm ci
npm run dev
```

Then open the documentation in your browser at the URL provided in the output after launching.

## Running the Vue3 Playground Project

```bash
cd playground/Coalesce.Web.Vue3
npm ci
npm run coalesce
```

Then, `dotnet run` or launch from Visual Studio.
