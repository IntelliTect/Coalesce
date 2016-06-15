using Intellitect.ComponentModel.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intellitect.ComponentModel.Helpers
{
    public static class TableHelper
    {
        public static HtmlString Create(Table table)
        {
            var html = new StringBuilder();

            html.AppendLine("<div>");
            html.AppendLine(Paging(table));
            html.AppendLine(Loading(table));

            html.AppendLine(CreateButton(table));
            html.AppendLine(Search(table));
            html.AppendLine("</div>");


            html.AppendLine(@"<table class=""table table-striped"">");
            // Header
            html.AppendLine("    <thead>");
            html.AppendLine("        <tr>");
            foreach (var col in table.Columns)
            {
                html.AppendLine("            <td>");
                html.AppendLine($"                <span>{col.TitleDisplay}</span>");
                html.AppendLine("            </td>");
            }
            if (table.HasRowDelete || table.HasRowEdit)
            {
                html.AppendLine(@"
                <td style=""width: 80px;""/>");
            }

            html.AppendLine("        </tr>");
            html.AppendLine("    </thead>");

            // Body
            html.AppendLine(@"    <tbody data-bind=""foreach: items"">");
            html.AppendLine("        <tr>");
            foreach (var col in table.Columns)
            {
                html.AppendLine("            <td>");
                html.AppendLine($"                {Display.Property(col.ViewModel, table.IsEditable)}");
                html.AppendLine("            </td>");
            }
            if (table.HasRowDelete || table.HasRowEdit)
            {
                html.AppendLine(@"
            <td>
                <span class=""btn-group pull-right"">");
                html.AppendLine(RowEdit(table));
                html.AppendLine(RowDelete(table));
                html.AppendLine(@"
                </span>
            </td>");
            }

            html.AppendLine(" </tr>");
            html.AppendLine("    </tbody>");

            html.AppendLine("<table>");

            return new HtmlString(html.ToString());
        }

        private static string Search(Table table)
        {
            var html = new StringBuilder();
            if (table.HasSearch)
            {
                html.AppendLine($@"
                            <input class=""form-control pull-right"" style=""width: 200px;"" data-bind=""textInput: search"" placeholder=""Search"" />
                ");
            }
            return html.ToString();
        }

        private static string Paging(Table table)
        {
            var html = new StringBuilder();
            if (table.HasPaging)
            {
                html.AppendLine($@"
            <div style=""display:inline-block; font-size: 1.1em; margin-right: 10px;"">
                <i class=""fa fa-arrow-circle-left"" data-bind=""enabled: previousPageEnabled(), click: previousPage""></i>
                Page
                <span data-bind=""text: page""></span>
                of
                <span data-bind=""text: pageCount""></span>
                <i class=""fa fa-arrow-circle-right"" data-bind=""enabled: nextPageEnabled(), click: nextPage""></i>
            </div>
            <select data-bind=""value:pageSize"" class=""form-control"" style=""width: 100px; display:inline-block"">
                <option value=""1"">1</option>
                <option value=""5"">5</option>
                <option value=""10"">10</option>
                <option value=""50"">50</option>
                <option value=""100"">100</option>
                <option value=""500"">500</option>
                <option value=""1000"">1000</option>
            </select>   
                ");
            }
            return html.ToString();
        }

        private static string Loading(Table table)
        {
            var html = new StringBuilder();
            if (table.HasLoading)
            {
                html.AppendLine($@"
                            <div class=""label label-info"" data-bind=""fadeVisible: isLoading"">Loading...</div>
                ");
            }
            return html.ToString();
        }

        private static string CreateButton(Table table)
        {
            var html = new StringBuilder();
            if (table.HasCreate)
            {
                //TODO: Fix to have query work.
                html.AppendLine($@"
                            <a href=""~/{table.ViewModel.ControllerName}/CreateEdit?Query"" role=""button"" class=""pull-right btn btn-default"" style=""margin-left:10px;"">Create...</a>

                ");
            }
            return html.ToString();
        }

        private static string RowEdit(Table table)
        {
            var html = new StringBuilder();
            if (table.HasRowEdit)
            {
                //TODO: Fix to have query work.
                html.AppendLine(@"
                        <a data-bind=""attr:{ href: editUrl }"" class=""btn btn-sm btn-default"">
                            <i class=""fa fa-pencil""></i>
                        </a>
                ");
            }
            return html.ToString();
        }


        private static string RowDelete(Table table)
        {
            var html = new StringBuilder();
            if (table.HasRowDelete)
            {
                //TODO: Fix to have query work.
                html.AppendLine(@"
                        <button data-bind=""click: deleteItemWithConfirmation"" class=""btn btn-sm btn-danger"">
                            <i class=""fa fa-remove""></i>
                        </button>
                ");
            }
            return html.ToString();
        }

    }
}
