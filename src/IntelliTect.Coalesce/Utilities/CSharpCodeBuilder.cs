using System;
using System.Collections.Generic;
using System.Text;

namespace IntelliTect.Coalesce.Utilities
{
    public class CSharpCodeBuilder : CodeBuilder
    {
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

            Line(blockPreamble);
            Line("{");
            Level++;

            return new Indentation(this, "}");
        }
    }
}
