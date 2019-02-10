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
        /// Returns the URL for the List Editor with the ???Id= query string.
        /// Ex: Adult/table?filter.adultId=
        /// </summary>
        public static string ListEditorUrl(this PropertyViewModel prop)
        {
            if (prop.InverseIdProperty == null) { return null; }
            return string.Format("{0}/Table?filter.{1}=", prop.Object.ControllerName, prop.InverseIdProperty.JsonName);
        }

        /// <summary>
        /// Returns the core URL for the List Editor.
        /// </summary>
        public static string ListEditorUrlName(this PropertyViewModel prop) => string.Format("{0}ListUrl", prop.JsVariable);


        /// <summary>
        /// Returns the action method on the controller for the upload of this property if it is a file.
        /// </summary>
        public static string UploadUrl(this PropertyViewModel prop)
        {
            if (!prop.IsFile) { return null; }
            return string.Format($"{prop.Name}Upload");
        }

        /// <summary>
        /// Gets the Knockout JS text for the validation.
        /// </summary>
        public static string ClientValidationKnockoutJs(this PropertyViewModel prop)
        {
            // Don't bother with validation on non-editable fields.
            if (!prop.IsClientWritable) return null;

            var isRequired = prop.GetAttributeValue<ClientValidationAttribute, bool>(a => a.IsRequired);
            var minValue = prop.GetAttributeValue<ClientValidationAttribute, double>(a => a.MinValue);
            var maxValue = prop.GetAttributeValue<ClientValidationAttribute, double>(a => a.MaxValue);
            var minLength = prop.GetAttributeValue<ClientValidationAttribute, int>(a => a.MinLength);
            var maxLength = prop.GetAttributeValue<ClientValidationAttribute, int>(a => a.MaxLength);
            var pattern = prop.GetAttributeValue<ClientValidationAttribute>(a => a.Pattern);
            var step = prop.GetAttributeValue<ClientValidationAttribute, double>(a => a.Step);
            var isEmail = prop.GetAttributeValue<ClientValidationAttribute, bool>(a => a.IsEmail);
            var isPhoneUs = prop.GetAttributeValue<ClientValidationAttribute, bool>(a => a.IsPhoneUs);
            var equal = prop.GetAttributeValue<ClientValidationAttribute>(a => a.Equal);
            var notEqual = prop.GetAttributeValue<ClientValidationAttribute>(a => a.NotEqual);
            var isDate = prop.GetAttributeValue<ClientValidationAttribute, bool>(a => a.IsDate);
            var isDateIso = prop.GetAttributeValue<ClientValidationAttribute, bool>(a => a.IsDateIso);
            var isNumber = prop.GetAttributeValue<ClientValidationAttribute, bool>(a => a.IsNumber);
            var isDigit = prop.GetAttributeValue<ClientValidationAttribute, bool>(a => a.IsDigit);
            var customName = prop.GetAttributeValue<ClientValidationAttribute>(a => a.CustomName);
            var customValue = prop.GetAttributeValue<ClientValidationAttribute>(a => a.CustomValue);
            var errorMessage = prop.GetAttributeValue<ClientValidationAttribute>(a => a.ErrorMessage);


            var validations = new List<string>();

            if (prop.Type.IsDate)
            {
                validations.Add("moment: { unix: true }");
            }

            if (isRequired.HasValue && isRequired.Value)
            {
                validations.Add($"required: {KoValidationOptions("true", errorMessage ?? $"{(prop.ReferenceNavigationProperty ?? prop).DisplayName} is required.")}");
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

                validations.Add($"required: {KoValidationOptions("true", message)}");
            }

            if (prop.Type.IsString)
            {
                if (prop.Range != null)
                {
                    var message = prop.GetAttributeValue<RangeAttribute>(a => a.ErrorMessage);
                    validations.Add($"minLength: {KoValidationOptions(prop.Range.Item1.ToString(), message)}, maxLength: {KoValidationOptions(prop.Range.Item2.ToString(), message)}");
                }
                else
                {
                    if (prop.MinLength.HasValue)
                    {
                        var message = prop.GetAttributeValue<MinLengthAttribute>(a => a.ErrorMessage);
                        validations.Add($"minLength: {KoValidationOptions(prop.MinLength.Value.ToString(), message)}");
                    }
                    else if (minLength.HasValue && minLength.Value != int.MaxValue)
                    {
                        validations.Add($"minLength: {KoValidationOptions(minLength.Value.ToString(), errorMessage)}");
                    }

                    if (prop.MaxLength.HasValue)
                    {
                        var message = prop.GetAttributeValue<MaxLengthAttribute>(a => a.ErrorMessage);
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
                    var message = prop.GetAttributeValue<RangeAttribute>(a => a.ErrorMessage);
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
                validations.Add($"date: {KoValidationOptions("true", errorMessage)}");
            if (isDateIso.HasValue && isDateIso.Value)
                validations.Add($"dateISO: {KoValidationOptions("true", errorMessage)}");
            if (isNumber.HasValue && isNumber.Value)
                validations.Add($"number: {KoValidationOptions("true", errorMessage)}");
            if (isDigit.HasValue && isDigit.Value)
                validations.Add($"digit: {KoValidationOptions("true", errorMessage)}");
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
