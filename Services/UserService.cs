

using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AvaloniaPdbAccounts.Models; // Import model

using System.Data;
using System;
using System.Linq;

namespace AvaloniaPdbAccounts.Services;
public class UserService
{
    private readonly string _connectionString;

    public UserService(string connectionString= DatabaseSettings.ConnectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<ObservableCollection<string>> GetUsersAndRolesAsync()
    {
        var result = new ObservableCollection<string>();
        
        using (var conn = new OracleConnection(_connectionString))
        {
            await conn.OpenAsync();
            
            // Get users
            var users = await GetUsersAsync(conn);
            // Get roles
            var roles = await GetRolesAsync(conn);
            
            foreach (var user in users) result.Add(user);
            foreach (var role in roles) result.Add(role);
        }
        
        return result;
    }

    public async Task<List<string>> GetUsersAsync(OracleConnection conn)
    {
        var users = new List<string>();
        using (var cmd = new OracleCommand(
            "SELECT username FROM dba_users WHERE common = 'NO' ORDER BY username", conn))
        using (var reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                users.Add(reader.GetString(0));
            }
        }
        return users;
    }

    public async Task<List<string>> GetRolesAsync(OracleConnection conn)
    {
        // Implement similar logic for roles
        return new List<string>();
    }

public async Task CreateUserAsync(string username, string password)
{
    using (var conn = new OracleConnection(_connectionString))
    {
        await conn.OpenAsync();
        
        // Use parameterized queries to prevent SQL injection
        using (var cmdCreate = new OracleCommand(
            "CREATE USER :username IDENTIFIED BY :password", conn))
        {
            cmdCreate.Parameters.Add("username", OracleDbType.Varchar2).Value = username;
            cmdCreate.Parameters.Add("password", OracleDbType.Varchar2).Value = password;
            await cmdCreate.ExecuteNonQueryAsync();
        }

        using (var cmdGrant = new OracleCommand(
            "GRANT CONNECT, RESOURCE TO :username", conn))
        {
            cmdGrant.Parameters.Add("username", OracleDbType.Varchar2).Value = username;
            await cmdGrant.ExecuteNonQueryAsync();
        }

        // Optionally grant additional privileges if needed
        using (var cmdQuota = new OracleCommand(
            "ALTER USER :username QUOTA UNLIMITED ON USERS", conn))
        {
            cmdQuota.Parameters.Add("username", OracleDbType.Varchar2).Value = username;
            await cmdQuota.ExecuteNonQueryAsync();
        }
    }
}

    public async Task DeleteUserAsync(string username)
    {
        using (var conn = new OracleConnection(_connectionString))
        {
            await conn.OpenAsync();
            using (var cmd = new OracleCommand(
                $"DROP USER {username} CASCADE", conn))
            {
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }

    public async Task ChangePasswordAsync(string username, string newPassword)
    {
        using (var conn = new OracleConnection(_connectionString))
        {
            await conn.OpenAsync();
            using (var cmd = new OracleCommand(
                $"ALTER USER {username} IDENTIFIED BY \"{newPassword}\"", conn))
            {
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
     public async Task<List<string>> GetAllUsersAsync(OracleConnection conn)
        {
            var users = new List<string>();
            using (var cmd = new OracleCommand("SELECT username FROM dba_users WHERE common = 'NO' ORDER BY username", conn))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    users.Add(reader.GetString(0));
                }
            }
            return users;
        }



        public async Task<DataTable?> QueryPrivilegesAsync(OracleConnection conn, string selectedType, string selectedName)
        {
            string query = selectedType switch
            {
                "ROLE" => "SELECT * FROM DBA_ROLE_PRIVS WHERE GRANTEE = :grantee",
                "SYSTEM" => "SELECT * FROM DBA_SYS_PRIVS WHERE GRANTEE = :grantee",
                "TABLE" => "SELECT * FROM DBA_TAB_PRIVS WHERE GRANTEE = :grantee",
                "COL" => "SELECT * FROM DBA_COL_PRIVS WHERE GRANTEE = :grantee",
                _ => throw new Exception("Loại quyền không hợp lệ")
            };

            var dt = new DataTable();
            using (var cmd = new OracleCommand(query, conn))
            {
                cmd.Parameters.Add(new OracleParameter("grantee", selectedName));
                using (var adapter = new OracleDataAdapter(cmd))
                {
                    adapter.Fill(dt);
                }
            }
            await Task.CompletedTask;
            return dt;
        }

     public async Task<ObservableCollection<string>> GetAccountsAsync(){
            var accounts = new ObservableCollection<string>();

            try{
                string connectionString = _connectionString;

                using (var conn = new OracleConnection(connectionString)){
                    await conn.OpenAsync();

                    using (var cmd = new OracleCommand("SELECT username FROM dba_users WHERE common = 'NO' ORDER BY username", conn))
                    using (var reader = await cmd.ExecuteReaderAsync()){
                        while (await reader.ReadAsync())
                        {
                            accounts.Add(reader.GetString(0));
                        }
                    }
                }
            }
            catch (Exception ex){
                accounts.Add($"Error: {ex.Message}");
            }

            return accounts;
        }

    public static implicit operator UserService(UserManagementViewModel v)
    {
        throw new NotImplementedException();
    }
}