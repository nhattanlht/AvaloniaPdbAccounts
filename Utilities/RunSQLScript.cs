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
    public static void RunAllSql()
    {
        RunSqlScript("script.sql");
        RunSqlScript("database.sql");
    }
    public static void RunSqlScript(string fileName)
    {
        string projectDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"../../../"));
        string scriptPath = Path.Combine(projectDir, "Script", fileName);

        Console.WriteLine($"Running script: {scriptPath}");

        string connectionString = AppConfig.GetConnectionString();
        Console.WriteLine("Using connection string: " + connectionString);

        RunProcessSqlPlus(connectionString, scriptPath);
    }

    private static void RunProcessSqlPlus(string connectionString, string sqlFilePath)
    {
        if (!File.Exists(sqlFilePath))
        {
            Console.WriteLine($"SQL file does not exist: {sqlFilePath}");
            return;
        }

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "sqlplus",
                Arguments = $"{connectionString} as sysdba @{sqlFilePath}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        Console.WriteLine($"Running sqlplus for file: {sqlFilePath}");

        process.Start();

        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();

        process.WaitForExit();

        Console.WriteLine($"Output for {Path.GetFileName(sqlFilePath)}:\n{output}");
        if (!string.IsNullOrWhiteSpace(error))
            Console.WriteLine($"Error for {Path.GetFileName(sqlFilePath)}:\n{error}");
    }
}
