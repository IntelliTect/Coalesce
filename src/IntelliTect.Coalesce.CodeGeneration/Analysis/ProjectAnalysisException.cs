using System;
using System.Collections.Generic;

namespace IntelliTect.Coalesce.CodeGeneration.Analysis;

#pragma warning disable RCS1194 // Implement exception constructors.
public class ProjectAnalysisException : Exception
#pragma warning restore RCS1194 // Implement exception constructors.
{
    public ProjectAnalysisException()
    {
    }

    public ProjectAnalysisException(string message, ICollection<string> outputLines) : base(message)
    {
        OutputLines = outputLines;
    }

    public ICollection<string> OutputLines { get; }
}
