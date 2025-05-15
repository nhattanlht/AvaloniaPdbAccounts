using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using AvaloniaPdbAccounts.Utilities;
using Oracle.ManagedDataAccess.Client;
using AvaloniaPdbAccounts.Models;
using AvaloniaPdbAccounts.Services;
using System.Collections.Generic;
using System.Data;

namespace AvaloniaPdbAccounts.ViewModels
{
    public class PrivilegeManagementViewModel : ViewModelBase
    {
        public static readonly string ConnectionString = DatabaseSettings.GetConnectionString();
        public readonly UserService userService = new UserService();
        public readonly RoleService roleService = new RoleService();
        // Danh sách các items
        public ObservableCollection<string> Grantees { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> Privileges { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> ObjectTypes { get; } = new ObservableCollection<string>();
        
        // Selected items
        public string SelectedGrantee { get; set; }
        public string SelectedPrivilege { get; set; }
        public string SelectedObjectType { get; set; }

        // Commands
        public ICommand LoadGranteesCommand { get; }
        public ICommand CheckPermissionsCommand { get; }

        public PrivilegeManagementViewModel()
        {
            // Khởi tạo command
            LoadGranteesCommand = new RelayCommand(async () => await LoadGranteesAsync());
            CheckPermissionsCommand = new RelayCommand(async () => await CheckPermissionsAsync());
            
            // Thêm items vào các danh sách cố định
            AddStaticItems();
        }

        public void AddStaticItems()
        {
            // Thêm từng item một
            Privileges.Add("SELECT");
            Privileges.Add("INSERT");
            Privileges.Add("UPDATE");
            Privileges.Add("DELETE");
            Privileges.Add("EXECUTE");
            
            ObjectTypes.Add("TABLE");
            ObjectTypes.Add("VIEW");
            ObjectTypes.Add("PROCEDURE");
            ObjectTypes.Add("FUNCTION");
        }

        public async Task LoadGranteesAsync()
        {
            try
            {
                Grantees.Clear(); // Xóa danh sách cũ
                
                using var conn = new OracleConnection(ConnectionString);
                await conn.OpenAsync();
                
                // Load users
               var users = await userService.GetAllUsersAsync(conn);
                var roles = await roleService.GetAllRolesAsync(conn);

                    var all = new List<string>();
                    foreach (var user in users)
                    {
                        Grantees.Add(user);
                    }
                    foreach (var role in roles)
                    {
                        Grantees.Add(role);
                    }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi ở đây
                Console.WriteLine($"Error loading grantees: {ex.Message}");
            }
        }

        public async Task CheckPermissionsAsync(string grantee, string privilegeType)
        {
            SelectedGrantee = grantee;
            SelectedObjectType = privilegeType;
            await CheckPermissionsAsync(); // Gọi phương thức hiện có
        }
        public async Task CheckPermissionsAsync()
        {
            if (string.IsNullOrEmpty(SelectedGrantee) || string.IsNullOrEmpty(SelectedObjectType))
            {
                return;
            }

            try
            {
                // Thực hiện kiểm tra quyền ở đây
                Console.WriteLine($"Checking permissions for {SelectedGrantee} on {SelectedObjectType}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking permissions: {ex.Message}");
            }
        }
    }
}