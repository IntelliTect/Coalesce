using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace IntelliTect.Coalesce.Utilities
{
    /// <summary>
    /// A string builder that simplifies the process of building code.
    /// Contains operations common to the process of constructing chunks of code, 
    /// fully supporting customizable indentation.
    /// </summary>
    public class CodeBuilder
    {
        public CodeBuilder(int initialLevel = 0, int indentSize = 4, char indentChar = ' ')
        {
            Level = initialLevel;
            this.indentSize = indentSize;
            this.indentChar = indentChar;
        }

        public int Level { get; protected set; }

        protected readonly StringBuilder sb = new StringBuilder();
        protected readonly int indentSize;
        protected readonly char indentChar;
        protected bool onNewLine = true;

        /// <summary>
        /// Write a blank line at the current indentation level.
        /// </summary>
        public CodeBuilder Line() => Line("");

        /// <summary>
        /// Write a line of text at the current indentation level.
        /// If <see cref="Append(string)"/> was called previously, no indentation will be added.
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public CodeBuilder Line(string? line)
        {
            if (onNewLine)
            {
                sb.Append(indentChar, Level * indentSize);
            }
            sb.Append(line).AppendLine();
            onNewLine = true;
            return this;
        }

        /// <summary>
        /// Calls <see cref="Line(string)"/> for each line.
        /// </summary>
        public CodeBuilder Lines(params string[] lines)
        {
            foreach (var line in lines) Line(line);
            return this;
        }

        /// <summary>
        /// Write a line that is indented one level past the current indentation level.
        /// </summary>
        /// <exception cref="InvalidOperationException">Not currently at the start of a blank line - cannot add indented text at the current location.</exception>
        public CodeBuilder Indented(string? line)
        {
            // NOTE: this method's name is deliberate - it is excactly 8 characters, while the regular "Line" method is 4 characters.
            // This lines up exactly with standard indentation size of 4 chars, so usages of this method are visually consistent with their output.
            if (onNewLine)
            {
                sb.Append(indentChar, (Level + 1) * indentSize);
            }
            else
            {
                throw new InvalidOperationException("Cannot start an indented line on a line that isn't empty.");
            }
            sb.Append(line).AppendLine();
            onNewLine = true;
            return this;
        }

        /// <summary>
        /// Increases indentation one level, returning an object that can be disposed to decrease indentation.
        /// </summary>
        /// <example>using (b.Indented()) { b.Line("line1"); b.Line("line2"); } </example>
        /// <returns></returns>
        public IDisposable Indented()
        {
            Level++;
            return new Indentation(this);
        }

        /// <summary>
        /// Write text to the current line. If currently on a new, blank line, the current indentation will be added.
        /// </summary>
        public CodeBuilder Append(string? text)
        {
            if (onNewLine)
            {
                sb.Append(indentChar, Level * indentSize);
                onNewLine = false;
            }
            sb.Append(text);
            return this;
        }

        /// <summary>
        /// Trim the given string from the end of the output if it exists.
        /// </summary>
        public CodeBuilder TrimEnd(string text)
        {
            if (sb.ToString(sb.Length - text.Length, text.Length) == text)
            {
                sb.Remove(sb.Length - text.Length, text.Length);
            }
            return this;
        }

        /// <summary>
        /// Trim whitespace from the end of the output.
        /// </summary>
        public CodeBuilder TrimWhitespace()
        {
            int count = 0;
            while (sb.Length - count > 0 && char.IsWhiteSpace(sb[sb.Length - 1 - count]))
            {
                count++;
            }
            if (count > 0)
            {
                sb.Remove(sb.Length - count, count);
            }
            return this;
        }

        /// <summary>
        /// Convert the value of the instance to a <see cref="System.String"/>.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => sb.ToString();


        protected class Indentation : IDisposable
        {
            private readonly CodeBuilder parent;
            private readonly string? closeWith;

            public Indentation(CodeBuilder parent, string? closeWith = null)
            {
                this.parent = parent;
                this.closeWith = closeWith;
            }
            
            private bool disposed = false; // To detect redundant calls

            public void Dispose()
            {
                if (disposed) return;
                disposed = true;
                this.parent.Level--;
                if (closeWith != null)
                {
                    this.parent.Line(closeWith);
                }
            }
        }
    }
}
