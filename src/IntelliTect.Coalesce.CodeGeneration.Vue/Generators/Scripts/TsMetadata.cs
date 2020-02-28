﻿using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.TypeDefinition;
using IntelliTect.Coalesce.TypeDefinition.Enums;
using IntelliTect.Coalesce.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static IntelliTect.Coalesce.DataAnnotations.DateTypeAttribute;

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
                b.Line("Domain, getEnumMeta, solidify, ModelType, ObjectType,");
                b.Line("PrimitiveProperty, ForeignKeyProperty, PrimaryKeyProperty,");
                b.Line("ModelCollectionNavigationProperty, ModelReferenceNavigationProperty");
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
            b.Line("solidify(domain)");
            b.Line();
            b.Line("export default domain as AppDomain");


            return Task.FromResult(b.ToString());
        }

        private void WriteCommonClassMetadata(TypeScriptCodeBuilder b, ClassViewModel model)
        {
            b.StringProp("name", model.ClientTypeName);
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

                var securityInfo = model.SecurityInfo;
                int flags = 
                    (securityInfo.IsCreateAllowed() ? 1 << 0 : 0) |
                    (securityInfo.IsEditAllowed() ? 1 << 1 : 0) | 
                    (securityInfo.IsDeleteAllowed() ? 1 << 2 : 0);
                b.Prop("behaviorFlags", flags.ToString());

                WriteClassPropertiesMetadata(b, model);

                WriteClassMethodMetadata(b, model);

                WriteDataSourcesMetadata(b, model);
            }
        }

        private void WriteEnumMetadata(TypeScriptCodeBuilder b, TypeViewModel model)
        {
            using (b.Block($"export const {model.ClientTypeName} = domain.enums.{model.ClientTypeName} ="))
            {
                b.StringProp("name", model.ClientTypeName);
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
                b.StringProp("name", model.ClientTypeName);
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

                        if (prop.InverseProperty != null)
                        {
                            b.Line($"get inverseNavigation() {{ return {GetClassMetadataRef(prop.Object)}.props.{prop.InverseProperty.JsVariable} as ModelCollectionNavigationProperty }},");
                        }

                        break;

                    case PropertyRole.CollectionNavigation:
                        // TS Type: "ModelCollectionNavigationProperty"
                        b.StringProp("role", "collectionNavigation");
                        b.Line($"get foreignKey() {{ return {GetClassMetadataRef(prop.Object)}.props.{prop.InverseProperty.ForeignKeyProperty.JsVariable} as ForeignKeyProperty }},");
                        
                        if (prop.InverseProperty != null)
                        {
                            b.Line($"get inverseNavigation() {{ return {GetClassMetadataRef(prop.Object)}.props.{prop.InverseProperty.JsVariable} as ModelReferenceNavigationProperty }},");
                        }

                        if (prop.IsManytoManyCollection)
                        {
                            using (b.Block("manyToMany:", ","))
                            {
                                var nearNavigation = prop.Object.ClientProperties.First(f => f.Type.EqualsType(model.Type));
                                var farNavigation = prop.ManyToManyCollectionProperty;

                                b.StringProp("name", prop.ManyToManyCollectionName.ToCamelCase());
                                b.StringProp("displayName", prop.ManyToManyCollectionName.ToProperCase());
                                b.Line($"get typeDef() {{ return {GetClassMetadataRef(farNavigation.Object)} }},");
                                b.Line($"get farForeignKey() {{ return {GetClassMetadataRef(prop.Object)}.props.{farNavigation.ForeignKeyProperty.JsVariable} as ForeignKeyProperty }},");
                                b.Line($"get farNavigationProp() {{ return {GetClassMetadataRef(prop.Object)}.props.{farNavigation.JsVariable} as ModelReferenceNavigationProperty }},");
                                b.Line($"get nearForeignKey() {{ return {GetClassMetadataRef(prop.Object)}.props.{nearNavigation.ForeignKeyProperty.JsVariable} as ForeignKeyProperty }},");
                                b.Line($"get nearNavigationProp() {{ return {GetClassMetadataRef(prop.Object)}.props.{nearNavigation.JsVariable} as ModelReferenceNavigationProperty }},");
                            }
                        }

                        break;

                    default:
                        b.StringProp("role", "value");
                        break;
                }

                int hiddenAreaFlags = 0;
                switch (prop.GetAttributeValue<HiddenAttribute, HiddenAttribute.Areas>(a => a.Area))
                {
                    case HiddenAttribute.Areas.List:
                        hiddenAreaFlags = 1 << 0;
                        break;
                    case HiddenAttribute.Areas.Edit:
                        hiddenAreaFlags = 1 << 1;
                        break;
                    case HiddenAttribute.Areas.All:
                        hiddenAreaFlags = (1 << 0 | 1 << 1);
                        break;
                }
                if (hiddenAreaFlags != 0)
                {
                    b.Prop("hidden", hiddenAreaFlags.ToString());
                }

                // We store the negative case instead of the positive
                // because there are likely going to be more that are serializable than not.
                if (!prop.IsClientSerializable)
                {
                    b.Line("dontSerialize: true,");
                }


                if (prop.IsClientWritable)
                {
                    // TODO: Handle all the rest (where it makes sense, anyway) of ClientValidationAttribute

                    // TODO: Handle 'ClientValidationAllowSave' by placing a field on the 
                    // validator function that contains the value of this flag.

                    var isRequired = prop.GetAttributeValue<ClientValidationAttribute, bool>(a => a.IsRequired);
                    var errorMessage = prop.GetAttributeValue<ClientValidationAttribute>(a => a.ErrorMessage);

                    var rules = new List<string>();

                    if (isRequired == true)
                    {
                        rules.Add($"required: val => !!val || \"{(errorMessage ?? $"{(prop.ReferenceNavigationProperty ?? prop).DisplayName} is required.").EscapeStringLiteralForTypeScript()}\"");
                    }
                    else if (prop.IsRequired)
                    {
                        string message = null;
                        if (prop.HasAttribute<RequiredAttribute>())
                        {
                            message = prop.GetAttributeValue<RequiredAttribute>(a => a.ErrorMessage);
                        }
                        if (string.IsNullOrWhiteSpace(message))
                        {
                            var name = (prop.ReferenceNavigationProperty ?? prop).DisplayName;
                            message = $"{name} is required.";
                        }

                        rules.Add($"required: val => !!val || \"{message.EscapeStringLiteralForTypeScript()}\"");
                    }

                    if (rules.Count > 0)
                    {
                        using (b.Block("rules:"))
                        {
                            foreach (var rule in rules)
                            {
                                b.Append(rule);
                                b.Line(",");
                            }
                        }
                    }
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

                if (method.IsStatic)
                {
                    b.Prop("isStatic", "true");
                }

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

                using (b.Block("props:", ','))
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

            if (value.Type.IsDate)
            {
                var dateType = value.GetAttributeValue<DateTypeAttribute, DateTypes>(a => a.DateType);
                switch (dateType)
                {
                    case DateTypes.DateOnly:
                        b.StringProp("dateKind", "date");
                        break;
                    default:
                        b.StringProp("dateKind", "datetime");
                        break;
                }

                if (value.Type.IsDateTime)
                {
                    b.Prop("noOffset", "true");
                }
            }

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
                        b.Line($"get {propName}() {{ return domain.enums.{t.ClientTypeName} }},");
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
