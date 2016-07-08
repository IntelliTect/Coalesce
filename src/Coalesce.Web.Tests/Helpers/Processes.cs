using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Coalesce.Web.Tests.Helpers
{
    public static class Processes
    {
        public static Process StartDotNet()
        {
            var startInfo = new ProcessStartInfo();
            startInfo.Arguments = $@"run";
            startInfo.FileName = "dotnet";
            startInfo.WorkingDirectory = @"..\..\..\..\..\Coalesce.Web";
            startInfo.UseShellExecute = false;

            var process = new Process();
            process.StartInfo = startInfo;
            process.Start();

            // Give it a few seconds to start up.
            Thread.Sleep(7000);

            return process;
        }
    }
}
