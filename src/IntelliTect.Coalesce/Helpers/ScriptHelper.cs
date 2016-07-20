using Microsoft.AspNetCore.Html;
using System.Text;
using IntelliTect.Coalesce.TypeDefinition;

namespace IntelliTect.Coalesce.Helpers
{
    public static class ScriptHelper
    {
        public static HtmlString StandardBinding<T>()
        {
            return StandardBinding(ReflectionRepository.GetClassViewModel<T>());
        }

        public static HtmlString StandardBinding(ClassViewModel model)
        {
            var html = new StringBuilder();

            html.AppendLine($@"
    <script>
        var {model.ListViewModelObjectName} = new ListViewModels.{model.ListViewModelClassName}();
        
        // Set up parent info based on the URL.
        @if (ViewBag.Query != null)
        {{
            @:{model.ListViewModelObjectName}.queryString = ""@(ViewBag.Query)"";
        }}

        // Save and restore values from the URL:
        var urlVariables = ['page', 'pageSize', 'search'];
        $.each(urlVariables, function(){{
            var param = intellitect.utilities.GetUrlParameter(this);
            if (param) {{{model.ListViewModelObjectName}[this](param);}}
        }})
        { model.ListViewModelObjectName}.isLoading.subscribe(function(){{
            var newUrl = window.location.href;
            
            $.each(urlVariables, function(){{
                var param = {model.ListViewModelObjectName}[this]();
                newUrl = intellitect.utilities.SetUrlParameter(newUrl, this, param);
            }})
            history.replaceState(null, document.title, newUrl);
        }});

        { model.ListViewModelObjectName}.isSavingAutomatically = false;
        ko.applyBindings({model.ListViewModelObjectName});
        {model.ListViewModelObjectName}.isSavingAutomatically = true;

        {model.ListViewModelObjectName}.includes = ""{model.ListViewModelClassName}Gen"";
        { model.ListViewModelObjectName}.load();

    </script>");

            return new HtmlString(html.ToString());
        }
    }
}
