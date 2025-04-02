using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.TypeDefinition;
using IntelliTect.Coalesce.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IntelliTect.Coalesce.CodeGeneration.Api.Generators
{
    public class ClassDto : StringBuilderCSharpGenerator<ClassViewModel>
    {
        public ClassDto(GeneratorServices services) : base(services)
        {
        }

        protected string DtoNamespace
        {
            get
            {
                string namespaceName = Namespace;
                if (!string.IsNullOrWhiteSpace(AreaName))
                {
                    namespaceName += "." + AreaName;
                }
                namespaceName += ".Models";
                return namespaceName;
            }
        }

        public override void BuildOutput(CSharpCodeBuilder b)
        {
            var namespaces = new List<string>
            {
                "IntelliTect.Coalesce",
                "IntelliTect.Coalesce.Mapping",
                "IntelliTect.Coalesce.Models",
                "System",
                "System.Linq",
                "System.Collections.Generic",
                "System.Security.Claims",
                "System.Text.Json.Serialization",
            };
            foreach (var ns in namespaces.OrderBy(n => n))
            {
                b.Line($"using {ns};");
            }

            b.Line();

            using (b.Block($"namespace {DtoNamespace}"))
            {
                WriteParameterDto(b);
                b.Line();
                WriteResponseDto(b);
            }
        }

        private void WriteParameterDto(CSharpCodeBuilder b)
        {
            var allVariants = Model.ClientDerivedTypes;
            if (allVariants.Any() && !Model.Type.IsAbstract) allVariants = allVariants.Prepend(Model);
            foreach (var derived in allVariants)
            {
                b.Line($"[JsonDerivedType(typeof({derived.ParameterDtoTypeName}), typeDiscriminator: {derived.ClientTypeName.QuotedStringLiteralForCSharp()})]");
            }

            ClassViewModel baseType = Model.ClientBaseTypes.FirstOrDefault();

            string inheritClause = baseType is not null
                ? $"{baseType.ParameterDtoTypeName}, IGeneratedParameterDto<{Model.FullyQualifiedName}>"
                : $"SparseDto, IGeneratedParameterDto<{Model.FullyQualifiedName}>";

            using (b.Block($"public partial class {Model.ParameterDtoTypeName} : {inheritClause}"))
            {
                b.Line($"public {Model.ParameterDtoTypeName}() {{ }}");

                var orderedProps = Model
                    .ClientProperties
                    .Where(p => p.IsClientSerializable)
                    // PK always first so it is available to guide decisions in IPropertyRestrictions
                    .OrderBy(p => !p.IsPrimaryKey)
                        // Scalars before objects
                        .ThenBy(p => p.PureType.HasClassViewModel)
                        // Finally, preserve original field order.
                        .ThenBy(p => p.ClassFieldOrder)
                    .ToList();

                var ownProps = orderedProps.Where(p => baseType?.PropertyByName(p.Name) is null);

                b.Line();
                foreach (PropertyViewModel prop in ownProps)
                {
                    b.Line($"private {prop.Type.NullableTypeForDto(isInput: true, dtoNamespace: DtoNamespace)} _{prop.Name};");
                }

                b.Line();
                foreach (PropertyViewModel prop in ownProps)
                {
                    using (b.Block($"public {prop.Type.NullableTypeForDto(isInput: true, dtoNamespace: DtoNamespace)} {prop.Name}"))
                    {
                        b.Line($"get => _{prop.Name};");
                        b.Line($"set {{ _{prop.Name} = value; Changed(nameof({prop.Name})); }}");
                    }
                }

                b.DocComment("Map from the current DTO instance to the domain object.");
                using (b.Block($"public void MapTo({Model.FullyQualifiedName} entity, IMappingContext context)"))
                {
                    b.Line("var includes = context.Includes;");
                    b.Line();

                    WriteSetters(b, orderedProps
                        .Where(p => p.SecurityInfo.Edit.IsAllowed())
                        .Select(p =>
                        {
                            var (conditional, setter) = DtoToModelPropertySetter(p, p.SecurityInfo.Edit);
                            conditional = conditional.Prepend($"ShouldMapTo(nameof({p.Name}))");
                            return (conditional, $"entity.{p.Name} = {setter};");
                        }));
                }

                b.DocComment("Map from the current DTO instance to a new instance of the domain object.");
                using (b.Block($"public {(baseType is null ? "" : "new ")}{Model.FullyQualifiedName} MapToNew(IMappingContext context)"))
                {
                    var properties = orderedProps
                        .Where(p => p.SecurityInfo.Init.IsAllowed())
                        .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

                    // Find the best ctor.
                    // We want to use the smallest ctor so we can favor default ctors if one exists,
                    // since prior to `init` and `required`, this was how Coalesce always worked.
                    var bestCtor = Model.Constructors
                        .OrderBy(c => c.Parameters.Count())
                        .Where(c => c.DtoMapToNewConstructorUsage.IsAcceptable)
                        .FirstOrDefault();
                    
                    if (bestCtor == null)
                    {
                        var reasons = Model.Constructors
                            .Select(c => $"{c.ToStringWithoutReturn()}: {c.DtoMapToNewConstructorUsage.Reason}")
                            .ToList();
                        if (!reasons.Any())
                        {
                            reasons.Add("Type has no public constructors.");
                        }

                        b.Line("// Unacceptable constructors:");
                        foreach (var reason in reasons) b.Append("// ").Line(reason);

                        if (properties.Count == 0)
                        {
                            // There's no constructor we can use, but also no properties that can even be mapped.
                            b.Line("throw new NotSupportedException(" +
                                $"\"Type {Model.Name} has no initializable properties and so cannot be used as an input to any Coalesce-generated APIs.\");");
                        }
                        else if (properties.All(p => p.Value.SecurityInfo.Init.IsUnused))
                        {
                            // If the type has no default constructor but also isn't used as an input by Coalesce,
                            // don't stop code gen - just gen an exception (that will never be hit at runtime)
                            b.Line("throw new NotSupportedException(" +
                                $"\"Type {Model.Name} does not have a constructor suitable for use by Coalesce for new object instantiation. " +
                                "Fortunately, this type appears to never be used in an input position in a Coalesce-generated API.\");");
                        }
                        else
                        {
                            throw new Exception($"Unable to find an appropriate constructor for type {Model.FullyQualifiedName}. The following public constructors were found to be insufficient: \n\n {string.Join("\n", reasons)}");
                        }
                    }
                    else
                    {
                        var ctorUsage = bestCtor.DtoMapToNewConstructorUsage;
                        var ctorParams = ctorUsage.CtorParams;
                        var initParams = ctorUsage.InitParams;

                        if (!ctorParams.Any() && !initParams.Any())
                        {
                            b.Line($"var entity = new {Model.FullyQualifiedName}();");
                            b.Line($"MapTo(entity, context);");
                            b.Line($"return entity;");
                        }
                        else
                        {
                            b.Line("var includes = context.Includes;");
                            b.Line();

                            b.Append($"var entity = new {Model.FullyQualifiedName}(");
                            if (ctorParams.Any()) b.Line();
                            for (int i = 0; i < ctorParams.Count; i++)
                            {
                                b.Indented(InlinePropertyRhs(ctorParams[i]) + (i < ctorParams.Count - 1 ? "," : ""));
                            }
                            b.Append(")");

                            if (initParams.Any())
                            {
                                using (b.Block(closeWith: ";"))
                                {
                                    foreach (var prop in initParams)
                                    {
                                        b.Line($"{prop.Name} = {InlinePropertyRhs(prop)},");
                                    }
                                }
                            }
                            else
                            {
                                b.Append(";");
                            }

                            WriteSetters(b, ctorUsage.SetParams
                                .Select(p =>
                                {
                                    var (conditional, setter) = DtoToModelPropertySetter(p, p.SecurityInfo.Init);
                                    conditional = conditional.Prepend($"ShouldMapTo(nameof({p.Name}))");
                                    return (conditional, $"entity.{p.Name} = {setter};");
                                }));

                            b.Line();
                            b.Line("return entity;");
                        }

                        string InlinePropertyRhs(PropertyViewModel p)
                        {
                            // Init-only props must be set here where we instantiate the type.
                            // They cannot be handled by the record
                            var (conditional, setter) = DtoToModelPropertySetter(p, p.SecurityInfo.Init, modelVar: null);

                            if (conditional.Any())
                            {
                                return $"({string.Join(" && ", conditional)}) ? {setter} : default";
                            }
                            else
                            {
                                return setter;
                            }
                        }
                    }
                }

                b.Line();
                b.Line($$"""
                    public {{Model.FullyQualifiedName}} MapToModelOrNew({{Model.FullyQualifiedName}} obj, IMappingContext context)
                    {
                        if (obj is null) return MapToNew(context);
                        MapTo(obj, context);
                        return obj;
                    }
                    """);
            }
        }

        private void WriteResponseDto(CSharpCodeBuilder b)
        {
            var allVariants = Model.ClientDerivedTypes;
            if (allVariants.Any() && !Model.Type.IsAbstract) allVariants = allVariants.Prepend(Model);
            foreach (var derived in allVariants)
            {
                b.Line($"[JsonDerivedType(typeof({derived.ResponseDtoTypeName}), typeDiscriminator: {derived.ClientTypeName.QuotedStringLiteralForCSharp()})]");
            }

            ClassViewModel baseType = Model.ClientBaseTypes.FirstOrDefault();

            string inheritClause = baseType is not null
                ? $"{baseType.ResponseDtoTypeName}, IGeneratedResponseDto<{Model.FullyQualifiedName}>"
                : $"IGeneratedResponseDto<{Model.FullyQualifiedName}>";

            using (b.Block($"public partial class {Model.ResponseDtoTypeName} : {inheritClause}"))
            {
                b.Line($"public {Model.ResponseDtoTypeName}() {{ }}");

                b.Line();

                var orderedProps = Model
                    .ClientProperties
                    .Where(p => p.SecurityInfo.Read.IsAllowed())
                    // PK always first so it is available to guide decisions in IPropertyRestrictions
                    .OrderBy(p => !p.IsPrimaryKey)
                        // Scalars before objects
                        .ThenBy(p => p.PureType.HasClassViewModel)
                        // Finally, preserve original field order.
                        .ThenBy(p => p.ClassFieldOrder)
                    .ToList();

                var ownProps = orderedProps.Where(p => baseType?.PropertyByName(p.Name) is null);

                foreach (PropertyViewModel prop in ownProps)
                {
                    b.Line($"public {prop.Type.NullableTypeForDto(isInput: false, dtoNamespace: DtoNamespace)} {prop.Name} {{ get; set; }}");
                }

                b.DocComment("Map from the domain object to the properties of the current DTO instance.");
                using (b.Block($"public void MapFrom({Model.FullyQualifiedName} obj, IMappingContext context, IncludeTree tree = null)"))
                {
                    b.Line("if (obj == null) return;");
                    b.Line("var includes = context.Includes;");
                    b.Line();

                    WriteSetters(b, orderedProps
                        .Select(ModelToDtoPropertySetter));
                }
            }
        }



        void WriteSetters(CSharpCodeBuilder b, IEnumerable<(IEnumerable<string> conditionals, string setter)> settersAndConditionals)
        {
            foreach (var conditionGroup in settersAndConditionals
                .GroupBy(s => s.conditionals.FirstOrDefault(), s => s))
            {
                if (conditionGroup.Key is null)
                {
                    foreach (var setter in conditionGroup)
                    {
                        b.Line(setter.setter);
                    }
                }
                else if (
                    // There are multiple setters that use `conditionGroup.Key` as at least part of
                    // their conditional. Group them together in a single `if` that consumes
                    // that shared part of the mapping conditional.
                    conditionGroup.Count() > 1 ||
                    // Also use a full block if the setter contains multiple statements
                    // that would make a braceless block impossible.
                    (conditionGroup.Single() is var singleSetter && singleSetter.setter.Count(x => x == ';') > 1)
                )
                {
                    using (b.Block($"if ({conditionGroup.Key})"))
                    {
                        WriteSetters(b, conditionGroup.Select(g => (g.conditionals.Skip(1), g.setter)));
                    }
                    b.Line();
                }
                else
                {
                    // There's only one setter in `conditionGroup`,
                    // such that we can merge all its conditions together.
                    b.Append("if (");
                    b.Append(string.Join(" && ", singleSetter.conditionals));
                    b.Append(") ");
                    b.Line(singleSetter.setter);
                }
            }
        }

        /// <summary>
        /// Get a C# boolean expression that could be evaluated to determine if a property is settable,
        /// taking into account the current user and whether the property mapping is incoming or outgoing.
        /// </summary>
        /// <param name="property">The property whose permissions will be evaluated.</param>
        /// <param name="permission">The permission info to pull the required roles from.</param>
        /// <param name="modelVar">The variable that holds the entity/model instance.</param>
        /// <returns></returns>
        private IEnumerable<string> GetPropertySetterConditional(
            PropertyViewModel property, 
            PropertySecurityPermission permission, 
            string modelVar)
        {
            string RoleCheck(string role) => $"context.IsInRoleCached(\"{role.EscapeStringLiteralForCSharp()}\")";
            string IncludesCheck(string include) => $"includes == \"{include.EscapeStringLiteralForCSharp()}\"";

            string roles = string.Join(" && ", permission.RoleLists
                .Select(rl => rl.Count == 1
                    // Only add wrapping parens if we need them:
                    ? RoleCheck(rl.Single())
                    : "(" + string.Join(" || ", rl.Select(RoleCheck)) + ")"
                )
                .Distinct());

            var includes = string.Join(" || ", property.DtoIncludes.Select(IncludesCheck));
            var excludes = string.Join(" || ", property.DtoExcludes.Select(IncludesCheck));

            var statement = new List<string>();
            if (!string.IsNullOrEmpty(roles)) statement.Add($"({roles})");
            if (!string.IsNullOrEmpty(includes)) statement.Add($"({includes})");
            if (!string.IsNullOrEmpty(excludes)) statement.Add($"!({excludes})");

            foreach (var restriction in property.SecurityInfo.Restrictions)
            {
                var restrictionExpr = $"context.GetPropertyRestriction<{restriction.FullyQualifiedName}>()";
                if (permission.Name == "Read")
                {
                    statement.Add($"{restrictionExpr}.UserCanRead(context, nameof({property.Name}), {modelVar ?? "null"})");
                }
                else
                {
                    statement.Add($"{restrictionExpr}.UserCanWrite(context, nameof({property.Name}), {modelVar ?? "null"}, {property.Name})");
                }
            }

            return statement;
        }

        /// <summary>
        /// Get the conditional and a C# expression that will map the property from a DTO to a local model object.
        /// </summary>
        private (IEnumerable<string> conditionals, string setter) DtoToModelPropertySetter(
            PropertyViewModel property,
            PropertySecurityPermission permission,
            string modelVar = "entity"
        )
        {
            string name = property.Name;

            string setter;
            if (property.Type.IsDictionary)
            {
                // Dictionaries aren't officially supported by Coalesce.
                // This only supports dictionaries of types that require no type mapping.
                // This is only a stop-gap to bridge apparent functionality that existed in 2.x versions
                // of Coalesce where Dictionaries apparently "accidentally" worked to a limited extent.
                // There is no frontend support at all.
                setter = $"{name}?.ToDictionary(k => k.Key, v => v.Value)";
            }
            else if (property.Object != null)
            {
                if (property.Type.IsCollection)
                {
                    setter = $"{name}?.Select(f => f.MapToNew(context)).{(property.Type.IsArray ? "ToArray" : "ToList")}()";
                }
                else if (modelVar != null)
                {
                    setter = $"{name}?.MapToModelOrNew({modelVar}.{name}, context)";
                }
                else
                {
                    setter = $"{name}?.MapToNew(context)";
                }
            }
            else if (!property.Type.IsArray && property.Type.IsA(typeof(IList<>)))
            {
                // Lists of scalar values, whose DTO properties will be ICollection<>, preventing direct assignment.
                setter = $"{name}?.ToList()";
            }
            else
            {
                var newValue = name;
                if (!property.Type.IsReferenceOrNullableValue && property.Type.CsDefaultValue != "null")
                {
                    var fallback = modelVar == null ? "default" : $"{modelVar}.{name}";
                    newValue = $"({newValue} ?? {fallback})";
                }
                setter = $"{newValue}";
            }

            var statement = GetPropertySetterConditional(property, permission, modelVar);
            return (statement, setter);
        }

        /// <summary>
        /// Get the conditional and a C# expression that will map the property from a local object to a DTO.
        /// </summary>
        /// <param name="property">The property to map</param>
        private (IEnumerable<string> conditionals, string setter) ModelToDtoPropertySetter(PropertyViewModel property)
        {
            string name = property.Name;
            string dtoVar = "this";

            string setter;
            string mapCall() => property.Object.IsCustomDto
                ? "" // If we hang an IClassDto off an external type, or another IClassDto, no mapping needed - it is already the desired type.
                : $".MapToDto<{property.Object.FullyQualifiedName}, {property.Object.ResponseDtoTypeName}>(context, tree?[nameof({dtoVar}.{name})])";

            if (property.Type.IsCollection)
            {
                if (property.Object != null)
                {
                    // Only check the includes tree for things that are in the database.
                    // Otherwise, this would break IncludesExternal.
                    var sb = new CodeBuilder(3);

                    // Set this as a variable once and then use it below. This prevents multiple-evaluation of computed getter-only properties.
                    sb.Line($"var propVal{name} = obj.{name};");
                    sb.Append($"if (propVal{name} != null");
                    if (property.Object.HasDbSet)
                    {
                        sb.Append($" && (tree == null || tree[nameof({dtoVar}.{name})] != null)");
                    }
                    sb.Line(") {");
                    using (sb.Indented())
                    {
                        sb.Line($"{dtoVar}.{name} = propVal{name}");

                        var defaultOrderBy = property.Object.DefaultOrderBy;
                        if (defaultOrderBy.Count > 0)
                        {
                            var orderByStatements = defaultOrderBy
                                .Select((orderInfo, i) =>
                                {
                                    string prefix = i == 0 ? ".OrderBy" : ".ThenBy";

                                    if (orderInfo.OrderByDirection == DefaultOrderByAttribute.OrderByDirections.Ascending)
                                    {
                                        return $"{prefix}(f => {orderInfo.OrderExpression("f")})";
                                    }
                                    else
                                    {
                                        return $"{prefix}Descending(f => {orderInfo.OrderExpression("f")})";
                                    }
                                });

                            sb.Indented(string.Concat(orderByStatements));
                        }

                        sb.Indented($".Select(f => f{mapCall()}).{(property.Type.IsArray ? "ToArray" : "ToList")}();");
                    }

                    if (property.Object.HasDbSet)
                    {
                        // If we know for sure that we're loading these things (becuse the IncludeTree said so),
                        // but EF didn't load any, then add a blank collection so the client will delete any that already exist.
                        sb.Line($"}} else if (propVal{name} == null && tree?[nameof({dtoVar}.{name})] != null) {{");
                        sb.Indented($"{dtoVar}.{name} = new {property.Object.ResponseDtoTypeName}[0];");
                        sb.Line("}");
                    }
                    else
                    {
                        sb.Line("}");
                    }

                    setter = sb.ToString();
                }
                else
                {
                    if (
                        property.Type.IsA(typeof(IReadOnlyCollection<>)) ||
                        property.Type.IsA(typeof(ICollection<>))
                    )
                    {
                        // Collection types which emit properly compatible property types on the DTO.
                        // No coersion to a real collection type required.
                        setter = $"{dtoVar}.{name} = obj.{name};";
                    }
                    else
                    {
                        // Collection is not really a collection. Probably an IEnumerable.
                        // We will have emitted the property type as ICollection,
                        // so we need to do a ToList() so that it can be assigned.
                        setter = $"{dtoVar}.{name} = obj.{name}?.ToList();";
                    }
                }

            }
            else if (property.Type.HasClassViewModel)
            {
                // Only check the includes tree for things that are in the database.
                // Otherwise, this would break IncludesExternal.
                string treeCheck = property.Type.ClassViewModel.HasDbSet 
                    ? $"if (tree == null || tree[nameof({dtoVar}.{name})] != null)" 
                    : "";

                setter = $@"{treeCheck}
                {dtoVar}.{name} = obj.{name}{mapCall()};
";
            }
            else
            {
                setter = $"{dtoVar}.{name} = obj.{name};";
            }

            var statement = GetPropertySetterConditional(property, property.SecurityInfo.Read, "obj");
            return (statement, setter);
        }
    }
}
