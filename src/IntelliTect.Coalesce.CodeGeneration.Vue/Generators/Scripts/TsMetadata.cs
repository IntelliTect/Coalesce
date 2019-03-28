using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.TypeDefinition;
using IntelliTect.Coalesce.TypeDefinition.Enums;
using IntelliTect.Coalesce.Utilities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.CodeGeneration.Vue.Generators
{
    public class TsMetadata : StringBuilderFileGenerator<ReflectionRepository>
    {
        public TsMetadata(GeneratorServices services) : base(services)
        {
        }

        public override Task<string> BuildOutputAsync()
        {
            var b = new TypeScriptCodeBuilder(indentSize: 2);

            using (b.Block("import", " from 'coalesce-vue/lib/metadata'"))
            {
                b.Line("Domain, getEnumMeta, ModelType, ObjectType,");
                b.Line("PrimitiveProperty, ModelReferenceNavigationProperty, ForeignKeyProperty, PrimaryKeyProperty");
            }
            b.Line();
            b.Line();

            // Assigning each property as a member of domain ensures we don't break type contracts.
            // Exporting each model individually offers easier usage in imports.
            b.Line("const domain: Domain = { enums: {}, types: {}, services: {} }");



            foreach (var model in Model.ClientEnums.OrderBy(e => e.ClientTypeName))
            {
                WriteEnumMetadata(b, model);
            }

            foreach (var model in Model.ApiBackedClasses.OrderBy(e => e.ClientTypeName))
            {
                WriteApiBackedTypeMetadata(b, model);
            }

            foreach (var model in Model.ExternalTypes.OrderBy(e => e.ClientTypeName))
            {
                WriteExternalTypeMetadata(b, model);
            }

            foreach (var model in Model.Services.OrderBy(e => e.ClientTypeName))
            {
                WriteServiceMetadata(b, model);
            }

            // Create an enhanced Domain definition for deep intellisense.
            b.Line();
            using (b.Block("interface AppDomain extends Domain"))
            {
                using (b.Block("enums:"))
                {
                    foreach (var model in Model.ClientEnums.OrderBy(e => e.Name))
                    {
                        b.Line($"{model.Name}: typeof {model.Name}");
                    }
                }
                using (b.Block("types:"))
                {
                    foreach (var model in Model.ClientClasses.OrderBy(e => e.ClientTypeName))
                    {
                        b.Line($"{model.ClientTypeName}: typeof {model.ClientTypeName}");
                    }
                }
                using (b.Block("services:"))
                {
                    foreach (var model in Model.Services.OrderBy(e => e.ClientTypeName))
                    {
                        b.Line($"{model.ClientTypeName}: typeof {model.ClientTypeName}");
                    }
                }
            }
            
            b.Line();
            b.Line("export default domain as AppDomain");


            return Task.FromResult(b.ToString());
        }

        private void WriteCommonClassMetadata(TypeScriptCodeBuilder b, ClassViewModel model)
        {
            b.StringProp("name", model.ClientTypeName.ToCamelCase());
            b.StringProp("displayName", model.DisplayName);
            if (model.ListTextProperty != null)
            {
                // This might not be defined for external types, because sometimes it just doesn't make sense. We'll accommodate on the client.
                b.Line($"get displayProp() {{ return this.props.{model.ListTextProperty.JsVariable} }}, ");
            }
        }

        private void WriteExternalTypeMetadata(TypeScriptCodeBuilder b, ClassViewModel model)
        {
            using (b.Block($"export const {model.ViewModelClassName} = domain.types.{model.ViewModelClassName} ="))
            {
                WriteCommonClassMetadata(b, model);
                b.StringProp("type", "object");

                WriteClassPropertiesMetadata(b, model);
            }
        }

        private void WriteApiBackedTypeMetadata(TypeScriptCodeBuilder b, ClassViewModel model)
        {
            using (b.Block($"export const {model.ViewModelClassName} = domain.types.{model.ViewModelClassName} ="))
            {
                WriteCommonClassMetadata(b, model);
                b.StringProp("type", "model");
                b.StringProp("controllerRoute", model.ApiRouteControllerPart);
                b.Line($"get keyProp() {{ return this.props.{model.PrimaryKey.JsVariable} }}, ");

                WriteClassPropertiesMetadata(b, model);

                WriteClassMethodMetadata(b, model);

                WriteDataSourcesMetadata(b, model);
            }
        }

        private void WriteEnumMetadata(TypeScriptCodeBuilder b, TypeViewModel model)
        {
            using (b.Block($"export const {model.ClientTypeName} = domain.enums.{model.ClientTypeName} ="))
            {
                b.StringProp("name", model.ClientTypeName.ToCamelCase());
                b.StringProp("displayName", model.DisplayName);
                b.StringProp("type", "enum");

                string enumShape = string.Join("|", model.EnumValues.Select(ev => $"\"{ev.Value}\""));
                b.Line($"...getEnumMeta<{enumShape}>([");
                foreach (var value in model.EnumValues)
                {
                    // TODO: allow for localization of displayName
                    b.Indented($"{{ value: {value.Key}, strValue: '{value.Value}', displayName: '{value.Value.ToProperCase()}' }},");
                }
                b.Line("]),");
            }
        }

        private void WriteServiceMetadata(TypeScriptCodeBuilder b, ClassViewModel model)
        {
            using (b.Block($"export const {model.ClientTypeName} = domain.services.{model.ClientTypeName} ="))
            {
                b.StringProp("name", model.ClientTypeName.ToCamelCase());
                b.StringProp("displayName", model.DisplayName);

                b.StringProp("type", "service");
                b.StringProp("controllerRoute", model.ApiRouteControllerPart);

                WriteClassMethodMetadata(b, model);
            }
        }

        private void WriteClassPropertiesMetadata(TypeScriptCodeBuilder b, ClassViewModel model)
        {
            using (b.Block("props:", ','))
            {
                foreach (var prop in model.ClientProperties)
                {
                    WriteClassPropertyMetadata(b, model, prop);
                }
            }
        }

        private static string GetClassMetadataRef(ClassViewModel obj = null)
        {
            // We need to qualify with "domain." instead of the exported const
            // because in the case of a self-referential property, TypeScript can't handle recursive implicit type definitions.

            return $"(domain.types.{obj.ViewModelClassName} as {(obj.IsDbMappedType ? "ModelType" : "ObjectType")})";
        }

        private void WriteClassPropertyMetadata(TypeScriptCodeBuilder b, ClassViewModel model, PropertyViewModel prop)
        {
            using (b.Block($"{prop.JsVariable}:", ','))
            {
                WriteValueCommonMetadata(b, prop);

                switch (prop.Role)
                {
                    case PropertyRole.PrimaryKey:
                        // TS Type: "PrimaryKeyProperty"
                        b.StringProp("role", "primaryKey");
                        break;

                    case PropertyRole.ForeignKey:
                        // TS Type: "ForeignKeyProperty"
                        var navProp = prop.ReferenceNavigationProperty;
                        b.StringProp("role", "foreignKey");
                        b.Line($"get principalKey() {{ return {GetClassMetadataRef(navProp.Object)}.props.{navProp.Object.PrimaryKey.JsVariable} as PrimaryKeyProperty }},");
                        b.Line($"get principalType() {{ return {GetClassMetadataRef(navProp.Object)} }},");
                        b.Line($"get navigationProp() {{ return {GetClassMetadataRef(model)}.props.{navProp.JsVariable} as ModelReferenceNavigationProperty }},");
                        break;

                    case PropertyRole.ReferenceNavigation:
                        // TS Type: "ModelReferenceNavigationProperty"
                        b.StringProp("role", "referenceNavigation");
                        b.Line($"get foreignKey() {{ return {GetClassMetadataRef(model)}.props.{prop.ForeignKeyProperty.JsVariable} as ForeignKeyProperty }},");
                        b.Line($"get principalKey() {{ return {GetClassMetadataRef(prop.Object)}.props.{prop.Object.PrimaryKey.JsVariable} as PrimaryKeyProperty }},");
                        break;

                    case PropertyRole.CollectionNavigation:
                        // TS Type: "ModelCollectionNavigationProperty"
                        b.StringProp("role", "collectionNavigation");
                        b.Line($"get foreignKey() {{ return {GetClassMetadataRef(prop.Object)}.props.{prop.InverseProperty.ForeignKeyProperty.JsVariable} as ForeignKeyProperty }},");
                        break;

                    default:
                        b.StringProp("role", "value");
                        break;
                }

                // We store the negative case instead of the positive
                // because there are likely going to be more that are serializable than not.
                if (!prop.IsClientSerializable)
                {
                    b.Line("dontSerialize: true,");
                }
            }
        }

        /// <summary>
        /// Write the metadata for all methods of a class
        /// </summary>
        private void WriteClassMethodMetadata(TypeScriptCodeBuilder b, ClassViewModel model)
        {
            using (b.Block("methods:", ','))
            {
                foreach (var method in model.ClientMethods)
                {
                    WriteClassMethodMetadata(b, model, method);
                }
            }
        }

        /// <summary>
        /// Write the metadata for an entire method
        /// </summary>
        private void WriteClassMethodMetadata(TypeScriptCodeBuilder b, ClassViewModel model, MethodViewModel method)
        {
            using (b.Block($"{method.JsVariable}:", ','))
            {

                b.StringProp("name", method.JsVariable);
                b.StringProp("displayName", method.DisplayName);
                b.StringProp("transportType", method.TransportType.ToString().Replace("Result", "").ToLower());
                b.StringProp("httpMethod", method.ApiActionHttpMethod.ToString().ToUpperInvariant());

                using (b.Block("params:", ','))
                {
                    // TODO: should we be writing out the implicit 'id' param as metadata? Or handling some other way?
                    // If we do keep it as metadata, should probably add some prop to mark it as what it is.
                    if (method.IsModelInstanceMethod)
                    {
                        using (b.Block($"id:", ','))
                        {
                            b.StringProp("name", "id");
                            b.StringProp("displayName", "Primary Key"); // TODO: Is this what we want? Also, i18n.
                            b.StringProp("role", "value");
                            WriteTypeCommonMetadata(b, model.PrimaryKey.Type);
                        }
                    }

                    foreach (var param in method.ClientParameters)
                    {
                        WriteMethodParameterMetadata(b, method, param);
                    }
                }

                using (b.Block("return:", ','))
                {
                    b.StringProp("name", "$return");
                    b.StringProp("displayName", "Result"); // TODO: i18n
                    b.StringProp("role", "value");
                    WriteTypeCommonMetadata(b, method.ResultType);
                }
            }
        }

        /// <summary>
        /// Write the metadata for a specific parameter to a specific method
        /// </summary>
        private void WriteMethodParameterMetadata(TypeScriptCodeBuilder b, MethodViewModel method, ParameterViewModel parameter)
        {
            using (b.Block($"{parameter.JsVariable}:", ','))
            {
                WriteValueCommonMetadata(b, parameter);
                b.StringProp("role", "value");
            }
        }

        /// <summary>
        /// Write the metadata for all data sources of a class
        /// </summary>
        private void WriteDataSourcesMetadata(TypeScriptCodeBuilder b, ClassViewModel model)
        {
            using (b.Block("dataSources:", ','))
            {
                var dataSources = model.ClientDataSources(this.Model);
                foreach (var source in dataSources)
                {
                    WriteDataSourceMetadata(b, model, source);
                }

                // Not sure we need to explicitly declare the default source.
                // We can just use the absense of a data source to represent the default.
                /*
                var defaultSource = dataSources.SingleOrDefault(s => s.IsDefaultDataSource);
                if (defaultSource != null)
                {
                    var name = defaultSource.ClientTypeName.ToCamelCase();
                    b.Line($"get default() {{ return this.{name} }},");
                }
                else
                {
                    using (b.Block($"default:", ','))
                    {
                        b.StringProp("type", "dataSource");
                        b.StringProp("name", "default");
                        b.StringProp("displayName", "Default");
                        b.Line("params: {}");
                    }
                }
                */
            }
        }

        /// <summary>
        /// Write the metadata for all data sources of a class
        /// </summary>
        private void WriteDataSourceMetadata(TypeScriptCodeBuilder b, ClassViewModel model, ClassViewModel source)
        {
            // TODO: Should we be camel-casing the names of data sources in the metadata?
            // TODO: OR, should we be not camel casing the members we place on the domain[key: string] objects?
            using (b.Block($"{source.ClientTypeName.ToCamelCase()}:", ','))
            {
                b.StringProp("type", "dataSource");

                WriteCommonClassMetadata(b, source);

                if (source.IsDefaultDataSource)
                {
                    b.Line("isDefault: true,");
                }

                using (b.Block("params:", ','))
                {
                    foreach (var prop in source.DataSourceParameters)
                    {
                        WriteClassPropertyMetadata(b, model, prop);
                    }
                }
            }
        }

        /// <summary>
        /// Write metadata common to all value representations, like properties and method parameters.
        /// </summary>
        private void WriteValueCommonMetadata(TypeScriptCodeBuilder b, IValueViewModel value)
        {
            b.StringProp("name", value.JsVariable);
            b.StringProp("displayName", value.DisplayName);

            WriteTypeCommonMetadata(b, value.Type);
        }

        /// <summary>
        /// Write metadata common to all type representations, 
        /// like properties, method parameters, method returns, etc.
        /// </summary>
        private void WriteTypeCommonMetadata(TypeScriptCodeBuilder b, TypeViewModel type)
        {
            void WriteTypeDiscriminator(string propName, TypeViewModel t)
            {
                var kind = t.TsTypeKind;
                switch (kind)
                {
                    case TypeDiscriminator.Unknown:
                        // We assume any unknown props are strings.
                        b.Line("// Type not supported natively by Coalesce - falling back to string.");
                        b.StringProp(propName, "string");
                        break;

                    default:
                        b.StringProp(propName, kind.ToString().ToLowerInvariant());
                        break;
                }
            }

            void WriteTypeDef(string propName, TypeViewModel t)
            {
                var kind = t.TsTypeKind;
                switch (kind)
                {
                    case TypeDiscriminator.Enum:
                        b.Line($"get {propName}() {{ return domain.enums.{t.Name} }},");
                        break;

                    case TypeDiscriminator.Model:
                    case TypeDiscriminator.Object:
                        b.Line($"get {propName}() {{ return {GetClassMetadataRef(t.ClassViewModel)} }},");
                        break;
                }
            }


            WriteTypeDiscriminator("type", type);
            WriteTypeDef("typeDef", type);

            // For collections, write the references to the underlying type.
            if (type.TsTypeKind == TypeDiscriminator.Collection)
            {
                if (type.PureType.TsTypeKind == TypeDiscriminator.Collection)
                {
                    throw new InvalidOperationException("Collections of collections aren't supported by Coalesce as exposed types");
                }

                using (b.Block($"itemType:", ','))
                {
                    b.StringProp("name", "$collectionItem");
                    b.StringProp("displayName", "");
                    b.StringProp("role", "value");
                    WriteTypeCommonMetadata(b, type.PureType);
                }
            }
        }
    }
}
