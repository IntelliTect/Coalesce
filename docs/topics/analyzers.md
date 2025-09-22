# Analyzers

Coalesce includes a set of C# Roslyn analyzers that help enforce best practices and catch common issues during development. These analyzers run in your IDE as well as part of your build process and provide warnings or errors when potential problems are detected.

The analyzers are shipped as part of the `IntelliTect.Coalesce.Analyzer` NuGet package and are automatically included when you reference the main Coalesce packages.

## Available Rules

The following analyzer rules are currently available in Coalesce:

<script setup>
import AnalyzerRules from './AnalyzerRules.vue'
</script>

<AnalyzerRules />

## Configuring Analyzers

The recommended way to configure the behavior of each analyzer rule is through your project's `.editorconfig` file. For example:

```ini
[*.cs]
# Suppress COA0002 analyzer
dotnet_diagnostic.COA0002.severity = none

# Change COA0003 to error
dotnet_diagnostic.COA0003.severity = error
```

You can also suppress specific analyzer warnings inline using `#pragma` directives:

```csharp
#pragma warning disable COA0002
// Code that triggers COA0002
#pragma warning restore COA0002
```
