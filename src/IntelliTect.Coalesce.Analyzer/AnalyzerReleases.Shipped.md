; Shipped analyzer releases
; https://github.com/dotnet/roslyn/blob/main/src/RoslynAnalyzers/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

## Release 6.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
COA0001 | Usage | Error | PermissionLevel is only valid for class-level security attributes. For property-level security, use the Roles property to specify role-based access control.
COA0002 | Usage | Warning | InjectAttribute can only be used on parameters of methods that are exposed by the Coalesce framework.
COA0003 | Usage | Warning | Nested data sources and behaviors are automatically associated with their containing type and do not need the [Coalesce] attribute.
COA0004 | Usage | Warning | The [Coalesce] attribute should only be applied to types that are supported by the Coalesce framework.
COA0005 | Usage | Warning | Types marked with [Service], [StandaloneEntity], or [SimpleModel] require [Coalesce] attribute to be properly processed by the Coalesce framework.
COA0006 | Usage | Warning | Methods marked with [Execute] require either [Coalesce] or [SemanticKernel] attribute to be properly processed by the Coalesce framework.
COA0007 | Usage | Warning | SemanticKernelAttribute should not be used on service types or behavior types.
COA0008 | Usage | Warning | Detects Coalesce attributes that have no effect when applied in certain contexts and should be removed for cleaner code.
COA0009 | Usage | Error | A nested behaviors class should not be defined when the containing model has Create, Edit, and Delete attributes all set to DenyAll, as the behaviors will never be used.
COA0010 | Usage | Error | Save-related methods should not be overridden when the containing model has Create and Edit attributes both set to DenyAll, as these methods will never be called.
COA0011 | Usage | Error | Delete-related methods should not be overridden when the containing model has Delete attribute set to DenyAll, as these methods will never be called.
COA0012 | Usage | Warning | Ordering operations (OrderBy, OrderByDescending, ThenBy, ThenByDescending) applied to queries returned from GetQuery methods may be overridden by client-specified sorting. Consider moving the ordering logic to ApplyListDefaultSorting or using [DefaultOrderBy] attributes on model properties.
COA0013 | Usage | Error | Types can only have one of the following: [Service], [StandaloneEntity], [SimpleModel] attributes, or inherit from DbContext, IDataSource<T>, IBehaviors<T>, or IClassDto<T>.
COA0201 | Usage | Info | IFile parameters on Coalesce-exposed methods should specify suggested file types using the [FileType] attribute to improve default user experience.
COA1001 | Style | Info | ItemResult and ItemResult<T> constructors can often be replaced with implicit conversions from boolean, string, and object values. This provides cleaner, more readable code while maintaining the same functionality.
COA1002 | Style | Hidden | Marks the unnecessary parts of ItemResult constructor calls that can be removed when using implicit conversions. This diagnostic helps IDE syntax highlighting identify which portions of the code will be simplified by the COA1001 code fix.
COA2001 | Security | Warning | Data sources that perform authorization checks should ensure their served type has a default data source to prevent security bypasses. Without a default data source, clients can directly access the served type without the authorization logic.