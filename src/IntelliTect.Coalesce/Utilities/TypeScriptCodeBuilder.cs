using System;
using System.Collections.Generic;
using System.Text;

namespace IntelliTect.Coalesce.Utilities
{
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
        public IDisposable Block(string blockPreamble, string closeWith = null)
        {
            if (!onNewLine)
            {
                throw new InvalidOperationException("Cannot start a block on a line that isn't empty");
            }

            Append(blockPreamble).Append(" {").Line();
            Level++;

            return new Indentation(this, closeWith != null ? "}" + closeWith : "}");
        }
        
        public TypeScriptCodeBuilder StringProp(string propName, string stringLiteral)
        {
            Append(propName).Append(": \"").Append(stringLiteral.EscapeStringLiteralForTypeScript()).Append("\",").Line();
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

            // Always put a blank line before a doc comment.
            Line().Append("/** ").Append(comment).Append(" */").Line();
            return this;
        }
        public TypeScriptCodeBuilder DocComment(string[] comment)
        {
            // Always put a blank line before a doc comment.
            Line().Line("/** ");
            foreach (var line in comment)
            {
                Indented(line);
            }
            Line("*/");
            return this;
        }
    }
}
