using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaPdbAccounts.Services;
using AvaloniaPdbAccounts.ViewModels;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Threading.Tasks;
using AvaloniaPdbAccounts.Utilities;
using AvaloniaPdbAccounts.Models;
using System.Collections.Generic;
using System.Data;
using static AvaloniaPdbAccounts.Utilities.Helpers;
using System.Linq;

namespace AvaloniaPdbAccounts
{
    public partial class MainWindow : Window
    {
        private readonly DialogService _dialogService;
        private readonly UserManagementViewModel _userManagementVM;
        private readonly RoleManagementViewModel _roleManagementVM = new RoleManagementViewModel();
        private readonly PrivilegeManagementViewModel _privilegeManagementVM = new PrivilegeManagementViewModel();
        private readonly PrivilegeGrant _privilegeGrant = new PrivilegeGrant();
        private string _lastGrantee = "";
        private string _lastType = "";
        private List<string> _lastPermissions = new();
        public MainWindow()
        {
            _dialogService = new DialogService(this);
            _userManagementVM = new UserManagementViewModel(_dialogService);
            InitializeComponent();
            var vm = this.DataContext as PrivilegeGrant;
            if (vm != null)
            {
                _selectedObjectName = vm.SelectedObjectName;
                // dùng objectName tại đây
            }
        }
        private async Task ShowInfo(string message)
        {
            await MessageBox.Show(this, message, "Thông báo", MessageBox.MessageBoxButtons.Ok);
        }

        private async Task ShowInfo(string message, string messageType)
        {
            await MessageBox.Show(this, message, messageType, MessageBox.MessageBoxButtons.Ok);
        }
        #region User Management Methods

        private async void LoadAccounts_Click(object sender, RoutedEventArgs e)
        {
            await _userManagementVM.LoadUsersAsync();
            AccountsListBox.ItemsSource = _userManagementVM.Users;
        }
        private async void AddUser_Click(object sender, RoutedEventArgs e)
        {
            _userManagementVM.AddUserCommand.Execute(null);
            await _userManagementVM.LoadUsersAsync(); // Refresh the list
        }
        private async void DeleteAccount_Click(object sender, RoutedEventArgs e)
        {
            if (AccountsListBox.SelectedItem != null)
            {
                _userManagementVM.SelectedUser = AccountsListBox.SelectedItem.ToString();
                await _userManagementVM.DeleteUserAsync();
            }
        }
        private async void EditAccount_Click(object sender, RoutedEventArgs e)
        {
            if (AccountsListBox.SelectedItem != null)
            {
                _userManagementVM.SelectedUser = AccountsListBox.SelectedItem.ToString();
                _userManagementVM.EditUserCommand.Execute(null);
                await Task.CompletedTask; // Ensure the method remains asynchronous
            }
        }
        private async void CheckPermission_Click(object sender, RoutedEventArgs e)
        {

            await ShowInfo("Vui lòng chọn User hoặc Role và loại quyền!");
            CheckArea.IsVisible = !CheckArea.IsVisible;
            if (CheckArea.IsVisible)
            {
                _ = LoadGranteesForCheck(); // Load grantees when showing the check area
            }
        }
        #endregion
        #region Role Management Methods
        private async void LoadRoles_Click(object sender, RoutedEventArgs e)
        {
            await _roleManagementVM.LoadRolesAsync();
            AccountsListBox.ItemsSource = _roleManagementVM.Roles;
        }
        private async void AddRole_Click(object sender, RoutedEventArgs e)
        {
            _roleManagementVM.AddRoleCommand.Execute(null);
            await _roleManagementVM.LoadRolesAsync(); // Refresh the list
        }
        private async void DeleteRole_Click(object sender, RoutedEventArgs e)
        {
            if (AccountsListBox.SelectedItem != null)
            {
                _roleManagementVM.SelectedRole = AccountsListBox.SelectedItem.ToString();
                await _roleManagementVM.DeleteRoleAsync();
            }
        }
        #endregion
        #region Privilege Management Methods
        private async Task LoadGranteesForCheck()
        {
            await _privilegeManagementVM.LoadGranteesAsync();
            UserRoleComboBox.ItemsSource = _privilegeManagementVM.Grantees;
        }
        private async Task LoadRolesForGrant()
        {
            // Gọi LoadRolesForGrantAsync để lấy danh sách các role có thể cấp quyền
            await _roleManagementVM.LoadRolesAsync();
            RoleComboBox.ItemsSource = _roleManagementVM.Roles;  // Gán các role vào ComboBox
            RoleComboBox.IsVisible = true;  // Hiển thị ComboBox role
        }

