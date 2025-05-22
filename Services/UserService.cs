

using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AvaloniaPdbAccounts.Models; // Import model

using System.Data;
using System;
using Oracle.ManagedDataAccess.Client;

namespace AvaloniaPdbAccounts.Services;



public class UserService
{
    private readonly string _connectionString;

    public UserService()
    {
        _connectionString = DatabaseSettings.GetConnectionString();
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
    // Validate username
    // if (string.IsNullOrWhiteSpace(username) || 
    //     !Regex.IsMatch(username, @"^[A-Za-z][A-Za-z0-9_]{1,29}$"))
    //     throw new ArgumentException("Invalid username. Only letters, digits, underscore, starting with a letter, max 30 chars.");

    Console.WriteLine($"Creating user: {username}");
    Console.WriteLine($"With password: {password}");

    using (var conn = new OracleConnection(_connectionString))
    {
        await conn.OpenAsync();

        // Escape identifiers with double quotes to preserve case (optional)
        string quotedUsername = $"\"{username.ToUpper()}\"";

        using (var cmdCreate = new OracleCommand(
            $"CREATE USER {quotedUsername} IDENTIFIED BY \"{password}\"", conn))
        {
            await cmdCreate.ExecuteNonQueryAsync();
        }

        using (var cmdGrant = new OracleCommand(
            $"GRANT CONNECT, RESOURCE TO {quotedUsername}", conn))
        {
            await cmdGrant.ExecuteNonQueryAsync();
        }

        // using (var cmdQuota = new OracleCommand(
        //     $"ALTER USER {quotedUsername} QUOTA UNLIMITED ON USERS", conn))
        // {
        //     await cmdQuota.ExecuteNonQueryAsync();
        // }
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
            Console.WriteLine(users);
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


public async Task<List<Employee>> GetEmployeeDataAsync()
{
    var employees = new List<Employee>();
    
    using (var conn = new OracleConnection(_connectionString))
    {
        await conn.OpenAsync();
        
        string query = "SELECT MANV, VAITRO FROM NHANVIEN";
        
        using (var cmd = new OracleCommand(query, conn))
        using (var reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                employees.Add(new Employee
                {
                    MANV = reader.GetString(0),
                    Role = reader.GetString(1)
                });
            }
        }
    }
    
    return employees;
}


    public async Task<List<Student>> GetStudentDataAsync()
    {
        var students = new List<Student>();
        
        using (var conn = new OracleConnection(_connectionString))
        {
            await conn.OpenAsync();
            
            // Query to get student data (MASV)
            string query = "SELECT MASV FROM SINHVIEN";
            
            using (var cmd = new OracleCommand(query, conn))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    students.Add(new Student
                    {
                        MASV = reader.GetString(0)
                    });
                }
            }
        }
        
        return students;
    }

    public async Task<List<EmployeeModel>> GetEmployeeModelDataAsync()
    {
        var employees = new List<EmployeeModel>();

        using (var conn = new OracleConnection(_connectionString))
        {
            await conn.OpenAsync();

            string query = "SELECT MANV, HOTEN, PHAI, NGSINH, LUONG, PHUCAP, DT, VAITRO, MADV FROM adminpdb.NHANVIEN";

            using (var cmd = new OracleCommand(query, conn))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    employees.Add(new EmployeeModel
                    {
                        EmployeeID = reader.GetString(0),
                        FullName = reader.GetString(1),
                        Gender = reader.GetString(2),
                        BirthDate = reader.GetDateTime(3),
                        Salary = reader.GetDecimal(4),
                        Allowance = reader.GetDecimal(5),
                        Phone = reader.GetString(6),
                        Role = reader.GetString(7),
                        Department = reader.GetString(8),
                    });
                }
            }
        }

        return employees;
    }
    public async Task UpdateEmployeePhoneNumberAsync(string employeeId, string newPhone)
    {
        using (var conn = new OracleConnection(_connectionString))
        {
            await conn.OpenAsync();

            string query = "UPDATE adminpdb.NHANVIEN SET DT = :phone WHERE MANV = :id";

            using (var cmd = new OracleCommand(query, conn))
            {
                cmd.Parameters.Add(new OracleParameter("phone", newPhone));
                cmd.Parameters.Add(new OracleParameter("id", employeeId));

                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}