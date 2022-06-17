// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SpaServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;


namespace IntelliTect.Coalesce.Vue.DevMiddleware
{
    internal static class ViteDevelopmentServerMiddleware
    {
        private const string LogCategoryName = "IntelliTect.Coalesce.Vue";

        public static Task<int> Attach(
            ISpaBuilder spaBuilder,
            ViteServerOptions options
        )
        {
            var pkgManagerCommand = spaBuilder.Options.PackageManagerCommand;
            var sourcePath = spaBuilder.Options.SourcePath;
            var devServerPort = options.DevServerPort;
            if (string.IsNullOrEmpty(sourcePath))
            {
                throw new ArgumentException("Property 'SourcePath' cannot be null or empty", nameof(spaBuilder));
            }

            if (string.IsNullOrEmpty(options.NpmScriptName))
            {
                throw new ArgumentException("Cannot be null or empty", nameof(options.NpmScriptName));
            }

            // Start vite and attach to middleware pipeline
            var appBuilder = spaBuilder.ApplicationBuilder;
            var applicationStoppingToken = appBuilder.ApplicationServices.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping;
            var logger = GetOrCreateLogger(appBuilder);
            var portTask = StartViteServerAsync(
                options,
                sourcePath,
                pkgManagerCommand,
                devServerPort,
                logger,
                applicationStoppingToken);

            SpaProxyingExtensions.UseProxyToSpaDevelopmentServer(spaBuilder, async () =>
            {
                // On each request, we create a separate startup task with its own timeout. That way, even if
                // the first request times out, subsequent requests could still work.
                var timeout = spaBuilder.Options.StartupTimeout;
                var port = await portTask.WaitAsync(timeout);

                return new UriBuilder(options.UseHttps ? "https" : "http", "localhost", port).Uri;
            });

            return portTask;
        }

        internal static ILogger GetOrCreateLogger(IApplicationBuilder appBuilder)
        {
            // If the DI system gives us a logger, use it. Otherwise, set up a default one
            var loggerFactory = appBuilder.ApplicationServices.GetService<ILoggerFactory>();
            return loggerFactory?.CreateLogger(LogCategoryName) ?? NullLogger.Instance;
        }

        private static int FindAvailablePort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            try
            {
                return ((IPEndPoint)listener.LocalEndpoint).Port;
            }
            finally
            {
                listener.Stop();
            }
        }

        private static async Task<int> StartViteServerAsync(
            ViteServerOptions options,
            string sourcePath,
            string pkgManagerCommand,
            int? portNumber,
            ILogger logger,
            CancellationToken applicationStoppingToken)
        {
            if (!portNumber.HasValue || portNumber == 0)
            {
                portNumber = FindAvailablePort();
            }
            logger.LogInformation($"Starting vite server on port {portNumber}...");

            var scriptRunner = new NodeScriptRunner(
                sourcePath,
                options.NpmScriptName,
                $" --port {portNumber}",
                null,
                pkgManagerCommand,
                applicationStoppingToken);
            scriptRunner.AttachToLogger(logger);

            using (var stdErrReader = new EventedStreamStringReader(scriptRunner.StdErr))
            {
                try
                {
                    var match = await scriptRunner.StdOut.WaitForMatch(
                        new Regex(options.OutputOnReady, RegexOptions.None, TimeSpan.FromSeconds(2)));
                    if (match.Groups.Count > 0)
                    {
                        return int.Parse(match.Groups[0].Value);
                    }
                }
                catch (EndOfStreamException ex)
                {
                    throw new InvalidOperationException(
                        $"The {pkgManagerCommand} script '{options.NpmScriptName}' exited without indicating that the " +
                        "server was listening for requests. The error output was: " +
                        $"{stdErrReader.ReadAsString()}", ex);
                }
            }

            return portNumber.Value;
        }
    }



}
