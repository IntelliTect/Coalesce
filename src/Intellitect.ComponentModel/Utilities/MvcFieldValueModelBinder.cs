using Microsoft.AspNet.Mvc.ModelBinding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Intellitect.ComponentModel.Utilities
{
    [ExcludeFromCodeCoverage()]
    public class MvcFieldValueModelBinder : IModelBinder
    {
        //Define original source data list
        public Task<ModelBindingResult> BindModelAsync(ModelBindingContext bindingContext)
        {
            var kvps = new List<KeyValuePair<string, string>>();
            var qs = bindingContext.OperationBindingContext.HttpContext.Request.Query;
            foreach (var key in qs)
            {
                kvps.Add(new KeyValuePair<string, string>(key.Key, key.Value));
            }
            var result = ModelBindingResult.SuccessAsync("", kvps);
            return result;
        }
    }
}
