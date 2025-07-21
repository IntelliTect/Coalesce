using System;
using System.Linq;

namespace IntelliTect.Coalesce.Utilities;

public class TypeScriptCodeBuilder : CodeBuilder
{
    public TypeScriptCodeBuilder(int initialLevel = 0, int indentSize = 4, char indentChar = ' ') 
        : base(initialLevel, indentSize, indentChar)
    {
    }

    /// <summary>
    /// Writes the given text, followed by a space, opening brace, and newline.
    /// Increases indentation one level, returning an object that can be disposed to decrease indentation and write a closing curly brace
    /// </summary>
    public IDisposable Block(string blockPreamble, char closeWith) => Block(blockPreamble, closeWith.ToString());

    /// <summary>
    /// Writes the given text, followed by a space, opening brace, and newline.
    /// Increases indentation one level, returning an object that can be disposed to decrease indentation and write a closing curly brace
    /// </summary>
    public IDisposable Block(string blockPreamble, string? closeWith = null, bool leadingSpace = true)
    {
        if (!onNewLine)
        {
            throw new InvalidOperationException("Cannot start a block on a line that isn't empty");
        }

        Append(blockPreamble).Append(leadingSpace ? " {" : "{").Line();
        Level++;

        return new Indentation(this, closeWith != null ? "}" + closeWith : "}");
    }

    public TypeScriptCodeBuilder StringProp(
        string propName,
        string? stringLiteral,
        bool omitIfNull = false,
        bool asConst = false)
    {
        if (stringLiteral is null && omitIfNull) return this;

        Append(propName).Append(": \"").Append(stringLiteral.EscapeStringLiteralForTypeScript()).Append("\"");

        if (asConst) Append(" as const");

        Append(",").Line();
        return this;
    }

    public TypeScriptCodeBuilder Prop(string propName, string propValue)
    {
        Append(propName).Append(": ").Append(propValue).Append(",").Line();
        return this;
    }
    
    public TypeScriptCodeBuilder DocComment(string comment, bool alwaysAddBlankLine = false)
    {
        if (string.IsNullOrWhiteSpace(comment))
        {
            if (alwaysAddBlankLine) Line();
            return this;
        }

        var commentLines = comment.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        DocComment(commentLines);
        return this;
    }

    public TypeScriptCodeBuilder DocComment(string[] comment)
    {
        // Skip all blank lines at the start or the end, but never in the middle.
        var lines = comment
            // Skip blank lines at the start
            .SkipWhile(l => string.IsNullOrWhiteSpace(l))
            // Reverse, then skip blank lines that will be at the end
            .Reverse()
            .SkipWhile(l => string.IsNullOrWhiteSpace(l))
            // Restore original ordering.
            .Reverse()
            .ToArray();

        if (lines.Length == 0) return this;

        // Always put a blank line before a doc comment.
        Line();

        if (lines.Length > 1)
        {
            Line("/** ");
            foreach (var line in lines)
            {
                Indented(line);
            }
            Line("*/");
        }
        else
        {
            // Comment is a one-liner. Keep it that way.
            Append("/** ").Append(lines.Single()).Append(" */").Line();
        }

        return this;
    }
}
