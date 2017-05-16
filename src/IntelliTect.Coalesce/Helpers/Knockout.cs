using IntelliTect.Coalesce.Utilities;
using Microsoft.AspNetCore.Html;
using System;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using IntelliTect.Coalesce.TypeDefinition;
using IntelliTect.Coalesce.DataAnnotations;

namespace IntelliTect.Coalesce.Helpers
{
    public static class Knockout
    {
        public enum DateTimePreservationOptions
        {
            None = 0,
            Date = 1,
            Time = 2
        }

        public static int DefaultLabelCols { get; set; } = 3;
        public static int DefaultInputCols { get; set; } = 9;
        public static string DefaultDateFormat { get; set; } = "M/D/YYYY";
        public static string DefaultTimeFormat { get; set; } = "h:mm a";
        public static string DefaultDateTimeFormat { get; set; } = "M/D/YYYY h:mm a";

        /// <summary>
        /// Wraps an HtmlString in a div class=form-group
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static HtmlString AddFormGroup(this HtmlString content)
        {
            return content.ToString().AddFormGroup();
        }
        /// <summary>
        /// Wraps an string in a div class=form-group
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static HtmlString AddFormGroup(this string content)
        {
            string result = string.Format(@"
            <div class=""form-group"">
                {0}
            </div>", content);
            return new HtmlString(result);
        }

        public static HtmlString AddLabel(this string content, string label, int? labelCols = null, int? inputCols = null)
        {
            int myLabelCols = labelCols ?? DefaultLabelCols;
            int myInputCols = inputCols ?? DefaultInputCols;

            string result = string.Format(@"
                    <label class=""col-sm-{1} control-label"">{0}</label>
                    <div class=""col-sm-{2}"">
                        {3}
                    </div>", label, myLabelCols, myInputCols, content);
            return new HtmlString(result);
        }

        public static HtmlString AddLabel(this HtmlString content, string label, int? labelCols = null, int? inputCols = null)
        {
            return content.ToString().AddLabel(label, labelCols, inputCols);
        }


        #region Dates

        public static HtmlString DateTimeWithLabel(
            string label, string dataBinding, string format = null, DateTimePreservationOptions preserve = DateTimePreservationOptions.None, int? stepping = null, int? labelCols = null, int? inputCols = null, bool delaySave = true)
        {
            return DateTime(dataBinding, format, preserve, stepping, delaySave).AddLabel(label, labelCols, inputCols);
        }

        public static HtmlString DateTime(
            string dataBinding, string format = null, DateTimePreservationOptions preserve = DateTimePreservationOptions.None, int? stepping = null, bool delaySave = true)
        {
            string icon = "fa-calendar";
            string steppingText = "";
            if (stepping.HasValue) steppingText = ", stepping: " + stepping.ToString();
            if (string.IsNullOrWhiteSpace(format))
            {
                switch (preserve)
                {
                    case DateTimePreservationOptions.Date:
                        format = DefaultDateFormat;
                        break;
                    case DateTimePreservationOptions.Time:
                        format = DefaultTimeFormat;
                        break;
                    default:
                        format = DefaultDateTimeFormat;
                        break;
                }
            }
            if (!format.Contains("D")) icon = "fa-clock-o";
            string result = string.Format(@"
                    <div class=""input-group date"">
                        <input data-bind=""datePicker: {0}, format: '{1}', preserveTime: {2}, preserveDate: {3} {5}, {6}"" type=""text"" class=""form-control"" />
                        <span class=""input-group-addon"">
                            <span class=""fa {4}""></span>
                        </span>
                    </div>",
                    dataBinding,
                    format,
                    (preserve == DateTimePreservationOptions.Time).ToString().ToLower(),
                    (preserve == DateTimePreservationOptions.Date).ToString().ToLower(),
                    icon,
                    steppingText,
                    delaySave ? "delaySave: true" : "");
            return new HtmlString(result);
        }


        public static HtmlString InputFor<T>(Expression<Func<T, DateTimeOffset>> propertySelector,
            string format = null, DateTimePreservationOptions preserve = DateTimePreservationOptions.None, int? stepping = null)
        {
            var propertyModel = ReflectionRepository.PropertyBySelector(propertySelector);
            return DateTime(propertyModel.JsVariableForBinding, format, preserve, stepping);
        }
        public static HtmlString InputFor<T>(Expression<Func<T, DateTimeOffset?>> propertySelector,
            string format = null, DateTimePreservationOptions preserve = DateTimePreservationOptions.None, int? stepping = null)
        {
            var propertyModel = ReflectionRepository.PropertyBySelector(propertySelector);
            return DateTime(propertyModel.JsVariableForBinding, format, preserve, stepping);
        }

        public static HtmlString InputFor<T>(Expression<Func<T, DateTime>> propertySelector,
            string format = null, DateTimePreservationOptions preserve = DateTimePreservationOptions.None, int? stepping = null)
        {
            var propertyModel = ReflectionRepository.PropertyBySelector(propertySelector);
            return DateTime(propertyModel.JsVariableForBinding, format, preserve, stepping);
        }
        public static HtmlString InputFor<T>(Expression<Func<T, DateTime?>> propertySelector,
            string format = null, DateTimePreservationOptions preserve = DateTimePreservationOptions.None, int? stepping = null)
        {
            var propertyModel = ReflectionRepository.PropertyBySelector(propertySelector);
            return DateTime(propertyModel.JsVariableForBinding, format, preserve, stepping);
        }

        public static HtmlString InputWithLabelFor<T>(Expression<Func<T, DateTimeOffset?>> propertySelector,
            string format = null, int? labelCols = null, int? inputCols = null, string label = null, DateTimePreservationOptions preserve = DateTimePreservationOptions.None, int stepping = 1)
        {
            var propertyModel = ReflectionRepository.PropertyBySelector(propertySelector);
            return DateTimeWithLabel(propertyModel.DisplayNameLabel(label), propertyModel.JsVariableForBinding, format, preserve, stepping, labelCols, inputCols);
        }

        public static HtmlString InputWithLabelFor<T>(Expression<Func<T, DateTimeOffset>> propertySelector,
            string format = "M/D/YYYY", int? labelCols = null, int? inputCols = null, string label = null, DateTimePreservationOptions preserve = DateTimePreservationOptions.None, int stepping = 1)
        {
            var propertyModel = ReflectionRepository.PropertyBySelector(propertySelector);
            return DateTimeWithLabel(propertyModel.DisplayNameLabel(label), propertyModel.JsVariableForBinding, format, preserve, stepping, labelCols, inputCols);
        }

        public static HtmlString InputWithLabelFor<T>(Expression<Func<T, DateTime?>> propertySelector,
            string format = "M/D/YYYY", int? labelCols = null, int? inputCols = null, string label = null, DateTimePreservationOptions preserve = DateTimePreservationOptions.None, int stepping = 1)
        {
            var propertyModel = ReflectionRepository.PropertyBySelector(propertySelector);
            return DateTimeWithLabel(propertyModel.DisplayNameLabel(label), propertyModel.JsVariableForBinding, format, preserve, stepping, labelCols, inputCols);
        }

        public static HtmlString InputWithLabelFor<T>(Expression<Func<T, DateTime>> propertySelector,
            string format = "M/D/YYYY", int? labelCols = null, int? inputCols = null, string label = null, DateTimePreservationOptions preserve = DateTimePreservationOptions.None, int stepping = 1)
        {
            var propertyModel = ReflectionRepository.PropertyBySelector(propertySelector);
            return DateTimeWithLabel(propertyModel.DisplayNameLabel(label), propertyModel.JsVariableForBinding, format, preserve, stepping, labelCols, inputCols);
        }


        #endregion

        #region TextArea
        public static HtmlString TextArea(
            string bindingValue, string bindingName = "value", int? rows = 4)
        {
            string result = string.Format(@"
                <textarea class=""form-control"" data-bind=""{1}: {0}"" rows=""{2}""></textarea>", bindingValue, bindingName, rows.ToString());
            return new HtmlString(result);
        }

        public static HtmlString TextAreaWithLabel(
            string label, string bindingValue, string bindingName = "value", int? rows = null,
            int? labelCols = null, int? textCols = null)
        {
            return TextArea(bindingValue, bindingName, rows).AddLabel(label, labelCols, textCols);
        }

        public static HtmlString TextAreaFor<T>(Expression<Func<T, string>> propertySelector,
            int? rows = null, string bindingName = "value")
        {
            var propertyModel = ReflectionRepository.PropertyBySelector(propertySelector);
            return TextArea(propertyModel.JsVariableForBinding, bindingName, rows);
        }


        public static HtmlString TextAreaWithLabelFor<T>(Expression<Func<T, string>> propertySelector,
            int? rows = null, int? labelCols = null, int? inputCols = null, string label = null, string bindingName = "value")
        {
            var propertyModel = ReflectionRepository.PropertyBySelector(propertySelector);
            return TextAreaFor(propertySelector, rows).AddLabel(propertyModel.DisplayNameLabel(label), labelCols, inputCols);
        }

        #endregion

        #region TextInput

        public static HtmlString TextInput(
            string bindingValue, string bindingName = "value")
        {
            string result = string.Format(@"
                <input type = ""text"" class=""form-control"" data-bind=""{1}: {0}"" />
                ", bindingValue, bindingName);
            return new HtmlString(result);
        }

        public static HtmlString TextInputWithLabel(
            string label, string bindingValue, string bindingName = "value", int? labelCols = null, int? textCols = null)
        {
            return TextInput(bindingValue, bindingName).AddLabel(label, labelCols, textCols);
        }

        public static HtmlString InputWithLabelFor<T>(Expression<Func<T, string>> propertySelector,
            int? labelCols = null, int? inputCols = null, string label = null, string bindingName = "value")
        {
            var propertyModel = ReflectionRepository.PropertyBySelector(propertySelector);
            return InputFor(propertySelector, bindingName).AddLabel(propertyModel.DisplayNameLabel(label), labelCols, inputCols);
        }

        public static HtmlString InputFor<T>(Expression<Func<T, string>> propertySelector,
            string bindingName = "value")
        {
            var propertyModel = ReflectionRepository.PropertyBySelector(propertySelector);
            return TextInput(propertyModel.JsVariableForBinding, bindingName);
        }
        #endregion

        #region Int
        public static HtmlString InputWithLabelFor<T>(Expression<Func<T, int>> propertySelector,
            int? labelCols = null, int? inputCols = null, string label = null, string bindingName = "value")
        {
            var propertyModel = ReflectionRepository.PropertyBySelector(propertySelector);
            return InputFor(propertySelector).AddLabel(propertyModel.DisplayNameLabel(label), labelCols, inputCols);
        }

        public static HtmlString InputFor<T>(Expression<Func<T, int>> propertySelector,
            string bindingName = "value")
        {
            var propertyModel = ReflectionRepository.PropertyBySelector(propertySelector);
            return TextInput(propertyModel.JsVariableForBinding, bindingName);
        }

        public static HtmlString InputWithLabelFor<T>(Expression<Func<T, int?>> propertySelector,
    int? labelCols = null, int? inputCols = null, string label = null, string bindingName = "value")
        {
            var propertyModel = ReflectionRepository.PropertyBySelector(propertySelector);
            return InputFor(propertySelector).AddLabel(propertyModel.DisplayNameLabel(label), labelCols, inputCols);
        }

        public static HtmlString InputFor<T>(Expression<Func<T, int?>> propertySelector,
            string bindingName = "value")
        {
            var propertyModel = ReflectionRepository.PropertyBySelector(propertySelector);
            return TextInput(propertyModel.JsVariableForBinding, bindingName);
        }
        #endregion

        #region boolean
        public static HtmlString Checkbox(
            string bindingValue, string bindingName = "checked")
        {
            return new HtmlString(string.Format(@"
                <input type = ""checkbox"" data-bind=""{1}: {0}"" />
                ", bindingValue, bindingName));
        }

        public static HtmlString BooleanSelect(string bindingValue, string trueText, string falseText)
        {
            return new HtmlString($@"<select class=""form-control"" data-bind=""booleanValue: {bindingValue}"" tabindex=""-1"" aria-hidden=""true"">
                        <option value=""true"">{trueText}</option>
                        <option value=""false"">{falseText}</option></select>");
        }

        public static HtmlString InputFor<T>(Expression<Func<T, bool>> propertySelector, string bindingName = "checked")
        {
            var propertyModel = ReflectionRepository.PropertyBySelector(propertySelector);
            return Checkbox(propertyModel.JsVariableForBinding, bindingName);
        }

        public static HtmlString InputWithLabelFor<T>(Expression<Func<T, bool>> propertySelector,
            int? labelCols = null, int? inputCols = null, string label = null, string bindingName = "checked")
        {
            var propertyModel = ReflectionRepository.PropertyBySelector(propertySelector);
            return InputFor<T>(propertySelector, bindingName).AddLabel(propertyModel.DisplayNameLabel(label), labelCols, inputCols);
        }

        public static HtmlString InputFor<T>(Expression<Func<T, bool?>> propertySelector, string bindingName = "checked")
        {
            var propertyModel = ReflectionRepository.PropertyBySelector(propertySelector);
            return Checkbox(propertyModel.JsVariableForBinding, bindingName);
        }

        public static HtmlString InputWithLabelFor<T>(Expression<Func<T, bool?>> propertySelector,
            int? labelCols = null, int? inputCols = null, string label = null, string bindingName = "checked")
        {
            var propertyModel = ReflectionRepository.PropertyBySelector(propertySelector);
            return InputFor<T>(propertySelector, bindingName).AddLabel(propertyModel.DisplayNameLabel(label), labelCols, inputCols);
        }

        public static HtmlString BooleanValueFor<T>(Expression<Func<T, bool>> propertySelector, string trueText = "Yes", string falseText = "No")
        {
            var propertyModel = ReflectionRepository.PropertyBySelector(propertySelector);
            return BooleanSelect(propertyModel.JsVariableForBinding, trueText, falseText);
        }
        public static HtmlString BooleanValueFor<T>(Expression<Func<T, bool?>> propertySelector, string trueText = "Yes", string falseText = "No")
        {
            var propertyModel = ReflectionRepository.PropertyBySelector(propertySelector);
            return BooleanSelect(propertyModel.JsVariableForBinding, trueText, falseText);
        }

        #endregion

        #region StringSelect
        public static HtmlString SelectFor<T>(Expression<Func<T, string>> propertySelector,
            string placeholder = "")
        {
            var propertyModel = ReflectionRepository.PropertyBySelector(propertySelector);
            placeholder = placeholder ?? propertyModel.DisplayName;
            return SelectString(propertyModel, placeholder);
        }

        public static HtmlString SelectWithLabelFor<T>(Expression<Func<T, string>> propertySelector,
            int? labelCols = null, int? inputCols = null, string label = null, string placeholder = "")
        {
            var propertyModel = ReflectionRepository.PropertyBySelector(propertySelector);
            return SelectFor<T>(propertySelector, placeholder).AddLabel(propertyModel.DisplayNameLabel(label), labelCols, inputCols);
        }

        public static HtmlString SelectString(PropertyViewModel propertyModel, string placeholder = "")
        {
            string result = string.Format(@"
                    <select class=""form-control"" 
                        data-bind=""select2AjaxText: {0}, url: coalesceConfig.baseApiUrl() + '/{2}', property: '{3}'"" placeholder=""{1}"">
                        <option></option>
                    </select >",
                propertyModel.JsVariableForBinding, placeholder, propertyModel.Parent.Name + "/PropertyValues", propertyModel.Name);
            // TODO: this may be wrong? I don't know enough about the code yet to know.
            return new HtmlString(result);
        }

        #endregion

        #region SelectObject
        public static HtmlString SelectWithLabelForObject<T>(Expression<Func<T, object>> propertySelector,
            int? labelCols = null, int? inputCols = null, string label = null, string placeholder = "")
        {
            var propertyModel = ReflectionRepository.PropertyBySelector(propertySelector);
            return SelectForObject<T>(propertySelector, placeholder).AddLabel(propertyModel.DisplayNameLabel(label), labelCols, inputCols);
        }

        public static HtmlString SelectForObject<T>(Expression<Func<T, object>> propertySelector,
            string placeholder = "", string prefix = "")
        {

            var propertyModel = ReflectionRepository.PropertyBySelector(propertySelector);
            if (propertyModel != null)
            {
                return SelectObject(propertyModel, placeholder, prefix, propertyModel.ObjectIdProperty == null ? !propertyModel.IsRequired : !propertyModel.ObjectIdProperty.IsRequired);
            }
            return HtmlString.Empty;
        }

        public static HtmlString SelectObject(PropertyViewModel propertyModel, string placeholder = "", string prefix = "", bool allowsClear = true)
        {

            if (prefix == "" && propertyModel.JsVariableIsReserved)
            {
                // This fixes bug #7799: in cases where the object that we're binding to is a js reserved word,
                // we need to prefix it with $data, which is used by knockout internally as follows.
                // I also made changes to a whole ton of other spots so that the new JsVariableForBinding is used where appropriate for knockout bindings.
                /*
                    // Build the source for a function that evaluates "expression"
                    // For each scope variable, add an extra level of "with" nesting
                    // Example result: with(sc1) { with(sc0) { return (expression) } }
                    var rewrittenBindings = ko.expressionRewriting.preProcessBindings(bindingsString, options),
                        functionBody = "with($context){with($data||{}){return{" + rewrittenBindings + "}}}";
                    return new Function("$context", "$element", functionBody);
                 */
                // Note that $data is indeed publicly documented at http://knockoutjs.com/documentation/binding-context.html, so this should be safe to use.
                prefix = "$data.";
            }


            string filterString = "";

            if (propertyModel.Wrapper.HasAttribute<SelectFilterAttribute>())
            {
                var foreignPropName = propertyModel.Wrapper.GetAttributeValue<SelectFilterAttribute>(nameof(SelectFilterAttribute.ForeignPropertyName)).ToString();
                var localValue = propertyModel.Wrapper.GetAttributeValue<SelectFilterAttribute>(nameof(SelectFilterAttribute.StaticPropertyValue));

                var foreignProp = propertyModel.Object.PropertyByName(foreignPropName);
                if (localValue != null)
                {
                    filterString = $"?{foreignProp.JsVariable}={localValue}";
                }
                else
                {
                    var localPropName = propertyModel.Wrapper.GetAttributeValue<SelectFilterAttribute>(nameof(SelectFilterAttribute.LocalPropertyName));
                    var localPropObjName = propertyModel.Wrapper.GetAttributeValue<SelectFilterAttribute>(nameof(SelectFilterAttribute.LocalPropertyObjectName));


                    if (localPropObjName != null)
                    {
                        var localPropObj = propertyModel.Parent.PropertyByName(localPropObjName.ToString());
                        var localProp = localPropObj.PureType.ClassViewModel.PropertyByName((localPropName ?? foreignPropName).ToString());

                        filterString = $"?{foreignProp.JsVariable}=' + ({localPropObj.JsVariableForBinding}() ? {localPropObj.JsVariableForBinding}().{localProp.JsVariable}() : 'null') + '";
                    }
                    else
                    {
                        var localProp = propertyModel.Parent.PropertyByName((localPropName ?? foreignPropName).ToString());
                        filterString = $"?{foreignProp.JsVariable}=' + {localProp.JsVariableForBinding}() + '";
                    }
                }
            }

            string result = string.Format(@"
                    <select class=""form-control"" 
                        data-bind=""select2Ajax: {6}{0}, url: function() {{ return '/api/{1}/customlist{8}' }}, idField: '{2}', textField: '{3}', object: {6}{4}, allowClear: {7}"" 
                        placeholder=""{5}"">
                            <option>{5}</option>
                    </select >
                    ",
                propertyModel.ObjectIdProperty.JsVariable, propertyModel.PureType.Name, propertyModel.Object.PrimaryKey.Name,
                propertyModel.Object.ListTextProperty.Name, propertyModel.JsVariable, placeholder, prefix, allowsClear.ToString().ToLowerInvariant(), filterString);
            return new HtmlString(result);
        }

        public static HtmlString SelectForManyToMany<T>(Expression<Func<T, object>> propertySelector, string placeholder = "", string prefix = "", string areaName = "")
        {
            var propertyModel = ReflectionRepository.PropertyBySelector(propertySelector);
            if (propertyModel != null)
            {
                return SelectForManyToMany(propertyModel, placeholder, prefix, areaName);
            }
            return HtmlString.Empty;
        }
        public static HtmlString SelectForManyToMany(PropertyViewModel propertyModel, string placeholder = "", string prefix = "", string areaName = "")
        {
            string result = string.Format(@"
                <select data-bind = ""select2AjaxMultiple: {0}{1}, itemViewModel: {6}ViewModels.{2}, 
                    idFieldName: '{3}', textFieldName: '{4}', url: '/api/{5}/customlist'""
                    class=""form-control"" multiple=""multiple"">
                </select>",
                prefix, propertyModel.ManyToManyCollectionName.ToCamelCase(), propertyModel.ManyToManyCollectionProperty.Object.ViewModelClassName,
                propertyModel.ManyToManyCollectionProperty.Object.PrimaryKey.Name,
                propertyModel.ManyToManyCollectionProperty.Object.ListTextProperty.Name, propertyModel.ManyToManyCollectionProperty.Object.Name,
                (string.IsNullOrWhiteSpace(areaName) ? "" : $"{areaName}."));
            return new HtmlString(result);
        }


        public static HtmlString SelectForWithLabel<T>(Expression<Func<T, string>> propertySelector,
            string placeholder = "", int? labelCols = null, int? inputCols = null, string label = null)
        {
            var propertyModel = ReflectionRepository.PropertyBySelector(propertySelector);
            return SelectFor(propertySelector, placeholder).AddLabel(propertyModel.DisplayNameLabel(label), labelCols, inputCols);
        }
        #endregion


        public static HtmlString InputFor<T>(Expression<Func<T, object>> propertySelector,
            string bindingName = "value")
        {
            var propertyModel = ReflectionRepository.PropertyBySelector(propertySelector);
            return TextInput(propertyModel.JsVariableForBinding, bindingName);
        }

        public static HtmlString SelectWithLabelFor<T>(Expression<Func<T, Enum>> propertySelector,
            string placeholder = "", int? labelCols = null, int? inputCols = null, string label = null)
        {
            var propertyModel = ReflectionRepository.PropertyBySelector(propertySelector);
            return SelectFor(propertySelector, placeholder).AddLabel(propertyModel.DisplayNameLabel(label), labelCols, inputCols);
        }

        public static HtmlString SelectFor<T>(Expression<Func<T, Enum>> propertySelector,
            string placeholder = "")
        {
            var propertyModel = ReflectionRepository.PropertyBySelector(propertySelector);
            return SelectEnum(propertyModel, placeholder);
        }

        public static HtmlString SelectEnum(PropertyViewModel propertyModel, string placeholder = "")
        {
            string result = string.Format(@"
                <select class=""form-control"" 
                    data-bind=""select2: {0}"" 
                    placeholder=""{1}"">",
                propertyModel.JsVariableForBinding, placeholder);

            foreach (var item in propertyModel.Type.EnumValues)
            {
                result += string.Format(@"<option value=""{0}"">{1}</option>", item.Key, item.Value.ToProperCase());
            }
            result += "</select>";
            return new HtmlString(result);
        }



        public static HtmlString ExpandButton(double size = 2)
        {
            string result = string.Format(@"
                <div class=""pull-right"" data-bind=""click: changeIsExpanded"" style=""font-size: {0}em; cursor:pointer;"">
                         <i class=""fa fa-minus-circle"" data-bind=""visible: isExpanded()""></i>
                    <i class=""fa fa-plus-circle"" data-bind=""visible: !isExpanded()""></i>
                </div>", size);

            return new HtmlString(result);
        }
        public static HtmlString ModifiedOnFromNow(double size = 1, string format = "M/D/YY")
        {
            string result = string.Format(@"
                <div class=""modified-on"" style=""font-size: {1}em;"">
                    <span data-bind=""text: modifiedOn().fromNow()""></span>
                </div>"
                , format, size);

            return new HtmlString(result);
        }
        public static HtmlString ModifiedOn(double size = 1, string format = "M/D/YY")
        {
            string result = string.Format(@"
                <div class=""modified-on"" style=""font-size: {1}em;"">
                    <span data-bind=""moment: modifiedOn, format: '{0}'""></span>
                </div>"
                , format, size);

            return new HtmlString(result);
        }


        #region Display
        public static HtmlString DisplayDateTime(string bindingValue, string format = null)
        {
            if (format == null) format = DefaultDateTimeFormat;
            string result = string.Format(@"
                <div class=""form-control-static"" data-bind=""moment: {0}, format: '{1}'"" ></div>"
                , bindingValue, format);

            return new HtmlString(result);
        }

        public static HtmlString DisplayFor<T>(Expression<Func<T, DateTimeOffset>> propertySelector,
            string format = null, DateTimePreservationOptions preserve = DateTimePreservationOptions.None)
        {
            var propertyModel = ReflectionRepository.PropertyBySelector(propertySelector);
            HtmlString returnString;
            switch (preserve)
            {
                case (DateTimePreservationOptions.Date):
                    returnString = DisplayDate(propertyModel.JsVariableForBinding, format);
                    break;
                case (DateTimePreservationOptions.Time):
                    returnString = DisplayTime(propertyModel.JsVariableForBinding, format);
                    break;
                default:
                    returnString = DisplayDateTime(propertyModel.JsVariableForBinding, format);
                    break;
            }

            return returnString;
        }

        public static HtmlString DisplayDate(string bindingValue, string format = null)
        {
            if (format == null) format = DefaultDateFormat;
            return DisplayDateTime(bindingValue, format);
        }

        public static HtmlString DisplayTime(string bindingValue, string format = null)
        {
            if (format == null) format = DefaultTimeFormat;
            return DisplayDateTime(bindingValue, format);
        }

        public static HtmlString DisplayManyToMany(PropertyViewModel propertyModel)
        {
            string result = string.Format(@"
                <div data-bind = ""foreach: {0} "">
                    <div class=""form-control-static"" data-bind=""text: {1}""></div>
                </div>",
                propertyModel.ManyToManyCollectionName.ToCamelCase(), propertyModel.ManyToManyCollectionProperty.Object.ListTextProperty.JsVariableForBinding);

            return new HtmlString(result);
        }

        public static HtmlString DisplayCheckbox(string bindingValue)
        {
            string result = string.Format(@"
                <input type = ""checkbox"" disabled data-bind=""checked: {0}"" />",
                bindingValue);

            return new HtmlString(result);
        }

        public static HtmlString DisplayFor<T>(Expression<Func<T, object>> propertySelector, bool linkObject = false)
        {
            var propertyModel = ReflectionRepository.PropertyBySelector(propertySelector);
            HtmlString returnString;

            if (propertyModel.IsDateOnly)
            {
                returnString = DisplayDate(propertyModel.JsVariableForBinding);
            }
            else if (propertyModel.Type.IsDate)
            {
                returnString = DisplayDateTime(propertyModel.JsVariableForBinding);
            }
            else if (propertyModel.IsManytoManyCollection)
            {
                returnString = DisplayManyToMany(propertyModel);
            }
            else if (propertyModel.Type.IsBool)
            {
                returnString = DisplayCheckbox(propertyModel.JsVariableForBinding);
            }
            else if (propertyModel.IsPOCO && !propertyModel.IsComplexType)
            {
                returnString = DisplayObject(propertyModel, linkObject);
            }
            else if (propertyModel.Type.IsEnum || (propertyModel.PureType.IsEnum && propertyModel.Type.IsNullable))
            {
                returnString = DisplayEnum(propertyModel);
            }
            else
            {
                returnString = DisplayText(propertyModel.JsVariableForBinding);
            }

            return returnString;
        }

        public static HtmlString DisplayObject(PropertyViewModel propertyModel, bool linkObject = false)
        {
            string result = "";
            result += $@"
                <div data-bind=""if: {propertyModel.JsVariableForBinding}()"">";
            if (linkObject && propertyModel.PureTypeOnContext)
            {
                result += $@"
                    <a class=""form-control-static"" data-bind=""attr: {{href: {propertyModel.JsVariableForBinding}().editUrl()}}, text: {propertyModel.JsVariableForBinding}().{propertyModel.Object.ListTextProperty.JsVariable}""></a>";
            }
            else
            {
                result += $@"
                    <div class=""form-control-static"" data-bind=""text: {propertyModel.JsVariableForBinding}().{propertyModel.Object.ListTextProperty.JsVariable}""></div>";
            }
            result += $@"
                </div>";
            result += $@"
                <div data-bind=""if: !{propertyModel.JsVariableForBinding}()"">
                    <div class=""form-control-static"">None</div>
                </div>";

            return new HtmlString(result);
        }

        public static HtmlString DisplayEnum(PropertyViewModel propertyModel)
        {
            return DisplayText(propertyModel.JsTextPropertyNameForBinding);
        }

        public static HtmlString DisplayText(string bindingValue)
        {
            string result = string.Format(@"
                <div class=""form-control-static"" data-bind=""text: {0}()""></div>",
                bindingValue);
            return new HtmlString(result);
        }

        public static HtmlString DisplayHtml<T>(Expression<Func<T, object>> propertySelector)
        {
            var propertyModel = ReflectionRepository.PropertyBySelector(propertySelector);

            string result = string.Format(@"
                <div class=""form-control-static"" data-bind=""html: {0}()""></div>",
                propertyModel.JsVariableForBinding);
            return new HtmlString(result);
        }

        public static HtmlString DisplayComment(string bindingValue)
        {
            string result = string.Format(@"
                <!-- Attempted to Display: {0} -->",
                bindingValue);
            return new HtmlString(result);
        }



        #endregion

        //public static HtmlString Select(string valueId)
        //{
        //    string result = string.Format(@"
        //        <select class="""" style=""width: 100%;"" 
        //            data-bind=""select2Ajax: {0}, url: '/api/adult/customlist', idField: 'AdultId', textField: 'NameAndBirthday', object: callLog().adult" placeholder = "Select Caller" >
        //              < option ></ option >
        //          </ select > ", 
        //          valueId);


        //    return new HtmlString(result);
        //}
    }
}
