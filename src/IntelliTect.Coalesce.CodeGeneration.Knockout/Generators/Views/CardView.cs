using IntelliTect.Coalesce.CodeGeneration.Generation;
using System;
using System.Collections.Generic;
using System.Text;
using IntelliTect.Coalesce.CodeGeneration.Knockout.BaseGenerators;
using IntelliTect.Coalesce.Utilities;
using System.Linq;
using IntelliTect.Coalesce.Knockout.Helpers;
using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.Knockout.TypeDefinition;

namespace IntelliTect.Coalesce.CodeGeneration.Knockout.Generators
{
    public class CardView : KnockoutViewGenerator
    {
        public CardView(GeneratorServices services) : base(services)
        {
        }

        public override void BuildOutput(HtmlCodeBuilder b)
        {
            b.Line("@{ ViewBag.Fluid = true; }");

            b.Line();
            using (b.TagBlock("style"))
            {
                b.Lines(
                    ".card-view-header {",
                    "    padding: 10px 15px;",
                    "}",
                    "img.form-control-static {",
                    "    max-width: 100px;",
                    "    max-height: 100px;",
                    "}",
                    ".coalesce-upload-icon {",
                    "    cursor: pointer;",
                    "}"
                );
            }

            b.Line();
            using (b.TagBlock("div", $"card-view obj-{Model.ClientTypeName.ToCamelCase()}"))
            {
                WriteAdminTableControls(b, "card");
                
                b.Line();
                b.EmptyTag("hr");

                b.Line();
                using (b.TagBlock("div",
                    @class: "flex-card-container card-view-body", 
                    dataBind: "foreach: items, visible: isLoaded", 
                    style: "display: none"))
                {
                    var titleProp = Model.ListTextProperty;
                    
                    // The card for each object.
                    using (b.TagBlock("div", "flex-card"))
                    {
                        if (titleProp != null)
                        {
                            using (b.TagBlock("div", "card-heading").TagBlock("h3", "card-title"))
                            {
                                b.Line($"<span data-bind=\"text: {titleProp.JsVariableForBinding()}\"></span>");
                            }
                        }

                        using (b.TagBlock("div", "card-body flex"))
                        {
                            // List of all data properties on the object
                            using (b.TagBlock("dl"))
                            {
                                var properties = Model.ClientProperties
                                    .Where(f => !f.IsHidden(HiddenAttribute.Areas.List))
                                    .OrderBy(f => f.EditorOrder)
                                    .ToList();
                                foreach (var prop in properties)
                                {
                                    b.Line($"<dt>{prop.DisplayName}</dt>");
                                    b.Line("<dd>");
                                    b.Indented($"{Display.PropertyHelper(prop, false)}");
                                    b.Line("</dd>");
                                }
                            }

                            // Edit/Delete buttons, and instance method dropdown.
                            b.Line();
                            using (b.TagBlock("div", style: "margin-top: auto"))
                            {
                                b.Line("<!-- Editor buttons -->");
                                using (b.TagBlock("div", new { @class = "pull-right", role = "group" }))
                                {
                                    WriteListViewObjectButtons(b);
                                }
                                b.Line("<span class=\"form-control-static\" data-bind=\"text: errorMessage\"></span>");
                            }
                        }
                    }
                }
            }
            
            b.Line();
            WriteMethodsCard(b, Model.ClientMethods.Where(f => f.IsStatic));

            b.Line();
            foreach (var method in Model.ClientMethods.Where(f => f.IsStatic && f.ClientParameters.Any()))
            {
                b.Line(IntelliTect.Coalesce.Knockout.Helpers.Knockout.ModalFor(method).ToString());
            }

            WriteListViewScript(b);
        }
    }
}
