

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
        
        string query = "SELECT MANLD, VAITRO FROM NHANVIEN";
        
        using (var cmd = new OracleCommand(query, conn))
        using (var reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                employees.Add(new Employee
                {
                    MANLD = reader.GetString(0),
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
    //Xử lí cho trang employee
    public async Task<List<EmployeeModel>> GetEmployeeModelDataAsync()
    {
        var employees = new List<EmployeeModel>();

        using (var conn = new OracleConnection(_connectionString))
        {
            await conn.OpenAsync();

            string query = "SELECT MANLD, HOTEN, PHAI, NGSINH, LUONG, PHUCAP, DT, VAITRO, MADV FROM adminpdb.NHANVIEN_NVCB";

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
    public async Task<List<EmployeeModel>> GetManagedEmployeeAsync()
    {
        var employees = new List<EmployeeModel>();

        using (var conn = new OracleConnection(_connectionString))
        {
            await conn.OpenAsync();

            string query = "SELECT MANLD, HOTEN, PHAI, NGSINH, LUONG, PHUCAP, DT, VAITRO, MADV FROM adminpdb.NHANVIEN_TRGDV";

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

            string query = "UPDATE adminpdb.NHANVIEN_NVCB SET DT = :phone WHERE MANLD = :id";

            using (var cmd = new OracleCommand(query, conn))
            {
                cmd.Parameters.Add(new OracleParameter("phone", newPhone));
                cmd.Parameters.Add(new OracleParameter("id", employeeId));

                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
    //Xử lí cho trang registration
    public async Task<List<RegistrationModel>> GetRegistrationModelDataAsync()
    {
        var registrations = new List<RegistrationModel>();

        using (var conn = new OracleConnection(_connectionString))
        {
            await conn.OpenAsync();

            string query = "SELECT MASV, MAMM, DIEMTH, DIEMQT, DIEMCK, DIEMTK FROM adminpdb.DANGKY";

            using (var cmd = new OracleCommand(query, conn))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    registrations.Add(new RegistrationModel
                    {
                        StudentID = reader.GetString(0),
                        CourseID = reader.GetString(1),
                        PracticeScore = reader.GetDecimal(2),
                        ProcessScore = reader.GetDecimal(3),
                        FinalScore = reader.GetDecimal(4),
                        TotalScore = reader.GetDecimal(5),
                    });
                }
            }
        }

        return registrations;
    }
    public async Task UpdateRegistrationScoreAsync(string studentId,string courseId, decimal practiceScore, decimal processScore, decimal finalScore, decimal totalScore)
    {
        using (var conn = new OracleConnection(_connectionString))
        {
            await conn.OpenAsync();

            string query = "UPDATE adminpdb.DANGKY SET DIEMTH = :practiceScore, DIEMQT = :processScore, DIEMCK = :finalScore, DIEMTK = :totalScore WHERE MASV = :studentId and MAMM = :courseId";

            using (var cmd = new OracleCommand(query, conn))
            {
                cmd.Parameters.Add(new OracleParameter("practiceScore", practiceScore));
                cmd.Parameters.Add(new OracleParameter("processScore", processScore));
                cmd.Parameters.Add(new OracleParameter("finalScore", finalScore));
                cmd.Parameters.Add(new OracleParameter("totalScore", totalScore));
                cmd.Parameters.Add(new OracleParameter("studentId", studentId));
                cmd.Parameters.Add(new OracleParameter("courseId", courseId));

                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
    //Xử lí cho trang course
    public async Task<List<CourseOfferingModel>> GetCourseOfferingNVPDTAsync()
    {
        var courses = new List<CourseOfferingModel>();
        using (var conn = new OracleConnection(_connectionString))
        {
            await conn.OpenAsync();
            string query = "SELECT MAMM, MAHP, MAGV, HK, NAM FROM adminpdb.MOMON_PDT";
            using (var cmd = new OracleCommand(query, conn))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    courses.Add(new CourseOfferingModel
                    {
                        OfferingID = reader.GetString(0),
                        ModuleID = reader.GetString(1),
                        InstructorID = reader.GetString(2),
                        Semester = reader.GetInt32(3),
                        Year = reader.GetInt32(4),
                    });
                }
            }
        }
        return courses;
    }
    public async Task UpdateCourseOfferingNVPDTAsync(CourseOfferingModel course)
    {
        using (var conn = new OracleConnection(_connectionString))
        {
            await conn.OpenAsync();
            string query = "UPDATE adminpdb.MOMON_PDT SET MAHP  = :moduleId, MAGV = :instructorId, HK = :semester, NAM = :year WHERE MAMM = :offeringId";

            using (var cmd = new OracleCommand(query, conn))
            {
                cmd.Parameters.Add(new OracleParameter("offeringId", course.OfferingID));
                cmd.Parameters.Add(new OracleParameter("moduleId", course.ModuleID));
                cmd.Parameters.Add(new OracleParameter("instructorId", course.InstructorID));
                cmd.Parameters.Add(new OracleParameter("semester", course.Semester));
                cmd.Parameters.Add(new OracleParameter("year", course.Year));

                await cmd.ExecuteNonQueryAsync();
            }
        }
    }

    public async Task DeleteCourseOfferingNVPDTAsync(string offeringId)
    {
        using (var conn = new OracleConnection(_connectionString))
        {
            await conn.OpenAsync();
            string query = "DELETE FROM adminpdb.MOMON_PDT WHERE MASV = :offeringId";

            using (var cmd = new OracleCommand(query, conn))
            {
                cmd.Parameters.Add(new OracleParameter("offeringId", offeringId));

                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}