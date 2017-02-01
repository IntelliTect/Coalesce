// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Internal;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Linq;
using NuGet.Frameworks;
using System.Text;

namespace Microsoft.Extensions.ProjectModel
{
    public static class ArgumentEscaper
    {
        /// <summary>
        /// Undo the processing which took place to create string[] args in Main,
        /// so that the next process will receive the same string[] args
        /// 
        /// See here for more info:
        /// http://blogs.msdn.com/b/twistylittlepassagesallalike/archive/2011/04/23/everyone-quotes-arguments-the-wrong-way.aspx
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string EscapeAndConcatenateArgArrayForProcessStart(IEnumerable<string> args)
        {
            return string.Join(" ", EscapeArgArray(args));
        }

        /// <summary>
        /// Undo the processing which took place to create string[] args in Main,
        /// so that the next process will receive the same string[] args
        /// 
        /// See here for more info:
        /// http://blogs.msdn.com/b/twistylittlepassagesallalike/archive/2011/04/23/everyone-quotes-arguments-the-wrong-way.aspx
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private static IEnumerable<string> EscapeArgArray(IEnumerable<string> args)
        {
            var escapedArgs = new List<string>();

            foreach (var arg in args)
            {
                escapedArgs.Add(EscapeSingleArg(arg));
            }

            return escapedArgs;
        }

        public static string EscapeSingleArg(string arg)
        {
            var sb = new StringBuilder();

            var needsQuotes = ShouldSurroundWithQuotes(arg);
            var isQuoted = needsQuotes || IsSurroundedWithQuotes(arg);

            if (needsQuotes) sb.Append("\"");

            for (int i = 0; i < arg.Length; ++i)
            {
                var backslashCount = 0;

                // Consume All Backslashes
                while (i < arg.Length && arg[i] == '\\')
                {
                    backslashCount++;
                    i++;
                }

                // Escape any backslashes at the end of the arg
                // when the argument is also quoted.
                // This ensures the outside quote is interpreted as
                // an argument delimiter
                if (i == arg.Length && isQuoted)
                {
                    sb.Append('\\', 2 * backslashCount);
                }

                // At then end of the arg, which isn't quoted,
                // just add the backslashes, no need to escape
                else if (i == arg.Length)
                {
                    sb.Append('\\', backslashCount);
                }

                // Escape any preceding backslashes and the quote
                else if (arg[i] == '"')
                {
                    sb.Append('\\', (2 * backslashCount) + 1);
                    sb.Append('"');
                }

                // Output any consumed backslashes and the character
                else
                {
                    sb.Append('\\', backslashCount);
                    sb.Append(arg[i]);
                }
            }

            if (needsQuotes) sb.Append("\"");

            return sb.ToString();
        }

        internal static bool ShouldSurroundWithQuotes(string argument)
        {
            // Don't quote already quoted strings
            if (IsSurroundedWithQuotes(argument))
            {
                return false;
            }

            // Only quote if whitespace exists in the string
            return ArgumentContainsWhitespace(argument);
        }

        internal static bool IsSurroundedWithQuotes(string argument)
        {
            return argument.StartsWith("\"", StringComparison.Ordinal) &&
                   argument.EndsWith("\"", StringComparison.Ordinal);
        }

        internal static bool ArgumentContainsWhitespace(string argument)
        {
            return argument.Contains(" ") || argument.Contains("\t") || argument.Contains("\n");
        }
    }

    // This is from https://github.com/aspnet/Scaffolding/tree/dev/src/Shared/Cli.Utils
    public struct CommandResult
    {
        public static readonly CommandResult Empty = new CommandResult();
        public ProcessStartInfo StartInfo { get; }
        public int ExitCode { get; }

        public CommandResult(ProcessStartInfo startInfo, int exitCode)
        {
            StartInfo = startInfo;
            ExitCode = exitCode;
        }
    }

    // This is MODIFIED from https://github.com/aspnet/Scaffolding/tree/dev/src/Shared/Cli.Utils
    internal class Command
    {
        private readonly Process _process;
        private bool _running = false;
        private Action<string> _stdErrorHandler;
        private Action<string> _stdOutHandler;

