using System.Text;
using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.AspNetCore.Html;

namespace IntelliTect.Coalesce.Knockout.Helpers
{
    public static class ScriptHelper
    {
        public static HtmlString StandardBinding<T>()
        {
            return StandardBinding(ReflectionRepository.Global.GetClassViewModel<T>());
        }

        public static HtmlString StandardBinding(ClassViewModel model)
        {
            var html = new StringBuilder();

            html.AppendLine($@"
    <script>
        @if (!ViewBag.Editable)
        {{
            @:Coalesce.GlobalConfiguration.viewModel.setupValidationAutomatically(false);
        }}
        var {model.ListViewModelObjectName} = new ListViewModels.{model.ListViewModelClassName}();
        
        // Set up parent info based on the URL.
        @if (ViewBag.Query != null)
        {{
            @:{model.ListViewModelObjectName}.queryString = ""@(ViewBag.Query)"";
        }}

        // Save and restore values from the URL:
        var urlVariables = ['page', 'pageSize', 'search', 'orderBy', 'orderByDescending'];
        $.each(urlVariables, function(){{
            var param = Coalesce.Utilities.GetUrlParameter(this);
            if (param) {{{model.ListViewModelObjectName}[this](param);}}
        }})
        { model.ListViewModelObjectName}.isLoading.subscribe(function(){{
            var newUrl = window.location.href;
            
            $.each(urlVariables, function(){{
                var param = {model.ListViewModelObjectName}[this]();
                newUrl = Coalesce.Utilities.SetUrlParameter(newUrl, this, param);
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
