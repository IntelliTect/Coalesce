using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace IntelliTect.Coalesce.CodeGeneration.Utilities;

public class SimpleConsoleLoggerProvider : ILoggerProvider
{
    public void Dispose() { }

    public ILogger CreateLogger(string categoryName)
    {
        return new SimpleConsoleLogger(categoryName);
    }


    public class SimpleConsoleLogger : ILogger
    {

        private readonly string _categoryName;
        private static readonly object sync = new object();

        private static readonly Dictionary<LogLevel, ConsoleColor> colorLevelMap = new Dictionary<LogLevel, ConsoleColor>
        {
            { LogLevel.Trace, ConsoleColor.DarkGray },
            { LogLevel.Debug, ConsoleColor.Gray },
            { LogLevel.Information, ConsoleColor.DarkGreen },
            { LogLevel.Warning, ConsoleColor.Yellow },
            { LogLevel.Error, ConsoleColor.Red },
            { LogLevel.Critical, ConsoleColor.Red },
        };

        public SimpleConsoleLogger(string categoryName)
        {
            _categoryName = categoryName;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var levelString = logLevel == LogLevel.Information ? "Info" : logLevel.ToString();

            lock (sync)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("[");
                Console.ForegroundColor = colorLevelMap[logLevel];
                Console.Write(levelString.ToLower()[0]);
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write(":");
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.Write($"{ApplicationTimer.Stopwatch.ElapsedMilliseconds / 1000d:0.000}");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("] ");
                if (logLevel == LogLevel.Information)
                {
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = colorLevelMap[logLevel];
                }
                Console.WriteLine(formatter(state, exception));
                Console.ResetColor();
                Console.Out.Flush();
            }
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }
    }
}
