using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Linq.Expressions;

namespace IntelliTect.Coalesce.TagHelpers
{
    // Just playing around with this idea for now, but eventually we should do it 
    /*
    [HtmlTargetElement("input", Attributes = "ko-for")]
    public class KoInputTagHelper : TagHelper
    {
        [HtmlAttributeName("ko-for")]
        public LambdaExpression For { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var dataBind = context.AllAttributes["data-bind"].Value.ToString();
            if (string.IsNullOrWhiteSpace(dataBind))
            {
                dataBind = "";
            }
            else
            {
                dataBind = ", " + dataBind;
            }

            output.Attributes.SetAttribute("data-bind", dataBind);

            base.Process(context, output);
        }
    }
    */
}
