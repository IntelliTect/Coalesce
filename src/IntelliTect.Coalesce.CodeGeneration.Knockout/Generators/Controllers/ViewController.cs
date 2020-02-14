using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using IntelliTect.Coalesce.CodeGeneration.Knockout.BaseGenerators;
using IntelliTect.Coalesce.Utilities;

namespace IntelliTect.Coalesce.CodeGeneration.Knockout.Generators
{
    public class ViewController : StringBuilderCSharpGenerator<ClassViewModel>
    {
        public ViewController(GeneratorServices services) : base(services) { }
        
        public override void BuildOutput(CSharpCodeBuilder b)
        {
            string namespaceName = Namespace;
            string viewLocation = "~/Views";
            if (!string.IsNullOrWhiteSpace(AreaName))
            {
                namespaceName += "." + AreaName;
                viewLocation = "~/Areas/" + AreaName + "/Views";
            }


            b.Line("using IntelliTect.Coalesce.Knockout.Controllers;");
            b.Line("using Microsoft.AspNetCore.Authorization;");
            b.Line("using Microsoft.AspNetCore.Mvc;");
            b.Line("using Microsoft.AspNetCore.Hosting;");

            b.Line();
            using (b.Block($"namespace {namespaceName}.Controllers"))
            {
                if (!string.IsNullOrWhiteSpace(AreaName))
                {
                    b.Line($"[Area(\"{AreaName}\")]");
                }
                b.Line($"{Model.SecurityInfo.ClassAnnotation}");

                using (b.Block($"public partial class {Model.ViewControllerClassName} : BaseViewController<{Model.FullyQualifiedName}>"))
                {
                    b.Line($"{Model.SecurityInfo.ReadAnnotation}");
                    using (b.Block("public ActionResult Cards()"))
                    {
                        b.Line($"return IndexImplementation(false, @\"{viewLocation}/Generated/{Model.ClientTypeName}/Cards.cshtml\");");
                    }

                    b.Line();
                    b.Line($"{Model.SecurityInfo.ReadAnnotation}");
                    using (b.Block("public ActionResult Table()"))
                    {
                        b.Line($"return IndexImplementation(false, @\"{viewLocation}/Generated/{Model.ClientTypeName}/Table.cshtml\");");
                    }

                    b.Line();
                    if (Model.SecurityInfo.IsEditAllowed())
                    {
                        b.Line();
                        b.Line($"{Model.SecurityInfo.EditAnnotation}");
                        using (b.Block("public ActionResult TableEdit()"))
                        {
                            b.Line($"return IndexImplementation(true, @\"{viewLocation}/Generated/{Model.ClientTypeName}/Table.cshtml\");");
                        }

                        b.Line();
                        b.Line($"{Model.SecurityInfo.EditAnnotation}");
                        using (b.Block("public ActionResult CreateEdit()"))
                        {
                            b.Line($"return CreateEditImplementation(@\"{viewLocation}/Generated/{Model.ClientTypeName}/CreateEdit.cshtml\");");
                        }

                        b.Line();
                        b.Line($"{Model.SecurityInfo.EditAnnotation}");
                        using (b.Block("public ActionResult EditorHtml(bool simple = false)"))
                        {
                            b.Line("return EditorHtmlImplementation(simple);");
                        }

                        // This is defunct.
                        //b.Line();
                        //b.Line($"{Model.SecurityInfo.EditAnnotation}");
                        //using (b.Block("public ActionResult Docs([FromServices] IHostingEnvironment hostingEnvironment)"))
                        //{
                        //    b.Line("return DocsImplementation(hostingEnvironment);");
                        //}
                    }
                }
            }
        }
    }
}
