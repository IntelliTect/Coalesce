using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IntelliTect.Coalesce.Knockout.TypeDefinition
{
    public static class PropertyViewModelExtensions
    {

        /// <summary>
        /// Returns the correctly-prefixed version of the value of JsVariable for use in Knockout bindings
        /// </summary>
        public static string JsVariableForBinding(this PropertyViewModel prop)
            => prop.JsVariableIsReserved ? $"$data.{prop.JsVariable}" : prop.JsVariable;


        /// <summary>
        /// Text property name for knockout bindings, for things like enums. PureType+'Text'
        /// </summary>
        public static string JsTextPropertyNameForBinding(this PropertyViewModel prop)
            => prop.JsVariableForBinding() + "Text";




        /// <summary>
        /// Gets the Knockout JS text for the validation.
        /// </summary>
        public static string ClientValidationKnockoutJs(this PropertyViewModel prop)
        {
            // Don't bother with validation on non-editable fields.
            if (!prop.CanWrite) return null;

            var isRequired = prop.GetAttributeValue<ClientValidationAttribute>(nameof(ClientValidationAttribute.IsRequired)) as bool?;
            var minValue = prop.GetAttributeValue<ClientValidationAttribute>(nameof(ClientValidationAttribute.MinValue)) as double?;
            var maxValue = prop.GetAttributeValue<ClientValidationAttribute>(nameof(ClientValidationAttribute.MaxValue)) as double?;
            var minLength = prop.GetAttributeValue<ClientValidationAttribute>(nameof(ClientValidationAttribute.MinLength)) as int?;
            var maxLength = prop.GetAttributeValue<ClientValidationAttribute>(nameof(ClientValidationAttribute.MaxLength)) as int?;
            var pattern = prop.GetAttributeObject<ClientValidationAttribute, string>(nameof(ClientValidationAttribute.Pattern));
            var step = prop.GetAttributeValue<ClientValidationAttribute>(nameof(ClientValidationAttribute.Step)) as double?;
            var isEmail = prop.GetAttributeValue<ClientValidationAttribute>(nameof(ClientValidationAttribute.IsEmail)) as bool?;
            var isPhoneUs = prop.GetAttributeValue<ClientValidationAttribute>(nameof(ClientValidationAttribute.IsPhoneUs)) as bool?;
            var equal = prop.GetAttributeObject<ClientValidationAttribute, string>(nameof(ClientValidationAttribute.Equal));
            var notEqual = prop.GetAttributeObject<ClientValidationAttribute, string>(nameof(ClientValidationAttribute.NotEqual));
            var isDate = prop.GetAttributeValue<ClientValidationAttribute>(nameof(ClientValidationAttribute.IsDate)) as bool?;
            var isDateIso = prop.GetAttributeValue<ClientValidationAttribute>(nameof(ClientValidationAttribute.IsDateIso)) as bool?;
            var isNumber = prop.GetAttributeValue<ClientValidationAttribute>(nameof(ClientValidationAttribute.IsNumber)) as bool?;
            var isDigit = prop.GetAttributeValue<ClientValidationAttribute>(nameof(ClientValidationAttribute.IsDigit)) as bool?;
            var customName = prop.GetAttributeObject<ClientValidationAttribute, string>(nameof(ClientValidationAttribute.CustomName));
            var customValue = prop.GetAttributeObject<ClientValidationAttribute, string>(nameof(ClientValidationAttribute.CustomValue));
            var errorMessage = prop.GetAttributeObject<ClientValidationAttribute, string>(nameof(ClientValidationAttribute.ErrorMessage));


            var validations = new List<string>();

            if (prop.Type.IsDate)
            {
                validations.Add("moment: { unix: true }");
            }

            if (isRequired.HasValue && isRequired.Value)
            {
                validations.Add($"required: {KoValidationOptions("true", errorMessage ?? $"{(prop.IdPropertyObjectProperty ?? prop).DisplayName} is required.")}");
            }
            else if (prop.IsRequired)
            {
                string message = null;
                if (prop.HasAttribute<RequiredAttribute>())
                {
                    message = prop.GetAttributeObject<RequiredAttribute, string>(nameof(RequiredAttribute.ErrorMessage));
                }
                if (string.IsNullOrWhiteSpace(message))
                {
                    var name = (prop.IdPropertyObjectProperty ?? prop).DisplayName;
                    message = $"{name} is required.";
                }

                validations.Add($"required: {KoValidationOptions("true", message)}");
            }

            if (prop.Type.IsString)
            {
                if (prop.Range != null)
                {
                    var message = prop.GetAttributeObject<RangeAttribute, string>(nameof(RangeAttribute.ErrorMessage));
                    validations.Add($"minLength: {KoValidationOptions(prop.Range.Item1.ToString(), message)}, maxLength: {KoValidationOptions(prop.Range.Item2.ToString(), message)}");
                }
                else
                {
                    if (prop.MinLength.HasValue)
                    {
                        var message = prop.GetAttributeObject<MinLengthAttribute, string>(nameof(MinLengthAttribute.ErrorMessage));
                        validations.Add($"minLength: {KoValidationOptions(prop.MinLength.Value.ToString(), message)}");
                    }
                    else if (minLength.HasValue && minLength.Value != int.MaxValue)
                    {
                        validations.Add($"minLength: {KoValidationOptions(minLength.Value.ToString(), errorMessage)}");
                    }

                    if (prop.MaxLength.HasValue)
                    {
                        var message = prop.GetAttributeObject<MaxLengthAttribute, string>(nameof(MaxLengthAttribute.ErrorMessage));
                        validations.Add($"maxLength: {KoValidationOptions(prop.MaxLength.Value.ToString(), message)}");
                    }
                    else if (maxLength.HasValue && maxLength.Value != int.MinValue)
                    {
                        validations.Add($"maxLength: {KoValidationOptions(maxLength.Value.ToString(), errorMessage)}");
                    }
                }
            }
            else if (prop.Type.IsNumber)
            {
                if (prop.Range != null)
                {
                    var message = prop.GetAttributeObject<RangeAttribute, string>(nameof(RangeAttribute.ErrorMessage));
                    validations.Add($"min: {KoValidationOptions(prop.Range.Item1.ToString(), message)}, max: {KoValidationOptions(prop.Range.Item2.ToString(), message)}");
                }
                else
                {
                    if (minValue.HasValue && minValue.Value != double.MaxValue)
                        validations.Add($"min: {KoValidationOptions(minValue.Value.ToString(), errorMessage)}");
                    if (maxValue.HasValue && maxValue.Value != double.MinValue)
                        validations.Add($"max: {KoValidationOptions(maxValue.Value.ToString(), errorMessage)}");
                }
            }

            if (pattern != null)
                validations.Add($"pattern: {KoValidationOptions($"'{pattern}'", errorMessage)}");
            if (step.HasValue && step.Value != 0)
                validations.Add($"step: {KoValidationOptions($"{step.Value}", errorMessage)}");
            if (isEmail.HasValue && isEmail.Value)
                validations.Add($"email: {KoValidationOptions("true", errorMessage)}");
            if (isPhoneUs.HasValue && isPhoneUs.Value)
                validations.Add($"phoneUS: {KoValidationOptions("true", errorMessage)}");
            if (equal != null)
                validations.Add($"equal: {KoValidationOptions($"{equal}", errorMessage)}");
            if (notEqual != null)
                validations.Add($"notEqual: {KoValidationOptions($"{notEqual}", errorMessage)}");
            if (isDate.HasValue && isDate.Value)
                validations.Add($"isDate: {KoValidationOptions("true", errorMessage)}");
            if (isDateIso.HasValue && isDateIso.Value)
                validations.Add($"isDateISO: {KoValidationOptions("true", errorMessage)}");
            if (isNumber.HasValue && isNumber.Value)
                validations.Add($"isNumber: {KoValidationOptions("true", errorMessage)}");
            if (isDigit.HasValue && isDigit.Value)
                validations.Add($"isDigit: {KoValidationOptions("true", errorMessage)}");
            if (!string.IsNullOrWhiteSpace(customName) && !string.IsNullOrWhiteSpace(customValue))
                validations.Add($"{customName}: {customValue}");

            return string.Join(", ", validations);
        }

        private static string AddErrorMessage(string errorMessage)
        {
            string message = null;

            if (!string.IsNullOrWhiteSpace(errorMessage)) message = $", message: \"{errorMessage}\"";

            return message;
        }

        private static string KoValidationOptions(string value, string errorMessage)
        {
            string message = AddErrorMessage(errorMessage);
            if (!string.IsNullOrWhiteSpace(message))
            {
                return $"{{params: {value}, message: \"{errorMessage}\"}}";
            }
            else
            {
                return value;
            }
        }
    }
}