        private async void ConfirmCheckButton_Click(object sender, RoutedEventArgs e)
        {
            // Kiểm tra nếu chưa chọn User hoặc Role và loại quyền
            if (UserRoleComboBox.SelectedItem == null || PrivilegeTypeComboBox.SelectedItem is not ComboBoxItem selectedTypeItem)
            {
                await ShowInfo("Vui lòng chọn User hoặc Role và loại quyền!");
                return;
            }
            // Lấy tên người dùng/role và loại quyền
            _lastGrantee = UserRoleComboBox.SelectedItem.ToString();
            _lastType = selectedTypeItem.Content!.ToString();

            await ReloadPermissionsAsync();
            // Show/hide appropriate areas based on privilege type
            GrantArea.IsVisible = _lastType == "TABLE" || _lastType == "COL";
            GrantRoleArea.IsVisible = _lastType == "ROLE";

            // Nếu hiển thị GrantRoleArea, tải các role
            if (_lastType == "ROLE")
            {
                await LoadRolesForGrant(); // Tải Role vào ComboBox
                GrantRoleArea.IsVisible = true;
            }
            else
            {
                GrantRoleArea.IsVisible = false;
            }

            GrantArea.IsVisible = _lastType == "TABLE" || _lastType == "COL";
        }
        private async void ObjectTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedObjectType = (ObjectTypeComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
            if (string.IsNullOrEmpty(selectedObjectType)) return;

            _privilegeGrant.SelectedObjectType = selectedObjectType;
            await _privilegeGrant.LoadObjectNamesAsync();
            ObjectNameComboBox.Items.Clear();
            foreach (var name in _privilegeGrant.ObjectNames)
            {
                ObjectNameComboBox.Items.Add(new ComboBoxItem { Content = name });
            }
            ColumnNameComboBox.Items.Clear(); // Reset luôn cột
        }
        private string _selectedObjectName = "";

