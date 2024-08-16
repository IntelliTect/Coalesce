using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.TypeDefinition;
using IntelliTect.Coalesce.TypeDefinition.Enums;
using IntelliTect.Coalesce.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
                b.Line("ModelCollectionNavigationProperty, ModelReferenceNavigationProperty,");
                b.Line("HiddenAreas, BehaviorFlags");
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

            foreach (var model in Model.CrudApiBackedClasses.OrderBy(e => e.ClientTypeName))
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
            // "as unknown" needed for some change in Typescript after around 3.8?
            // This weirdly isn't needed in Coalese.Web.Vue, but is needed in basically
            // all other consuming projects.
            b.Line("export default domain as unknown as AppDomain");


            return Task.FromResult(b.ToString());
        }

        private void WriteCommonClassMetadata(TypeScriptCodeBuilder b, ClassViewModel model)
        {
            b.StringProp("name", model.ClientTypeName);
            b.StringProp("displayName", model.DisplayName);

            var description = model.GetAttributeValue<DescriptionAttribute>(a => a.Description)
                ?? model.GetAttributeValue<DisplayAttribute>(a => a.Description);

            if (!string.IsNullOrWhiteSpace(description))
            {
                b.StringProp("description", description);
            }

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
                b.Prop("behaviorFlags", flags.ToString() + " as BehaviorFlags");

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

                string enumShape = string.Join("|", model.EnumValues.Select(ev => $"\"{ev.Name}\""));
                b.Line($"...getEnumMeta<{enumShape}>([");
                foreach (var value in model.EnumValues)
                {
                    using (b.Block("", ",", leadingSpace: false))
                    {
                        b.Prop("value", value.Value.ToString());
                        b.StringProp("strValue", value.Name);
                        b.StringProp("displayName", value.DisplayName);
                        if (!string.IsNullOrWhiteSpace(value.Description))
                        {
                            b.StringProp("description", value.Description);
                        }
                    }
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
                foreach (var prop in model.ClientProperties.OrderBy(f => f.EditorOrder))
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
                        var principal = prop.ForeignKeyPrincipalType;
                        b.StringProp("role", "foreignKey");
                        b.Line($"get principalKey() {{ return {GetClassMetadataRef(principal)}.props.{principal.PrimaryKey.JsVariable} as PrimaryKeyProperty }},");
                        b.Line($"get principalType() {{ return {GetClassMetadataRef(principal)} }},");

                        if (prop.ReferenceNavigationProperty is { } navProp)
                        {
                            b.Line($"get navigationProp() {{ return {GetClassMetadataRef(model)}.props.{navProp.JsVariable} as ModelReferenceNavigationProperty }},");
                        }
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
                        b.Line($"get foreignKey() {{ return {GetClassMetadataRef(prop.Object)}.props.{prop.ForeignKeyProperty.JsVariable} as ForeignKeyProperty }},");
                        
                        if (prop.InverseProperty != null)
                        {
                            b.Line($"get inverseNavigation() {{ return {GetClassMetadataRef(prop.Object)}.props.{prop.InverseProperty.JsVariable} as ModelReferenceNavigationProperty }},");
                        }

                        if (prop.IsManytoManyCollection)
                        {
                            using (b.Block("manyToMany:", ","))
                            {
                                var nearNavigation = prop.ManyToManyNearNavigationProperty;
                                var farNavigation = prop.ManyToManyFarNavigationProperty;

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

                int hiddenAreaFlags = (int)prop.HiddenAreas;
                if (hiddenAreaFlags != 0)
                {
                    b.Prop("hidden", hiddenAreaFlags.ToString() + " as HiddenAreas");
                }

                // We store the negative case instead of the positive
                // because there are likely going to be more that are serializable than not.
                if (!prop.IsClientSerializable)
                {
                    b.Line("dontSerialize: true,");
                }

                if (prop.IsCreateOnly)
                {
                    b.Line("createOnly: true,");
                }


                if (prop.IsClientWritable && prop.IsClientSerializable && (
                    prop.Type.TsTypeKind is 
                        TypeDiscriminator.Number or 
                        TypeDiscriminator.String or 
                        TypeDiscriminator.Boolean or 
                        TypeDiscriminator.Enum or 
                        TypeDiscriminator.Date
                ))
                {
                    var defaultValue = prop.DefaultValue;
                    if (defaultValue is not null)
                    {
                        if (
                            prop.Type.TsTypeKind is TypeDiscriminator.String &&
                            defaultValue is string stringValue
                        )
                        {
                            b.StringProp("defaultValue", stringValue);
                        }
                        else if (
                            prop.Type.TsTypeKind is TypeDiscriminator.Boolean &&
                            defaultValue is true or false
                        )
                        {
                            b.Prop("defaultValue", defaultValue.ToString().ToLowerInvariant());
                        }
                        else if (
                            prop.Type.TsTypeKind is TypeDiscriminator.Number or TypeDiscriminator.Enum &&
                            double.TryParse(defaultValue.ToString(), out _)
                        )
                        {
                            b.Prop("defaultValue", defaultValue.ToString());
                        }
                        else
                        {
                            throw new InvalidOperationException(
                                $"Default value {defaultValue} does not match property type {prop.Type}, " +
                                $"or type does not support default values.");
                        }
                    }

                    List<string> rules = GetValidationRules(prop, (prop.ReferenceNavigationProperty ?? prop).DisplayName);

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

        private static List<string> GetValidationRules(ValueViewModel prop, string propName)
        {
            // TODO: Handle 'ClientValidationAllowSave' by placing a field on the 
            // validator function that contains the value of this flag.

            var clientValidationError = prop.GetAttributeValue<ClientValidationAttribute>(a => a.ErrorMessage);

            var rules = new List<string>();
            string Error(string message, string fallback)
            {
                if (string.IsNullOrWhiteSpace(message))
                {
                    message = fallback;
                }

                return $"|| \"{message.EscapeStringLiteralForTypeScript()}\"";
            }


            // A simple falsey check will treat a numeric zero as "absent", so we explicitly check for
            // null/undefined instead.
            var requiredPredicate = prop.Type.IsString ? "(val != null && val !== '')" : "val != null";

            var isRequired = prop.GetAttributeValue<ClientValidationAttribute, bool>(a => a.IsRequired);
            if (isRequired == true)
            {
                rules.Add($"required: val => {requiredPredicate} {Error(clientValidationError, $"{propName} is required.")}");
            }
            else if (prop.IsRequired)
            {
                string message = null;
                if (prop.GetValidationAttribute<RequiredAttribute>() is (true, string requiredMessage))
                {
                    message = requiredMessage;
                    if (prop.GetAttributeValue<RequiredAttribute, bool>(a => a.AllowEmptyStrings) == true)
                    {
                        requiredPredicate = "val != null";
                    }
                }
                if (string.IsNullOrWhiteSpace(message))
                {
                    message = $"{propName} is required.";
                }

                rules.Add($"required: val => {requiredPredicate} || \"{message.EscapeStringLiteralForTypeScript()}\"");
            }

            var pattern = prop.GetAttributeValue<ClientValidationAttribute>(a => a.Pattern);
            if (pattern == null && prop.Type.IsGuid)
            {
                pattern = @"^\s*[{(]?[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}[)}]?\s*$";
            }
            if (!string.IsNullOrEmpty(pattern))
            {
                rules.Add($"pattern: val => !val || /{pattern}/.test(val) {Error(clientValidationError, $"{propName} does not match expected format.")}");
            }

            if (prop.Type.IsString)
            {
                void Min(object value, string error) => rules.Add($"minLength: val => !val || val.length >= {value} {Error(error, $"{propName} must be at least {value} characters.")}");
                void Max(object value, string error) => rules.Add($"maxLength: val => !val || val.length <= {value} {Error(error, $"{propName} may not be more than {value} characters.")}");

                if (prop.GetValidationAttribute<StringLengthAttribute, int>(x => x.MinimumLength) is (true, int min, string minMessage))
                    Min(min, minMessage);
                else if (prop.GetValidationAttribute<MinLengthAttribute, int>(x => x.Length) is (true, int min2, string min2Message))
                    Min(min2, min2Message);
#if NET8_0_OR_GREATER
                else if (prop.GetValidationAttribute<LengthAttribute, int>(x => x.MinimumLength) is (true, int min3, string min3Message))
                    Min(min3, min3Message);
#endif
                else if (prop.GetAttributeValue<ClientValidationAttribute, int>(a => a.MinLength) is int minLength and not int.MaxValue)
                    Min(minLength, clientValidationError);

                if (prop.GetValidationAttribute<StringLengthAttribute, int>(x => x.MaximumLength) is (true, int max, string maxMessage))
                    Max(max, maxMessage);
                else if (prop.GetValidationAttribute<MaxLengthAttribute, int>(x => x.Length) is (true, int max2, string max2Message))
                    Max(max2, max2Message);
#if NET8_0_OR_GREATER
                else if (prop.GetValidationAttribute<LengthAttribute, int>(x => x.MaximumLength) is (true, int max3, string max3Message))
                    Max(max3, max3Message);
#endif
                else if (prop.GetAttributeValue<ClientValidationAttribute, int>(a => a.MaxLength) is int maxLength and not int.MinValue)
                    Max(maxLength, clientValidationError);

                if (prop.GetValidationAttribute<UrlAttribute>() is (true, string urlMessage))
                {
                    const string urlPattern = @"^((https?|ftp):\/\/.)";
                    rules.Add($"url: val => !val || /{urlPattern}/.test(val) {Error(clientValidationError, $"{propName} must be a valid URL.")}");
                }

                // https://emailregex.com/
                const string emailRegex = @"^(([^<>()\[\]\\.,;:\s@""]+(\.[^<> ()\[\]\\.,;:\s@""]+)*)|("".+ ""))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$";
                if (prop.GetAttributeValue<ClientValidationAttribute, bool>(a => a.IsEmail) == true)
                {
                    rules.Add($"email: val => !val || /{emailRegex}/.test(val.trim()) {Error(clientValidationError, $"{propName} must be a valid email address.")}");
                }
                else if (prop.GetValidationAttribute<EmailAddressAttribute>() is (true, string message))
                {
                    // This is actually much more strict on the client than what EmailAddressAttribute actually validates on the server.
                    rules.Add($"email: val => !val || /{emailRegex}/.test(val.trim()) {Error(message, $"{propName} must be a valid email address.")}");
                }

                if (prop.GetAttributeValue<ClientValidationAttribute, bool>(a => a.IsPhoneUs) == true)
                {
                    const string phoneRegex = @"^(1-?)?(\([2-9]\d{2}\)|[2-9]\d{2})-?[2-9]\d{2}-?\d{4}$";
                    rules.Add($"phone: val => !val || /{phoneRegex}/.test(val.replace(/\\s+/g, '')) {Error(clientValidationError, $"{propName} must be a valid US phone number.")}");
                }
                else if (prop.GetValidationAttribute<PhoneAttribute>() is (true, string message))
                {
                    // This regex from https://github.com/dotnet/runtime/blob/ab0a4ff9fa8002fa17703a9f9571869b820846c3/src/libraries/System.ComponentModel.Annotations/src/System/ComponentModel/DataAnnotations/PhoneAttribute.cs#LL15C9-L15C9
                    // and matches the behavior of the server validation
                    const string phoneRegex = @"^(\+\s?)?((?<!\+.*)\(\+?\d+([\s\-\.]?\d+)?\)|\d+)([\s\-\.]?(\(\d+([\s\-\.]?\d+)?\)|\d+))*(\s?(x|ext\.?)\s?\d+)?$";
                    rules.Add($"phone: val => !val || /{phoneRegex}/.test(val.replace(/\\s+/g, '')) {Error(message, $"{propName} must be a valid phone number.")}");
                }
            }
            else if (prop.Type.IsNumber)
            {
                void Min(object value, bool exclusive, string error) => rules.Add(exclusive
                    ? $"min: val => val == null || val > {value} {Error(error, $"{propName} must be more than {value}.")}"
                    : $"min: val => val == null || val >= {value} {Error(error, $"{propName} must be at least {value}.")}");
                void Max(object value, bool exclusive, string error) => rules.Add(exclusive
                    ? $"max: val => val == null || val < {value} {Error(error, $"{propName} must be less than {value}.")}"
                    : $"max: val => val == null || val <= {value} {Error(error, $"{propName} may not be more than {value}.")}");

                var range = prop.GetAttribute<RangeAttribute>();
                if (range != null)
                {
                    var message = range.GetValue(a => a.ErrorMessage);
#if NET8_0_OR_GREATER
                    Min(range.GetValue(r => r.Minimum), range.GetValue(r => r.MinimumIsExclusive) ?? false, message);
                    Max(range.GetValue(r => r.Maximum), range.GetValue(r => r.MaximumIsExclusive) ?? false, message);
#else
                    Min(range.GetValue(r => r.Minimum), false, message);
                    Max(range.GetValue(r => r.Maximum), false, message);
#endif
                }
                else
                {
                    if (prop.GetAttributeValue<ClientValidationAttribute, double>(a => a.MinValue) is double minValue and not double.MaxValue)
                        Min(minValue, false, clientValidationError);

                    if (prop.GetAttributeValue<ClientValidationAttribute, double>(a => a.MaxValue) is double maxValue and not double.MinValue)
                        Max(maxValue, false, clientValidationError);
                }
            }

            return rules;
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


                int hiddenAreaFlags = (int)method.HiddenAreas;
                if (hiddenAreaFlags != 0)
                {
                    b.Prop("hidden", hiddenAreaFlags.ToString() + " as HiddenAreas");
                }

                if (method.IsStatic)
                {
                    b.Prop("isStatic", "true");
                }

                if (method.GetAttributeValue<ExecuteAttribute, bool>(e => e.AutoClear) == true)
                {
                    b.Prop("autoClear", "true");
                }

                using (b.Block("params:", ','))
                {
                    foreach (var param in method.ApiParameters)
                    {
                        WriteMethodParameterMetadata(b, method, param);
                    }
                }

                using (b.Block("return:", ','))
                {
                    WriteValueCommonMetadata(b, method.Return);
                    b.StringProp("role", "value");
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
                if (parameter.ParentSourceProp != null)
                {
                    b.Line($"get source() {{ return {GetClassMetadataRef(method.Parent)}.props.{parameter.ParentSourceProp.JsVariable} }},");
                }

                List<string> rules = GetValidationRules(parameter, parameter.DisplayName);

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
            }
        }

        /// <summary>
        /// Write the metadata for all data sources of a class
        /// </summary>
        private void WriteDataSourceMetadata(TypeScriptCodeBuilder b, ClassViewModel model, ClassViewModel source)
        {
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
        private void WriteValueCommonMetadata(TypeScriptCodeBuilder b, ValueViewModel value)
        {
            b.StringProp("name", value.JsVariable);
            b.StringProp("displayName", value.DisplayName);
            
            if (!string.IsNullOrWhiteSpace(value.Description))
            {
                b.StringProp("description", value.Description);
            }

            WriteTypeCommonMetadata(b, value.Type, value);
        }

        /// <summary>
        /// Write metadata common to all type representations, 
        /// like properties, method parameters, method returns, etc.
        /// </summary>
        private void WriteTypeCommonMetadata(TypeScriptCodeBuilder b, TypeViewModel type, ValueViewModel definingMember)
        {
            var kind = type.TsTypeKind;
            var subtype = definingMember.GetAttributeValue<DataTypeAttribute, DataType>(a => a.DataType);

            switch (kind)
            {
                case TypeDiscriminator.Unknown:
                    if (type.FullyQualifiedName != "System.Object")
                    {
                        // System.Object technically _is_ supported via "unknown", but any derived type
                        // that isn't otherwise explicitly supported by Coalesce should have this message.
                        b.Line("// Type not supported natively by Coalesce - falling back to unknown.");
                    }
                    b.StringProp("type", "unknown");
                    break;

                default:
                    b.StringProp("type", kind.ToString().ToLowerInvariant());
                    break;
            }

            switch (kind)
            {
                case TypeDiscriminator.Enum:
                    b.Line($"get typeDef() {{ return domain.enums.{type.ClientTypeName} }},");
                    break;

                case TypeDiscriminator.Model:
                case TypeDiscriminator.Object:
                    b.Line($"get typeDef() {{ return {GetClassMetadataRef(type.ClassViewModel)} }},");
                    break;

                case TypeDiscriminator.Collection:
                    // For collections, write the references to the underlying type.
                    if (type.PureType.TsTypeKind == TypeDiscriminator.Collection)
                    {
                        throw new InvalidOperationException("Collections of collections aren't supported by Coalesce as exposed types");
                    }

                    using (b.Block($"itemType:", ','))
                    {
                        b.StringProp("name", "$collectionItem");
                        b.StringProp("displayName", "");
                        b.StringProp("role", "value");
                        WriteTypeCommonMetadata(b, type.PureType, definingMember);
                    }
                    break;

                case TypeDiscriminator.Date:
                    var dateType = definingMember.DateType;

                    b.StringProp("dateKind", dateType switch
                    {
                        DateTypes.DateOnly => "date",
                        DateTypes.TimeOnly => "time",
                        _ => "datetime"
                    });

                    if (!type.IsDateTimeOffset)
                    {
                        b.Prop("noOffset", "true");
                    }

                    if (type.IsDateTime && dateType.GetValueOrDefault(DateTypes.DateTime) != DateTypes.DateTime)
                    {
                        // For System.DateTime props with a date kind of "time" or "date",
                        // these must be serialized with both date and time so they can correctly be deserialized by the server.
                        // Otherwise, coalesce-vue will serialize these with only the date part or time part,
                        // assuming that the target on the server is a DateOnly or a TimeOnly.

                        // New applications should use DateOnly and TimeOnly types. This supports legacy behavior.
                        b.StringProp("serializeAs", "datetime");
                    }
                    break;

                case TypeDiscriminator.Binary:
                    if (definingMember is PropertyViewModel)
                    {
                        b.Prop("base64", "true");
                    }
                    break;

                case TypeDiscriminator.String:
                    b.StringProp("subtype", subtype switch
                    {
                        // HTML <input type="">s:
                        DataType.Password => "password",
                        DataType.Url => "url",
                        DataType.EmailAddress => "email",
                        DataType.PhoneNumber => "tel",

                        // Others:
                        DataType.Html => "multiline", // No special handling for html (yet?), so just treat as multiline for now.
                        DataType.MultilineText => "multiline",
                        DataType.ImageUrl => "url-image",
                        _ => definingMember.GetAttributeValue<DataTypeAttribute>(a => a.CustomDataType)?.ToLowerInvariant() switch
                        {
                            "color" => "color",
                            _ =>
                                definingMember.HasAttribute<UrlAttribute>() ? "url" : 
                                definingMember.HasAttribute<EmailAddressAttribute>() ? "email" :
                                definingMember.HasAttribute<PhoneAttribute>() ? "tel" :
                                null
                        }
                    }, omitIfNull: true);
                    break;
            }
        }
    }
}
