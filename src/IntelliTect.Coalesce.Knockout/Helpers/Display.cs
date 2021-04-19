using Microsoft.AspNetCore.Html;
using System;
using System.Linq.Expressions;
using IntelliTect.Coalesce.TypeDefinition;
using IntelliTect.Coalesce.Knockout.TypeDefinition;
using System.Linq;
using IntelliTect.Coalesce.Utilities;

namespace IntelliTect.Coalesce.Knockout.Helpers
{
    public static class Display
    {
        public static HtmlString Property<T, TProp>(Expression<Func<T, TProp>> propertySelector, bool editable)
        {
            var propertyModel = ReflectionRepository.Global.PropertyBySelector(propertySelector);
            return Property(propertyModel, editable);
        }

        public static HtmlString Property(PropertyViewModel prop, bool editable)
        {
            if (prop.Type.IsCollection && !prop.IsManytoManyCollection)
            {
                var result = $@"<a data-bind='attr: {{href: {prop.ListEditorUrlName()}}}, text: {prop.JsVariableForBinding()}().length + "" - Edit""' class='btn btn-default btn-sm'></a>";
                return new HtmlString(result);
            }
            else if (editable && prop.IsClientWritable && !prop.IsInternalUse)
            {
                if (prop.Type.IsDate)
                {
                    return Knockout.DateTime(prop.JsVariableForBinding(), prop.IsDateOnly ? "M/D/YYYY" : "M/D/YYYY h:mm a");
                }
                else if (prop.Type.IsEnum)
                {
                    return Knockout.SelectEnum(prop);
                }
                else if (prop.Type.IsBool)
                {
                    return Knockout.Checkbox(prop.JsVariableForBinding());
                }
                else if (prop.HasValidValues)
                {
                    if (prop.IsManytoManyCollection)
                    {
                        return Knockout.SelectForManyToMany(prop);
                    }
                    else
                    {
                        return Knockout.SelectObject(prop);
                    }
                }
                else
                {
                    return Knockout.TextInput(prop.JsVariableForBinding());
                }
            }
            else if (prop.IsDateOnly)
            {
                return Knockout.DisplayDate(prop.JsVariableForBinding());
            }
            else if (prop.Type.IsDate)
            {
                return Knockout.DisplayDateTime(prop.JsVariableForBinding());
            }
            else if (prop.IsManytoManyCollection)
            {
                return Knockout.DisplayManyToMany(prop);
            }
            else if (prop.Type.IsBool)
            {
                return Knockout.DisplayCheckbox(prop.JsVariableForBinding());
            }
            else if (prop.IsPOCO)
            {
                return Knockout.DisplayObject(prop);
            }
            else if (prop.Type.IsEnum)
            {
                return Knockout.DisplayEnum(prop);
            }
            else
            {
                return Knockout.DisplayText(prop.JsVariableForBinding());
            }
        }

        public static string PropertyHelper(PropertyViewModel prop, bool editable, string areaName = "", bool objectLink = false)
        {
            if (prop.Type.IsCollection && !prop.IsManytoManyCollection)
            {
                if (prop.PureTypeOnContext)
                {
                    return $"<a data-bind='attr: {{href: {prop.ListEditorUrlName()}}}, text: {prop.JsVariableForBinding()}().length + \" - Edit\"' class='btn btn-default btn-sm'></a>";
                }

                return $"<div class='form-control-static' style='font-family: monospace; white-space: nowrap' data-bind='text: ({prop.JsVariableForBinding()}() || []).length + \" Items\"' ></div>";
            }
            else
            {
                if (editable)
                {
                    if (prop.Type.IsEnum)
                    {
                        return $"@(Knockout.SelectFor<{prop.Parent.FullyQualifiedName}>(p => p.{prop.Name}))";
                    }
                    else if (prop.HasValidValues)
                    {
                        if (prop.IsManytoManyCollection)
                        {
                            if (!string.IsNullOrWhiteSpace(areaName))
                            {
                                return $"@(Knockout.SelectForManyToMany<{prop.Parent.FullyQualifiedName}>(p => p.{prop.Name}, areaName: \"{areaName.EscapeStringLiteralForCSharp()}\"))";
                            }
                            else
                            {
                                return $"@(Knockout.SelectForManyToMany<{prop.Parent.FullyQualifiedName}>(p => p.{prop.Name}))";
                            }
                            //return Knockout.SelectForManyToMany(prop);
                        }
                        else
                        {
                            return $"@(Knockout.SelectForObject<{prop.Parent.FullyQualifiedName}>(p => p.{prop.Name}))";
                            //return Knockout.SelectObject(prop);
                        }

                    }
                    else
                    {
                        return $"@(Knockout.InputFor<{prop.Parent.FullyQualifiedName}>(p => p.{prop.Name}))";
                    }
                }
                else
                {
                    return $"@(Knockout.DisplayFor<{prop.Parent.FullyQualifiedName}>(p => p.{prop.Name}, {objectLink.ToString().ToLower()}))";
                }
            }

        }

        /// <summary>
        /// Adds a button for a method
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public static string MethodHelper(MethodViewModel method)
        {
            var callFunction = method.ClientParameters.Any() ? "invokeWithPrompts" : "invoke";
            return $@"<a href=""#"" data-bind='click: function(){{ {method.JsVariable}.{callFunction}() }}'>{method.DisplayName}</a>";
        }

        public static string PropertyHelperWithSurroundingDiv(PropertyViewModel prop, bool editable, string areaName = "", int cols = 8)
        {
            var propertyHelper = PropertyHelper(prop, editable, areaName);
            string description = !string.IsNullOrWhiteSpace(prop.Description) ? $"<p class='help-block'>{prop.Description.EscapeForHtml()}</p>" : "";
            return $"<div class=\"col-md-{cols} prop-{prop.JsonName}\">{propertyHelper}{description}</div>";
        }
    }
}
