﻿using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Text;
using IntelliTect.Coalesce.CodeGeneration.Knockout.BaseGenerators;
using IntelliTect.Coalesce.Utilities;
using IntelliTect.Coalesce.Knockout.Helpers;
using System.Linq;
using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.Knockout.TypeDefinition;

namespace IntelliTect.Coalesce.CodeGeneration.Knockout.Generators
{
    public class CreateEditView : KnockoutViewGenerator
    {
        public CreateEditView(GeneratorServices services) : base(services) { }
        
        public override void BuildOutput(HtmlCodeBuilder b)
        {
            using (b.TagBlock("style"))
            {
                b.Lines(
                    "img.form-control-static {",
                    "    max-width: 100px;",
                    "    max-height: 100px;",
                    "}",
                    ".coalesce-upload-icon {",
                    "    cursor: pointer;",
                    "}"
                );
            }

            string viewModelsNamespace = "ViewModels";
            if (!string.IsNullOrWhiteSpace(AreaName))
            {
                viewModelsNamespace = AreaName + "." + viewModelsNamespace;
            }
            if (!string.IsNullOrWhiteSpace(ModulePrefix))
            {
                viewModelsNamespace = ModulePrefix + "." + viewModelsNamespace;
            }

            using (b.TagBlock("div", "card"))
            {
                using (b.TagBlock("div", "card-heading"))
                {
                    using (b.TagBlock("div", "btn-group pull-right"))
                    {
                        b.Line("<button onclick=\"window.history.back()\" class=\"btn btn-xs btn-default\"><i class=\"fa fa-arrow-left\"></i> Back</button>");
                        b.Line("<button data-bind=\"click:function() { load(); }\" class=\"btn btn-xs btn-default\"><i class=\"fa fa-refresh\"></i> Refresh</button>");
                    }
                    b.Line($"<h1 class=\"card-title clearfix\" style=\"display:inline-block;\">{Model.DisplayName}</h1>");
                    b.Line("<span class=\"label label-info\" data-bind=\"fadeVisible: isLoading()\">Loading...</span>");
                }

                using (b.TagBlock("div", "card-body").TagBlock("div", "form-horizontal"))
                {
                    using (b.TagBlock("div", "form-group btn-warning", dataBind: "if: errorMessage(), visible: errorMessage()", style: "display: none"))
                    {
                        b.Line("<label class=\"col-md-4 control-label\">Error</label>");
                        b.Line("<div class=\"col-md-8\">");
                        b.Indented("<div class=\"form-control-static\" data-bind=\"text: errorMessage\"></div>");
                        b.Line("</div>");
                    }

                    foreach (var prop in Model.ClientProperties.Where(f => !f.IsHidden(HiddenAttribute.Areas.Edit)).OrderBy(f => f.EditorOrder))
                    {
                        using (b.TagBlock("div", "form-group"))
                        {
                            b.Line($"<label class=\"col-md-4 control-label\">{prop.DisplayName}</label>");
                            if (prop.IsPOCO && prop.PureTypeOnContext)
                            {
                                b.Line(Display.PropertyHelperWithSurroundingDiv(prop, !prop.IsReadOnly, AreaName, 7));
                                b.Line($"<div class=\"col-md-1\" data-bind=\"with: {prop.JsVariableForBinding()}\">");
                                b.Indented("<a data-bind=\"attr: {href: editUrl}\" class=\"btn btn-default pull-right\"><i class=\"fa fa-ellipsis-h \"></i></a>");
                                b.Line("</div>");
                            }
                            else
                            {
                                b.Line(Display.PropertyHelperWithSurroundingDiv(prop, !prop.IsReadOnly, AreaName, 8));
                            }
                        }
                    }
                }
            }
            
            b.Line();
            WriteMethodsCard(b, Model.ClientMethods.Where(f => !f.IsStatic));

            b.Line();
            foreach (var method in Model.ClientMethods.Where(f => !f.IsStatic && f.ClientParameters.Any()))
            {
                b.Line(IntelliTect.Coalesce.Knockout.Helpers.Knockout.ModalFor(method).ToString());
            }

            b.Line();
            b.Line("@section Scripts");
            b.Line("{");
            b.Line("<script>");
            using (b.Indented())
            {
                b.Line($"var model = new {viewModelsNamespace}.{Model.ViewModelClassName}();");
                b.Line("model.includes = \"Editor\";");
                b.Line("model.saveCallbacks.push(function(obj){");
                b.Line("    // If there is a new id, set the one for this page");
                b.Line("    if (!Coalesce.Utilities.GetUrlParameter('id')){");
                b.Line("        if (model.myId) {");
                b.Line("            var newUrl = Coalesce.Utilities.SetUrlParameter(window.location.href, \"id\", model.myId);");
                b.Line("            window.history.replaceState(null, window.document.title, newUrl);");
                b.Line("        }");
                b.Line("    }");
                b.Line("});");

                foreach (var method in GetFileAutoDownloadMethods())
                {
                    // Reload parameterless file downloads when their url changes.
                    // The setTimeout is because knockout doesn't batch its subscriptions (unlike vue's watchers),
                    // so when the model comes back from the server it will trigger this multiple times
                    // as each dependent property gets loaded.
                    b.Line($"model.{method.JsVariable}.url.extend({{ throttle: 1 }}).subscribe(() => model.myId && model.{method.JsVariable}.invoke(undefined, false))");
                }

                b.Line("@if (ViewBag.Id != null)");
                b.Line("{");
                b.Line("    @:model.load(\'@ViewBag.Id\');");
                b.Line("}");
                b.Line("@foreach (KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues> kvp in ViewBag.ParentIds)");
                b.Line("{");
                b.Line("    @:model.@(((string)(@kvp.Key)))(@kvp.Value);");
                b.Line("}");
                b.Line();
                b.Line("window.onbeforeunload = function(){");
                b.Line("    if (model.isDirty()) model.save();");
                b.Line("}");
                b.Line("model.coalesceConfig.autoSaveEnabled(false);");
                b.Line("model.loadChildren(function() {");
                b.Line("    ko.applyBindings(model, document.body);");
                b.Line("    model.coalesceConfig.autoSaveEnabled(true);");
                b.Line("});");
            }
            b.Line("</script>");
            b.Line("}");
        }

        /// <summary>
        /// Gets the methods that should be automatically invoked when their URL changes.
        /// The effect of this is that methods that download a file that we can detect changes in (via VaryByProperty)
        /// get auto loaded. If they're images or videos, they'll get rendered in the method result area.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<MethodViewModel> GetFileAutoDownloadMethods()
        {
            // TODO: We could add HTTP HEAD support to file-returning GET endpoints
            // and then look at the size on the client to determine if an auto-download is practical.

            return Model.ClientMethods.Where(m => 
                m.IsModelInstanceMethod && 
                m.ResultType.IsFile && 
                m.ApiActionHttpMethod == HttpMethod.Get && 
                m.VaryByProperty?.IsClientProperty == true &&
                !m.ClientParameters.Any()
            );
        }
    }
}
