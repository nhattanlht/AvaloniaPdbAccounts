using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Configuration;
namespace AvaloniaPdbAccounts.Utilities;

public static class AppConfig
{
    public static IConfigurationRoot Configuration { get; }

    static AppConfig()
    {
        Configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true)
            .Build();
    }

    public static string GetConnectionString()
    {
        return Configuration["Database:ConnectionString"];
    }
}

public static class RunSQLScriptUtility
{
    public static void RunSqlScript()
    {   
        Console.WriteLine("Starting run script file...");

        string projectDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"../../../"));
        string scriptPath = Path.Combine(projectDir, "Script", "script.sql");
        Console.WriteLine($"Script path: {scriptPath}");

        if (string.IsNullOrEmpty(scriptPath))
        {
            Console.WriteLine("Script path is null or empty.");
            return;
        }
        string connectionString = AppConfig.GetConnectionString(); // sửa lại cho đúng

        Console.WriteLine("Getting the connection string successfully" + connectionString);

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "sqlplus",
                Arguments = $"{connectionString} as sysdba @{scriptPath}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        Console.WriteLine($"Started sqlplus process (PID: {process.Id})");

        Console.WriteLine("Processing connect sqlplus...");

        string output = process.StandardOutput.ReadToEnd();
        Console.WriteLine("Read file successfully");


        string error = process.StandardError.ReadToEnd();
        Console.WriteLine("Loading error");

        process.WaitForExit();
        Console.WriteLine("Exitting process");

        Console.WriteLine("Output:\n" + output);
        if (!string.IsNullOrWhiteSpace(error))
            Console.WriteLine("Error:\n" + error);
    }
}
