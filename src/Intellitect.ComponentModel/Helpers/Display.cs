using Intellitect.ComponentModel.TypeDefinition;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Intellitect.ComponentModel.Helpers
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
                var result = @"<a data-bind = 'attr: {href: " + prop.ListEditorUrlName + @"}, text: " + prop.JsVariable + @"().length + "" - Edit""' class='btn btn-default btn-sm'></a>";
                return new HtmlString(result);
            }
            else if (editable && prop.CanWrite && !prop.IsInternalUse)
            {
                if (prop.Type.IsDate)
                {
                    return Knockout.DateTime(prop.JsVariable, prop.DateFormat);
                }
                else if (prop.Type.IsEnum)
                {
                    return Knockout.SelectEnum(prop);
                }
                else if (prop.Type.IsBool)
                {
                    return Knockout.Checkbox(prop.JsVariable);
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
                    return Knockout.TextInput(prop.JsVariable);
                }
            }
            else
            {
                if (prop.IsDateOnly)
                {
                    return Knockout.DisplayDate(prop.JsVariable);
                }
                else if (prop.Type.IsDate)
                {
                    return Knockout.DisplayDateTime(prop.JsVariable);
                }
                else if (prop.IsManytoManyCollection)
                {
                    return Knockout.DisplayManyToMany(prop);
                }
                else if (prop.Type.IsBool)
                {
                    return Knockout.DisplayCheckbox(prop.JsVariable);
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
                    return Knockout.DisplayText(prop.JsVariable);
                }
            }
            // If in doubt do nothing. But put a comment in
            //return Knockout.DisplayComment(prop.JsVariable);
        }

        public static string PropertyHelper(PropertyViewModel prop, bool editable, string areaName = "")
        {
            if (prop.Type.IsCollection && !prop.IsManytoManyCollection)
            {
                return @"<a data-bind = 'attr: {href: " + prop.ListEditorUrlName + @"}, text: " + prop.JsVariable + @"().length + "" - Edit""' class='btn btn-default btn-sm'></a>";
            }
            else
            {
                if (editable)
                {
                    if (prop.Type.IsEnum)
                    {
                        return $"@(Knockout.SelectFor<{prop.Parent.ViewModelClassName}>(p => p.{prop.Name}))";
                    }
                    else if (prop.HasValidValues)
                    {
                        if (prop.IsManytoManyCollection)
                        {
                            if (!string.IsNullOrWhiteSpace(areaName))
                            {
                                return $"@(Knockout.SelectForManyToMany<{prop.Parent.ViewModelClassName}>(p => p.{prop.Name}, areaName: \"StokesTest\"))";
                            }
                            else
                            {
                                return $"@(Knockout.SelectForManyToMany<{prop.Parent.ViewModelClassName}>(p => p.{prop.Name}))";
                            }
                            //return Knockout.SelectForManyToMany(prop);
                        }
                        else
                        {
                            return $"@(Knockout.SelectForObject<{prop.Parent.ViewModelClassName}>(p => p.{prop.Name}))";
                            //return Knockout.SelectObject(prop);
                        }

                    }
                    else if (prop.ListGroup != null)
                    {
                        return $"@(Knockout.SelectFor<{prop.Parent.ViewModelClassName}>(p => p.{prop.Name}))";
                        //return Knockout.SelectString(prop);
                    }
                    else
                    {
                        return $"@(Knockout.InputFor<{prop.Parent.ViewModelClassName}>(p => p.{prop.Name}))";
                    }
                }
                else
                {
                    return $"@(Knockout.DisplayFor<{prop.Parent.ViewModelClassName}>(p => p.{prop.Name}))";
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

    }
}
