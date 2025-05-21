using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using AvaloniaPdbAccounts.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using AvaloniaPdbAccounts.Utilities;

namespace AvaloniaPdbAccounts.Services
{
    public sealed class DatabaseService
    {
        private static readonly Lazy<DatabaseService> _instance =
            new(() => new DatabaseService());

        private OracleConnection? _connection;
        public bool IsConnected => _connection?.State == ConnectionState.Open;

        public static DatabaseService Instance => _instance.Value;

        public List<string> UserRole { get; }

        // Danh sách role hiện tại của user sau khi login
        public List<Role> RolesList { get; private set; } = new();
        public static bool system = false;

        private DatabaseService()
        {
            // Khởi tạo kết nối ban đầu (chưa mở)
            _connection = new OracleConnection(DatabaseSettings.GetConnectionString());
        }

        /// <summary>
        /// Đăng nhập với user/password, kết nối và tải thông tin role hiện tại.
        /// </summary>
public static List<Role> CurrentRoles { get; set; } = new List<Role>();

        public void Login(string? userId = null, string? password = null)
        {
            if (_connection != null && _connection.State == ConnectionState.Open)
            {
                _connection.Close();
            }

            string connectionString;

            if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(password))
            {
                var settings = new DatabaseSettings();
                connectionString = settings.GetConnectionString(userId, password);
            }
            else
            {
                connectionString = DatabaseSettings.GetConnectionString();
            }

            if (_connection == null)
                throw new InvalidOperationException("Database connection is not initialized.");

            _connection.ConnectionString = connectionString;
            _connection.Open();

            var builder = new OracleConnectionStringBuilder(connectionString);
            if (builder.UserID.Equals("AdminPdb", StringComparison.OrdinalIgnoreCase))
            {
                system = true;
            }

            Console.WriteLine(system);

            List<Role> CurrentRoles = GetCurrentRoles();
        }
        public List<Role> GetCurrentRoles()
        {
            var roles = new List<Role>();

            using var cmd = CreateCommand();
            cmd.CommandText = "SELECT GRANTED_ROLE FROM USER_ROLE_PRIVS";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string roleName = reader.GetString(0);
                Console.WriteLine(roleName); // In ra role
                roles.Add(new Role { RoleName = roleName });
            }

            return roles;
        }




        /// <summary>
        /// Lấy danh sách các role của user hiện tại từ USER_ROLE_PRIVS.
        /// </summary>
        public void GetUserRolesList()
        {
            if (!IsConnected)
                throw new InvalidOperationException("Database connection is not open.");

            var roles = new List<Role>();

            using var cmd = CreateCommand();
            cmd.CommandText = "SELECT GRANTED_ROLE FROM USER_ROLE_PRIVS";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var roleName = reader.GetString(0);
                roles.Add(new Role
                {
                    RoleName = roleName,
                    Description = "" // TODO: Có thể bổ sung thêm mô tả nếu cần
                });
            }

            RolesList = roles;
        }

        /// <summary>
        /// Tạo lệnh OracleCommand mới, kiểm tra kết nối trước khi tạo.
        /// </summary>
        public OracleCommand CreateCommand()
        {
            if (_connection == null || _connection.State != ConnectionState.Open)
                throw new InvalidOperationException("Database connection is not open. Please login first.");

            return _connection.CreateCommand();
        }

        public DataTable ExecuteQuery(string sql)
        {
            using var command = CreateCommand();
            command.CommandText = sql;

            var dataTable = new DataTable();
            using var adapter = new OracleDataAdapter(command);
            adapter.Fill(dataTable);

            return dataTable;
        }

        public int ExecuteNonQuery(string sql)
        {
            using var command = CreateCommand();
            command.CommandText = sql;
            return command.ExecuteNonQuery();
        }

        public object ExecuteScalar(string sql)
        {
            using var command = CreateCommand();
            command.CommandText = sql;
            return command.ExecuteScalar();
        }

        public OracleTransaction BeginTransaction()
        {
            if (!IsConnected)
                throw new InvalidOperationException("Database connection is not open.");
            if (_connection == null)
                throw new InvalidOperationException("Database connection is not initialized.");

            return _connection.BeginTransaction();
        }

        public async Task<DataTable> ExecuteQueryAsync(string sql, params OracleParameter[] parameters)
        {
            using var cmd = CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.AddRange(parameters);

            var dataTable = new DataTable();
            using var adapter = new OracleDataAdapter(cmd);
            await Task.Run(() => adapter.Fill(dataTable));

            return dataTable;
        }

        public async Task<int> ExecuteNonQueryAsync(string sql, params OracleParameter[] parameters)
        {
            using var cmd = CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.AddRange(parameters);

            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<object> ExecuteScalarAsync(string sql, params OracleParameter[] parameters)
        {
            using var cmd = CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.AddRange(parameters);

            var result = await cmd.ExecuteScalarAsync();
            return result ?? throw new InvalidOperationException("ExecuteScalarAsync returned null.");
        }

        public void Dispose()
        {
            _connection?.Dispose();
            _connection = null;
        }
    }
}
