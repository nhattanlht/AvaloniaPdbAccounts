using System;
using System.Threading.Tasks;
using AvaloniaPdbAccounts.Models;
using Oracle.ManagedDataAccess.Client;

namespace AvaloniaPdbAccounts.Services
{
    public class AccessService
    {
        private static UserAccount userAccount = new UserAccount();

        public static UserAccount CurrentUser => userAccount;
        public static bool IsLoggedIn => DatabaseService.Instance.IsConnected;

        public static async Task<bool> Login(string username, string password)
        {
            try
            {
                // Sử dụng instance Singleton của DatabaseService
                DatabaseService.Instance.Login(username, password);
                
                // Kiểm tra kết nối thành công
                if (DatabaseService.Instance.IsConnected)
                {
                    userAccount.Username = username;
                    userAccount.Password = password; // Lưu ý: Lưu password trong bộ nhớ không an toàn

                    Console.WriteLine("✅ Đăng nhập thành công vào Oracle DB");
                    return true;
                }
                
                Console.WriteLine("⚠️ Không thể kết nối đến Oracle DB");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi khi kết nối đến Oracle DB: {ex.Message}");
                return false;
            }
        }

        public static async Task Logout()
        {
            DatabaseService.Instance.Dispose();
            userAccount = new UserAccount(); // Reset thông tin user
            Console.WriteLine("🔌 Đã đăng xuất và đóng kết nối database");
        }

        // Các phương thức thực thi query có thể thêm ở đây
        // Hoặc sử dụng trực tiếp DatabaseService.Instance
    }
}