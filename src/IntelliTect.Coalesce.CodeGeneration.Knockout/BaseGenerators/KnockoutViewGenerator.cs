﻿using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.Knockout.Helpers;
using IntelliTect.Coalesce.Knockout.TypeDefinition;
using IntelliTect.Coalesce.TypeDefinition;
using IntelliTect.Coalesce.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.CodeGeneration.Knockout.BaseGenerators
{
    public abstract class KnockoutViewGenerator : StringBuilderFileGenerator<ClassViewModel>
    {
        public KnockoutViewGenerator(GeneratorServices services) : base(services)
        {
            Services = services;
        }

        public GeneratorServices Services { get; }
        public GenerationContext GenerationContext => Services.GenerationContext;
        public string AreaName => GenerationContext.AreaName;
        public string ModulePrefix => GenerationContext.TypescriptModulePrefix;

        public override Task<string> BuildOutputAsync()
        {
            var b = new HtmlCodeBuilder();
            
            b.Line("@using IntelliTect.Coalesce.Knockout.Helpers");
            b.Line();

            BuildOutput(b);
            return Task.FromResult(b.ToString());
        }

        public abstract void BuildOutput(HtmlCodeBuilder b);
        

        protected void WriteAdminTableControls(HtmlCodeBuilder b, string classPrefix, Action additionalButtons = null)
        {
            using (b.TagBlock("div", $"{classPrefix}-view-header"))
            {
                using (b
                    .TagBlock("div", "clearfix")
                    .TagBlock("h1", style: "display: inline-block")
                )
                {
                    b.Line($"{Model.Name.ToProperCase()} List");
                    b.Line($"<span style=\"font-size: .5em; padding-left: 20px;\"><a href=\"~/{Model.ControllerName}/Docs\">API Docs</a></span>");
                }

                using (b.TagBlock("div", "clearfix"))
                {
                    using (b.TagBlock("div", style: "display: inline-block; font-size: 1.1em; margin-right: 10px;"))
                    {
                        b.Line("<i class=\"fa fa-arrow-circle-left\" data-bind=\"enabled: previousPageEnabled() && !isLoading(), click: previousPage\"></i>");
                        b.Line("Page");
                        b.Line("<input data-bind=\"value: page\" style=\"width: 35px\">");
                        b.Line("of");
                        b.Line("<span data-bind=\"text: pageCount\"></span>");
                        b.Line("<i class=\"fa fa-arrow-circle-right\" data-bind=\"enabled: nextPageEnabled() && !isLoading(), click: nextPage\"></i>");
                    }
                    
                    // Page size dropdown
                    using (b.TagBlock("select", "form-control", dataBind: "value: pageSize", style: "width: 100px; display: inline-block;"))
                    {
                        foreach (var n in new[] { 1, 5, 10, 50, 100, 500, 1000 })
                        {
                            b.Line($"<option value=\"{n}\">{n}</option>");
                        }
                    }

                    b.Line("<input class=\"form-control pull-right\" style=\"width: 250px; margin-left: 20px\" data-bind=\"textInput: search\" placeholder=\"Search\" />");

                    // Create / Refresh / CSV buttons
                    using (b.TagBlock("div", "btn-group pull-right"))
                    {
                        if (Model.SecurityInfo.IsCreateAllowed())
                        {
                            b.Line($"<a href=\"~/{Model.ControllerName}/CreateEdit?@(ViewBag.Query)\" role=\"button\" class=\"btn btn-sm btn-default \"><i class=\"fa fa-plus\"></i> Create</a>");
                        }
                        b.Line("<button data-bind=\"click:load\" class=\"btn btn-sm btn-default \"><i class=\"fa fa-refresh\"></i> Refresh</button>");

                        additionalButtons?.Invoke();

                        b.Line("<a href=\"#\" role=\"button\" class=\"btn btn-sm btn-default \" data-bind=\"attr:{href: downloadAllCsvUrl}\"><i class=\"fa fa-download\"></i> CSV</a>");
                        if (Model.SecurityInfo.IsCreateAllowed() && Model.SecurityInfo.IsEditAllowed())
                        {
                            b.Line("<button role=\"button\" class=\"btn btn-sm btn-default \" data-bind=\"click: csvUploadUi\"><i class=\"fa fa-upload\"></i> CSV</button>");
                        }
                    }
                }
            }
        }

        protected void WriteListViewObjectButtons(HtmlCodeBuilder b)
        {
            // Dropdown for invoking instance methods.
            WriteInstanceMethodsDropdownButton(b);

            if (Model.SecurityInfo.IsEditAllowed())
            {
                using (b.TagBlock("a", "btn btn-sm btn-default", "attr: { href: editUrl }"))
                {
                    b.Line("<i class=\"fa fa-pencil\"></i>");
                }
            }

            if (Model.SecurityInfo.IsDeleteAllowed())
            {
                using (b.TagBlock("button", "btn btn-sm btn-danger", "click: deleteItemWithConfirmation"))
                {
                    b.Line("<i class=\"fa fa-remove\"></i>");
                }
            }
        }

        protected void WriteInstanceMethodsDropdownButton(HtmlCodeBuilder b)
        {
            if (Model.ClientMethods.Any(f => !f.IsHidden(HiddenAttribute.Areas.List) && !f.IsStatic))
            {
                b.Line("<!-- Action buttons -->");

                using (b.TagBlock("div", new { @class = "btn-group", role = "group" }))
                {
                    b.Line("<button type=\"button\" class=\"btn btn-sm btn-default dropdown-toggle\" data-toggle=\"dropdown\" aria-haspopup=\"true\" aria-expanded=\"false\">");
                    b.Indented("Actions <span class=\"caret\"></span>");
                    b.Line("</button>");
                    using (b.TagBlock("ul", "dropdown-menu"))
                    {
                        foreach (var method in Model.ClientMethods.Where(f => !f.IsHidden(HiddenAttribute.Areas.List) && !f.IsStatic))
                        {
                            b.Line($"<li>{Display.MethodHelper(method)}</li>");
                        }
                    }
                }
            }
        }

        protected void WriteMethodsCard(HtmlCodeBuilder b, IEnumerable<MethodViewModel> methods)
        {
            if (!methods.Any()) return;

            using (b.TagBlock("div", "card"))
            {
                using (b.TagBlock("div", "card-heading"))
                {
                    b.Line("<h3 class=\"card-title\">Actions</h3>");
                }

                using (b.TagBlock("div", "card-body").TagBlock("table", "table"))
                {
                    using (b.TagBlock("thead").TagBlock("tr"))
                    {
                        b.Line("<th style=\"width: 20%;\">Action</th>");
                        b.Line("<th style=\"width: 50%;\">Result</th>");
                        b.Line("<th style=\"width: 20%;\">Successful</th>");
                        b.Line("<th style=\"width: 10%;\"></th>");
                    }
                    using (b.TagBlock("tbody"))
                    {
                        foreach (MethodViewModel method in methods)
                        {
                            using (b.TagBlock("tr", dataBind: $"with: {method.JsVariable}"))
                            {
                                using (b.TagBlock("td"))
                                {
                                    string clickBinding = method.ClientParameters.Any()
                                        ? $"click: function(){{ $('#method-{method.Name}').modal() }}"
                                        : $"click: function(){{ invoke() }}";

                                    using (b.TagBlock("button", "btn btn-default btn-xs", dataBind: clickBinding))
                                    {
                                        b.Line(method.DisplayName);
                                    }
                                }
                                        
                                using (b.TagBlock("td"))
                                {
                                    if (method.ResultType.IsCollection)
                                    {
                                        b.Line("<ul data-bind=\"foreach: result\">");
                                        b.Indented("<li data-bind=\"text: $data\"></li>");
                                        b.Line("</ul>");
                                    }
                                    else if (method.ResultType.HasClassViewModel)
                                    {
                                        using (b.TagBlock("dl", "dl-horizontal", dataBind: "with: result"))
                                        {
                                            foreach (var prop in method.ResultType.ClassViewModel.ClientProperties.Where(f => !f.IsHidden(HiddenAttribute.Areas.Edit)))
                                            {
                                                b.Line($"<dt>{prop.DisplayName}</dt>");
                                                b.Line($"<dd data-bind=\"text: {prop.JsVariableForBinding()}\"></dd>");
                                            }
                                        }
                                    }
                                    else
                                    {
                                        b.Line("<span data-bind=\"text: result\"></span>");
                                    }
                                }
                                using (b.TagBlock("td"))
                                {
                                    b.Line("<span data-bind=\"text: wasSuccessful\"></span>");
                                    b.Line("<span data-bind=\"text: message\"></span>");
                                }
                                using (b.TagBlock("td"))
                                {
                                    b.Line("<span class=\"label label-info\" data-bind=\"fadeVisible: isLoading\">Loading</span>");
                                }
                            }
                        }
                    }
                }
            }
        }

        protected void WriteListViewScript(HtmlCodeBuilder b)
        {
            b.Line("@section Scripts");
            b.Line("{");
            b.Line($@"
    <script>
        @if (!ViewBag.Editable)
        {{
            @:Coalesce.GlobalConfiguration.viewModel.setupValidationAutomatically(false);
        }}
        var {Model.ListViewModelObjectName} = new ListViewModels.{Model.ListViewModelClassName}();
        
        // Set up parent info based on the URL.
        @if (ViewBag.Query != null)
        {{
            @:{Model.ListViewModelObjectName}.queryString = ""@(ViewBag.Query)"";
        }}

        // Save and restore values from the URL:
        var urlVariables = ['page', 'pageSize', 'search', 'orderBy', 'orderByDescending'];
        $.each(urlVariables, function(){{
            var param = Coalesce.Utilities.GetUrlParameter(this);
            if (param) {{{Model.ListViewModelObjectName}[this](param);}}
        }})
        { Model.ListViewModelObjectName}.isLoading.subscribe(function(){{
            var newUrl = window.location.href;
            
            $.each(urlVariables, function(){{
                var param = {Model.ListViewModelObjectName}[this]();
                newUrl = Coalesce.Utilities.SetUrlParameter(newUrl, this, param);
            }})
            history.replaceState(null, document.title, newUrl);
        }});

        { Model.ListViewModelObjectName}.isSavingAutomatically = false;
        ko.applyBindings({Model.ListViewModelObjectName});
        {Model.ListViewModelObjectName}.isSavingAutomatically = true;

        {Model.ListViewModelObjectName}.includes = ""{Model.ListViewModelClassName}Gen"";
        { Model.ListViewModelObjectName}.load();

    </script>");

            b.Line("}");
        }
    }
}
