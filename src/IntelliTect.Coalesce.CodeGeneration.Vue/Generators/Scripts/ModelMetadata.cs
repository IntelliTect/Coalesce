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

            b.Line("import { Domain, getEnumMeta } from './coalesce/metadata' ");
            b.Line();
            // Assigning each property as a member of domain ensures we don't break type contracts.
            // Exporting each model individually lets us access the full structure of the model from other ts files
            b.Line("const domain: Domain = {}");
            b.Line("export default domain");


            void WriteCommon(ClassViewModel model)
            {
                b.StringProp("name", model.Name.ToCamelCase());
                b.StringProp("displayName", model.DisplayName);
                b.Line($"get displayProp() {{ return this.props.{model.ListTextProperty.JsVariable} }}, ");
            }

            foreach (var model in Model.ApiBackedClasses)
            { 
                using (b.Block($"export const {model.ViewModelClassName} = domain.{model.ViewModelClassName} ="))
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
                using (b.Block($"export const {model.ViewModelClassName} = domain.{model.ViewModelClassName} ="))
                {
                    WriteCommon(model);
                    b.StringProp("type", "object");

                    WriteProps(model, b);
                }
            }

            return Task.FromResult(b.ToString());
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

                        if (   prop.Type.IsNumber 
                            || prop.Type.IsBool 
                            || prop.Type.IsString 
                            || prop.Type.IsByteArray
                        )
                        {
                            b.StringProp("type", prop.Type.TsType);
                        }
                        else if (prop.Type.IsEnum)
                        {
                            b.StringProp("type", "enum");
                            // Maybe don't have an 'enum' property. This doesn't feel like a concern of the metadata - 
                            // more a concern of the implementation classes. Not sure. Can add later if wanted here.
                            // b.Prop("enum", "String");
                            // b.Line("...getEnumMeta<typeof Gender>([");

                            b.Line("...getEnumMeta([");
                            foreach (var value in prop.Type.EnumValues)
                            {
                                // TODO: allow for localization of displayName
                                b.Indented($"{{ value: {value.Key}, strValue: '{value.Value}', displayName: '{value.Value}' }},");
                            }
                            b.Line("]),");
                        }
                        else if (prop.Type.IsDate)
                        {
                            b.StringProp("type", "date");
                        }
                        else if (prop.Type.TsTypePlain == "any")
                        {
                            // We assume any known props are strings.
                            b.StringProp("type", "string");
                        }
                                
                        // Some getters need to qualify with "domain." instead of the exported const
                        // because in the case of a self-referential property, TypeScript can't handle recursive implicit type definitions.
                        if (prop.Object != null)
                        {
                            var classMeta = $"domain.{prop.Object.ViewModelClassName}";

                            if (prop.Type.IsPOCO && prop.ObjectIdProperty != null)
                            {
                                b.StringProp("type", "model");
                                b.StringProp("role", "referenceNavigation");
                                b.Line($"get keyProp() {{ return {classMeta}.props.{prop.ObjectIdProperty.JsVariable} }},");
                            }
                            else if (prop.Type.IsCollection && prop.Object.PrimaryKey != null)
                            {
                                b.StringProp("type", "collection");
                                b.StringProp("role", "collectionNavigation");
                                b.Line($"get keyProp() {{ return {classMeta}.props.{prop.InverseProperty.ObjectIdProperty.JsVariable} }},");
                            }
                            else
                            {
                                // External types
                                b.StringProp("type", "object");
                                b.StringProp("role", "value");
                            }
                            b.Line($"get model() {{ return {classMeta} }},");
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
