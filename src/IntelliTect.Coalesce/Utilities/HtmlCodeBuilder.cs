using System;

namespace IntelliTect.Coalesce.Utilities;

public class HtmlCodeBuilder : CodeBuilder
{
    public HtmlCodeBuilder(int initialLevel = 0, int indentSize = 4, char indentChar = ' ') : base(initialLevel, indentSize, indentChar)
    {
    }

    private void TagStart(string elName, object? attributes = null)
    {
        Append("<").Append(elName);
        if (attributes != null)
        {
            foreach (var prop in attributes.GetType().GetProperties())
            {
                object? value = prop.GetValue(attributes);
                if (value != null)
                {
                    Append(" ").Append(prop.Name.Replace("_", "-")).Append("=\"");

                    // We intentionally don't encode attribute values, since these aren't user input.
                    // There are cases where we need to insert razor syntax into attribute strings,
                    // in which case we can't be encoding the values.
                    // Append(HttpUtility.HtmlAttributeEncode(value.ToString()).Replace("&#39;", "'"));
                    Append(value.ToString());

                    Append("\"");
                }
            }
        }
    }
    
    public ITagBlockChainBuilder TagBlock(string tagName, object attributes)
    {
        if (!onNewLine)
        {
            throw new InvalidOperationException("Cannot start a block on a line that isn't empty");
        }

        TagStart(tagName, attributes);
        Line(">");

        Level++;

        return new TagBlockScope(this, null, new Indentation(this, $"</{tagName}>"));
    }

    public ITagBlockChainBuilder TagBlock(string tagName, string? @class = null, string? dataBind = null, string? style = null)
    {
        return TagBlock(tagName, new { @class, style, data_bind = dataBind });
    }

    public HtmlCodeBuilder EmptyTag(string tagName, object? attributes = null)
    {
        TagStart(tagName, attributes);
        Line(" />");
        return this;
    }

    /// <summary>
    /// Class that lets us chain multiple tag blocks starts together to make the handling of
    /// deeply nested, simple tag structures much nicer.
    /// </summary>
    private class TagBlockScope : ITagBlockChainBuilder, IDisposable
    {
        private readonly HtmlCodeBuilder b;
        private readonly ITagBlockChainBuilder? parent;
        private readonly IDisposable block;

        public TagBlockScope(HtmlCodeBuilder b, ITagBlockChainBuilder? parent, IDisposable block)
        {
            this.b = b ?? throw new ArgumentNullException(nameof(b));
            this.parent = parent; // Parent is optional
            this.block = block ?? throw new ArgumentNullException(nameof(block));
        }

        public void Dispose()
        {
            block.Dispose();
            parent?.Dispose();
        }

        public ITagBlockChainBuilder TagBlock(string tagName, object attributes)
            => new TagBlockScope(b, this, b.TagBlock(tagName, attributes));

        public ITagBlockChainBuilder TagBlock(string tagName, string? @class = null, string? dataBind = null, string? style = null)
            => new TagBlockScope(b, this, b.TagBlock(tagName, @class: @class, dataBind: dataBind, style: style));
    }
}

public interface ITagBlockChainBuilder : IDisposable
{
    ITagBlockChainBuilder TagBlock(string tagName, object attributes);
    ITagBlockChainBuilder TagBlock(string tagName, string? @class = null, string? dataBind = null, string? style = null);
}
