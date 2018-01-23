using System;
using System.Collections.Generic;
using System.Text;

namespace IntelliTect.Coalesce.CodeGeneration.Utilities
{
    public class IndentingStringBuilder
    {
        public IndentingStringBuilder(int initialLevel = 0, int indentSize = 4, char indentChar = ' ')
        {
            Level = initialLevel;
            this.indentSize = indentSize;
            this.indentChar = indentChar;
        }

        public int Level { get; private set; }

        private StringBuilder sb = new StringBuilder();
        private readonly int indentSize;
        private readonly char indentChar;
        private bool onNewLine = true;


        public IndentingStringBuilder Line(object obj)
        {
            if (onNewLine)
            {
                sb.Append(indentChar, Level * indentSize);
            }
            sb.Append(obj).AppendLine();
            onNewLine = true;
            return this;
        }

        public IndentingStringBuilder IndentedLine(object obj)
        {
            if (onNewLine)
            {
                sb.Append(indentChar, (Level + 1) * indentSize);
            }
            else
            {
                throw new InvalidOperationException("Not on a blank line - cannot indent current line");
            }
            sb.Append(obj).AppendLine();
            onNewLine = true;
            return this;
        }

        public IndentingStringBuilder Append(object obj)
        {
            if (onNewLine)
            {
                sb.Append(indentChar, Level * indentSize);
                onNewLine = false;
            }
            sb.Append(obj);
            return this;
        }

        public Indentation Block()
        {
            Level++;
            return new Indentation(this);
        }


        public override string ToString() => sb.ToString();

        public class Indentation : IDisposable
        {
            private readonly IndentingStringBuilder parent;

            public Indentation(IndentingStringBuilder parent)
            {
                this.parent = parent;
            }
            
            private bool disposed = false; // To detect redundant calls
            public void Dispose()
            {
                if (disposed) return;
                this.parent.Level--;
            }
        }
    }
}
