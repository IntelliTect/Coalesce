using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Text;
using IntelliTect.Coalesce.CodeGeneration.Knockout.BaseGenerators;
using IntelliTect.Coalesce.Utilities;
using System.Linq;
using IntelliTect.Coalesce.Knockout.Helpers;
using IntelliTect.Coalesce.DataAnnotations;

namespace IntelliTect.Coalesce.CodeGeneration.Knockout.Generators
{
    public class TableView : KnockoutViewGenerator
    {
        public TableView(GeneratorServices services) : base(services)
        {
        }

        public override void BuildOutput(HtmlCodeBuilder b)
        {
            b.Line("@{ ViewBag.Fluid = true; }");

            b.Line();
            using (b.TagBlock("style"))
            {
                b.Lines(
                    "td div a {",
                    "    display: block;",
                    "}",
                    "th.sortable-header {",
                    "    cursor: pointer;",
                    "}",
                    "th.sortable-header:hover {",
                    "    background-color: #e6e6e6",
                    "}",
                    ".table-view-header {",
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
            
            using (b.TagBlock("div", $"table-view obj-{Model.ClientTypeName.ToCamelCase()}"))
            {
                WriteAdminTableControls(b, "table", additionalButtons: () =>
                {
                    b.Line("@if (ViewBag.Editable)");
                    b.Line("{");
                    b.Indented($"<a href=\"~/{Model.ControllerName}/Table?@(ViewBag.Query)\" role=\"button\" class=\"btn btn-sm btn-default \"><i class=\"fa fa-lock\"></i> Make Read-only</a>");
                    b.Line("}");
                    if (Model.SecurityInfo.IsEditAllowed())
                    {
                        b.Line("else");
                        b.Line("{");
                        b.Indented($"<a href=\"~/{Model.ControllerName}/TableEdit?@ViewBag.Query\" role=\"button\" class=\"btn btn-sm btn-default \"><i class=\"fa fa-pencil\"></i> Make Editable</a>");
                        b.Line("}");
                    }
                });

                b.Line("<hr />");

                using (b
                    .TagBlock("div", "card table-view-body")
                    .TagBlock("div", "card-body")
                    .TagBlock("table", "table @(ViewBag.Editable ? \"editable\" : \"\" )")
                )
                {
                    using (b.TagBlock("thead").TagBlock("tr"))
                    {
                        // Data column headers
                        foreach (var prop in Model.AdminPageProperties.Where(f => !f.IsHidden(HiddenAttribute.Areas.List)).OrderBy(f => f.EditorOrder))
                        {
                            if (!prop.Type.IsCollection)
                            {
                                using (b.TagBlock("th", "sortable-header", dataBind: $"click: function(){{orderByToggle('{prop.Name}')}}"))
                                {
                                    b.Line(prop.DisplayName);
                                    b.Line($"<i class=\"pull-right fa\" data-bind=\"css:{{'fa-caret-up': orderBy() == '{prop.Name}', 'fa-caret-down': orderByDescending() == '{prop.Name}'}}\"></i>");
                                }
                            }
                            else
                            {
                                b.Line($"<th>{prop.DisplayName}</th>");
                            }
                        }

                        // Buttons column header
                        b.Line("<th style=\"width: 1%\">");
                        b.Line("</th>");
                    }

                    using (b
                        .TagBlock("tbody", dataBind: "foreach: items")
                        .TagBlock("tr", dataBind: $"css: {{'btn-warning': errorMessage()}}, attr: {{id: {Model.PrimaryKey.Name.ToCamelCase()}}}")
                    )
                    {
                        var properties = Model.AdminPageProperties
                            .Where(f => !f.IsHidden(HiddenAttribute.Areas.List))
                            .OrderBy(f => f.EditorOrder);


                        b.Line("@if (@ViewBag.Editable)");
                        b.Line("{");
                        foreach (var prop in properties)
                        {
                            // Edit mode -- display property inputs
                            b.Indented($"<td class=\"prop-{prop.JsonName}\">{Display.PropertyHelper(prop, !prop.IsReadOnly, null, true)}</td>");
                        }
                        b.Line("}");
                        b.Line("else");
                        b.Line("{");
                        foreach (var prop in properties)
                        {
                            // Read-only mode -- display properties only
                            b.Indented($"<td class=\"prop-{prop.JsonName}\">{Display.PropertyHelper(prop, false, null, true)}</td>");
                        }
                        b.Line("}");

                        // Edit/Delete buttons, and instance method dropdown.
                        using (b.TagBlock("td"))
                        {
                            b.Line("<!-- Editor buttons -->");
                            using (b.TagBlock("div", new { @class = "btn-group pull-right", role = "group", style = "display: inline-flex" }))
                            {
                                WriteListViewObjectButtons(b);
                            }
                            b.Line("<div class=\"form-control-static\" data-bind=\"text: errorMessage\"></div>");
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
