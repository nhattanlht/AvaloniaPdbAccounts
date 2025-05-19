using System;
using System.Diagnostics;
using System.IO;

namespace AvaloniaPdbAccounts.Utilities
{
    public static class RunScriptBatch
    {
        public static void RunScriptFile()
        {
            Console.WriteLine("Starting run script file...");
            string scriptPath;
            string shell;
            string args;

            if (OperatingSystem.IsWindows())
            {
                string projectDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"../../../"));
                scriptPath = Path.Combine(projectDir, "Script", "run_all.bat");
                shell = "cmd.exe";
                args = $"/C \"{scriptPath}\"";
            }
            else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
            {
                string projectDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"../../../"));
                scriptPath = Path.Combine(projectDir, "Script", "run_all.sh");
                shell = "/bin/bash";
                args = $"bash -c \"{scriptPath}\"";
            }
            else
            {
                throw new PlatformNotSupportedException();
            }

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = shell,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            Console.WriteLine("Script path: " + args);
            process.Start();
            Console.WriteLine($"Started sqlplus process (PID: {process.Id})");

            Console.WriteLine("Processing connect sqlplus...");
            string output = process.StandardOutput.ReadToEnd();
            Console.WriteLine("Read file successfully");

            string error = process.StandardError.ReadToEnd();
            Console.WriteLine("Loading error");

            process.WaitForExit();

            Console.WriteLine("Output:\n" + output);
            if (!string.IsNullOrWhiteSpace(error))
                Console.WriteLine("Error:\n" + error);
        }

    }
}
