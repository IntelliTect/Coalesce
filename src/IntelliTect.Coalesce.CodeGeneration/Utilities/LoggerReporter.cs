using Microsoft.Extensions.Logging;

namespace IntelliTect.Coalesce.CodeGeneration.Utilities
{
#if NET5_0_OR_GREATER
    internal class LoggerReporter : Microsoft.EntityFrameworkCore.Design.Internal.IOperationReporter
    {
        public LoggerReporter(ILogger logger)
        {
            Logger = logger;
        }

        public ILogger Logger { get; }

        public void WriteError(string message)
        {
            Logger.LogError(message);
        }

        public void WriteInformation(string message)
        {
            Logger.LogInformation(message);
        }

        public void WriteVerbose(string message)
        {
            Logger.LogTrace(message);
        }

        public void WriteWarning(string message)
        {
            Logger.LogWarning(message);
        }
    }
#endif
}
