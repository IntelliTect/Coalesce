namespace IntelliTect.Coalesce.Analyzer.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AttributeUsageAnalyzer : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor InvalidInjectAttributeUsageRule = new(
        id: "COA0002",
        title: "Invalid InjectAttribute usage",
        messageFormat: "InjectAttribute is only valid on parameters of Coalesce client methods",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        customTags: [WellKnownDiagnosticTags.Unnecessary],
        description: "InjectAttribute can only be used on parameters of methods that are exposed by the Coalesce framework.");

    public static readonly DiagnosticDescriptor InvalidCoalesceUsageOnNestedTypesRule = new(
        id: "COA0003",
        title: "Invalid CoalesceAttribute usage on nested data source or behavior",
        messageFormat: "CoalesceAttribute is not needed to expose nested data sources or behaviors - they are automatically discovered by their containing type",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        customTags: [WellKnownDiagnosticTags.Unnecessary],
        description: "Nested data sources and behaviors are automatically associated with their containing type and do not need the [Coalesce] attribute.");

    public static readonly DiagnosticDescriptor InvalidCoalesceUsageRule = new(
        id: "COA0004",
        title: "Invalid CoalesceAttribute usage on unsupported type",
        messageFormat: "CoalesceAttribute can only expose types that inherit from DbContext, implement IDataSource<T>, IBehaviors<T>, IClassDto<T>, or are marked with [Service], [StandaloneEntity], or [SimpleModel]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "The [Coalesce] attribute should only be applied to types that are supported by the Coalesce framework.");

    public static readonly DiagnosticDescriptor UnexposedSecondaryAttributeForTypesRule = new(
        id: "COA0005",
        title: "Unexposed secondary attribute",
        messageFormat: "{0} must be accompanied by [Coalesce] to be exposed by the Coalesce framework",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Types marked with [Service], [StandaloneEntity], or [SimpleModel] require [Coalesce] attribute to be properly processed by the Coalesce framework.");

    public static readonly DiagnosticDescriptor UnexposedSecondaryAttributeForMethodsRule = new(
        id: "COA0006",
        title: "Unexposed secondary attribute",
        messageFormat: "{0} attribute does not function on a method that is not exposed by Coalesce",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Methods marked with [Execute] require either [Coalesce] or [SemanticKernel] attribute to be properly processed by the Coalesce framework.");

    public static readonly DiagnosticDescriptor MissingFileTypeAttributeRule = new(
        id: "COA0201",
        title: "Consider adding FileTypeAttribute to IFile parameter",
        messageFormat: "Consider adding [FileType] attribute to specify suggested file types in the browser for this IFile parameter",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "IFile parameters on Coalesce-exposed methods should specify suggested file types using the [FileType] attribute to improve default user experience.");

    public static readonly DiagnosticDescriptor InvalidSemanticKernelAttributeUsageRule = new(
        id: "COA0007",
        title: "Invalid SemanticKernelAttribute usage on service or behavior type",
        messageFormat: "SemanticKernelAttribute is not valid on {0}",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "SemanticKernelAttribute should not be used on service types or behavior types.");

    public static readonly DiagnosticDescriptor GenericInvalidAttributeUsageRule = new(
        id: "COA0008",
        title: "Invalid attribute usage",
        messageFormat: "{0} has no effect in this location",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Detects Coalesce attributes that have no effect when applied in certain contexts and should be removed for cleaner code.");

    public static readonly DiagnosticDescriptor MutuallyExclusiveCoalesceTypesRule = new(
        id: "COA0013",
        title: "Mutually exclusive Coalesce type markers",
        messageFormat: "Type cannot have multiple Coalesce type markers: {0}",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Types can only have one of the following: [Service], [StandaloneEntity], [SimpleModel] attributes, or inherit from DbContext, IDataSource<T>, IBehaviors<T>, or IClassDto<T>.");

    public static readonly DiagnosticDescriptor NoAutoIncludeOnNonObjectPropertyRule = new(
        id: "COA0014",
        title: "NoAutoInclude has no effect on non-object properties",
        messageFormat: "NoAutoInclude has no effect on non-object properties",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        customTags: [WellKnownDiagnosticTags.Unnecessary],
        description: "NoAutoInclude only affects navigation properties (objects or collections). It has no effect on simple data properties like strings, numbers, or dates.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(InvalidInjectAttributeUsageRule, InvalidCoalesceUsageOnNestedTypesRule, InvalidCoalesceUsageRule, UnexposedSecondaryAttributeForTypesRule, UnexposedSecondaryAttributeForMethodsRule, MissingFileTypeAttributeRule, InvalidSemanticKernelAttributeUsageRule, GenericInvalidAttributeUsageRule, MutuallyExclusiveCoalesceTypesRule, NoAutoIncludeOnNonObjectPropertyRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
        context.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
        context.RegisterSymbolAction(AnalyzeProperty, SymbolKind.Property);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var typeSymbol = (INamedTypeSymbol)context.Symbol;

        // COA0013: Check for mutually exclusive Coalesce type markers
        var typeMarkers = new List<string>();

        // Check for attributes
        var serviceAttr = typeSymbol.GetAttributeByName("IntelliTect.Coalesce.ServiceAttribute");
        if (serviceAttr is not null) typeMarkers.Add("[Service]");

        var standaloneEntityAttr = typeSymbol.GetAttributeByName("IntelliTect.Coalesce.StandaloneEntityAttribute");
        if (standaloneEntityAttr is not null) typeMarkers.Add("[StandaloneEntity]");

        var simpleModelAttr = typeSymbol.GetAttributeByName("IntelliTect.Coalesce.SimpleModelAttribute");
        if (simpleModelAttr is not null) typeMarkers.Add("[SimpleModel]");

        var exposeAttr = standaloneEntityAttr ?? serviceAttr ?? simpleModelAttr;

        // Check for interface/base class implementations
        if (typeSymbol.InheritsFromOrImplements("Microsoft.EntityFrameworkCore.DbContext"))
            typeMarkers.Add("DbContext");
        if (typeSymbol.InheritsFromOrImplements("IntelliTect.Coalesce.IDataSource`1"))
            typeMarkers.Add("IDataSource<T>");
        if (typeSymbol.InheritsFromOrImplements("IntelliTect.Coalesce.IBehaviors`1"))
            typeMarkers.Add("IBehaviors<T>");
        if (typeSymbol.InheritsFromOrImplements("IntelliTect.Coalesce.IClassDto`1"))
            typeMarkers.Add("IClassDto<T>");

        if (typeMarkers.Count > 1)
        {
            var markersString = string.Join(", ", typeMarkers);
            var location = typeSymbol.Locations.FirstOrDefault() ?? Location.None;
            context.ReportDiagnostic(Diagnostic.Create(MutuallyExclusiveCoalesceTypesRule, location, markersString));
        }

        var coalesceAttr = typeSymbol.GetAttributeByName("IntelliTect.Coalesce.CoalesceAttribute");
        var semanticKernelAttr = typeSymbol.GetAttributeByName("IntelliTect.Coalesce.SemanticKernelAttribute");

        // COA0007: Check for SemanticKernelAttribute on services or IBehaviors
        if (semanticKernelAttr is not null)
        {
            var isService = serviceAttr is not null;
            var isBehavior = typeSymbol.InheritsFromOrImplements("IntelliTect.Coalesce.IBehaviors`1");

            if (isService || isBehavior)
            {
                var typeDescription = isService ? "services" : "behaviors";
                context.ReportDiagnostic(Diagnostic.Create(InvalidSemanticKernelAttributeUsageRule, semanticKernelAttr.GetLocation(), typeDescription));
            }
        }

        if (coalesceAttr is null)
        {
            // COA0005: Check for StandaloneEntity, Service, or SimpleModel attributes without Coalesce
            if (exposeAttr is not null)
            {
                context.ReportDiagnostic(Diagnostic.Create(UnexposedSecondaryAttributeForTypesRule, exposeAttr.GetLocation(), exposeAttr.AttributeClass!.Name!));
            }
        }
        else if (!coalesceAttr.ConstructorArguments.Any() && !coalesceAttr.NamedArguments.Any())
        {
            // If there are no ctor args and no named args, it isn't being used to configure ClientTypeName

            // COA0004: Check for CoalesceAttribute on types that aren't valid targets for CoalesceAttribute.
            if (
                !typeSymbol.InheritsFromOrImplements(
                    "Microsoft.EntityFrameworkCore.DbContext",
                    "IntelliTect.Coalesce.IDataSource`1",
                    "IntelliTect.Coalesce.IBehaviors`1",
                    "IntelliTect.Coalesce.IClassDto`1") &&
                serviceAttr is null && standaloneEntityAttr is null && simpleModelAttr is null
            )
            {
                context.ReportDiagnostic(Diagnostic.Create(InvalidCoalesceUsageRule, coalesceAttr.GetLocation()));
            }

            // COA0003: Check for useless CoalesceAttribute usage on nested crud strategies
            if (typeSymbol.ContainingType is not null &&
                typeSymbol.InheritsFromOrImplements(
                    "IntelliTect.Coalesce.IBehaviors`1",
                    "IntelliTect.Coalesce.IDataSource`1"
            ))
            {
                context.ReportDiagnostic(Diagnostic.Create(InvalidCoalesceUsageOnNestedTypesRule, coalesceAttr.GetLocation()));
            }
        }
    }

    private static void AnalyzeMethod(SymbolAnalysisContext context)
    {
        var methodSymbol = (IMethodSymbol)context.Symbol;

        // Skip special methods like property getters/setters, event accessors, operators, etc., but allow constructors
        if (methodSymbol.MethodKind is not (MethodKind.Ordinary or MethodKind.Constructor))
            return;

        // COA0008: Check for general useless attributes inside crud strategies
        if (methodSymbol.GetAttributesByName(
                "IntelliTect.Coalesce.CoalesceAttribute",
                "IntelliTect.Coalesce.SemanticKernelAttribute",
                "IntelliTect.Coalesce.DataAnnotations.ExecuteAttribute",
                "IntelliTect.Coalesce.DataAnnotations.HiddenAttribute",
                "IntelliTect.Coalesce.DataAnnotations.InternalUseAttribute",
                "IntelliTect.Coalesce.DataAnnotations.LoadFromDataSourceAttribute"
            ).FirstOrDefault() is { } badAttrInsideCrudStrategy &&
            methodSymbol.ContainingType.InheritsFromOrImplements(
                "IntelliTect.Coalesce.IDataSource`1",
                "IntelliTect.Coalesce.IBehaviors`1"
            ) &&
            badAttrInsideCrudStrategy?.GetLocation() is Location attrLocation
        )
        {
            context.ReportDiagnostic(Diagnostic.Create(GenericInvalidAttributeUsageRule, attrLocation, badAttrInsideCrudStrategy.AttributeClass!.Name));
        }

        if (IsValidCoalesceMethod(methodSymbol, true))
        {
            if (IsValidCoalesceMethod(methodSymbol, false))
            {
                // COA0201 Suggest FileType on IFile parameters on exposed signatures (not implementations)
                foreach (var parameter in methodSymbol.Parameters)
                {
                    if (parameter.Type is INamedTypeSymbol namedType &&
                        namedType.InheritsFromOrImplements("IntelliTect.Coalesce.Models.IFile") &&
                        parameter.GetAttributeByName("IntelliTect.Coalesce.DataAnnotations.FileTypeAttribute") is null &&
                        parameter.Locations.FirstOrDefault() is Location paramLocation
                    )
                    {
                        context.ReportDiagnostic(Diagnostic.Create(MissingFileTypeAttributeRule, paramLocation));
                    }
                }
            }
            return;
        }

        var executeAttr = methodSymbol.GetAttributeByName("IntelliTect.Coalesce.DataAnnotations.ExecuteAttribute");
        if (executeAttr is not null && executeAttr.GetLocation() is Location location)
        {
            var diagnostic = Diagnostic.Create(UnexposedSecondaryAttributeForMethodsRule, location, "[Execute]");
            context.ReportDiagnostic(diagnostic);
        }

        // Analyze parameters for InjectAttribute usage
        foreach (var parameter in methodSymbol.Parameters)
        {
            var injectAttr = parameter.GetAttributeByName("IntelliTect.Coalesce.DataAnnotations.InjectAttribute");
            if (injectAttr is not null && injectAttr.GetLocation() is Location location2)
            {
                context.ReportDiagnostic(Diagnostic.Create(InvalidInjectAttributeUsageRule, location2));
            }
        }
    }

    private static bool IsValidCoalesceMethod(IMethodSymbol methodSymbol, bool includeInterfaceImplementations = false)
    {
        // Check if method has [Coalesce] or [SemanticKernel] attribute
        if (methodSymbol.GetAttributesByName(
            "IntelliTect.Coalesce.CoalesceAttribute",
            "IntelliTect.Coalesce.SemanticKernelAttribute").Any())
        {
            return true;
        }

        // Check if method is on an interface marked with [Service]
        var containingType = methodSymbol.ContainingType;
        if (
            containingType != null &&
            containingType.TypeKind == TypeKind.Interface &&
            containingType.GetAttributeByName("IntelliTect.Coalesce.ServiceAttribute") != null)
        {
            return true;
        }

        // Check if method implements an interface method that would be valid
        if (includeInterfaceImplementations && containingType != null && containingType.TypeKind == TypeKind.Class)
        {
            foreach (var interfaceType in containingType.AllInterfaces)
            {
                var interfaceMethod = interfaceType.GetMembers(methodSymbol.Name)
                    .OfType<IMethodSymbol>()
                    .FirstOrDefault(m => methodSymbol.ContainingType.FindImplementationForInterfaceMember(m)?.Equals(methodSymbol, SymbolEqualityComparer.Default) == true);

                if (interfaceMethod != null && IsValidCoalesceMethod(interfaceMethod, includeInterfaceImplementations))
                    return true;
            }
        }

        return false;
    }

    private static void AnalyzeProperty(SymbolAnalysisContext context)
    {
        var propertySymbol = (IPropertySymbol)context.Symbol;

        // COA0014: Check for NoAutoInclude on non-object properties
        var readAttr = propertySymbol.GetAttributeByName("IntelliTect.Coalesce.DataAnnotations.ReadAttribute");
        if (readAttr is not null)
        {
            var noAutoIncludeArg = readAttr.NamedArguments.FirstOrDefault(arg => arg.Key == "NoAutoInclude");
            if (noAutoIncludeArg.Key is not null && noAutoIncludeArg.Value.Value is true && !IsPOCO(propertySymbol.Type))
            {
                var location = readAttr.GetLocation() ?? propertySymbol.Locations[0];
                context.ReportDiagnostic(Diagnostic.Create(NoAutoIncludeOnNonObjectPropertyRule, location));
            }
        }
    }

    private static bool IsPOCO(ITypeSymbol type)
    {
        // Unwrap nullable types
        if (type is INamedTypeSymbol namedType && namedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
        {
            type = namedType.TypeArguments[0];
        }

        // String is not an object type for our purposes
        if (type.SpecialType == SpecialType.System_String)
            return false;

        // byte[] is not a collection type for our purposes (it's typically binary data)
        if (type is IArrayTypeSymbol arrayType && arrayType.ElementType.SpecialType == SpecialType.System_Byte)
            return false;

        // Arrays of non-byte types are collections
        if (type is IArrayTypeSymbol)
            return true;

        // Class or interface types (excluding string which we already checked)
        // These are "object-like" types that could be navigation properties
        if (type.TypeKind == TypeKind.Class || type.TypeKind == TypeKind.Interface)
            return true;

        return false;
    }
}
