    using System;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using AvaloniaPdbAccounts.Models;
    using AvaloniaPdbAccounts.Services;
    using Oracle.ManagedDataAccess.Client;
    using AvaloniaPdbAccounts.Utilities;
    using AvaloniaPdbAccounts.ViewModels;

    public class RoleManagementViewModel : ViewModelBase
    {
        private readonly RoleService _roleService = new();
        
        // Properties
        public ObservableCollection<string> Roles { get; } = new();
        public ObservableCollection<PrivilegeItem> AvailablePrivileges { get; } = new();
        public string SelectedRole { get; set; }

        private readonly string connectionString = DatabaseSettings.GetConnectionString();

        // Commands
        public ICommand LoadRolesCommand { get; }
        public ICommand AddRoleCommand { get; }
        public ICommand EditRolePrivilegesCommand { get; }
        public ICommand DeleteRoleCommand { get; }
        public ICommand CheckPermissionsCommand { get; }
        
        private string _newRoleName;
        public string NewRoleName
        {
            get => _newRoleName;
            set => SetProperty(ref _newRoleName, value);
        }
        public RoleManagementViewModel()
        {
            AddRoleCommand = new RelayCommand(
                async () => 
                {
                    await AddRoleAsync(NewRoleName);
                    NewRoleName = ""; // Reset sau khi tạo
                },
                () => !string.IsNullOrEmpty(NewRoleName) // Chỉ enable khi có tên role
            );
            LoadRolesCommand = new RelayCommand(async () => await LoadRolesAsync());
            // AddRoleCommand = new RelayCommand(async () => await AddRoleAsync());
            EditRolePrivilegesCommand = new RelayCommand(async () => await EditRolePrivilegesAsync());
            DeleteRoleCommand = new RelayCommand(async () => await DeleteRoleAsync());
        }

        public async Task LoadRolesAsync()
    {
        try
        {
            using var conn = new OracleConnection(connectionString);
            await conn.OpenAsync(); // Ensure connection is open
            
            var roles = await _roleService.GetAllRolesAsync(conn);
            
            Roles.Clear();
            foreach (var role in roles)
                Roles.Add(role);

        }
        catch (Exception ex)
        {
            // Handle error (show message to user)
            Console.WriteLine($"Error loading roles: {ex.Message}");
        }
    }
        public async Task AddRoleAsync(string roleName)
        {
            await _roleService.CreateRoleAsync(roleName);
            await LoadRolesAsync();
        }

        public async Task EditRolePrivilegesAsync()
        {
            if (string.IsNullOrEmpty(SelectedRole)) return;
            
            var privileges = await _roleService.GetRolePrivilegesAsync(SelectedRole);
            // Hiển thị dialog chỉnh sửa quyền
            await _roleService.UpdateRolePrivilegesAsync(SelectedRole, privileges);
        }

        public async Task DeleteRoleAsync()
        {
            if (string.IsNullOrEmpty(SelectedRole)) return;
            await _roleService.DeleteRoleAsync(SelectedRole);
            await LoadRolesAsync();
        }
    }