using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AvaloniaPdbAccounts.Models; // Import model

using System.Data;
using System;
using Oracle.ManagedDataAccess.Client;
using System.Linq.Expressions;

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

    public async Task<ObservableCollection<string>> GetAccountsAsync()
    {
        var accounts = new ObservableCollection<string>();

        try
        {
            string connectionString = _connectionString;

            using (var conn = new OracleConnection(connectionString))
            {
                await conn.OpenAsync();

                using (var cmd = new OracleCommand("SELECT username FROM dba_users WHERE common = 'NO' ORDER BY username", conn))
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

            string query = "SELECT MANLD, HOTEN, PHAI, NGSINH, LUONG, PHUCAP, DT, VAITRO, MADV FROM adminpdb.NHANVIEN";

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
    public async Task<List<StudentModel>> GetStudentModelDataAsync()
    {
        var students = new List<StudentModel>();

        using (var conn = new OracleConnection(_connectionString))
        {
            await conn.OpenAsync();

            string query = "SELECT MASV, HOTEN, PHAI, NGSINH, DCHI, DT, KHOA, TINHTRANG FROM ADMINPDB.SINHVIEN";


            using (var cmd = new OracleCommand(query, conn))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    students.Add(new StudentModel
                    {
                        ID = reader.GetString(0),
                        NAME = reader.GetString(1),
                        GENDER = reader.GetString(2),
                        BIRTHDAY = reader.GetDateTime(3),
                        ADDRESS = reader.GetString(4),
                        PHONE = reader.GetString(5),
                        DEPARTMENT = reader.GetString(6),
                        STATUS = reader.IsDBNull(7) ? null : reader.GetString(7)
                    });
                }
            }
        }

        return students;
    }

    public async Task UpdateStudentAsync(string id, string address, string phone, string? status = null)
    {
        using var conn = new OracleConnection(_connectionString);
        await conn.OpenAsync();

        // Nếu người gọi không truyền status (null hoặc chuỗi rỗng) thì chỉ update địa chỉ & điện thoại.
        bool updateStatus = !string.IsNullOrWhiteSpace(status);

        string query = updateStatus
            ? @"UPDATE adminpdb.SINHVIEN
                 SET DCHI = :address,
                     DT   = :phone,
                     TINHTRANG = :status
               WHERE MASV = :id"
            : @"UPDATE adminpdb.SINHVIEN
                 SET DCHI = :address,
                     DT   = :phone
               WHERE MASV = :id";

        using var cmd = new OracleCommand(query, conn);
        cmd.Parameters.Add(new OracleParameter("address", address));
        cmd.Parameters.Add(new OracleParameter("phone", phone));
        cmd.Parameters.Add(new OracleParameter("id", id));

        if (updateStatus)
        {
            cmd.Parameters.Add(new OracleParameter("status", status));
        }

        await cmd.ExecuteNonQueryAsync();
    }



    public async Task<bool> DeleteEmployeeAsync(string employeeId)
    {
        using (var conn = new OracleConnection(_connectionString))
        {
            await conn.OpenAsync();

            using (var transaction = conn.BeginTransaction())
            {
                try
                {
                    var deleteEmpCmd = new OracleCommand("DELETE FROM adminpdb.NHANVIEN WHERE MANLD = :id", conn)
                    {
                        Transaction = transaction
                    };
                    deleteEmpCmd.Parameters.Add(new OracleParameter("id", employeeId));

                    int rows = await deleteEmpCmd.ExecuteNonQueryAsync();

                    transaction.Commit();
                    return rows > 0;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine(" DeleteEmployeeAsync ERROR: " + ex.Message);
                    return false;
                }
            }
        }
    }
    public async Task<bool> DeleteStudentAsync(string employeeId)
    {
        using (var conn = new OracleConnection(_connectionString))
        {
            await conn.OpenAsync();

            using (var transaction = conn.BeginTransaction())
            {
                try
                {
                    var deleteEmpCmd = new OracleCommand("DELETE FROM adminpdb.SINHVIEN WHERE MASV = :id", conn)
                    {
                        Transaction = transaction
                    };
                    deleteEmpCmd.Parameters.Add(new OracleParameter("id", employeeId));

                    int rows = await deleteEmpCmd.ExecuteNonQueryAsync();

                    if (rows == 0)
                    {


                        Console.WriteLine("Error do bị chặn bởi chính sách VPD ( Tinh trang must NULL");
                    }

                    transaction.Commit();
                    return rows > 0;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine(" DeleteStudentAsync ERROR: " + ex.Message);
                    return false;
                }
            }
        }
    }

    public async Task<List<EmployeeModel>> PersonalEmployeeAsync()
    {
        var result = new List<EmployeeModel>();

        using (var conn = new OracleConnection(_connectionString))
        {
            await conn.OpenAsync();

            var cmd = new OracleCommand(@"
            SELECT MANLD, HOTEN, PHAI, NGSINH, LUONG, PHUCAP, DT, VAITRO,MADV
            FROM AdminPdb.NHANVIEN_NVCB", conn);

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    result.Add(new EmployeeModel
                    {
                        EmployeeID = reader.GetString(0),
                        FullName = reader.GetString(1),
                        Gender = reader.GetString(2),
                        BirthDate = reader.GetDateTime(3),
                        Salary = reader.GetDecimal(4),
                        Allowance = reader.GetDecimal(5),
                        Phone = reader.GetString(6),
                        Role = reader.GetString(7),
                        Department = reader.GetString(8)
                    });
                }
            }
        }

        return result;
    }



    public async Task UpdateEmployeeAsync(string employeeId, decimal salary, decimal allowance, string phone, string departmentId)
    {
        using (var conn = new OracleConnection(_connectionString))
        {
            await conn.OpenAsync();

            string query = @"UPDATE adminpdb.NHANVIEN 
                         SET LUONG = :salary, PHUCAP = :allowance, 
                             DT = :phone, MADV = :departmentId
                         WHERE MANLD = :employeeId";

            using (var cmd = new OracleCommand(query, conn))
            {
                cmd.Parameters.Add(new OracleParameter("salary", salary));
                cmd.Parameters.Add(new OracleParameter("allowance", allowance));
                cmd.Parameters.Add(new OracleParameter("phone", phone));
                cmd.Parameters.Add(new OracleParameter("departmentId", departmentId));
                cmd.Parameters.Add(new OracleParameter("employeeId", employeeId));

                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
    public async Task AddEmployeeAsync(EmployeeModel emp)
    {
        using (var conn = new OracleConnection(_connectionString))
        {
            await conn.OpenAsync();

            var idCmd = new OracleCommand(
              @"SELECT NVL(MAX(TO_NUMBER(SUBSTR(MANLD, 3))), 0)
              FROM adminpdb.NHANVIEN
              WHERE REGEXP_LIKE(SUBSTR(MANLD, 3), '^\d+$')", conn);

            var scalar = await idCmd.ExecuteScalarAsync();
            int maxId = Convert.ToInt32(scalar);
            string newId = "NV" + (maxId + 1).ToString("D5");
            var cmd = new OracleCommand(@"
            INSERT INTO adminpdb.NHANVIEN (MANLD, HOTEN, PHAI, NGSINH, LUONG, PHUCAP, DT, VAITRO, MADV)
            VALUES (:id, :name, :gender, :dob, :salary, :allowance, :phone, 'NV', :department)", conn);

            cmd.Parameters.Add("id", OracleDbType.Varchar2).Value = newId;
            cmd.Parameters.Add("name", OracleDbType.Varchar2).Value = emp.FullName;
            cmd.Parameters.Add("gender", OracleDbType.Char).Value = emp.Gender;
            cmd.Parameters.Add("dob", OracleDbType.Date).Value = emp.BirthDate;
            cmd.Parameters.Add("salary", OracleDbType.Decimal).Value = emp.Salary;
            cmd.Parameters.Add("allowance", OracleDbType.Decimal).Value = emp.Allowance;
            cmd.Parameters.Add("phone", OracleDbType.Decimal).Value = emp.Phone;
            cmd.Parameters.Add("department", OracleDbType.Varchar2).Value = emp.Department;
            await cmd.ExecuteNonQueryAsync();
        }
    }
    public async Task AddStudentAsync(StudentModel st)
    {
        using (var conn = new OracleConnection(_connectionString))
        {
            await conn.OpenAsync();

            var idCmd = new OracleCommand(
              @"SELECT NVL(MAX(TO_NUMBER(SUBSTR(MASV, 3))), 0)
              FROM adminpdb.SINHVIEN
              WHERE REGEXP_LIKE(SUBSTR(MASV, 3), '^\d+$')", conn);

            var scalar = await idCmd.ExecuteScalarAsync();
            int maxId = Convert.ToInt32(scalar);
            string newId = "NV" + (maxId + 1).ToString("D5");
            var cmd = new OracleCommand(@"
            INSERT INTO adminpdb.SINHVIEN (MASV, HOTEN, PHAI, NGSINH, DCHI, DT, KHOA, TINHTRANG)
            VALUES (:id, :name, :gender, :dob, :addr, :dt, :dp, NULL)", conn);

            cmd.Parameters.Add("id", OracleDbType.Varchar2).Value = newId;
            cmd.Parameters.Add("name", OracleDbType.Varchar2).Value = st.NAME;
            cmd.Parameters.Add("gender", OracleDbType.Char).Value = st.GENDER;
            cmd.Parameters.Add("dob", OracleDbType.Date).Value = st.BIRTHDAY;
            cmd.Parameters.Add("addr", OracleDbType.Varchar2).Value = st.ADDRESS;
            cmd.Parameters.Add("dt", OracleDbType.Varchar2).Value = st.PHONE;
            cmd.Parameters.Add("dp", OracleDbType.Varchar2).Value = st.DEPARTMENT;

            await cmd.ExecuteNonQueryAsync();
        }
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
            string query = "SELECT * FROM adminpdb.DANGKY";

            using (var cmd = new OracleCommand(query, conn))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    registrations.Add(new RegistrationModel
                    {
                        StudentID = reader.GetString(0),  // MASV
                        CourseID = reader.GetString(1),   // MAMM
                        PracticeScore = reader.IsDBNull(2) ? null : reader.GetDecimal(2),  // DIEMTH
                        ProcessScore = reader.IsDBNull(3) ? null : reader.GetDecimal(3),   // DIEMQT
                        FinalScore = reader.IsDBNull(4) ? null : reader.GetDecimal(4),     // DIEMCK
                        TotalScore = reader.IsDBNull(5) ? null : reader.GetDecimal(5)      // DIEMTK
                    });
                }
            }
        }

        return registrations;
    }

    public async Task<List<CourseModel>> GetAvailableCoursesForRegistrationAsync()
    {
        var courses = new List<CourseModel>();

        using (var conn = new OracleConnection(_connectionString))
        {
            await conn.OpenAsync();

            // Lấy danh sách môn học từ view MOMON_SV (view này đã được lọc theo khoa của sinh viên)
            string query = @"SELECT MAMM, MAHP, MAGV, HK, NAM 
                           FROM ADMINPDB.MOMON_SV";

            using (var cmd = new OracleCommand(query, conn))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    courses.Add(new CourseModel
                    {
                        CourseID = reader.GetString(reader.GetOrdinal("MAMM")),
                        BaseCode = reader.GetString(reader.GetOrdinal("MAHP")),
                        TeacherID = reader.GetString(reader.GetOrdinal("MAGV")),
                        Semester = reader.GetInt32(reader.GetOrdinal("HK")),
                        Year = reader.GetInt32(reader.GetOrdinal("NAM"))
                    });
                }
            }
        }

        return courses;
    }

    public async Task AddRegistrationAsync(RegistrationModel model)
    {
        using var conn = new OracleConnection(_connectionString);
        await conn.OpenAsync();

        try
        {
            string sessionUser = null;
            using (var userCmd = new OracleCommand(@"
                SELECT 
                    SYS_CONTEXT('USERENV','SESSION_USER') as SESSION_USER
                FROM DUAL", conn))
            {
                using var reader = await userCmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    sessionUser = reader.GetString(0);
                }
            }

            // 4. Thực hiện đăng ký
            string query = @"
                INSERT INTO ADMINPDB.DANGKY 
                    (MASV, MAMM, DIEMTH, DIEMQT, DIEMCK, DIEMTK) 
                VALUES 
                    (:masv, :mamm, NULL, NULL, NULL, NULL)";
            using var cmd = new OracleCommand(query, conn);

            cmd.Parameters.Add(new OracleParameter("masv", sessionUser));
            cmd.Parameters.Add(new OracleParameter("mamm", model.CourseID));

            try
            {
                await cmd.ExecuteNonQueryAsync();
            }
            catch (OracleException ex)
            {
                Console.WriteLine($"Insert Error: {ex.Message}");
                Console.WriteLine($"Error Code: {ex.Number}");
                Console.WriteLine($"Error Source: {ex.Source}");
                if (ex.Number == 28115)
                {
                    throw new Exception("Không thể đăng ký môn học này. Có thể do không thuộc khoa của bạn, không trong thời gian đăng ký, hoặc đã có điểm.");
                }
                throw;
            }
        }
        catch (OracleException ex)
        {
            Console.WriteLine($"Oracle Error Number: {ex.Number}");
            Console.WriteLine($"Oracle Error Message: {ex.Message}");
            if (ex.Number == 942)
            {
                throw new Exception("Không có quyền truy cập bảng hoặc bảng không tồn tại. Vui lòng kiểm tra lại quyền của tài khoản.");
            }
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"AddRegistrationAsync ERROR: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> DeleteRegistrationAsync(string studentId, string courseId)
    {
        using var conn = new OracleConnection(_connectionString);
        try 
        {
            await conn.OpenAsync();
            Console.WriteLine("Connection opened successfully");

            // Kiểm tra session user
            using (var userCmd = new OracleCommand("SELECT SYS_CONTEXT('USERENV','SESSION_USER') FROM DUAL", conn))
            {
                var sessionUser = await userCmd.ExecuteScalarAsync() as string;
                Console.WriteLine($"Current session user: {sessionUser}");
            }

            // Kiểm tra thông tin môn học
            using (var courseCmd = new OracleCommand(@"
                SELECT MM.HK, MM.NAM, 
                       CASE 
                           WHEN SYSDATE <= get_semester_start_date(MM.HK, MM.NAM) + 14 
                           THEN 'TRUE' 
                           ELSE 'FALSE' 
                       END as IN_REGISTRATION_PERIOD
                FROM MOMON MM 
                WHERE MM.MAMM = :mamm", conn))
            {
                courseCmd.Parameters.Add(new OracleParameter("mamm", courseId));
                try
                {
                    using var reader = await courseCmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        var semester = reader.GetInt32(0);
                        var year = reader.GetInt32(1);
                        var inRegistrationPeriod = reader.GetString(2) == "TRUE";
                        Console.WriteLine($"Course info - Semester: {semester}, Year: {year}, In Registration Period: {inRegistrationPeriod}");
                    }
                }
                catch (OracleException ex)
                {
                    Console.WriteLine($"Error checking course info: {ex.Message}");
                    if (ex.Number == 942)
                    {
                        Console.WriteLine("No access to MOMON table or table doesn't exist");
                    }
                }
            }

            // Thực hiện xóa
            string query = "DELETE FROM ADMINPDB.DANGKY WHERE MASV = :masv AND MAMM = :mamm";
            using var cmd = new OracleCommand(query, conn);
            cmd.Parameters.Add(new OracleParameter("masv", studentId));
            cmd.Parameters.Add(new OracleParameter("mamm", courseId));

            try
            {
                int rowsAffected = await cmd.ExecuteNonQueryAsync();
                Console.WriteLine($"Rows affected by DELETE: {rowsAffected}");
                
                if (rowsAffected == 0)
                {
                    // Kiểm tra xem bản ghi có tồn tại không
                    var checkQuery = "SELECT COUNT(*) FROM ADMINPDB.DANGKY WHERE MASV = :masv AND MAMM = :mamm";
                    using var checkCmd = new OracleCommand(checkQuery, conn);
                    checkCmd.Parameters.Add(new OracleParameter("masv", studentId));
                    checkCmd.Parameters.Add(new OracleParameter("mamm", courseId));

                    var count = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());
                    if (count > 0)
                    {
                        throw new Exception("Không thể xóa đăng ký. Có thể đã quá thời hạn cho phép (14 ngày từ đầu học kỳ) hoặc đã có điểm");
                    }
                    else
                    {
                        throw new Exception("Không tìm thấy đăng ký môn học này");
                    }
                }
                return true;
            }
            catch (OracleException ex)
            {
                Console.WriteLine($"Oracle Error Number: {ex.Number}");
                Console.WriteLine($"Oracle Error Message: {ex.Message}");
                if (ex.Number == 942)
                {
                    throw new Exception("Không có quyền truy cập bảng DANGKY hoặc bảng không tồn tại");
                }
                throw;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DeleteRegistrationAsync ERROR: {ex.Message}");
            throw;
        }
    }

    public async Task UpdateRegistrationScoreAsync(string studentId, string courseId, decimal? practiceScore, decimal? processScore, decimal? finalScore, decimal? totalScore)
    {
        using (var conn = new OracleConnection(_connectionString))
        {
            await conn.OpenAsync();

            string query = "UPDATE adminpdb.DANGKY SET DIEMTH = :practiceScore, DIEMQT = :processScore, DIEMCK = :finalScore, DIEMTK = :totalScore WHERE MASV = :studentId and MAMM = :courseId";

            using (var cmd = new OracleCommand(query, conn))
            {
                cmd.Parameters.Add(new OracleParameter("practiceScore", practiceScore.HasValue ? (object)practiceScore.Value : DBNull.Value));
                cmd.Parameters.Add(new OracleParameter("processScore", processScore.HasValue ? (object)processScore.Value : DBNull.Value));
                cmd.Parameters.Add(new OracleParameter("finalScore", finalScore.HasValue ? (object)finalScore.Value : DBNull.Value));
                cmd.Parameters.Add(new OracleParameter("totalScore", totalScore.HasValue ? (object)totalScore.Value : DBNull.Value));
                cmd.Parameters.Add(new OracleParameter("studentId", studentId));
                cmd.Parameters.Add(new OracleParameter("courseId", courseId));

                await cmd.ExecuteNonQueryAsync();
            }
        }
    }

    public async Task AddCourseOfferingNVPDTAsync(CourseOfferingModel course)
    {
        using (var conn = new OracleConnection(_connectionString))
        {
            await conn.OpenAsync();
            string query = "INSERT INTO adminpdb.MOMON_PDT (MAMM, MAHP, MAGV, HK, NAM) VALUES (:offeringID, :moduleId, :instructorId, :semester, :year)";

            using (var cmd = new OracleCommand(query, conn))
            {
                cmd.Parameters.Add(new OracleParameter("offeringID", course.OfferingID));
                cmd.Parameters.Add(new OracleParameter("moduleId", course.ModuleID));
                cmd.Parameters.Add(new OracleParameter("instructorId", course.InstructorID));
                cmd.Parameters.Add(new OracleParameter("semester", course.Semester));
                cmd.Parameters.Add(new OracleParameter("year", course.Year));

                await cmd.ExecuteNonQueryAsync();
            }
        }
    }

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
                        Year = reader.GetInt32(4)
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
            string query = "UPDATE adminpdb.MOMON_PDT SET MAHP = :moduleId, MAGV = :instructorId, HK = :semester, NAM = :year WHERE MAMM = :offeringId";

            using (var cmd = new OracleCommand(query, conn))
            {
                cmd.Parameters.Add(new OracleParameter("moduleId",course.ModuleID));
                cmd.Parameters.Add(new OracleParameter("instructorId",course.InstructorID));
                cmd.Parameters.Add(new OracleParameter("semester",course.Semester));
                cmd.Parameters.Add(new OracleParameter("year",course.Year ));
                cmd.Parameters.Add(new OracleParameter("offeringId", course.OfferingID));

                await cmd.ExecuteNonQueryAsync();
            }
        }
    }

    public async Task DeleteCourseOfferingNVPDTAsync(string offeringId)
    {
        using (var conn = new OracleConnection(_connectionString))
        {
            await conn.OpenAsync();

            using (var transaction = conn.BeginTransaction())
            {
                try
                {
                    string deleteRegistrationQuery = "ADMINPDB.force_delete_dangky_and_momon";
                    using (var cmd1 = new OracleCommand(deleteRegistrationQuery, conn))
                    {
                        cmd1.CommandType = CommandType.StoredProcedure;
                        cmd1.Parameters.Add("p_mamm", OracleDbType.Varchar2).Value = offeringId;
                        await cmd1.ExecuteNonQueryAsync();
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception("Error deleting course offering: " + ex.Message);
                }
            }
        }
    }

    public async Task<List<EmployeeModel>> GetInstructorsAsync()
    {
        var instructors = new List<EmployeeModel>();

        using (var conn = new OracleConnection(_connectionString))
        using (var cmd = new OracleCommand("adminpdb.get_instructors", conn))
        {
            cmd.CommandType = CommandType.StoredProcedure;

            var output = cmd.Parameters.Add("p_result", OracleDbType.RefCursor);
            output.Direction = ParameterDirection.Output;

            await conn.OpenAsync();
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    instructors.Add(new EmployeeModel
                    {
                        EmployeeID = reader.GetString(0),
                        FullName = reader.GetString(1)
                    });
                }
            }
        }

        return instructors;
    }

    public async Task<List<ModuleModel>> GetModulesAsync()
    {
        var modules = new List<ModuleModel>();

        using (var conn = new OracleConnection(_connectionString))
        using (var cmd = new OracleCommand("adminpdb.get_modules", conn))
        {
            cmd.CommandType = CommandType.StoredProcedure;

            var output = cmd.Parameters.Add("p_result", OracleDbType.RefCursor);
            output.Direction = ParameterDirection.Output;

            await conn.OpenAsync();
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    modules.Add(new ModuleModel
                    {
                        ModuleID = reader.GetString(0),
                        ModuleName = reader.GetString(1)
                    });
                }
            }
        }

        return modules;
    }
    public async Task<List<CourseModel>> GetCoursesByStudentDepartmentAsync(string studentId)
    {
        var courses = new List<CourseModel>();

        using (var conn = new OracleConnection(_connectionString))
        {
            await conn.OpenAsync();

            // Only select from MOMON_SV view
            string query = @"SELECT MAMM, MAHP, MAGV, HK, NAM FROM ADMINPDB.MOMON_SV";

            using (var cmd = new OracleCommand(query, conn))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    courses.Add(new CourseModel
                    {
                        CourseID = reader.GetString(reader.GetOrdinal("MAMM")),
                        BaseCode = reader.GetString(reader.GetOrdinal("MAHP")),
                        TeacherID = reader.GetString(reader.GetOrdinal("MAGV")),
                        Semester = reader.GetInt32(reader.GetOrdinal("HK")),
                        Year = reader.GetInt32(reader.GetOrdinal("NAM"))
                    });
                }
            }
        }

        return courses;
    }
    public async Task<List<CourseModel>> GetCoursesForTRGDVAsync()
    {
        var courses = new List<CourseModel>();

        using (var conn = new OracleConnection(_connectionString))
        {
            await conn.OpenAsync();

            // Select from MOMON table
            string query = @"SELECT MAMM, MAHP, MAGV, HK, NAM FROM ADMINPDB.MOMON_TRGDV";

            using (var cmd = new OracleCommand(query, conn))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    courses.Add(new CourseModel
                    {
                        CourseID = reader.GetString(reader.GetOrdinal("MAMM")),
                        BaseCode = reader.GetString(reader.GetOrdinal("MAHP")),
                        TeacherID = reader.GetString(reader.GetOrdinal("MAGV")),
                        Semester = reader.GetInt32(reader.GetOrdinal("HK")),
                        Year = reader.GetInt32(reader.GetOrdinal("NAM"))
                    });
                }
            }
        }

        return courses;
    }
    public async Task<List<CourseModel>> GetCoursesForGVAsync()
    {
        var courses = new List<CourseModel>();

        using (var conn = new OracleConnection(_connectionString))
        {
            await conn.OpenAsync();

            // Select from MOMON_GV view
            string query = @"SELECT MAMM, MAHP, MAGV, HK, NAM FROM ADMINPDB.MOMON_GV";

            using (var cmd = new OracleCommand(query, conn))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    courses.Add(new CourseModel
                    {
                        CourseID = reader.GetString(reader.GetOrdinal("MAMM")),
                        BaseCode = reader.GetString(reader.GetOrdinal("MAHP")),
                        TeacherID = reader.GetString(reader.GetOrdinal("MAGV")),
                        Semester = reader.GetInt32(reader.GetOrdinal("HK")),
                        Year = reader.GetInt32(reader.GetOrdinal("NAM"))
                    });
                }
            }
        }

        return courses;
    }
    public async Task<List<EmployeeModel>> GetEmployeesForTRGDVAsync()
    {
        var employees = new List<EmployeeModel>();

        using (var conn = new OracleConnection(_connectionString))
        {
            await conn.OpenAsync();

            // Select from NHANVIEN_TRGDV view, excluding LUONG and PHUCAP
            string query = @"SELECT MANLD, HOTEN, PHAI, NGSINH, DT, VAITRO, MADV FROM ADMINPDB.NHANVIEN_TRGDV";

            using (var cmd = new OracleCommand(query, conn))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    employees.Add(new EmployeeModel
                    {
                        EmployeeID = reader.GetString(reader.GetOrdinal("MANLD")),
                        FullName = reader.GetString(reader.GetOrdinal("HOTEN")),
                        Gender = reader.GetString(reader.GetOrdinal("PHAI")),
                        BirthDate = reader.GetDateTime(reader.GetOrdinal("NGSINH")),
                        Phone = reader.GetString(reader.GetOrdinal("DT")),
                        Role = reader.GetString(reader.GetOrdinal("VAITRO")),
                        Department = reader.GetString(reader.GetOrdinal("MADV"))
                    });
                }
            }
        }

        return employees;
    }
    public async Task<List<RegistrationModel>> GetRegistrationsForNVPDTAsync()
    {
        var registrations = new List<RegistrationModel>();

        using (var conn = new OracleConnection(_connectionString))
        {
            await conn.OpenAsync();

            string query = @"SELECT MASV, MAMM, DIEMTH, DIEMQT, DIEMCK, DIEMTK FROM ADMINPDB.DANGKY";

            using (var cmd = new OracleCommand(query, conn))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    registrations.Add(new RegistrationModel
                    {
                        StudentID = reader.GetString(reader.GetOrdinal("MASV")),
                        CourseID = reader.GetString(reader.GetOrdinal("MAMM")),
                        PracticeScore = reader.IsDBNull(reader.GetOrdinal("DIEMTH")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("DIEMTH")),
                        ProcessScore = reader.IsDBNull(reader.GetOrdinal("DIEMQT")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("DIEMQT")),
                        FinalScore = reader.IsDBNull(reader.GetOrdinal("DIEMCK")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("DIEMCK")),
                        TotalScore = reader.IsDBNull(reader.GetOrdinal("DIEMTK")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("DIEMTK")),
                    });
                }
            }
        }

        return registrations;
    }

    public async Task<List<CourseModel>> GetCoursesNVPDTAsync()
    {
        var courses = new List<CourseModel>();

        using (var conn = new OracleConnection(_connectionString))
        {
            await conn.OpenAsync();

            // Select from MOMON_GV view
            string query = @"SELECT MAMM, MAHP, MAGV, HK, NAM FROM ADMINPDB.MOMON_PDT";

            using (var cmd = new OracleCommand(query, conn))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    courses.Add(new CourseModel
                    {
                        CourseID = reader.GetString(reader.GetOrdinal("MAMM")),
                        BaseCode = reader.GetString(reader.GetOrdinal("MAHP")),
                        TeacherID = reader.GetString(reader.GetOrdinal("MAGV")),
                        Semester = reader.GetInt32(reader.GetOrdinal("HK")),
                        Year = reader.GetInt32(reader.GetOrdinal("NAM"))
                    });
                }
            }
        }

        return courses;
    }

    public async Task UpdateRegistrationNVPDTAsync(RegistrationModel course)
    {
        //using (var conn = new OracleConnection(_connectionString))
        //{
        //    await conn.OpenAsync();
        //    string query = "UPDATE adminpdb.DANGKY SET studentId = :moduleId, MAGV = :instructorId, HK = :semester, NAM = :year WHERE MAMM = :offeringId";

        //    using (var cmd = new OracleCommand(query, conn))
        //    {
        //        cmd.Parameters.Add(new OracleParameter("moduleId", course.ModuleID));
        //        cmd.Parameters.Add(new OracleParameter("instructorId", course.InstructorID));
        //        cmd.Parameters.Add(new OracleParameter("semester", course.Semester));
        //        cmd.Parameters.Add(new OracleParameter("year", course.Year));
        //        cmd.Parameters.Add(new OracleParameter("offeringId", course.OfferingID));

        //        await cmd.ExecuteNonQueryAsync();
        //    }
        //}
    }

    public async Task DeleteRegistrationNVPDTAsync(string studentId, string courseId)
    {
        using (var conn = new OracleConnection(_connectionString))
        {
            await conn.OpenAsync();

            using (var transaction = conn.BeginTransaction())
            {
                try
                {
                    string deleteRegistrationQuery = "DELETE FROM ADMINPDB.DANGKY WHERE studentId = :studentId and courseId = :courseId";
                    using (var cmd = new OracleCommand(deleteRegistrationQuery, conn))
                    {
                        cmd.Parameters.Add(new OracleParameter("studentId", studentId));
                        cmd.Parameters.Add(new OracleParameter("courseId", courseId));
                        await cmd.ExecuteNonQueryAsync();
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception("Error deleting course offering: " + ex.Message);
                }
            }
        }
    }
    public async Task UpdateStudentStatusForNVPDTAsync(string id, string status)
    {
        using (var conn = new OracleConnection(_connectionString))
        {
            await conn.OpenAsync();
            string query = "UPDATE adminpdb.SINHVIEN SET TINHTRANG = :status WHERE MASV = :studentId";
            switch (status)
            {
                case "Active":
                    status = "Đang học";
                    break;
                case "Drop out":
                    status = "Đã thôi học";
                    break;
                case "Graduated":
                    status = "Đã tốt nghiệp";
                    break;
                case "Reservation":
                    status = "Bảo lưu kết quả học tập";
                    break;
                default:
                    throw new ArgumentException("Trạng thái không hợp lệ");
            }
            using (var cmd = new OracleCommand(query, conn))
            {
                cmd.Parameters.Add(new OracleParameter("status", status));
                cmd.Parameters.Add(new OracleParameter("studentId", id));

                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}