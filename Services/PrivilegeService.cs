

using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using AvaloniaPdbAccounts.Models; // Import model
using System;

namespace AvaloniaPdbAccounts.Services;
public class PrivilegeService
{   
     private string _lastGrantee = "";
    private string _lastType = "";
    private List<Dictionary<string, object>> _lastPermissions = new();
    private readonly string _connectionString;

    public PrivilegeService(string connectionString = DatabaseSettings.ConnectionString)
    {
        _connectionString = connectionString;
    }

    public async Task GrantPrivilegeAsync(
    string grantee,
    string objectType,
    string objectName,
    string privilege,
    bool withGrantOption,
    string columnName = "")
    {
        using var conn = new OracleConnection(_connectionString);
        await conn.OpenAsync();

    string grantSql = objectType switch
    {
        "TABLE" or "VIEW" =>
            (!string.IsNullOrEmpty(columnName) &&
            columnName != "Tất cả" &&
            (privilege == "SELECT" || privilege == "UPDATE"))
                ? $"GRANT {privilege} ({columnName}) ON {objectName} TO {grantee}"
                : $"GRANT {privilege} ON {objectName} TO {grantee}",

        "PROCEDURE" or "FUNCTION" =>
            $"GRANT EXECUTE ON {objectName} TO {grantee}",

        _ => throw new Exception("Loại đối tượng không hợp lệ")
    };




        if (withGrantOption)
            grantSql += " WITH GRANT OPTION";

        using var cmd = new OracleCommand(grantSql, conn);
        await cmd.ExecuteNonQueryAsync();

    }

    public async Task RevokePrivilegeAsync(
        string grantee,
        string objectType,
        string objectName,
        string privilege,
        bool withGrantOption,
        string columnName = "")
    {
        using var conn = new OracleConnection(_connectionString);
        await conn.OpenAsync();

        string revokeSql = objectType switch
        {
            "TABLE" or "VIEW" =>
                (!string.IsNullOrEmpty(columnName) &&
                columnName != "Tất cả" &&
                (privilege == "SELECT" || privilege == "UPDATE"))
                    ? $"REVOKE {privilege} ({columnName}) ON {objectName} FROM {grantee}"
                    : $"REVOKE {privilege} ON {objectName} FROM {grantee}",

            "PROCEDURE" or "FUNCTION" =>
                $"REVOKE EXECUTE ON {objectName} FROM {grantee}",

            _ => throw new Exception("Loại đối tượng không hợp lệ")
        };

        if (withGrantOption)
            revokeSql += " WITH GRANT OPTION";

        using var cmd = new OracleCommand(revokeSql, conn);
        await cmd.ExecuteNonQueryAsync();
    }
}