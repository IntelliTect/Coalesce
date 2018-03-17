using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace IntelliTect.Coalesce.Utilities
{
    public class HtmlCodeBuilder : CodeBuilder
    {
        public HtmlCodeBuilder(int initialLevel = 0, int indentSize = 4, char indentChar = ' ') : base(initialLevel, indentSize, indentChar)
        {
        }

        private void TagStart(string elName, object attributes = null)
        {
            Append("<").Append(elName);
            if (attributes != null)
            {
                    
                foreach (var prop in attributes.GetType().GetProperties())
                {
                    Append(" ").Append(prop.Name.Replace("_", "-")).Append("=\"");
                    object value = prop.GetValue(attributes);
                    if (value != null)
                    {
                        Append(HttpUtility.HtmlAttributeEncode(value.ToString()));
                    }
                    Append("\"");
                }
            }
        }

        public IDisposable TagBlock(string tagName, object attributes = null)
        {
            if (!onNewLine)
            {
                throw new InvalidOperationException("Cannot start a block on a line that isn't empty");
            }

            TagStart(tagName, attributes);
            Line(">");

            Level++;

            return new Indentation(this, $"</{tagName}>");
        }

        public HtmlCodeBuilder EmptyTag(string tagName, object attributes = null)
        {
            TagStart(tagName, attributes);
            Line(" />");
            return this;
        }
    }
}
