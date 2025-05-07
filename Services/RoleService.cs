
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AvaloniaPdbAccounts.Models; // Import model
using Avalonia.Controls.Primitives;
using System.Data;
using System;
using System.Linq;


namespace AvaloniaPdbAccounts.Services;
public class RoleService
{
    private readonly string _connectionString;

    public RoleService(string connectionString= DatabaseSettings.ConnectionString)
    {
        _connectionString = connectionString;
    }
    public async Task<ObservableCollection<string>> RLoadRolesAsync()
        {
            var accounts = new ObservableCollection<string>();

            try
            {
                string connectionString = _connectionString;

                using (var conn = new OracleConnection(connectionString))
                {
                    await conn.OpenAsync();

                    using (var cmd = new OracleCommand("SELECT role FROM dba_roles WHERE common = 'NO' GROUP BY role", conn))
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            accounts.Add(reader.GetString(0));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                accounts.Add($"Error: {ex.Message}");
            }

            return accounts;
    }
    
    public async Task<ObservableCollection<PrivilegeItem>> GetRolePrivilegesAsync(string role)
        {
            var privileges = new ObservableCollection<PrivilegeItem>();

            try
            {
                string connectionString = _connectionString;

                using (var conn = new OracleConnection(connectionString))
                {
                    await conn.OpenAsync();

                    // Define a list of basic system privileges
                    var basicPrivileges = new List<string>
                            {
                                "SELECT",
                                "INSERT",
                                "UPDATE",
                                "DELETE",
                                "EXECUTE",
                                "ALTER",
                                "CREATE",
                                "DROP",
                                "GRANT",
                                "REVOKE"
                            };

                    // Add basic privileges to the privileges collection
                    foreach (var privilegeName in basicPrivileges)
                    {
                        if (!privileges.Any(p => p.Name == privilegeName))
                        {
                            privileges.Add(new PrivilegeItem { Name = privilegeName, IsGranted = false });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                privileges.Add(new PrivilegeItem { Name = $"Error: {ex.Message}", IsGranted = false });
            }

            return privileges;
        }

        public async Task UpdateRolePrivilegesAsync(string role, ObservableCollection<PrivilegeItem> privileges)
        {
            try
            {
                string connectionString = _connectionString;

                using (var conn = new OracleConnection(connectionString))
                {
                    await conn.OpenAsync();

                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            foreach (var privilege in privileges)
                            {
                                Console.WriteLine($"Privilege: {privilege.Name}");
                                var sql = privilege.IsGranted
                                    ? $"GRANT \"{privilege.Name}\" TO \"{role}\""
                                    : $"REVOKE \"{privilege.Name}\" FROM \"{role}\"";

                                using (var cmd = new OracleCommand(sql, conn))
                                {
                                    cmd.Transaction = transaction;
                                    await cmd.ExecuteNonQueryAsync();
                                }
                            }

                            await transaction.CommitAsync();
                        }
                        catch (Exception innerEx)
                        {
                            await transaction.RollbackAsync();
                            throw new Exception($"Transaction failed: {innerEx.Message}", innerEx);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                privileges.Add(new PrivilegeItem { Name = $"Error: {ex.Message}", IsGranted = false });
            }
        }



}