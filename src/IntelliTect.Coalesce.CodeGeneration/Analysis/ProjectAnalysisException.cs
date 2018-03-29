using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace IntelliTect.Coalesce.CodeGeneration.Analysis
{
    public class ProjectAnalysisException : Exception
    {
        public ProjectAnalysisException()
        {
        }

        public ProjectAnalysisException(string message, string lastOutputLine) : base(message)
        {
            LastOutputLine = lastOutputLine ?? "";
        }

        public string LastOutputLine { get; }
    }
}
