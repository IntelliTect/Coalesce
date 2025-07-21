using System;

namespace IntelliTect.Coalesce.TypeDefinition;

public sealed class CoalesceModelException : Exception
{
    public CoalesceModelException() : base()
    {
    }

    public CoalesceModelException(string? message) : base(message)
    {
    }

    public CoalesceModelException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
