using System;
using System.Collections.Generic;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using AvaloniaPdbAccounts.Models;
using System.Linq;

namespace AvaloniaPdbAccounts.Services
{
    public class PKTService
    {
        private readonly string _connectionString = DatabaseSettings.GetConnectionString();
        private readonly bool _currentUserRole;

        public PKTService()
        {
            _currentUserRole = DatabaseService.CurrentRoles.Any(r => string.Equals(r.RoleName, "NVPKT", StringComparison.OrdinalIgnoreCase));
            if (!_currentUserRole)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền chức năng này");
            }
        }
        public List<DangKyModel> GetDanhSachDangKy()
        {
            var list = new List<DangKyModel>();

            try
            {
                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                const string query = @"
                    SELECT d.MASV, s.HOTEN, d.MAMM, h.TENHP, 
                           d.DIEMTH, d.DIEMQT, d.DIEMCK, d.DIEMTK
                    FROM ADMINPDB.DANGKY d
                    JOIN ADMINPDB.SINHVIEN s ON d.MASV = s.MASV
                    JOIN ADMINPDB.MOMON m ON d.MAMM = m.MAMM
                    JOIN ADMINPDB.HOCPHAN h ON m.MAHP = h.MAHP";

                using var cmd = new OracleCommand(query, conn);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    list.Add(new DangKyModel
                    {
                        MaSV = reader["MASV"]?.ToString(),
                        HoTenSV = reader["HOTEN"]?.ToString(),
                        MaMM = reader["MAMM"]?.ToString(),
                        TenMonHoc = reader["TENHP"]?.ToString(),
                        DiemTH = reader["DIEMTH"] != DBNull.Value ? Convert.ToDouble(reader["DIEMTH"]) : null,
                        DiemQT = reader["DIEMQT"] != DBNull.Value ? Convert.ToDouble(reader["DIEMQT"]) : null,
                        DiemCK = reader["DIEMCK"] != DBNull.Value ? Convert.ToDouble(reader["DIEMCK"]) : null,
                        DiemTK = reader["DIEMTK"] != DBNull.Value ? Convert.ToDouble(reader["DIEMTK"]) : null
                    });
                }
            }
            catch (Exception ex)
            {
                // TODO: Log the error (e.g., using Serilog, NLog, etc.)
                Console.WriteLine("Lỗi khi lấy danh sách đăng ký: " + ex.Message);
            }

            return list;
        }

    public bool CapNhatDiem(string maSV, string maMM, double? diemTH, double? diemQT, double? diemCK)
    {
        try
        {
            using var conn = new OracleConnection(_connectionString);
            conn.Open();
            
            // Kiểm tra quyền trước khi thực hiện
            if (!_currentUserRole)
                throw new UnauthorizedAccessException("Không có quyền cập nhật điểm");
            
            // Chỉ cập nhật các cột điểm
            var query = @"
                UPDATE ADMINPDB.DANGKY 
                SET DIEMTH = :diemTH, 
                    DIEMQT = :diemQT, 
                    DIEMCK = :diemCK
                WHERE MASV = :maSV AND MAMM = :maMM";
            
            using var cmd = new OracleCommand(query, conn);
            cmd.Parameters.Add("maSV", OracleDbType.Varchar2).Value = maSV;
            cmd.Parameters.Add("maMM", OracleDbType.Varchar2).Value = maMM;
            cmd.Parameters.Add("diemTH", OracleDbType.Double).Value = diemTH ?? (object)DBNull.Value;
            cmd.Parameters.Add("diemQT", OracleDbType.Double).Value = diemQT ?? (object)DBNull.Value;
            cmd.Parameters.Add("diemCK", OracleDbType.Double).Value = diemCK ?? (object)DBNull.Value;
            
            return cmd.ExecuteNonQuery() > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Lỗi khi cập nhật điểm: " + ex.Message);
            return false;
        }
    } 
    }
}
