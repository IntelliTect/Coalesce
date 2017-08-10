using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.PlatformAbstractions;
using System.IO;
using System.Linq;
using System.Net;
using IntelliTect.Coalesce.Models;
using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;

namespace IntelliTect.Coalesce.Controllers
{
    public abstract class BaseViewController<T, TContext> : BaseViewController<T, T, TContext>
        where T : class, new()
        where TContext : DbContext
    {
        protected BaseViewController() : base() { }
    }

    public abstract class BaseViewController<T, TDto, TContext> : BaseControllerWithDb<TContext>
        where T : class, new()
        where TContext : DbContext
    {
        protected ClassViewModel classViewModel;

        protected BaseViewController()
        {
            classViewModel = ReflectionRepository.GetClassViewModel<T>();
        }

        // Page Listing the items in the collection.
        //[OutputCache(Duration = 10000, VaryByParam = "*")]
        protected ActionResult IndexImplementation(bool editable, string viewName = "~/Views/Api/Index.cshtml")
        {
            ViewBag.ParentIdName = null;
            ViewBag.ParentId = null;
            ViewBag.Editable = editable;
            ViewBag.Query = "";
            string[] pageParams = { "page", "pageSize", "search" };
            foreach (var kvp in Request.Query.Where( kvp => !pageParams.Contains(kvp.Key, StringComparer.InvariantCultureIgnoreCase) ))
            {
                ViewBag.ParentIdName = kvp.Key;
                ViewBag.ParentId = kvp.Value;
                ViewBag.Query = ViewBag.Query + WebUtility.UrlEncode(kvp.Key) + "=" + WebUtility.UrlEncode(kvp.Value) + "&";
            }
            ViewBag.Query = ViewBag.Query == "" ? null : new HtmlString( ViewBag.Query );

            @ViewBag.Title = typeof(T).Name + " List";

            return View(viewName, classViewModel);
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
                if (kvp.Key == "id") id = kvp.Value;
                else if (kvp.Key != string.Empty)
                {
                    var varName = classViewModel.PropertyByName(kvp.Key)?.JsVariable;
                    if (varName != null)
                    {
                        parentIds.Add(varName, kvp.Value);
                    }
                    ViewBag.Query = kvp.Key + "=" + kvp.Value;
                }
            }

            @ViewBag.Title = typeof(T).Name + " Edit";
            ViewBag.Id = id;
            return View(viewName, classViewModel);
        }

        /// <summary>
        /// Gets partial HTML content for editing this object.
        /// </summary>
        /// <returns></returns>
        //[OutputCache(Duration = 10000, VaryByParam = "*")]
        protected ActionResult EditorHtmlImplementation(bool simple = false)
        {
            ViewBag.SimpleEditorOnly = simple;
            return PartialView("~/Views/Shared/_EditorHtml.cshtml", classViewModel);
        }


        [HttpGet]
        //[OutputCache(Duration = 10000, VaryByParam = "none")]
        protected ActionResult DocsImplementation()
        {
            // Load TypeScript docs
            var baseClassPath = Path.Combine(AppEnv.ContentRootPath, "Scripts", "Coalesce", "coalesce.ko.base.ts");
            var path = Path.Combine(AppEnv.ContentRootPath, "Scripts", "Generated", $"ko.{classViewModel.ViewModelClassName}.{(classViewModel.HasTypeScriptPartial ? "Partial." : "")}ts");

            ViewBag.ObjDoc = GenerateTypeScriptDocs(path, classViewModel.ViewModelGeneratedClassName);
            ViewBag.BaseObjDoc = GenerateTypeScriptDocs(baseClassPath, "BaseViewModel");

            path = Path.Combine(AppEnv.ContentRootPath, "Scripts", "Generated", $"ko.{classViewModel.ListViewModelClassName}.ts");

            ViewBag.ListDoc = GenerateTypeScriptDocs(path, classViewModel.ListViewModelClassName);
            ViewBag.BaseListDoc = GenerateTypeScriptDocs( baseClassPath, "BaseListViewModel");

            return View("~/Views/Api/Docs.cshtml", classViewModel);
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