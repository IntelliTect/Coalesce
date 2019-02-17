using IntelliTect.Coalesce.Utilities;
using Microsoft.AspNetCore.Html;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using IntelliTect.Coalesce.TypeDefinition;
using IntelliTect.Coalesce.Knockout.TypeDefinition;
using IntelliTect.Coalesce.DataAnnotations;
using System.Linq;
using System.Text;

namespace IntelliTect.Coalesce.Knockout.Helpers
{
    public static class Knockout
    {
        public enum DateTimePreservationOptions
        {
            None = 0,
            Date = 1,
            Time = 2
        }

        /// <summary>
        ///     The default number of Bootstrap grid columns the field label should span when calling
        ///     <see cref="AddLabel(string,string,System.Nullable{int},System.Nullable{int})" />.  This sets up a site-wide
        ///     default, which is 3 initially.
        /// </summary>
        public static int DefaultLabelCols { get; set; } = 3;

        /// <summary>
        ///     The default number of Bootstrap grid columns the form input should span when calling
        ///     <see cref="AddLabel(string,string,System.Nullable{int},System.Nullable{int})" />.  This sets up a site-wide
        ///     default, which is 9 initially.
        /// </summary>
        public static int DefaultInputCols { get; set; } = 9;


        /// <summary>
        ///     Sets the default date-only (<see cref="F:DateTimePreservationOptions.Date" />) format to be used by all
        ///     date/time pickers.  Initially set to 'M/D/YYYY'.
        /// </summary>
        /// <remarks>
        ///     See <a href="http://momentjs.com/docs/#/displaying/format/">The Moment.js documentation</a> for allowed formats.
        /// </remarks>
        public static string DefaultDateFormat { get; set; } = "M/D/YYYY";

        /// <summary>
        ///     Sets the default time-only (<see cref="F:DateTimePreservationOptions.Time" />) format to be used by all
        ///     date/time pickers.  Initially set to 'h:mm a'.
        /// </summary>
        /// <remarks>
        ///     See <a href="http://momentjs.com/docs/#/displaying/format/">The Moment.js documentation</a> for allowed formats.
        /// </remarks>
        public static string DefaultTimeFormat { get; set; } = "h:mm a";

        /// <summary>
        /// Sets the default format to be used by all date/time pickers that aren't date or time only.  Initially set to 'M/D/YYYY h:mm a'.
        /// </summary>
        /// <remarks>
        ///     See <a href="http://momentjs.com/docs/#/displaying/format/">The Moment.js documentation</a> for allowed formats.
        /// </remarks>>
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
            string result = $@"
            <div class=""form-group"">
                {content}
            </div>";
            return new HtmlString(result);
        }

        public static HtmlString AddLabel(this string content, string label, int? labelCols = null, int? inputCols = null)
        {
            int myLabelCols = labelCols ?? DefaultLabelCols;
            int myInputCols = inputCols ?? DefaultInputCols;

            string result = $@"
                    <label class=""col-sm-{myLabelCols} control-label"">{label}</label>
                    <div class=""col-sm-{myInputCols}"">
                        {content}
                    </div>";
            return new HtmlString(result);
        }

        public static HtmlString AddLabel(this HtmlString content, string label, int? labelCols = null, int? inputCols = null)
        {
            return content.ToString().AddLabel(label, labelCols, inputCols);
        }


        #region Dates

