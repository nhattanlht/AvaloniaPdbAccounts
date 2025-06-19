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
    // Giả sử ViewModelBase đã implement INotifyPropertyChanged
    public class PrivilegeManagementViewModel : ViewModelBase 
    {
        public static readonly string ConnectionString = DatabaseSettings.GetConnectionString();
        public readonly UserService userService = new UserService();
        public readonly RoleService roleService = new RoleService();
        
        // Danh sách các items
        public ObservableCollection<string> Grantees { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> Privileges { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> ObjectTypes { get; } = new ObservableCollection<string>();
        
        // --- SỬA ĐỔI Ở ĐÂY ---

        // Backing fields cho các thuộc tính selected
        private string _selectedGrantee;
        private string _selectedPrivilege;
        private string _selectedObjectType;

        // Selected items (chuyển sang full properties)
        public string SelectedGrantee
        {
            get => _selectedGrantee;
            set
            {
                // Gọi phương thức để thông báo sự thay đổi.
                // Tên phương thức có thể là SetProperty, RaisePropertyChanged, hoặc OnPropertyChanged
                // tùy thuộc vào implementation của ViewModelBase.
                // Dòng này sẽ gán giá trị và thông báo cho UI.
                SetProperty(ref _selectedGrantee, value); 
            }
        }
        
        public string SelectedPrivilege
        {
            get => _selectedPrivilege;
            set => SetProperty(ref _selectedPrivilege, value);
        }

        public string SelectedObjectType
        {
            get => _selectedObjectType;
            set => SetProperty(ref _selectedObjectType, value);
        }

        // Commands
        public ICommand LoadGranteesCommand { get; }
        public ICommand CheckPermissionsCommand { get; }

        public PrivilegeManagementViewModel()
        {
            LoadGranteesCommand = new RelayCommand(async () => await LoadGranteesAsync());
            CheckPermissionsCommand = new RelayCommand(async () => await CheckPermissionsAsync());
            AddStaticItems();
        }

        public void AddStaticItems()
        {
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
                Grantees.Clear();
                using var conn = new OracleConnection(ConnectionString);
                await conn.OpenAsync();
                
                var users = await userService.GetAllUsersAsync(conn);
                var roles = await roleService.GetAllRolesAsync(conn);

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
                Console.WriteLine($"Error loading grantees: {ex.Message}");
            }
        }

        // Phương thức này không được Command gọi trực tiếp, bạn có thể giữ lại
        // để dùng cho mục đích khác hoặc xóa đi nếu không cần.
        public async Task CheckPermissionsAsync(string grantee, string privilegeType)
        {
            SelectedGrantee = grantee;
            SelectedObjectType = privilegeType;
            await CheckPermissionsAsync();
        }
        
        // Command sẽ gọi phương thức này
        public async Task CheckPermissionsAsync()
        {
            if (string.IsNullOrEmpty(SelectedGrantee) || string.IsNullOrEmpty(SelectedObjectType))
            {
                Console.WriteLine("Vui lòng chọn Grantee và Object Type.");
                return;
            }

            try
            {
                Console.WriteLine($"Checking permissions for {SelectedGrantee} on {SelectedObjectType}");
                // Thực hiện kiểm tra quyền ở đây
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking permissions: {ex.Message}");
            }
        }

        /* * LƯU Ý: Nếu lớp ViewModelBase của bạn không có phương thức SetProperty,
         * bạn sẽ cần tự implement nó. Dưới đây là một ví dụ phổ biến:
         *
         * protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
         * {
         * if (EqualityComparer<T>.Default.Equals(field, newValue))
         * {
         * return false;
         * }
         * field = newValue;
         * OnPropertyChanged(propertyName); // Giả sử có phương thức OnPropertyChanged để gọi event
         * return true;
         * }
        */
    }
}