using System;

namespace IntelliTect.Coalesce.Utilities;

public class CSharpCodeBuilder : CodeBuilder
{
    /// <summary>
    /// Writes the given text, then a newline, opening brace, and newline.
    /// Increases indentation one level, returning an object that can be disposed to decrease indentation and write a closing curly brace
    /// </summary>
    public IDisposable Block(string? blockPreamble = null, string? closeWith = null)
    {
        if (!string.IsNullOrWhiteSpace(blockPreamble))
        {
            Line(blockPreamble);
        }
        Line("{");
        Level++;

        return new Indentation(this, closeWith != null ? "}" + closeWith : "}");
    }

    /// <summary>
    /// Writes a blank line, followed by a C# xmldoc comment with the given summary.
    /// </summary>
    /// <param name="summary"></param>
    /// <returns></returns>
    public CSharpCodeBuilder DocComment(string summary)
    {
        Line();
        Line("/// <summary>");
        Line($"/// {summary}");
        Line("/// </summary>");

        return this;
    }
}
