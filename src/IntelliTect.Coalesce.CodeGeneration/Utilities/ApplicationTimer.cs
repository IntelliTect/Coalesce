using System.Diagnostics;

namespace IntelliTect.Coalesce.CodeGeneration.Utilities;

/// <summary>
/// Just a static class with a stopwatch for measuring the total elapsed time of code generation.
/// Not started by default - it gets started when the CLI starts.
/// </summary>
public static class ApplicationTimer
{
    public static readonly Stopwatch Stopwatch = new Stopwatch();
}
