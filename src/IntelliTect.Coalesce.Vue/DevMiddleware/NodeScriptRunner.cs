// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.NodeServices.Npm;
using Microsoft.AspNetCore.NodeServices.Util;
using Microsoft.AspNetCore.SpaServices.Extensions.Util;
using Microsoft.AspNetCore.SpaServices.Util;
using Microsoft.Extensions.Logging;


namespace IntelliTect.Coalesce.Vue.DevMiddleware
{
    /// <summary>
    /// Executes the <c>script</c> entries defined in a <c>package.json</c> file,
    /// capturing any output written to stdio.
    /// </summary>
    internal class NodeScriptRunner : IDisposable
    {
        private Process? _npmProcess;
        public EventedStreamReader StdOut { get; private set;  }
        public EventedStreamReader StdErr { get; private set; }

        public event EventHandler? Exited;

        public NodeScriptRunner(
            string workingDirectory,
            string scriptName,
            string? arguments,
            IDictionary<string, string>? envVars,
            string pkgManagerCommand
        )
        {
            if (string.IsNullOrEmpty(workingDirectory))
            {
                throw new ArgumentException("Cannot be null or empty.", nameof(workingDirectory));
            }

            if (string.IsNullOrEmpty(scriptName))
            {
                throw new ArgumentException("Cannot be null or empty.", nameof(scriptName));
            }

            if (string.IsNullOrEmpty(pkgManagerCommand))
            {
                throw new ArgumentException("Cannot be null or empty.", nameof(pkgManagerCommand));
            }

            var exeToRun = pkgManagerCommand;
            var completeArguments = $"run {scriptName} -- {arguments ?? string.Empty}";
            if (OperatingSystem.IsWindows())
            {
                // On Windows, the node executable is a .cmd file, so it can't be executed
                // directly (except with UseShellExecute=true, but that's no good, because
                // it prevents capturing stdio). So we need to invoke it via "cmd /c".
                exeToRun = "cmd";
                completeArguments = $"/c {pkgManagerCommand} {completeArguments}";
            }

            var processStartInfo = new ProcessStartInfo(exeToRun)
            {
                Arguments = completeArguments,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = workingDirectory,
                Environment =
                {
                    // The PID of the parent aspnetcore app is passed so that the vite server
                    // can shut itself down when the parent shuts down.
                    ["ASPNETCORE_VITE_PID"] = Environment.ProcessId.ToString()
                }
            };

            if (envVars != null)
            {
                foreach (var keyValuePair in envVars)
                {
                    processStartInfo.Environment[keyValuePair.Key] = keyValuePair.Value;
                }
            }

            _npmProcess = LaunchNodeProcess(processStartInfo, pkgManagerCommand);
            StdOut = new EventedStreamReader(_npmProcess.StandardOutput);
            StdErr = new EventedStreamReader(_npmProcess.StandardError);
            _npmProcess.Exited += (sender, e) =>
            {
                Dispose();
                Exited?.Invoke(sender, e);
            };
        }

        public void AttachToLogger(ILogger logger)
        {
            // When the node task emits complete lines, pass them through to the real logger.

            // Set the console output encoding to a unicode format so that glyphs like ➜
            // that vite outputs aren't obliterated by the conhost.exe (it still can't render them,
            // but this lets them show up as "?" rather than "Γ₧£", and lets them work properly in wt/pwsh/etc.)
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            StdOut.OnReceivedLine += line =>
            {
                Console.Write("\u001b[32mvite\u001b[0m: " + line);
            };

            StdErr.OnReceivedLine += line =>
            {
                Console.Write("\u001b[31mvite\u001b[0m: " + line);
            };
        }

        private Process LaunchNodeProcess(ProcessStartInfo startInfo, string commandName)
        {
            try
            {
                var process = Process.Start(startInfo)!;
                process.EnableRaisingEvents = true;

                return process;
            }
            catch (Exception ex)
            {
                var message = $"Failed to start '{commandName}'. To resolve this:.\n\n"
                            + $"[1] Ensure that '{commandName}' is installed and can be found in one of the PATH directories.\n"
                            + $"    Current PATH enviroment variable is: { Environment.GetEnvironmentVariable("PATH") }\n"
                            + "    Make sure the executable is in one of those directories, or update your PATH.\n\n"
                            + "[2] See the InnerException for further details of the cause.";
                throw new InvalidOperationException(message, ex);
            }
        }

        public void Dispose()
        {
            if (_npmProcess != null && !_npmProcess.HasExited)
            {
                _npmProcess.Kill(entireProcessTree: true);
                _npmProcess = null;
            }
        }

        ~NodeScriptRunner()
        {
            Dispose();
        }
    }
}
