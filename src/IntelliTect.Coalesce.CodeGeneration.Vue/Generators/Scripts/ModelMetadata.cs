using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.TypeDefinition;
using IntelliTect.Coalesce.Utilities;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.CodeGeneration.Vue.Generators
{
    public class ModelMetadata : StringBuilderFileGenerator<ReflectionRepository>
    {
        public ModelMetadata(GeneratorServices services) : base(services)
        {
        }

        public override Task<string> BuildOutputAsync()
        {
            var b = new TypeScriptCodeBuilder();

            b.Line("import { Domain, getEnumMeta, ModelType, ExternalType } from './coalesce/core/metadata' ");
            b.Line();
            // Assigning each property as a member of domain ensures we don't break type contracts.
            // Exporting each model individually lets us access the full structure of the model from other ts files
            b.Line("const domain: Domain = { types: {}, enums: {} }");
            //b.Line("const domain: Domain = { models: {}, externalTypes: {}, enums: {} }");
            b.Line("export default domain");


            void WriteCommon(ClassViewModel model)
            {
                b.StringProp("name", model.Name.ToCamelCase());
                b.StringProp("displayName", model.DisplayName);
                if (model.ListTextProperty != null)
                {
                    // This might not be defined for external types, because sometimes it just doesn't make sense. We'll accommodate on the client.
                    b.Line($"get displayProp() {{ return this.props.{model.ListTextProperty.JsVariable} }}, ");
                }
            }

            foreach (var model in Model.ClientEnums)
            {
                using (b.Block($"export const {model.Name} = domain.enums.{model.Name} ="))
                {
                    b.StringProp("name", model.Name.ToCamelCase());
                    b.StringProp("displayName", model.DisplayName);
                    b.StringProp("type", "enum");
                    
                    b.Line("...getEnumMeta([");
                    foreach (var value in model.EnumValues)
                    {
                        // TODO: allow for localization of displayName
                        b.Indented($"{{ value: {value.Key}, strValue: '{value.Value}', displayName: '{value.Value}' }},");
                    }
                    b.Line("]),");
                }
            }

            foreach (var model in Model.ApiBackedClasses)
            { 
                //using (b.Block($"export const {model.ViewModelClassName} = domain.models.{model.ViewModelClassName} ="))
                using (b.Block($"export const {model.ViewModelClassName} = domain.types.{model.ViewModelClassName} ="))
                {
                    WriteCommon(model);
                    b.StringProp("type", "model");
                    b.Line($"get keyProp() {{ return this.props.{model.PrimaryKey.JsVariable} }}, ");

                    WriteProps(model, b);

                    // TODO: methods
                    b.Line("methods: {},");
                }
            }

            foreach (var model in Model.ExternalTypes)
            {
                //using (b.Block($"export const {model.ViewModelClassName} = domain.externalTypes.{model.ViewModelClassName} ="))
                using (b.Block($"export const {model.ViewModelClassName} = domain.types.{model.ViewModelClassName} ="))
                {
                    WriteCommon(model);
                    b.StringProp("type", "object");

                    WriteProps(model, b);
                }
            }

            return Task.FromResult(b.ToString());
        }

        private string GetValueTypeString(TypeViewModel type)
        {
            if (type.IsNumber
                || type.IsBool
                || type.IsString
                || type.IsByteArray
            ) return type.TsType;

            if (type.IsEnum) return "enum";
            if (type.IsDate) return "date";
            return null;
        }

        private void WriteProps(ClassViewModel model, TypeScriptCodeBuilder b)
        {
            using (b.Block("props:", ','))
            {
                foreach (var prop in model.ClientProperties)
                {
                    using (b.Block($"{prop.JsVariable}:", ','))
                    {
                        b.StringProp("name", prop.JsVariable);
                        b.StringProp("displayName", prop.DisplayName);
                        var valueTypeString = GetValueTypeString(prop.Type);
                        if (valueTypeString != null)
                        {
                            b.StringProp("type", valueTypeString);

                            if (prop.Type.IsEnum)
                            {
                                b.Line($"get typeDef() {{ return domain.enums.{prop.Type.Name} }},");
                            }
                        }
                        else if (prop.Type.TsTypePlain == "any")
                        {
                            // We assume any unknown props are strings.
                            b.StringProp("type", "string");
                        }
                                
                        // Some getters need to qualify with "domain." instead of the exported const
                        // because in the case of a self-referential property, TypeScript can't handle recursive implicit type definitions.
                        if (prop.Object != null)
                        {
                            string classMeta = $"(domain.types.{prop.Object.ViewModelClassName} as ModelType)";
                            //string classMeta = $"domain.models.{prop.Object.ViewModelClassName}";

                            if (prop.Type.IsPOCO && prop.ObjectIdProperty != null)
                            {
                                // Reference navigations
                                b.StringProp("type", "model");
                                b.StringProp("role", "referenceNavigation");
                                b.Line($"get foreignKey() {{ return {classMeta}.props.{prop.ObjectIdProperty.JsVariable} }},");
                            }
                            else if (prop.Type.IsCollection && prop.Object.PrimaryKey != null)
                            {
                                // Collection navigations
                                b.StringProp("type", "collection");
                                b.StringProp("role", "collectionNavigation");
                                b.Line($"get foreignKey() {{ return {classMeta}.props.{prop.InverseProperty.ObjectIdProperty.JsVariable} }},");
                            }
                            else
                            {
                                // External type collections
                                b.StringProp("type", "object");
                                b.StringProp("role", "value");
                                //classMeta = $"domain.externalTypes.{prop.Object.ViewModelClassName}";
                                classMeta = $"domain.types.{prop.Object.ViewModelClassName} as ExternalType";
                            }
                            b.Line($"get typeDef() {{ return {classMeta} }},");
                        }
                        else if (prop.Type.IsCollection)
                        {
                            // Primitive collections
                            b.StringProp("type", "collection");
                            b.StringProp("role", "value");
                            b.StringProp("model", GetValueTypeString(prop.PureType));
                        }
                        else
                        {
                            // All non-object/collection properties:
                            if (prop.IsPrimaryKey)
                                b.StringProp("role", "primaryKey");
                            else if (prop.IsForeignKey)
                                b.StringProp("role", "foreignKey");
                            else
                                b.StringProp("role", "value");
                        }

                    }
                }
            }
        }
    }
}
