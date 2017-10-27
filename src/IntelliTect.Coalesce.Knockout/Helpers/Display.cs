using Microsoft.AspNetCore.Html;
using System;
using System.Linq.Expressions;
using IntelliTect.Coalesce.TypeDefinition;
using IntelliTect.Coalesce.Knockout.TypeDefinition;

namespace IntelliTect.Coalesce.Knockout.Helpers
{
    public static class Display
    {
        public static HtmlString Property<T, TProp>(Expression<Func<T, TProp>> propertySelector, bool editable)
        {
            var propertyModel = ReflectionRepository.PropertyBySelector(propertySelector);
            return Property(propertySelector, editable);
        }

        public static HtmlString Property(PropertyViewModel prop, bool editable)
        {
            if (prop.Type.IsCollection && !prop.IsManytoManyCollection)
            {
                var result = @"<a data-bind = 'attr: {href: " + prop.ListEditorUrlName + @"}, text: " + prop.JsVariableForBinding() + @"().length + "" - Edit""' class='btn btn-default btn-sm'></a>";
                return new HtmlString(result);
            }
            else if (editable && prop.CanWrite && !prop.IsInternalUse)
            {
                if (prop.Type.IsDate)
                {
                    return Knockout.DateTime(prop.JsVariableForBinding(), prop.DateFormat);
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
                else if (prop.ListGroup != null)
                {
                    return Knockout.SelectString(prop);
                }
                else
                {
                    return Knockout.TextInput(prop.JsVariableForBinding());
                }
            }
            else
            {
                if (prop.IsDateOnly)
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
                else if (prop.IsPOCO && !prop.IsComplexType)
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
            // If in doubt do nothing. But put a comment in
            //return Knockout.DisplayComment(prop.JsVariable);
        }

        public static string PropertyHelper(PropertyViewModel prop, bool editable, string areaName = "", bool objectLink = false)
        {
            if (prop.Type.IsCollection && !prop.IsManytoManyCollection)
            {
                if (prop.PureTypeOnContext)
                {
                    return @"<a data-bind='attr: {href: " + prop.ListEditorUrlName + @"}, text: " + prop.JsVariableForBinding() + @"().length + "" - Edit""' class='btn btn-default btn-sm'></a>";
                }

                return @"<div class='form-control-static' style='font-family: monospace; white-space: nowrap' data-bind='text: " + prop.JsVariableForBinding() + @"().length + "" Items""' ></div>";
            }
            else
            {
                if (editable)
                {
                    if ((prop.Type.IsNullable && prop.PureType.IsEnum) || prop.Type.IsEnum)
                    {
                        return $"@(Knockout.SelectFor<{prop.Parent.FullName}>(p => p.{prop.Name}))";
                    }
                    else if (prop.HasValidValues)
                    {
                        if (prop.IsManytoManyCollection)
                        {
                            if (!string.IsNullOrWhiteSpace(areaName))
                            {
                                return $"@(Knockout.SelectForManyToMany<{prop.Parent.FullName}>(p => p.{prop.Name}, areaName: \"StokesTest\"))";
                            }
                            else
                            {
                                return $"@(Knockout.SelectForManyToMany<{prop.Parent.FullName}>(p => p.{prop.Name}))";
                            }
                            //return Knockout.SelectForManyToMany(prop);
                        }
                        else
                        {
                            return $"@(Knockout.SelectForObject<{prop.Parent.FullName}>(p => p.{prop.Name}))";
                            //return Knockout.SelectObject(prop);
                        }

                    }
                    else if (prop.ListGroup != null)
                    {
                        return $"@(Knockout.SelectFor<{prop.Parent.FullName}>(p => p.{prop.Name}))";
                        //return Knockout.SelectString(prop);
                    }
                    else
                    {
                        return $"@(Knockout.InputFor<{prop.Parent.FullName}>(p => p.{prop.Name}))";
                    }
                }
                else
                {
                    return $"@(Knockout.DisplayFor<{prop.Parent.FullName}>(p => p.{prop.Name}, {objectLink.ToString().ToLower()}))";
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
            return $@"<a href=""#"" data-bind = 'click: {method.JsVariableUi}'>{method.DisplayName}</a>";
        }

        public static string PropertyHelperWithSurroundingDiv(PropertyViewModel prop, bool editable, string areaName = "", int cols = 8)
        {
            var propertyHelper = PropertyHelper(prop, editable, areaName);
            return $"<div class=\"col-md-{cols} prop-{prop.JsonName}\">{propertyHelper}</div>";
        }
    }
}
