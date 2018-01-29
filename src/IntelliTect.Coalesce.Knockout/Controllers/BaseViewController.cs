using IntelliTect.Coalesce.Knockout.Models;
using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace IntelliTect.Coalesce.Knockout.Controllers
{
    public abstract class BaseViewController<T> : Controller
        where T : class, new()
    {
        protected ClassViewModel ClassViewModel { get; }

        protected BaseViewController()
        {
            ClassViewModel = ReflectionRepository.Global.GetClassViewModel<T>();
        }

        // Page Listing the items in the collection.
        //[OutputCache(Duration = 10000, VaryByParam = "*")]
        protected ActionResult IndexImplementation(bool editable, string viewName = "~/Views/Api/Table.cshtml")
        {
            ViewBag.Editable = editable;
            ViewBag.Query = "";
            string[] pageParams = { "page", "pageSize", "search", "orderBy", "orderByDescending" };
            foreach (var kvp in Request.Query.Where( kvp => !pageParams.Contains(kvp.Key, StringComparer.InvariantCultureIgnoreCase) ))
            {
                ViewBag.Query = ViewBag.Query + WebUtility.UrlEncode(kvp.Key) + "=" + WebUtility.UrlEncode(kvp.Value) + "&";
            }
            ViewBag.Query = ViewBag.Query == "" ? null : new HtmlString( ViewBag.Query );

            @ViewBag.Title = ClassViewModel.DisplayName + " List";

            return View(viewName, ClassViewModel);
        }

        // GET: Editing page
        //[OutputCache(Duration = 10000, VaryByParam = "*")]
        protected ActionResult CreateEditImplementation(string viewName = "~/Views/Api/CreateEdit.cshtml")
        {
            string id = null;
            var parentIds = new Dictionary<string, StringValues>();
            ViewBag.ParentIds = parentIds;

            foreach (var kvp in Request.Query)
            {
                var propName = kvp.Key;
                const string filterPrefix = "filter.";
                if (propName.StartsWith(filterPrefix))
                {
                    propName = propName.Substring(filterPrefix.Length);
                }

                if (propName == "id") id = kvp.Value;
                else if (propName != string.Empty)
                {
                    var varName = ClassViewModel.PropertyByName(propName)?.JsVariable;
                    if (varName != null)
                    {
                        parentIds.Add(varName, kvp.Value);
                    }
                }
            }

            ViewBag.Title = ClassViewModel.DisplayName + " Edit";
            ViewBag.Id = id;
            return View(viewName, ClassViewModel);
        }

        /// <summary>
        /// Gets partial HTML content for editing this object.
        /// </summary>
        /// <returns></returns>
        //[OutputCache(Duration = 10000, VaryByParam = "*")]
        protected ActionResult EditorHtmlImplementation(bool simple = false)
        {
            ViewBag.SimpleEditorOnly = simple;
            return PartialView("~/Views/Api/EditorHtml.cshtml", ClassViewModel);
        }

        
        //[OutputCache(Duration = 10000, VaryByParam = "none")]
        protected ActionResult DocsImplementation([FromServices] IHostingEnvironment hostingEnvironment)
        {
            // Load TypeScript docs
            var contentRoot = hostingEnvironment.ContentRootPath;

            var baseClassPath = Path.Combine(contentRoot, "Scripts", "Coalesce", "coalesce.ko.base.ts");
            var path = Path.Combine(contentRoot, "Scripts", "Generated", $"ko.{ClassViewModel.ViewModelClassName}.{(ClassViewModel.HasTypeScriptPartial ? "Partial." : "")}g.ts");

            ViewBag.ObjDoc = GenerateTypeScriptDocs(path, ClassViewModel.ViewModelGeneratedClassName);
            ViewBag.BaseObjDoc = GenerateTypeScriptDocs(baseClassPath, "BaseViewModel");

            path = Path.Combine(contentRoot, "Scripts", "Generated", $"ko.{ClassViewModel.ListViewModelClassName}.g.ts");

            ViewBag.ListDoc = GenerateTypeScriptDocs(path, ClassViewModel.ListViewModelClassName);
            ViewBag.BaseListDoc = GenerateTypeScriptDocs( baseClassPath, "BaseListViewModel");

            return View("~/Views/Api/Docs.cshtml", ClassViewModel);
        }

        private TypeScriptDocumentation GenerateTypeScriptDocs(string path, string className = null)
        {
            var doc = new TypeScriptDocumentation();
            var file = new FileInfo(path);

            // don't gen JSON documentation for definition files
            if (!file.FullName.EndsWith(".d.ts"))
            {
                var reader = file.OpenText();
                doc.TsFilename = file.Name;
                doc.Generate(reader.ReadToEnd(), className);
            }

            return doc;
        }
    }
}