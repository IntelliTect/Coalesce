using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Text;
using IntelliTect.Coalesce.Utilities;
using System.Linq;

namespace IntelliTect.Coalesce.CodeGeneration.Api.Generators
{
    public class ClassDto : StringBuilderCSharpGenerator<ClassViewModel>
    {
        public ClassDto(GeneratorServices services) : base(services)
        {
        }

        public override void BuildOutput(CSharpCodeBuilder b)
        {
            var namespaces = new List<string>
            {
                "IntelliTect.Coalesce",
                "IntelliTect.Coalesce.Mapping",
                "IntelliTect.Coalesce.Models",
                "Newtonsoft.Json",
                "System",
                "System.Linq",
                "System.Linq.Dynamic.Core",
                "System.Collections.Generic",
                "System.Security.Claims"
            };
            foreach (var ns in namespaces.OrderBy(n => n))
            {
                b.Line($"using {ns};");
            }

            string namespaceName = Namespace;
            if (!string.IsNullOrWhiteSpace(AreaName))
            {
                namespaceName += "." + AreaName;
            }
            namespaceName += ".Models";
            b.Line();
            using (b.Block($"namespace {namespaceName}"))
            {
                WriteDtoClass(b, namespaceName);
            }
        }

        private void WriteDtoClass(CSharpCodeBuilder b, string namespaceName)
        {
            using (b.Block($"public partial class {Model.DtoName} : GeneratedDto<{Model.FullyQualifiedName}>"))
            {
                b.Line($"public {Model.DtoName}() {{ }}");

                b.Line();
                foreach (PropertyViewModel prop in Model.ClientProperties)
                {
                    b.Line($"public {prop.Type.NullableTypeForDto(namespaceName)} {prop.Name} {{ get; set; }}");
                }

                b.Line();
                b.Line("/// <summary>");
                b.Line("/// Map from the domain object to the properties of the current DTO instance.");
                b.Line("/// </summary>");
                using (b.Block($"public override void MapFrom({Model.FullyQualifiedName} obj, IMappingContext context, IncludeTree tree = null)"))
                {
                    b.Line("if (obj == null) return;");
                    b.Line("var includes = context.Includes;");
                    b.Line();
                    b.Line("// Fill the properties of the object.");
                    b.Line();
                    foreach (var conditionGroup in Model
                        .ClientProperties
                        .OrderBy(p => p.PureType.HasClassViewModel)
                        .Select(p => p.ObjToDtoPropertySetter("this"))
                        .Where(p => p != null)
                        .GroupBy(s => s.Value.conditional))
                    {
                        if (!string.IsNullOrWhiteSpace(conditionGroup.Key))
                        {
                            b.Line($"if ({conditionGroup.Key}) {{");
                            foreach (var setter in conditionGroup)
                            {
                                b.Indented(setter.Value.setter);
                            }
                            b.Line("}");
                        }
                        else
                        {
                            foreach (var setter in conditionGroup)
                            {
                                b.Line(setter.Value.setter);
                            }
                        }
                    }
                }

                b.Line();
                b.Line("/// <summary>");
                b.Line("/// Map from the current DTO instance to the domain object.");
                b.Line("/// </summary>");
                using (b.Block($"public override void MapTo({Model.FullyQualifiedName} entity, IMappingContext context)"))
                {
                    b.Line("var includes = context.Includes;");
                    b.Line();
                    b.Line("if (OnUpdate(entity, context)) return;");
                    b.Line();
                    foreach (var conditionGroup in Model
                        .ClientProperties
                        .Where(p => p.IsClientSerializable)
                        .Select(p => p.DtoToObjPropertySetter())
                        .Where(p => p != null)
                        .GroupBy(s => s.Value.conditional))
                    {
                        if (!string.IsNullOrWhiteSpace(conditionGroup.Key))
                        {
                            b.Line($"if ({conditionGroup.Key}) {{");
                            foreach (var setter in conditionGroup)
                            {
                                b.Indented(setter.Value.setter);
                            }
                            b.Line("}");
                        }
                        else
                        {
                            foreach (var setter in conditionGroup)
                            {
                                b.Line(setter.Value.setter);
                            }
                        }
                    }
                }
            }
        }
    }
}
