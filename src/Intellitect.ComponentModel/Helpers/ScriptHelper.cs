using Intellitect.ComponentModel.Models;
using Intellitect.ComponentModel.TypeDefinition;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intellitect.ComponentModel.Helpers
{
    public static class ScriptHelper
    {
        public static HtmlString StandardBinding<T>()
        {
            var model = ReflectionRepository.GetClassViewModel<T>();
            var html = new StringBuilder();

            html.AppendLine($@"
    <script>
        var {model.ListViewModelObjectName} = new ListViewModels.{model.ListViewModelClassName}();
        
        // Set up parent info based on the URL.
        {model.ListViewModelObjectName}.queryString = """";  // TODO: Finish this

        {model.ListViewModelObjectName}.isSavingAutomatically = false;
        ko.applyBindings({model.ListViewModelObjectName});
        {model.ListViewModelObjectName}.isSavingAutomatically = true;

        {model.ListViewModelObjectName}.includes = ""Editor"";
        { model.ListViewModelObjectName}.load();

    </script>");

            return new HtmlString(html.ToString());
        }
    }
}