        private async void ObjectNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _privilegeGrant.SelectedObjectName = (ObjectNameComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";

            // Nếu chọn TABLE thì load column
            if (_privilegeGrant.SelectedObjectType != "TABLE") return;

            await _privilegeGrant.LoadColumnsAsync(); // Đã có logic trong ViewModel
            _privilegeGrant.ColumnNames.Insert(0, "Tất cả");
            ColumnNameComboBox.Items.Clear();
            foreach (var column in _privilegeGrant.ColumnNames)
            {
                ColumnNameComboBox.Items.Add(new ComboBoxItem { Content = column });
            }
        }
        private void PrivilegeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PrivilegeComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string privilege = selectedItem.Content?.ToString()?.ToUpper() ?? "";
                // Chỉ hiển thị ColumnNameComboBox khi chọn SELECT hoặc UPDATE
                if (privilege == "SELECT" || privilege == "UPDATE")
                {
                    ColumnNameComboBox.IsVisible = true;
                }
                else
                {
                    ColumnNameComboBox.IsVisible = false;
                }
            }
        }
        private async Task CheckEmptyAndNotifyAsync(object? selectedItem, string message)
        {
            var selectedStr = selectedItem?.ToString();
            if (string.IsNullOrWhiteSpace(selectedStr))
            {
                await ShowInfo(message);
            }
        }
        private async void GrantPrivilege_Click(object sender, RoutedEventArgs e)
        {
            if (ObjectTypeComboBox.SelectedItem != null &&
                ObjectNameComboBox.SelectedItem != null &&
                PrivilegeComboBox.SelectedItem != null)
            {
                var selectedUser = (UserRoleComboBox.SelectedItem as string) ?? "";
                var selectedPrivilege = (PrivilegeComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
                var selectedObjectType = (ObjectTypeComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
                var selectedObjectName = _selectedObjectName;
                var selectedColumnName = (ColumnNameComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
                var withGrantOption = WithGrantOptionCheckBox.IsChecked == true;


                await CheckEmptyAndNotifyAsync(selectedUser, "Vui lòng chọn User hoặc Role trước!");
                await CheckEmptyAndNotifyAsync(selectedPrivilege, "Vui lòng chọn loại quyền trước!");
                await CheckEmptyAndNotifyAsync(selectedObjectType, "Vui lòng chọn loại đối tượng trước!");
                await CheckEmptyAndNotifyAsync(selectedObjectName, "Vui lòng chọn tên đối tượng trước!");

                // Kiểm tra ràng buộc quyền với loại đối tượng
                if ((selectedPrivilege == "SELECT" || selectedPrivilege == "INSERT" || selectedPrivilege == "UPDATE" || selectedPrivilege == "DELETE")
                && selectedObjectType != "TABLE")
                {
                    await ShowInfo($"Quyền {selectedPrivilege} chỉ áp dụng cho TABLE!", "Lỗi");
                    return;
                }

                if (selectedPrivilege == "EXECUTE" && (selectedObjectType != "PROCEDURE" && selectedObjectType != "FUNCTION"))
                {
                    await ShowInfo("Quyền EXECUTE chỉ áp dụng cho PROCEDURE hoặc FUNCTION!", "Lỗi");
                    return;
                }

                var privilegeService = new PrivilegeService();

                // Nếu không lỗi thì thực hiện cấp quyền
                try
                {
                    await privilegeService.GrantPrivilegeAsync(
                        grantee: selectedUser,
                        objectType: selectedObjectType,
                        objectName: selectedObjectName,
                        privilege: selectedPrivilege,
                        withGrantOption: withGrantOption,
                        columnName: selectedColumnName
                    );
                    await ShowInfo($"Đã cấp quyền '{selectedPrivilege}' cho '{selectedUser}' trên '{selectedObjectName}' thành công!", "Thành công");
                }
                catch (Exception ex)
                {
                    await ShowInfo("Lỗi khi cấp quyền: " + ex.Message, "Lỗi");
                }
            }
        }
        private async void GrantRoleToUser_Click(object sender, RoutedEventArgs e)
        {
            if (UserRoleComboBox.SelectedItem != null && RoleComboBox.SelectedItem != null)
            {
                string user = UserRoleComboBox.SelectedItem.ToString();
                string role = RoleComboBox.SelectedItem.ToString();
                var roleService = new RoleService();
                await roleService.GrantRoleToUserAsync(role, user);
                await ShowInfo($"Đã cấp quyền '{role}' cho '{user}' thành công!", "Thành công");
            }
        }
        private async void Revoke_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_lastGrantee) || string.IsNullOrEmpty(_lastType))
            {
                await ShowInfo("Bạn cần phải bấm Check Permission trước!");
                return;
            }

            if (_lastPermissions.Count == 0)
            {
                await ShowInfo("Không có quyền nào được chọn.");
                return;
            }

            int idx = PermissionListBox.SelectedIndex;
            if (idx < 0 || idx >= _lastPermissions.Count)
            {
                await ShowInfo("Vui lòng chọn 1 quyền để thu hồi!");
                return;
            }
            var privilegeService = new PrivilegeService();
            var selectedRow = ParseSelectedRow(_lastPermissions[idx]);
            var (query, confirmMessage) = privilegeService.BuildRevokeQuery(_lastType, _lastGrantee, selectedRow);

            await CheckEmptyAndNotifyAsync(query, "Loại quyền không hợp lệ.");

            var confirm = await MessageBox.Show(this, confirmMessage, "Xác nhận thu hồi", MessageBox.MessageBoxButtons.YesNo);
            if (confirm != MessageBox.MessageBoxResult.Yes) return;

            try
            {
                using var conn = new OracleConnection(DatabaseSettings.ConnectionString);
                await conn.OpenAsync();
                using var cmd = new OracleCommand(query, conn);
                await cmd.ExecuteNonQueryAsync();

                await ShowInfo("Đã thu hồi quyền thành công!");
                await ReloadPermissionsAsync();
            }
            catch (Exception ex)
            {
                await ShowInfo("Lỗi khi thu hồi quyền: " + ex.Message, "Lỗi");
            }
        }
        private async Task ReloadPermissionsAsync()
        {
            if (string.IsNullOrEmpty(_lastGrantee) || string.IsNullOrEmpty(_lastType))
                return;

            // Gán lại selected vào ComboBox
            UserRoleComboBox.SelectedItem = _lastGrantee;
            PrivilegeTypeComboBox.SelectedIndex = GetPrivilegeTypeIndex(_lastType);

            // Mở kết nối và query
            using var conn = new OracleConnection(DatabaseSettings.ConnectionString);
            await conn.OpenAsync();
            var userService = new UserService();
            var dataTable = await userService.QueryPrivilegesAsync(conn, _lastType, _lastGrantee);
            if (dataTable == null || dataTable.Rows.Count == 0)
            {
                PermissionListBox.ItemsSource = null;
                return;
            }

            _lastPermissions = dataTable != null
                ? ConvertDataTableToList(dataTable).ConvertAll(dict => string.Join(" | ", dict.Select(kv => $"{kv.Key}: {kv.Value}")))
                : new List<string>();
            var lines = new List<string>(_lastPermissions.Count);
            if (dataTable == null || dataTable.Rows.Count == 0)
            {
                lines.Add("Không có dữ liệu quyền!");
            }
            else
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    var parts = new List<string>(dataTable.Columns.Count);
                    foreach (DataColumn col in dataTable.Columns)
                    {
                        parts.Add($"{col.ColumnName}: {row[col]}");
                    }
                    lines.Add(string.Join(" | ", parts));
                }
            }

            PermissionListBox.ItemsSource = lines;
        }

        #endregion
    }

}
