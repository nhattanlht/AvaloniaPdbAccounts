namespace AvaloniaPdbAccounts.Models;

public class DatabaseSettings
{
    // public static string ConnectionString = "User Id=SYSTEM;Password=123456;Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=QLNHANVIEN)));";
    public const string ConnectSystem = "User Id=AdminPdb;Password=123;Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=PDB)));";

    public static string ConnectionString = "User Id=AdminPdb;Password=123;Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=PDB)));";

    public static string GetConnectionString()
    {
        return ConnectionString;
    }

    public string GetConnectionString(string userId, string password, string host, string port, string serviceName)
    {
        ConnectionString = $"User Id={userId};Password={password};Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST={host})(PORT={port}))(CONNECT_DATA=(SERVICE_NAME={serviceName})));";
        return ConnectionString;
    }
    public string GetConnectionString(string userId, string password)
    {
        ConnectionString = $"User Id={userId};Password={password};Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=PDB)));";
        return ConnectionString;
    }

}