        /// <summary>
        /// 
        /// </summary>
        /// <param name="label"></param>
        /// <param name="dataBinding"></param>
        /// <param name="format"></param>
        /// <param name="preserve"></param>
        /// <param name="stepping"></param>
        /// <param name="labelCols"></param>
        /// <param name="inputCols"></param>
        /// <returns></returns>
        public static HtmlString DateTimeWithLabel(
            string label, string dataBinding, string format = null, DateTimePreservationOptions preserve = DateTimePreservationOptions.None, int? stepping = null, int? labelCols = null, int? inputCols = null)
        {
            return DateTime(dataBinding, format, preserve, stepping).AddLabel(label, labelCols, inputCols);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataBinding"></param>
        /// <param name="format"></param>
        /// <param name="preserve"></param>
        /// <param name="stepping"></param>
        /// <returns></returns>
        public static HtmlString DateTime(
            string dataBinding, string format = null, DateTimePreservationOptions preserve = DateTimePreservationOptions.None, int? stepping = null)
        {
            string icon = "fa-calendar";
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
            string result = $@"
                    <div class=""input-group date"">
                        <input data-bind=""datePicker: {dataBinding}, format: '{format}', preserveTime: {(preserve == DateTimePreservationOptions.Time).ToString().ToLower()}, preserveDate: {(preserve == DateTimePreservationOptions.Date).ToString().ToLower()} {(stepping.HasValue ? $", stepping: {stepping}" : "")}"" type=""text"" class=""form-control"" />
                        <span class=""input-group-addon"">
                            <span class=""fa {icon}""></span>
                        </span>
                    </div>";
            return new HtmlString(result);
        }


        public static HtmlString InputFor<T>(Expression<Func<T, DateTimeOffset>> propertySelector,
            string format = null, DateTimePreservationOptions preserve = DateTimePreservationOptions.None, int? stepping = null)
        {
            PropertyViewModel propertyModel = ReflectionRepository.Global.PropertyBySelector(propertySelector);
            return DateTime(propertyModel.JsVariableForBinding(), format, preserve, stepping);
        }

        public static HtmlString InputFor<T>(Expression<Func<T, DateTimeOffset?>> propertySelector,
            string format = null, DateTimePreservationOptions preserve = DateTimePreservationOptions.None, int? stepping = null)
        {
            PropertyViewModel propertyModel = ReflectionRepository.Global.PropertyBySelector(propertySelector);
            return DateTime(propertyModel.JsVariableForBinding(), format, preserve, stepping);
        }

        public static HtmlString InputFor<T>(Expression<Func<T, DateTime>> propertySelector,
            string format = null, DateTimePreservationOptions preserve = DateTimePreservationOptions.None, int? stepping = null)
        {
            PropertyViewModel propertyModel = ReflectionRepository.Global.PropertyBySelector(propertySelector);
            return DateTime(propertyModel.JsVariableForBinding(), format, preserve, stepping);
        }

        public static HtmlString InputFor<T>(Expression<Func<T, DateTime?>> propertySelector,
            string format = null, DateTimePreservationOptions preserve = DateTimePreservationOptions.None, int? stepping = null)
        {
            PropertyViewModel propertyModel = ReflectionRepository.Global.PropertyBySelector(propertySelector);
            return DateTime(propertyModel.JsVariableForBinding(), format, preserve, stepping);
        }

        public static HtmlString InputWithLabelFor<T>(Expression<Func<T, DateTimeOffset?>> propertySelector,
            string format = null, int? labelCols = null, int? inputCols = null, string label = null, DateTimePreservationOptions preserve = DateTimePreservationOptions.None, int stepping = 1)
        {
            PropertyViewModel propertyModel = ReflectionRepository.Global.PropertyBySelector(propertySelector);
            return DateTimeWithLabel(label ?? propertyModel.DisplayName, propertyModel.JsVariableForBinding(), format, preserve, stepping, labelCols, inputCols);
        }

        public static HtmlString InputWithLabelFor<T>(Expression<Func<T, DateTimeOffset>> propertySelector,
            string format = "M/D/YYYY", int? labelCols = null, int? inputCols = null, string label = null, DateTimePreservationOptions preserve = DateTimePreservationOptions.None, int stepping = 1)
        {
            PropertyViewModel propertyModel = ReflectionRepository.Global.PropertyBySelector(propertySelector);
            return DateTimeWithLabel(label ?? propertyModel.DisplayName, propertyModel.JsVariableForBinding(), format, preserve, stepping, labelCols, inputCols);
        }

        public static HtmlString InputWithLabelFor<T>(Expression<Func<T, DateTime?>> propertySelector,
            string format = "M/D/YYYY", int? labelCols = null, int? inputCols = null, string label = null, DateTimePreservationOptions preserve = DateTimePreservationOptions.None, int stepping = 1)
        {
            PropertyViewModel propertyModel = ReflectionRepository.Global.PropertyBySelector(propertySelector);
            return DateTimeWithLabel(label ?? propertyModel.DisplayName, propertyModel.JsVariableForBinding(), format, preserve, stepping, labelCols, inputCols);
        }

        public static HtmlString InputWithLabelFor<T>(Expression<Func<T, DateTime>> propertySelector,
            string format = "M/D/YYYY", int? labelCols = null, int? inputCols = null, string label = null, DateTimePreservationOptions preserve = DateTimePreservationOptions.None, int stepping = 1)
        {
            PropertyViewModel propertyModel = ReflectionRepository.Global.PropertyBySelector(propertySelector);
            return DateTimeWithLabel(label ?? propertyModel.DisplayName, propertyModel.JsVariableForBinding(), format, preserve, stepping, labelCols, inputCols);
        }


        #endregion

        #region TextArea
        public static HtmlString TextArea(
            string bindingValue, string bindingName = "value", int? rows = 4)
        {
            string result = $@"
                <textarea class=""form-control"" data-bind=""{bindingName}: {bindingValue}"" rows=""{rows.ToString()}""></textarea>";
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
            PropertyViewModel propertyModel = ReflectionRepository.Global.PropertyBySelector(propertySelector);
            return TextArea(propertyModel.JsVariableForBinding(), bindingName, rows);
        }


        public static HtmlString TextAreaWithLabelFor<T>(Expression<Func<T, string>> propertySelector,
            int? rows = null, int? labelCols = null, int? inputCols = null, string label = null, string bindingName = "value")
        {
            PropertyViewModel propertyModel = ReflectionRepository.Global.PropertyBySelector(propertySelector);
            return TextAreaFor(propertySelector, rows).AddLabel(label ?? propertyModel.DisplayName, labelCols, inputCols);
        }

        #endregion

        #region File
        public static HtmlString FileUploadButton(
            string bindingValue, string bindingName = "fileUpload")
        {
            return new HtmlString($@"
                <button class=""btn btn-default coalesce-upload-button"" data-bind=""{bindingName}: {bindingValue}"">
                    <i class=""fa fa-upload""></i>
                </button>
                ");
        }

        public static HtmlString FileUpload(
   string bindingValue, string bindingName = "fileUpload")
        {
            return new HtmlString($@"
                <i type=""file"" class=""fa fa-upload"" data-bind=""{bindingName}: {bindingValue}""></i>
                ");
        }
        #endregion

        #region TextInput

        public static HtmlString TextInput(
            string bindingValue, string bindingName = "value")
        {
            return new HtmlString($@"
                <input type = ""text"" class=""form-control"" data-bind=""{bindingName}: {bindingValue}"" />
                ");
        }

        public static HtmlString TextInputWithLabel(
            string label, string bindingValue, string bindingName = "value", int? labelCols = null, int? textCols = null)
        {
            return TextInput(bindingValue, bindingName).AddLabel(label, labelCols, textCols);
        }

        public static HtmlString InputWithLabelFor<T>(Expression<Func<T, string>> propertySelector,
            int? labelCols = null, int? inputCols = null, string label = null, string bindingName = "value")
        {
            PropertyViewModel propertyModel = ReflectionRepository.Global.PropertyBySelector(propertySelector);
            return InputFor(propertySelector, bindingName).AddLabel(label ?? propertyModel.DisplayName, labelCols, inputCols);
        }

        public static HtmlString InputFor<T>(Expression<Func<T, string>> propertySelector,
            string bindingName = "value")
        {
            PropertyViewModel propertyModel = ReflectionRepository.Global.PropertyBySelector(propertySelector);
            return TextInput(propertyModel.JsVariableForBinding(), bindingName);
        }
        #endregion

        #region Int
        public static HtmlString InputWithLabelFor<T>(Expression<Func<T, int>> propertySelector,
            int? labelCols = null, int? inputCols = null, string label = null, string bindingName = "value")
        {
            PropertyViewModel propertyModel = ReflectionRepository.Global.PropertyBySelector(propertySelector);
            return InputFor(propertySelector).AddLabel(label ?? propertyModel.DisplayName, labelCols, inputCols);
        }

        public static HtmlString InputFor<T>(Expression<Func<T, int>> propertySelector,
            string bindingName = "value")
        {
            PropertyViewModel propertyModel = ReflectionRepository.Global.PropertyBySelector(propertySelector);
            return TextInput(propertyModel.JsVariableForBinding(), bindingName);
        }

        public static HtmlString InputWithLabelFor<T>(Expression<Func<T, int?>> propertySelector,
    int? labelCols = null, int? inputCols = null, string label = null, string bindingName = "value")
        {
            PropertyViewModel propertyModel = ReflectionRepository.Global.PropertyBySelector(propertySelector);
            return InputFor(propertySelector).AddLabel(label ?? propertyModel.DisplayName, labelCols, inputCols);
        }

        public static HtmlString InputFor<T>(Expression<Func<T, int?>> propertySelector,
            string bindingName = "value")
        {
            PropertyViewModel propertyModel = ReflectionRepository.Global.PropertyBySelector(propertySelector);
            return TextInput(propertyModel.JsVariableForBinding(), bindingName);
        }
        #endregion

        #region boolean
        public static HtmlString Checkbox(
            string bindingValue, string bindingName = "checked")
        {
            return new HtmlString($@"
                        <input type = ""checkbox"" data-bind=""{bindingName}: {bindingValue}"" />
                        ");
        }

        public static HtmlString BooleanSelect(string bindingValue, string trueText, string falseText)
        {
            return new HtmlString($@"<select class=""form-control"" data-bind=""booleanValue: {bindingValue}"" tabindex=""-1"" aria-hidden=""true"">
                        <option value=""true"">{trueText}</option>
                        <option value=""false"">{falseText}</option></select>");
        }

        public static HtmlString InputFor<T>(Expression<Func<T, bool>> propertySelector, string bindingName = "checked")
        {
            PropertyViewModel propertyModel = ReflectionRepository.Global.PropertyBySelector(propertySelector);
            return Checkbox(propertyModel.JsVariableForBinding(), bindingName);
        }

        public static HtmlString InputWithLabelFor<T>(Expression<Func<T, bool>> propertySelector,
            int? labelCols = null, int? inputCols = null, string label = null, string bindingName = "checked")
        {
            PropertyViewModel propertyModel = ReflectionRepository.Global.PropertyBySelector(propertySelector);
            return InputFor<T>(propertySelector, bindingName).AddLabel(label ?? propertyModel.DisplayName, labelCols, inputCols);
        }

        public static HtmlString InputFor<T>(Expression<Func<T, bool?>> propertySelector, string bindingName = "checked")
        {
            PropertyViewModel propertyModel = ReflectionRepository.Global.PropertyBySelector(propertySelector);
            return Checkbox(propertyModel.JsVariableForBinding(), bindingName);
        }

        public static HtmlString InputWithLabelFor<T>(Expression<Func<T, bool?>> propertySelector,
            int? labelCols = null, int? inputCols = null, string label = null, string bindingName = "checked")
        {
            PropertyViewModel propertyModel = ReflectionRepository.Global.PropertyBySelector(propertySelector);
            return InputFor<T>(propertySelector, bindingName).AddLabel(label ?? propertyModel.DisplayName, labelCols, inputCols);
        }

        public static HtmlString BooleanValueFor<T>(Expression<Func<T, bool>> propertySelector, string trueText = "Yes", string falseText = "No")
        {
            PropertyViewModel propertyModel = ReflectionRepository.Global.PropertyBySelector(propertySelector);
            return BooleanSelect(propertyModel.JsVariableForBinding(), trueText, falseText);
        }

        public static HtmlString BooleanValueFor<T>(Expression<Func<T, bool?>> propertySelector, string trueText = "Yes", string falseText = "No")
        {
            PropertyViewModel propertyModel = ReflectionRepository.Global.PropertyBySelector(propertySelector);
            return BooleanSelect(propertyModel.JsVariableForBinding(), trueText, falseText);
        }

        #endregion

        #region StringSelect

        public static HtmlString SelectFor<T>(Expression<Func<T, string>> propertySelector,
            string endpointName = null,
            string placeholder = "")
        {
            PropertyViewModel propertyModel = ReflectionRepository.Global.PropertyBySelector(propertySelector);
            placeholder = placeholder ?? propertyModel.DisplayName;
            return SelectString(propertyModel, endpointName: endpointName, placeholder: placeholder);
        }

        public static HtmlString SelectWithLabelFor<T>(Expression<Func<T, string>> propertySelector,
            string endpointActionName = null,
            int? labelCols = null, int? inputCols = null, string label = null, string placeholder = "")
        {
            PropertyViewModel propertyModel = ReflectionRepository.Global.PropertyBySelector(propertySelector);
            return SelectFor<T>(propertySelector, endpointActionName, placeholder: placeholder)
                .AddLabel(label ?? propertyModel.DisplayName, labelCols, inputCols);
        }

        public static HtmlString SelectString(PropertyViewModel propertyModel, string endpointName = null, string placeholder = "")
        {
            string resultField = "";
            if (endpointName == null)
            {
                endpointName = $"List?fields={propertyModel.JsonName}";
                resultField = $"resultField: '{propertyModel.JsonName}',";
            }

            return new HtmlString($@"
                    <select class=""form-control"" placeholder=""{placeholder}""
                        data-bind=""select2AjaxText: {propertyModel.JsVariableForBinding()}, {resultField}url: coalesceConfig.baseApiUrl() + '/{propertyModel.Parent.ApiRouteControllerPart}/{endpointName}'"">
                        <option></option>
                    </select >");
        }

        #endregion

        #region SelectObject
        public static HtmlString SelectWithLabelForObject<T>(Expression<Func<T, object>> propertySelector,
            int? labelCols = null, int? inputCols = null, string label = null, string placeholder = "")
        {
            PropertyViewModel propertyModel = ReflectionRepository.Global.PropertyBySelector(propertySelector);
            return SelectForObject<T>(propertySelector, placeholder).AddLabel(label ?? propertyModel.DisplayName, labelCols, inputCols);
        }

        public static HtmlString SelectForObject<T>(Expression<Func<T, object>> propertySelector,
            string placeholder = "", string prefix = "")
        {

            PropertyViewModel propertyModel = ReflectionRepository.Global.PropertyBySelector(propertySelector);
            return propertyModel != null ? SelectObject(propertyModel, placeholder, prefix, !propertyModel.ForeignKeyProperty?.IsRequired ?? !propertyModel.IsRequired) : HtmlString.Empty;
        }

        public static HtmlString SelectObject(PropertyViewModel propertyModel, string placeholder = "", string prefix = "", bool allowsClear = true)
        {

            if (prefix == "" && propertyModel.JsVariableIsReserved)
            {
                // This fixes bug #7799: in cases where the object that we're binding to is a js reserved word,
                // we need to prefix it with $data, which is used by knockout internally as follows.
                // I also made changes to a whole ton of other spots so that the new JsVariableForBinding() is used where appropriate for knockout bindings.
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

            if (propertyModel.HasAttribute<SelectFilterAttribute>())
            {
                string foreignPropName = propertyModel.GetAttributeValue<SelectFilterAttribute>(a => a.ForeignPropertyName);
                string localValue = propertyModel.GetAttributeValue<SelectFilterAttribute>(a => a.StaticPropertyValue);

                PropertyViewModel foreignProp = propertyModel.Object.PropertyByName(foreignPropName);
                if (localValue != null)
                {
                    filterString = $"?filter.{foreignProp.JsVariable}={System.Net.WebUtility.UrlEncode(localValue)}";
                }
                else
                {
                    string localPropName = propertyModel.GetAttributeValue<SelectFilterAttribute>(a => a.LocalPropertyName);
                    string localPropObjName = propertyModel.GetAttributeValue<SelectFilterAttribute>(a => a.LocalPropertyObjectName);


                    if (localPropObjName != null)
                    {
                        PropertyViewModel localPropObj = propertyModel.Parent.PropertyByName(localPropObjName);
                        PropertyViewModel localProp = localPropObj.PureType.ClassViewModel.PropertyByName(localPropName ?? foreignPropName);

                        filterString = $"?filter.{foreignProp.JsVariable}=' + ({localPropObj.JsVariableForBinding()}() ? {localPropObj.JsVariableForBinding()}().{localProp.JsVariable}() : 'null') + '";
                    }
                    else
                    {
                        PropertyViewModel localProp = propertyModel.Parent.PropertyByName(localPropName ?? foreignPropName);
                        filterString = $"?filter.{foreignProp.JsVariable}=' + {localProp.JsVariableForBinding()}() + '";
                    }
                }
            }

            string result = string.Format(@"
                    <select class=""form-control"" 
                        data-bind=""select2Ajax: {6}{0}, url: function() {{ return '/api/{1}/list{8}' }}, idField: '{2}', textField: '{3}', object: {6}{4}, allowClear: {7}"" 
                        placeholder=""{5}"">
                            <option>{5}</option>
                    </select >
                    ",
                /*0*/ propertyModel.ForeignKeyProperty.JsVariable,
                /*1*/ propertyModel.PureType.ClassViewModel.ApiControllerClassName.Replace("Controller", ""),
                /*2*/ propertyModel.Object.PrimaryKey.Name,
                /*3*/ propertyModel.Object.ListTextProperty.Name,
                /*4*/ propertyModel.JsVariable,
                /*5*/ placeholder,
                /*6*/ prefix,
                /*7*/ allowsClear.ToString().ToLowerInvariant(),
                /*8*/ filterString);

            return new HtmlString(result);
        }

        public static HtmlString SelectForManyToMany<T>(Expression<Func<T, object>> propertySelector, string placeholder = "", string prefix = "", string areaName = "", int pageSize = 25)
        {
            PropertyViewModel propertyModel = ReflectionRepository.Global.PropertyBySelector(propertySelector);
            if (propertyModel != null)
            {
                return SelectForManyToMany(propertyModel, placeholder, prefix, areaName, pageSize);
            }
            return HtmlString.Empty;
        }

        public static HtmlString SelectForManyToMany(PropertyViewModel propertyModel, string placeholder = "", string prefix = "", string areaName = "", int pageSize = 25)
        {
            if (string.IsNullOrWhiteSpace(propertyModel.ManyToManyCollectionName))
            {
                throw new ArgumentException($"Property {propertyModel.Name} is not marked as [ManyToMany], or has no name specified for the attribute.");
            }

            string result = string.Format(@"
                <select data-bind = ""select2AjaxMultiple: {0}{1}, itemViewModel: {6}ViewModels.{2}, 
                    idField: '{3}', textField: '{4}', url: '/api/{5}/list?includes=none', pageSize: '{7}',""
                    class=""form-control"" multiple=""multiple"">
                </select>",
                prefix,
                propertyModel.ManyToManyCollectionName.ToCamelCase(),
                propertyModel.ManyToManyCollectionProperty.Object.ViewModelClassName,
                propertyModel.ManyToManyCollectionProperty.Object.PrimaryKey.Name,
                propertyModel.ManyToManyCollectionProperty.Object.ListTextProperty.Name,
                propertyModel.ManyToManyCollectionProperty.Object.ApiRouteControllerPart,
                (string.IsNullOrWhiteSpace(areaName) ? "" : $"{areaName}."),
                pageSize
                );
            return new HtmlString(result);
        }
        #endregion


        public static HtmlString ModalFor<T>(string methodName, bool? isStatic = null, string elementId = null, bool includeWithBinding = true)
        {
            ClassViewModel classModel = ReflectionRepository.Global.GetClassViewModel<T>();
            MethodViewModel method = classModel.ClientMethods.FirstOrDefault(f => (isStatic == null || isStatic == f.IsStatic) && f.Name == methodName);
            return ModalFor(method, elementId, includeWithBinding);
        }

        public static HtmlString ModalFor(MethodViewModel method, string elementId = null, bool includeWithBinding = true)
        {
            if (elementId == null)
            {
                elementId = $"method-{method.Name}";
            }

            var b = new HtmlCodeBuilder();
            b.Line($"<!-- Modal for method: {method.Name} -->");
            string withBinding = includeWithBinding ? $"with: {method.JsVariable}" : null;

            using (b
                .TagBlock("div", new { @class = "modal fade", id = elementId, tabindex = -1, role = "dialog", data_bind = withBinding })
                .TagBlock("div", "modal-dialog")
                .TagBlock("div", "modal-content")
            )
            {
                using (b.TagBlock("div", "modal-header"))
                {
                    b.Line("<button type='button' class='close' data-dismiss='modal' aria-label='Close'><span aria-hidden='true'>&times;</span></button>");
                    b.Line($"<h4 class='modal-title'>{method.Name.ToProperCase()}</h4>");
                }

                using (b.TagBlock("div", "modal-body form-horizontal", "with: args"))
                {
                    foreach (ParameterViewModel arg in method.ClientParameters)
                    {
                        using (b.TagBlock("div", "form-group"))
                        {
                            b.Line($"<label class='col-md-4 control-label'>{arg.Name.ToProperCase()}</label>");
                            using (b.TagBlock("div", "col-md-8"))
                            {
                                b.Line($"<input type='text' class='form-control' data-bind='value: {arg.JsVariable}'>");
                            }
                        }
                    }
                }

                using (b.TagBlock("div", "modal-footer"))
                {
                    b.Line("<button type='button' class='btn btn-default' data-dismiss='modal'>Cancel</button>");
                    b.Line(@"<button type='button' class='btn btn-primary btn-ok'");
                    b.Indented(@"data-bind=""click: invokeWithArgs.bind(this, args, function(){jQuery($element).closest('.modal').modal('hide')}, null)"">");
                    b.Indented("OK");
                    b.Line("</button>");
                }
            }

            return new HtmlString(b.ToString());
        }


        public static HtmlString InputFor<T>(Expression<Func<T, object>> propertySelector,
            string bindingName = "value")
        {
            PropertyViewModel propertyModel = ReflectionRepository.Global.PropertyBySelector(propertySelector);
            return TextInput(propertyModel.JsVariableForBinding(), bindingName);
        }

        public static HtmlString InputFor<T>(Expression<Func<T, byte[]>> propertySelector,
            string bindingName = "fileUpload")
        {
            PropertyViewModel propertyModel = ReflectionRepository.Global.PropertyBySelector(propertySelector);
            return new HtmlString(DisplayFile(propertyModel).ToString() + FileUploadButton(propertyModel.JsVariableUrl, bindingName).ToString());
        }

        public static HtmlString SelectWithLabelFor<T>(Expression<Func<T, Enum>> propertySelector,
            string placeholder = "", int? labelCols = null, int? inputCols = null, string label = null)
        {
            PropertyViewModel propertyModel = ReflectionRepository.Global.PropertyBySelector(propertySelector);
            return SelectFor(propertySelector, placeholder).AddLabel(label ?? propertyModel.DisplayName, labelCols, inputCols);
        }

        public static HtmlString SelectFor<T>(Expression<Func<T, Enum>> propertySelector,
            string placeholder = "")
        {
            PropertyViewModel propertyModel = ReflectionRepository.Global.PropertyBySelector(propertySelector);
            return SelectEnum(propertyModel, placeholder);
        }

        public static HtmlString SelectEnum(PropertyViewModel propertyModel, string placeholder = "")
        {
            var sb = new StringBuilder();
            sb.AppendLine($@"
                <select class=""form-control"" 
                    data-bind=""select2: {propertyModel.JsVariableForBinding()}"" 
                    placeholder=""{placeholder}"">");

            foreach (KeyValuePair<int, string> item in propertyModel.Type.EnumValues)
            {
                sb.AppendLine($@"<option value=""{item.Key}"">{item.Value.ToProperCase()}</option>");
            }
            sb.AppendLine("</select>");
            return new HtmlString(sb.ToString());
        }



        public static HtmlString ExpandButton(double size = 2)
        {
            string result = $@"
                <div class=""pull-right"" data-bind=""click: toggleIsExpanded"" style=""font-size: {size}em; cursor:pointer;"">
                         <i class=""fa fa-minus-circle"" data-bind=""visible: isExpanded()""></i>
                    <i class=""fa fa-plus-circle"" data-bind=""visible: !isExpanded()""></i>
                </div>";

            return new HtmlString(result);
        }

        public static HtmlString ModifiedOnFromNow(double size = 1, string format = "M/D/YY")
        {
            string result = $@"
                <div class=""modified-on"" style=""font-size: {size}em;"">
                    <span data-bind=""moment: modifiedOn().fromNow(), format: '{format}'""></span>
                </div>";

            return new HtmlString(result);
        }

        public static HtmlString ModifiedOn(double size = 1, string format = "M/D/YY")
        {
            string result = $@"
                <div class=""modified-on"" style=""font-size: {size}em;"">
                    <span data-bind=""moment: modifiedOn, format: '{format}'""></span>
                </div>";

            return new HtmlString(result);
        }


        #region Display
        public static HtmlString DisplayDateTime(string bindingValue, string format = null)
        {
            if (format == null) format = DefaultDateTimeFormat;
            return new HtmlString($@"
                <div class=""form-control-static"" data-bind=""moment: {bindingValue}, format: '{format}'"" ></div>");
        }

        public static HtmlString DisplayFor<T>(Expression<Func<T, DateTimeOffset>> propertySelector,
            string format = null, DateTimePreservationOptions preserve = DateTimePreservationOptions.None)
        {
            PropertyViewModel propertyModel = ReflectionRepository.Global.PropertyBySelector(propertySelector);
            HtmlString returnString;
            switch (preserve)
            {
                case (DateTimePreservationOptions.Date):
                    returnString = DisplayDate(propertyModel.JsVariableForBinding(), format);
                    break;
                case (DateTimePreservationOptions.Time):
                    returnString = DisplayTime(propertyModel.JsVariableForBinding(), format);
                    break;
                default:
                    returnString = DisplayDateTime(propertyModel.JsVariableForBinding(), format);
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
            string result = $@"
                <div data-bind = ""foreach: {propertyModel.ManyToManyCollectionName.ToCamelCase()} "">
                    <div class=""form-control-static"" data-bind=""text: {propertyModel.ManyToManyCollectionProperty.Object.ListTextProperty.JsVariableForBinding()}""></div>
                </div>";

            return new HtmlString(result);
        }

        public static HtmlString DisplayCheckbox(string bindingValue)
        {
            return new HtmlString($@"
                <input type=""checkbox"" disabled data-bind=""checked: {bindingValue}"" />");
        }

        public static HtmlString DisplayFor<T>(Expression<Func<T, object>> propertySelector, bool linkObject = false)
        {
            PropertyViewModel propertyModel = ReflectionRepository.Global.PropertyBySelector(propertySelector);
            HtmlString returnString;

            if (propertyModel.IsDateOnly)
            {
                returnString = DisplayDate(propertyModel.JsVariableForBinding());
            }
            else if (propertyModel.Type.IsDate)
            {
                returnString = DisplayDateTime(propertyModel.JsVariableForBinding());
            }
            else if (propertyModel.IsManytoManyCollection)
            {
                returnString = DisplayManyToMany(propertyModel);
            }
            else if (propertyModel.Type.IsBool)
            {
                returnString = DisplayCheckbox(propertyModel.JsVariableForBinding());
            }
            else if (propertyModel.IsPOCO)
            {
                returnString = DisplayObject(propertyModel, linkObject);
            }
            else if (propertyModel.Type.IsEnum)
            {
                returnString = DisplayEnum(propertyModel);
            }
            else if (propertyModel.IsFile)
            {
                returnString = DisplayFile(propertyModel);
            }
            else
            {
                returnString = DisplayText(propertyModel.JsVariableForBinding());
            }

            return returnString;
        }

        public static HtmlString DisplayObject(PropertyViewModel propertyModel, bool linkObject = false)
        {
            var sb = new StringBuilder();
            sb.AppendLine($@"<div data-bind=""if: {propertyModel.JsVariableForBinding()}()"">");
            if (linkObject && propertyModel.PureTypeOnContext)
            {
                sb.AppendLine(
                    $@"  <a class=""form-control-static"" data-bind=""attr: {{href: {propertyModel.JsVariableForBinding()}().editUrl()}}, text: {propertyModel.JsVariableForBinding()}().{propertyModel.Object.ListTextProperty.JsVariable}""></a>");
            }
            else
            {
                sb.AppendLine(
                    $@"  <div class=""form-control-static"" data-bind=""text: {propertyModel.JsVariableForBinding()}().{propertyModel.Object.ListTextProperty.JsVariable}""></div>");
            }

            sb.AppendLine(@"</div>");
            sb.AppendLine($@"<div data-bind=""if: !{propertyModel.JsVariableForBinding()}()"">
                    <div class=""form-control-static"">None</div>
                </div>");

            return new HtmlString(sb.ToString());
        }

        public static HtmlString DisplayEnum(PropertyViewModel propertyModel)
        {
            return DisplayText(propertyModel.JsTextPropertyNameForBinding());
        }

        public static HtmlString DisplayText(string bindingValue)
        {
            return new HtmlString($@"
                <div class=""form-control-static"" data-bind=""text: {bindingValue}()""></div>");
        }

        public static HtmlString DisplayHtml<T>(Expression<Func<T, object>> propertySelector)
        {
            PropertyViewModel propertyModel = ReflectionRepository.Global.PropertyBySelector(propertySelector);

            return new HtmlString($@"
                <div class=""form-control-static"" data-bind=""html: {propertyModel.JsVariableForBinding()}()""></div>");
        }

        public static HtmlString DisplayComment(string bindingValue)
        {
            return new HtmlString($@"
                <!-- Attempted to Display: {bindingValue} -->");
        }

        public static HtmlString DisplayFile(PropertyViewModel propertyModel)
        {
            var mimeType = propertyModel.GetAttributeValue<FileAttribute>(f => f.MimeType);
            if (mimeType != null && mimeType.ToLower().Contains("image"))
            {
                return DisplayFileImage(propertyModel);
            }
            else
            {
                return DisplayFileDownloadButton(propertyModel);
            }
        }
        public static HtmlString DisplayFileImage(PropertyViewModel propertyModel)
        {
            return new HtmlString($@"
                <img class=""form-control-static"" data-bind=""attr: {{src: {propertyModel.JsVariableUrl}}}"" />");
        }

        public static HtmlString DisplayFileDownloadButton(PropertyViewModel propertyModel)
        {
            return DisplayFileDownload(propertyModel, "btn btn-default coalesce-download-button");
        }

        public static HtmlString DisplayFileDownload(PropertyViewModel propertyModel, string classes)
        {
            var filenameVariable = "'download'";
            if (propertyModel.HasFileFilenameProperty) filenameVariable = $"{propertyModel.FileFilenameProperty.Name}";
            return new HtmlString($@"
                <a href=""#"" class=""{classes}"" data-bind=""attr: {{href: {propertyModel.JsVariableUrl}, download: {filenameVariable.ToCamelCase()}}}""><i class=""fa fa-download""></i></a>");
        }




        #endregion

    }
}
