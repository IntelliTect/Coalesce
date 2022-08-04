﻿using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Text;
using IntelliTect.Coalesce.Utilities;
using System.Linq;
using IntelliTect.Coalesce.DataAnnotations;

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
                "System.Security.Claims"
            };
            foreach (var ns in namespaces.OrderBy(n => n))
            {
                b.Line($"using {ns};");
            }

            b.Line();
            using (b.Block($"namespace {DtoNamespace}"))
            {
                WriteDtoClass(b);
            }
        }

        private void WriteDtoClass(CSharpCodeBuilder b)
        {
            using (b.Block($"public partial class {Model.DtoName} : GeneratedDto<{Model.FullyQualifiedName}>"))
            {
                b.Line($"public {Model.DtoName}() {{ }}");

                b.Line();
                foreach (PropertyViewModel prop in Model.ClientProperties)
                {
                    b.Line($"private {prop.Type.NullableTypeForDto(DtoNamespace)} _{prop.Name};");
                }

                b.Line();
                foreach (PropertyViewModel prop in Model.ClientProperties)
                {
                    using (b.Block($"public {prop.Type.NullableTypeForDto(DtoNamespace)} {prop.Name}"))
                    {
                        b.Line($"get => _{prop.Name};");
                        b.Line($"set {{ _{prop.Name} = value; Changed(nameof({prop.Name})); }}");
                    }
                }

                void WriteSetters(IEnumerable<(string conditional, string setter)> settersAndConditionals)
                {
                    foreach (var conditionGroup in settersAndConditionals
                        .GroupBy(s => s.conditional, s => s.setter))
                    {
                        if (!string.IsNullOrWhiteSpace(conditionGroup.Key))
                        {
                            b.Line($"if ({conditionGroup.Key}) {{");
                            foreach (var setter in conditionGroup)
                            {
                                b.Indented(setter);
                            }
                            b.Line("}");
                        }
                        else
                        {
                            foreach (var setter in conditionGroup)
                            {
                                b.Line(setter);
                            }
                        }
                    }
                }

                b.DocComment("Map from the domain object to the properties of the current DTO instance.");
                using (b.Block($"public override void MapFrom({Model.FullyQualifiedName} obj, IMappingContext context, IncludeTree tree = null)"))
                {
                    b.Line("if (obj == null) return;");
                    b.Line("var includes = context.Includes;");
                    b.Line();
                    b.Line("// Fill the properties of the object.");
                    b.Line();

                    WriteSetters(Model
                        .ClientProperties
                        .OrderBy(p => p.PureType.HasClassViewModel)
                        .Select(ModelToDtoPropertySetter));
                }

                b.DocComment("Map from the current DTO instance to the domain object.");
                using (b.Block($"public override void MapTo({Model.FullyQualifiedName} entity, IMappingContext context)"))
                {
                    b.Line("var includes = context.Includes;");
                    b.Line();
                    b.Line("if (OnUpdate(entity, context)) return;");
                    b.Line();

                    WriteSetters(Model
                        .ClientProperties
                        .Where(p => p.IsClientSerializable)
                        .Select(DtoToModelPropertySetter));
                }
            }
        }

        /// <summary>
        /// Get a C# boolean expression that could be evaluated to determine if a property is settable,
        /// taking into account the current user and whether the property mapping is incoming or outgoing.
        /// </summary>
        /// <param name="property">The property whose permissions will be evaluated.</param>
        /// <param name="isForEdit">True to get a conditional for mapping incoming data, false for mapping outgoing data.</param>
        /// <returns></returns>
        private string GetPropertySetterConditional(PropertyViewModel property, bool isForEdit)
        {
            string RoleCheck(string role) => $"context.IsInRoleCached(\"{role.EscapeStringLiteralForCSharp()}\")";
            string IncludesCheck(string include) => $"includes == \"{include.EscapeStringLiteralForCSharp()}\"";

            string readRoles = string.Join(" || ", property.SecurityInfo.Read.RoleList.Select(RoleCheck));

            string editRoles = default;
            if (isForEdit)
            {
                editRoles = string.Join(" || ", property.SecurityInfo.Edit.RoleList.Select(RoleCheck));
            }

            var includes = string.Join(" || ", property.DtoIncludes.Select(IncludesCheck));
            var excludes = string.Join(" || ", property.DtoExcludes.Select(IncludesCheck));

            var statement = new List<string>();
            if (!string.IsNullOrEmpty(readRoles)) statement.Add($"({readRoles})");
            if (!string.IsNullOrEmpty(editRoles)) statement.Add($"({editRoles})");
            if (!string.IsNullOrEmpty(includes)) statement.Add($"({includes})");
            if (!string.IsNullOrEmpty(excludes)) statement.Add($"!({excludes})");

            return string.Join(" && ", statement);
        }

        /// <summary>
        /// Get the conditional and a C# expression that will map the property from a DTO to a local model object.
        /// </summary>
        /// <param name="property">The property to map</param>
        private (string conditional, string setter) DtoToModelPropertySetter(PropertyViewModel property)
        {
            string name = property.Name;
            string targetProp = $"entity.{name}";

            string setter;
            if (property.Type.IsDictionary)
            {
                // Dictionaries aren't officially supported by Coalesce.
                // This only supports dictionaries of types that require no type mapping.
                // This is only a stop-gap to bridge apparent functionality that existed in 2.x versions
                // of Coalesce where Dictionaries apparently "accidentally" worked to a limited extent.
                // There is no frontend support at all.
                setter = $"{targetProp} = {name}?.ToDictionary(k => k.Key, v => v.Value);";
            }
            else if (property.Object != null)
            {
                if (property.Type.IsCollection)
                {
                    string mapCall = $"MapToModel<{property.PureType.FullyQualifiedName}, {property.PureType.NullableTypeForDto(DtoNamespace, true)}> (new {property.Object.FullyQualifiedName}(), context)";
                    setter = $"{targetProp} = {name}?.Select(f => f.{mapCall}).{(property.Type.IsArray ? "ToArray" : "ToList")}();";
                }
                else
                {
                    string mapCall = $"MapToModel<{property.PureType.FullyQualifiedName}, {property.PureType.NullableTypeForDto(DtoNamespace, true)}>({targetProp} ?? new {property.Type.FullyQualifiedName}(), context)";
                    setter = $"{targetProp} = {name}?.{mapCall};";
                }
            }
            else
            {
                var newValue = name;
                if (!property.Type.IsNullable && property.Type.CsDefaultValue != "null")
                {
                    newValue = $"({newValue} ?? {targetProp})";
                }
                setter = $"{targetProp} = {newValue};";
            }


            setter = $"if (ShouldMapTo(nameof({name}))) " + setter;

            var statement = GetPropertySetterConditional(property, true);
            if (!string.IsNullOrWhiteSpace(statement))
            {
                return (statement, setter);
            }
            else
            {
                return (null, setter);
            }
        }

        /// <summary>
        /// Get the conditional and a C# expression that will map the property from a local object to a DTO.
        /// </summary>
        /// <param name="property">The property to map</param>
        private (string conditional, string setter) ModelToDtoPropertySetter(PropertyViewModel property)
        {
            string name = property.Name;
            string objectName = "this";

            string setter;
            string mapCall() => property.Object.IsDto
                ? "" // If we hang an IClassDto off an external type, or another IClassDto, no mapping needed - it is already the desired type.
                : $".MapToDto<{property.Object.FullyQualifiedName}, {property.Object.DtoName}>(context, tree?[nameof({objectName}.{name})])";

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
                        sb.Append($" && (tree == null || tree[nameof({objectName}.{name})] != null)");
                    }
                    sb.Line(") {");
                    using (sb.Indented())
                    {
                        sb.Line($"{objectName}.{name} = propVal{name}");

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
                        sb.Line($"}} else if (propVal{name} == null && tree?[nameof({objectName}.{name})] != null) {{");
                        sb.Indented($"{objectName}.{name} = new {property.Object.DtoName}[0];");
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
                        setter = $"{objectName}.{name} = obj.{name};";
                    }
                    else
                    {
                        // Collection is not really a collection. Probably an IEnumerable.
                        // We will have emitted the property type as ICollection,
                        // so we need to do a ToList() so that it can be assigned.
                        setter = $"{objectName}.{name} = obj.{name}?.ToList();";
                    }
                }

            }
            else if (property.Type.HasClassViewModel)
            {
                // Only check the includes tree for things that are in the database.
                // Otherwise, this would break IncludesExternal.
                string treeCheck = property.Type.ClassViewModel.HasDbSet 
                    ? $"if (tree == null || tree[nameof({objectName}.{name})] != null)" 
                    : "";

                setter = $@"{treeCheck}
                {objectName}.{name} = obj.{name}{mapCall()};
";
            }
            else
            {
                setter = $"{objectName}.{name} = obj.{name};";
            }

            var statement = GetPropertySetterConditional(property, false);
            if (!string.IsNullOrWhiteSpace(statement))
            {
                return (statement, setter);
            }
            else
            {
                return (null, setter);
            }
        }
    }
}