        internal static Command CreateDotNet(string commandName, IEnumerable<string> args, NuGetFramework framework = null, string configuration = "Debug")
        {
            return Create("dotnet.exe", new[] { commandName }.Concat(args), framework, configuration);
        }

        internal static Command Create(string commandName, IEnumerable<string> args, NuGetFramework framework = null, string configuration = "Debug")
        {
            return new Command(commandName, args, framework, configuration);
        }

        private Command(string commandName, IEnumerable<string> args, NuGetFramework framework, string configuration)
        {
            var psi = new ProcessStartInfo
            {
                FileName = commandName,
                Arguments = ArgumentEscaper.EscapeAndConcatenateArgArrayForProcessStart(args),
                UseShellExecute = false
            };

            _process = new Process
            {
                StartInfo = psi
            };
        }

        public CommandResult Execute()
        {
            ThrowIfRunning();
            _running = true;
            _process.EnableRaisingEvents = true;

            _process.OutputDataReceived += OnOutputReceived;
            _process.ErrorDataReceived += OnErrorReceived;

            _process.Start();

            _process.WaitForExit();

            var exitCode = _process.ExitCode;

            _process.OutputDataReceived -= OnOutputReceived;
            _process.ErrorDataReceived -= OnErrorReceived;

            return new CommandResult(
                _process.StartInfo,
                exitCode);
        }

        private void OnErrorReceived(object sender, DataReceivedEventArgs e)
        {
            _stdErrorHandler?.Invoke(e.Data);
        }

        private void OnOutputReceived(object sender, DataReceivedEventArgs e)
        {
            _stdOutHandler?.Invoke(e.Data);
        }

        public Command OnOutputLine(Action<string> handler)
        {
            ThrowIfRunning();
            _stdOutHandler = handler;
            return this;
        }

        public Command OnErrorLine(Action<string> handler)
        {
            ThrowIfRunning();
            _stdErrorHandler = handler;
            return this;
        }

        private void ThrowIfRunning([CallerMemberName] string memberName = null)
        {
            if (_running)
            {
                throw new InvalidOperationException($"Unable to invoke {memberName} after the command has been run");
            }
        }
    }

    public class MsBuildProjectContextBuilder
    {
        private string _projectPath;
        private string _targetLocation;
        private string _configuration;

        public MsBuildProjectContextBuilder(string projectPath, string targetsLocation, string configuration="Debug")
        {
            if (string.IsNullOrEmpty(projectPath))
            {
                throw new ArgumentNullException(nameof(projectPath));
            }

            if (string.IsNullOrEmpty(targetsLocation))
            {
                throw new ArgumentNullException(nameof(targetsLocation));
            }

            _configuration = configuration;
            _projectPath = projectPath;
            _targetLocation = targetsLocation;
        }

        public IProjectContext Build()
        {
            var errors = new List<string>();
            var output = new List<string>();
            var tmpFile = Path.GetTempFileName();
            var result = Command.CreateDotNet(
                "msbuild",
                new string[]
                {
                    _projectPath,
                    $"/t:EvaluateProjectInfoForCodeGeneration",
                    $"/p:OutputFile={tmpFile};CustomBeforeMicrosoftCSharpTargets={_targetLocation};Configuration={_configuration};BaseOutputPath=c:\\temp"
                })
                .OnErrorLine(e => errors.Add(e))
                .OnOutputLine(o => output.Add(o))
                .Execute();

            if (result.ExitCode != 0)
            {
                throw CreateProjectContextCreationFailedException(_projectPath, errors);
            }
            try
            {
                var info = File.ReadAllText(tmpFile);

                var buildContext = JsonConvert.DeserializeObject<CommonProjectContext>(info);

                return buildContext;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to read the BuildContext information.", ex);
            }
        }

        private Exception CreateProjectContextCreationFailedException(string _projectPath, List<string> errors)
        {
            var errorMsg = $"Failed to get Project Context for {_projectPath}.";
            if (errors != null)
            {
                errorMsg += $"{Environment.NewLine} { string.Join(Environment.NewLine, errors)} ";
            }

            return new InvalidOperationException(errorMsg);
        }
    }
}
