; Unshipped analyzer release
; https://github.com/dotnet/roslyn/blob/main/src/RoslynAnalyzers/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
COA0014 | Usage | Warning | NoAutoInclude only affects navigation properties (reference navigation and collection navigation properties). It has no effect on plain data properties like strings, integers, or other value types.