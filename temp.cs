// using Avalonia.Controls;
// using Avalonia.Controls.Primitives;
// using System;
// using System.Collections.Generic;
// using System.Data;
// using System.Collections.ObjectModel;
// using Oracle.ManagedDataAccess.Client;
// using System.Threading.Tasks;
// using System.Linq;
// using Avalonia.Interactivity;
// using System.Diagnostics;
// using AvaloniaPdbAccounts.Models; // Import model
// using AvaloniaPdbAccounts.Services; // Import service
// using static AvaloniaPdbAccounts.Utilities.Helpers;
// using AvaloniaPdbAccounts.ViewModels;
// using AvaloniaPdbAccounts.Utilities;

// namespace AvaloniaPdbAccounts{
//     public partial class MainWindow : Window{
//          private string _lastGrantee = "";
//         private string _lastType = "";
//         private List<Dictionary<string, object>> _lastPermissions = new();
//         private const string Infoconnect = DatabaseSettings.ConnectionString;
//         private UserService userService = new UserService();
//         private RoleService roleService = new RoleService();
//         private PrivilegeService privilegeService = new PrivilegeService();

//         private PrivilegeGrant privilegeGrantVM = new PrivilegeGrant();
//         private string _selectedObjectName = "";
//         public MainWindow(){
//             InitializeComponent();
//             this.Opened += (s, e) => {
//                 GrantArea.IsVisible = false;
//             };

//             var vm = this.DataContext as PrivilegeGrant;
//             if (vm != null)
//             {
//                 _selectedObjectName = vm.SelectedObjectName;
//                 // dùng objectName tại đây
//             }

//         }

//         private async void LoadAccounts_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e){
//             try{
//                 string connectionString = Infoconnect;
//                 using (var conn = new OracleConnection(connectionString)){
//                     await conn.OpenAsync();
//                 var users = await userService.GetAllUsersAsync(conn);
//                     AccountsListBox.ItemsSource = users;
//                 }
//             }
//             catch (Exception ex)
//             {
//                 await MessageBox.Show(this, $"Error: {ex.Message}", "Lỗi", MessageBox.MessageBoxButtons.Ok);
//             }
//         }
       
//         private async void ReloadUser()
//         {
//             var accounts = await userService.GetAccountsAsync();
//             AccountsListBox.ItemsSource = accounts;
//         }
//         private async void DeleteAccount_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
//         {
//             if (AccountsListBox.SelectedItem is string selectedAccount)
//             {
//                 var confirm = await MessageBox.Show(this, $"Bạn có chắc muốn xóa user '{selectedAccount}'?", "Xác nhận", MessageBox.MessageBoxButtons.YesNo);

//                 if (confirm == MessageBox.MessageBoxResult.Yes)
//                 {
//                     try
//                     {
//                         string connectionString = Infoconnect;

//                         using (var conn = new OracleConnection(connectionString))
//                         {
//                             await conn.OpenAsync();
//                             using (var cmd = new OracleCommand($"DROP USER {selectedAccount} CASCADE", conn))
//                             {
//                                 await cmd.ExecuteNonQueryAsync();
//                             }
//                         }

//                         ReloadUser();
//                     }
//                     catch (Exception ex)
//                     {
//                         await MessageBox.Show(this, $"Error: {ex.Message}", "Lỗi", MessageBox.MessageBoxButtons.Ok);
//                     }
//                 }
//             }
//         }
//         private async void EditAccount_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
//         {
//             if (AccountsListBox.SelectedItem is string selectedAccount)
//             {
//                 var newPassword = await MessageBox.InputBox(this, $"Nhập mật khẩu mới cho '{selectedAccount}'", "Đổi mật khẩu");

//                 if (!string.IsNullOrEmpty(newPassword))
//                 {
//                     try
//                     {
//                         string connectionString = Infoconnect;

//                         using (var conn = new OracleConnection(connectionString))
//                         {
//                             await conn.OpenAsync();
//                             using (var cmd = new OracleCommand($"ALTER USER {selectedAccount} IDENTIFIED BY \"{newPassword}\"", conn))
//                             {
//                                 await cmd.ExecuteNonQueryAsync();
//                             }
//                         }

//                         await MessageBox.Show(this, "Đổi mật khẩu thành công!", "Thành công", MessageBox.MessageBoxButtons.Ok);
//                     }
//                     catch (Exception ex)
//                     {
//                         await MessageBox.Show(this, $"Error: {ex.Message}", "Lỗi", MessageBox.MessageBoxButtons.Ok);
//                     }
//                 }
//             }
//         }
//         private async void AddUser_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e){
//             var username = await MessageBox.InputBox(this, "Nhập tên user mới", "Tạo User");
//             if (string.IsNullOrWhiteSpace(username))
//                 return;

//             var password = await MessageBox.InputBox(this, $"Nhập mật khẩu cho user '{username}'", "Tạo User");
//             if (string.IsNullOrWhiteSpace(password))
//                 return;

//             try{
//                 string connectionString = Infoconnect;

//                 using (var conn = new OracleConnection(connectionString)){
//                     await conn.OpenAsync();

//                     // Tạo user
//                     using (var cmdCreate = new OracleCommand($"CREATE USER {username} IDENTIFIED BY \"{password}\"", conn)){
//                         await cmdCreate.ExecuteNonQueryAsync();
//                     }

//                     // Grant quyền kết nối
//                     using (var cmdGrant = new OracleCommand($"GRANT CONNECT, RESOURCE TO {username}", conn)){
//                         await cmdGrant.ExecuteNonQueryAsync();
//                     }
//                 }

//                 await MessageBox.Show(this, $"Tạo user '{username}' thành công!", "Thành công", MessageBox.MessageBoxButtons.Ok);

//                 // Reload danh sách
//                 ReloadUser();
//             }
//             catch (Exception ex){
//                 await MessageBox.Show(this, $"Error: {ex.Message}", "Lỗi", MessageBox.MessageBoxButtons.Ok);
//             }
//         }



//         private async Task LoadAccountsListAsync(){
//             try{
//                 string connectionString = Infoconnect;

//                 using (var conn = new OracleConnection(connectionString)){
//                     await conn.OpenAsync();

//                     var users = await userService.GetAllUsersAsync(conn);
//                     var roles = await roleService.GetAllRolesAsync(conn);

//                     var all = new List<string>();
//                     all.AddRange(users);
//                     all.AddRange(roles);




//                     UserRoleComboBox.ItemsSource = all;
//                     if (all.Count > 0)
//                         UserRoleComboBox.SelectedIndex = 0;


//                 }
//             }
//             catch (Exception ex){
//                 await MessageBox.Show(this, $"Error: {ex.Message}", "Lỗi", MessageBox.MessageBoxButtons.Ok);
//             }
//         }
//         private async void LoadRoles_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e){
//             var accounts = await roleService.RLoadRolesAsync();
//             AccountsListBox.ItemsSource = accounts;
//         }

//         private async void ReloadRole(){
//             var accounts = await roleService.RLoadRolesAsync();
//             AccountsListBox.ItemsSource = accounts;
//         }

//         private async void DeleteRole_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e){
//             if (AccountsListBox.SelectedItem is string selectedAccount){
//                 var confirm = await MessageBox.Show(this, $"Bạn có chắc muốn xóa role '{selectedAccount}'?", "Xác nhận", MessageBox.MessageBoxButtons.YesNo);

//                 if (confirm == MessageBox.MessageBoxResult.Yes)   {
//                     try{
//                         string connectionString = Infoconnect;

//                         using (var conn = new OracleConnection(connectionString))        {
//                             await conn.OpenAsync();
//                             using (var cmd = new OracleCommand($"DROP ROLE {selectedAccount}", conn))                  {
//                                 await cmd.ExecuteNonQueryAsync();
//                             }
//                         }

//                         ReloadRole();
//                     }
//                     catch (Exception ex)     {
//                         await MessageBox.Show(this, $"Error: {ex.Message}", "Lỗi", MessageBox.MessageBoxButtons.Ok);
//                     }
//                 }
//             }
//         }
//         private async void EditRole_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
//         {
//             if (AccountsListBox.SelectedItem is string selectedAccount)
//             {
//                 var role = selectedAccount;
//                 var privileges = await roleService.GetRolePrivilegesAsync(role);
//                 var selectedPrivileges = new ObservableCollection<PrivilegeItem>(privileges);

//                 // Store the currently selected item
//                 var previouslySelectedItem = AccountsListBox.SelectedItem;

//                 var result = await ShowEditPrivilegesDialog(this, role, selectedPrivileges);
//                 if (result)
//                 {
//                     await roleService.UpdateRolePrivilegesAsync(role, selectedPrivileges);
//                     ReloadRole();
//                 }
//                 AccountsListBox.SelectedItem = previouslySelectedItem;
//             }
//         }
//         private async void AddRole_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
//         {
//             var role = await MessageBox.InputBox(this, "Nhập tên role mới", "Tạo Role");
//             if (string.IsNullOrWhiteSpace(role))
//                 return;

//             try
//             {
//                 string connectionString = Infoconnect;

//                 using (var conn = new OracleConnection(connectionString))
//                 {
//                     await conn.OpenAsync();

//                     // Tạo user
//                     using (var cmdCreate = new OracleCommand($"CREATE ROLE {role} ", conn))
//                     {
//                         await cmdCreate.ExecuteNonQueryAsync();
//                     }

//                     // Grant quyền kết nối
//                     using (var cmdGrant = new OracleCommand($"GRANT CONNECT, RESOURCE TO {role}", conn))
//                     {
//                         await cmdGrant.ExecuteNonQueryAsync();
//                     }
//                 }

//                 await MessageBox.Show(this, $"Tạo role '{role}' thành công!", "Thành công", MessageBox.MessageBoxButtons.Ok);

//                 // Reload danh sách
//                 ReloadRole();
//             }
//             catch (Exception ex)
//             {
//                 await MessageBox.Show(this, $"Error: {ex.Message}", "Lỗi", MessageBox.MessageBoxButtons.Ok);
//             }
//         }

//     private async void CheckPermission_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
//         {
//             try
//             {
//                 await LoadAccountsListAsync(); // Load User + Role vào UserRoleComboBox

//                 // Hiện các trường lựa chọn quyền và đối tượng
//                 CheckArea.IsVisible = true;

//                 await MessageBox.Show(this, "Vui lòng chọn User/Role và loại quyền, sau đó bấm 'Check'!", "Thông báo", MessageBox.MessageBoxButtons.Ok);
//             }
//             catch (Exception ex)
//             {
//                 await MessageBox.Show(this, $"Error: {ex.Message}", "Lỗi", MessageBox.MessageBoxButtons.Ok);
//             }
//         }

//     private async void ConfirmCheckButton_Click(object? sender, RoutedEventArgs e)
// {
 

//     try
//     {
//         if (UserRoleComboBox.SelectedItem == null ||
//             PrivilegeTypeComboBox.SelectedItem is not ComboBoxItem selectedTypeItem)
//         {
//             await MessageBox.Show(this,
//                 "Vui lòng chọn User hoặc Role và loại quyền!",
//                 "Thông báo",
//                 MessageBox.MessageBoxButtons.Ok);
//             return;
//         }

//         var selectedName = UserRoleComboBox.SelectedItem.ToString()!;
//         var selectedType = selectedTypeItem.Content!.ToString()!;

//            _lastGrantee = selectedName;
//             _lastType = selectedType;

//         using var conn = new OracleConnection(Infoconnect);
//         await conn.OpenAsync();
//         var dataTable = await userService.QueryPrivilegesAsync(conn, selectedType, selectedName);
//             _lastPermissions = ConvertDataTableToList(dataTable);

//         var lines = new List<string>();

//         if (dataTable == null || dataTable.Rows.Count == 0)
//         {
//             lines.Add("Không có dữ liệu quyền!");
//         }
//         else
//         {
//             foreach (DataRow row in dataTable.Rows)
//             {
//                 var parts = new List<string>(dataTable.Columns.Count);
//                 foreach (DataColumn col in dataTable.Columns)
//                 {
//                     parts.Add($"{col.ColumnName}:{row[col]}");
//                 }
//                 lines.Add(string.Join(" | ", parts));
//             }
//         }

//         // Bind vào ListBox
//         PermissionListBox.ItemsSource = lines;

//         // Kiểm tra nếu loại quyền là ROLE thì show GrantRoleArea
//         if (selectedType == "ROLE")
//         {
//             GrantRoleArea.IsVisible = true; // Hiển thị GrantRoleArea
//             await LoadRolesAsync();
//         }
//         else
//         {
//             GrantRoleArea.IsVisible = false; // Ẩn GrantRoleArea nếu không phải ROLE
//         }

//         // Kiểm tra loại quyền để hiển thị các ô Grant
//         if (selectedType == "ROLE" || selectedType == "SYSTEM")
//         {
//             // Hiển thị Grant phần cột nếu loại quyền là ROLE hoặc SYSTEM
//             ColumnNameComboBox.IsVisible = false;  // Ẩn cột
//         }
//         else
//         {
//             ColumnNameComboBox.IsVisible = true;  // Hiển thị cột nếu không phải ROLE hoặc SYSTEM
//         }
//         GrantArea.IsVisible = true;

//     }
//     catch (Exception ex)
//     {
//         await MessageBox.Show(this,
//             $"Error: {ex.Message}",
//             "Lỗi",
//             MessageBox.MessageBoxButtons.Ok);
//     }
// }


//     private async void Revoke_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
// {
//     try
//     {
//         // 1. Kiểm tra xem đã Check Permission chưa
//         if (string.IsNullOrEmpty(_lastGrantee) || string.IsNullOrEmpty(_lastType))
//         {
//             await MessageBox.Show(this,
//                 "Bạn cần phải bấm Check Permission trước!",
//                 "Thông báo",
//                 MessageBox.MessageBoxButtons.Ok);
//             return;
//         }

//         // 2. Kiểm tra đã chọn dòng nào trong ListBox chưa
//         var idx = PermissionListBox.SelectedIndex;
//         if (idx < 0 || idx >= _lastPermissions.Count)
//         {
//             await MessageBox.Show(this,
//                 "Vui lòng chọn 1 quyền để thu hồi!",
//                 "Thông báo",
//                 MessageBox.MessageBoxButtons.Ok);
//             return;
//         }

//         // 3. Lấy dòng dữ liệu đã chọn
//         var selectedRow = _lastPermissions[idx];
//         string query = "";
//         string confirmMessage = "";
//         string privilege = "";
//         string user = _lastGrantee;

//         // 4. Tạo câu lệnh REVOKE tùy theo loại quyền
//         switch (_lastType)
//         {
//             case "ROLE":
//                 privilege = selectedRow["GRANTED_ROLE"].ToString()!;
//                 query = $"REVOKE {privilege} FROM {user}";
//                 confirmMessage = $"Bạn có chắc chắn muốn thu hồi quyền {privilege} từ {user}?";
//                 break;

//             case "SYSTEM":
//                 privilege = selectedRow["PRIVILEGE"].ToString()!;
//                 query = $"REVOKE {privilege} FROM {user}";
//                 confirmMessage = $"Bạn có chắc chắn muốn thu hồi quyền {privilege} từ {user}?";
//                 break;

//             case "TABLE":
//                 var table = selectedRow["TABLE_NAME"].ToString()!;
//                 var owner = selectedRow["OWNER"].ToString()!;
//                 privilege = selectedRow["PRIVILEGE"].ToString()!;
//                 query = $"REVOKE {privilege} ON {owner}.{table} FROM {user}";
//                 confirmMessage = $"Bạn có chắc chắn muốn thu hồi quyền {privilege} trên bảng {owner}.{table} từ {user}?";
//                 break;

//             case "COL":
//                 var column = selectedRow["COLUMN_NAME"].ToString()!;
//                 table = selectedRow["TABLE_NAME"].ToString()!;
//                 owner = selectedRow["OWNER"].ToString()!;
//                 privilege = selectedRow["PRIVILEGE"].ToString()!;
//                 query = $"REVOKE {privilege} ON {owner}.{table} FROM {user}";
//                 confirmMessage = $"Bạn có chắc chắn muốn thu hồi quyền {privilege} trên cột {column} của bảng {owner}.{table} từ {user}?";
//                 break;

//             default:
//                 await MessageBox.Show(this,
//                     "Loại quyền không hợp lệ.",
//                     "Lỗi",
//                     MessageBox.MessageBoxButtons.Ok);
//                 return;
//         }

//         // 5. Xác nhận với người dùng
//         var confirm = await MessageBox.Show(this,
//             confirmMessage,
//             "Xác nhận thu hồi",
//             MessageBox.MessageBoxButtons.YesNo);
//         if (confirm != MessageBox.MessageBoxResult.Yes)
//             return;

//         // 6. Thực hiện thu hồi
//         using var conn = new OracleConnection(Infoconnect);
//         await conn.OpenAsync();
//         using var cmd = new OracleCommand(query, conn);
//         await cmd.ExecuteNonQueryAsync();

//         await MessageBox.Show(this,
//             "Thu hồi quyền thành công!",
//             "Thành công",
//             MessageBox.MessageBoxButtons.Ok);

//         // 7. Refresh lại quyền mới
//         await ReloadPermissionsAsync();
//     }
//     catch (Exception ex)
//     {
//         await MessageBox.Show(this,
//             $"Lỗi khi thu hồi quyền: {ex.Message}",
//             "Lỗi",
//             MessageBox.MessageBoxButtons.Ok);
//     }
// }

//     // Hàm reload permission sau khi Revoke
//     private async Task ReloadPermissionsAsync()
//     {
//         if (string.IsNullOrEmpty(_lastGrantee) || string.IsNullOrEmpty(_lastType))
//             return;

//         // Gán lại selected vào ComboBox
//         UserRoleComboBox.SelectedItem = _lastGrantee;
//         PrivilegeTypeComboBox.SelectedIndex = GetPrivilegeTypeIndex(_lastType);

//         // Mở kết nối và query
//         using var conn = new OracleConnection(Infoconnect);
//         await conn.OpenAsync();

//         var dataTable = await userService.QueryPrivilegesAsync(conn, _lastType, _lastGrantee);
//         if (dataTable == null || dataTable.Rows.Count == 0)
//         {
//             PermissionListBox.ItemsSource = null;
//             return;
//         }

//         _lastPermissions = ConvertDataTableToList(dataTable);

//         var lines = new List<string>(_lastPermissions.Count);
//         foreach (var dict in _lastPermissions)
//         {
//             var parts = new List<string>();
//             foreach (var kv in dict)
//                 parts.Add($"{kv.Key}:{kv.Value}");
//             lines.Add(string.Join(" | ", parts));
//         }

//         PermissionListBox.ItemsSource = lines;
//     }

//             private async Task LoadRolesAsync()
//         {
//             try
//             {
//                 using var conn = new OracleConnection(Infoconnect);
//                 await conn.OpenAsync();

//                 var roles = new List<string>();
//                 string sql = "SELECT role FROM dba_roles";

//                 using var cmd = new OracleCommand(sql, conn);
//                 using var reader = await cmd.ExecuteReaderAsync();
//                 while (await reader.ReadAsync())
//                 {
//                     roles.Add(reader.GetString(0));
//                 }

//                 RoleComboBox.Items.Clear();
//                 foreach (var role in roles)
//                 {
//                     RoleComboBox.Items.Add(new ComboBoxItem { Content = role });
//                 }
//             }
//             catch (Exception ex)
//             {
//                 await MessageBox.Show(this, $"Error loading roles: {ex.Message}", "Lỗi", MessageBox.MessageBoxButtons.Ok);
//             }
//         }
//         private async void ObjectTypeComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
//         {
//             var selectedObjectType = (ObjectTypeComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
//             if (string.IsNullOrEmpty(selectedObjectType)) return;

//             List<string> objectNames = new();

//             using var conn = new OracleConnection(Infoconnect);
//             await conn.OpenAsync();

//             string sql = selectedObjectType switch
//             {
//                 "TABLE" => "SELECT table_name FROM user_tables",
//                 "VIEW" => "SELECT view_name FROM user_views",
//                 "PROCEDURE" => "SELECT object_name FROM user_procedures WHERE object_type = 'PROCEDURE'",
//                 "FUNCTION" => "SELECT object_name FROM user_procedures WHERE object_type = 'FUNCTION'",
//                 _ => ""
//             };

//             if (string.IsNullOrEmpty(sql)) return;

//             using var cmd = new OracleCommand(sql, conn);
//             using var reader = await cmd.ExecuteReaderAsync();
//             while (await reader.ReadAsync())
//             {
//                 objectNames.Add(reader.GetString(0));
//             }

//             ObjectNameComboBox.Items.Clear();
//             foreach (var name in objectNames)
//             {
//                 ObjectNameComboBox.Items.Add(new ComboBoxItem { Content = name });
//             }

//             ColumnNameComboBox.Items.Clear(); // Reset luôn cột
//         }

//         private async void ObjectNameComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
//         {
//             _selectedObjectName = (ObjectNameComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
//             Debug.WriteLine("[LOG] Selected Object Name: " + _selectedObjectName);

//             // Nếu chọn TABLE thì load column
//             var selectedObjectType = (ObjectTypeComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
//             if (selectedObjectType != "TABLE") return;

//             List<string> columns = new();
//             using var conn = new OracleConnection(Infoconnect);
//             await conn.OpenAsync();

//             string columnSql = "SELECT column_name FROM user_tab_columns WHERE table_name = :tableName";
//             using var columnCmd = new OracleCommand(columnSql, conn);
//             columnCmd.Parameters.Add(":tableName", _selectedObjectName);
//             using var columnReader = await columnCmd.ExecuteReaderAsync();
//             while (await columnReader.ReadAsync())
//             {
//                 columns.Add(columnReader.GetString(0));
//             }
//             columns.Insert(0, "Tất cả");

//             ColumnNameComboBox.Items.Clear();
//             foreach (var column in columns)
//             {
//                 ColumnNameComboBox.Items.Add(new ComboBoxItem { Content = column });
//             }
//         }
  
//         private void PrivilegeComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
// {
//     if (PrivilegeComboBox.SelectedItem is ComboBoxItem selectedItem)
//     {
//         string privilege = selectedItem.Content?.ToString()?.ToUpper() ?? "";
//         // Chỉ hiển thị ColumnNameComboBox khi chọn SELECT hoặc UPDATE
//         if (privilege == "SELECT" || privilege == "UPDATE"){
//             ColumnNameComboBox.IsVisible = true;

//         }
//          else
//         {
//             ColumnNameComboBox.IsVisible = false;
//         }
//     }
// }

//         private async void GrantRoleToUser_Click(object? sender, RoutedEventArgs e)
// {
//     var selectedUser = (UserRoleComboBox.SelectedItem as string) ?? "";
//     var selectedRole = (RoleComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";

//     if (string.IsNullOrWhiteSpace(selectedUser))
//     {
//         await MessageBox.Show(this, "Vui lòng chọn User!", "Lỗi", MessageBox.MessageBoxButtons.Ok);
//         return;
//     }
//     if (string.IsNullOrWhiteSpace(selectedRole))
//     {
//         await MessageBox.Show(this, "Vui lòng chọn Role!", "Lỗi", MessageBox.MessageBoxButtons.Ok);
//         return;
//     }
//     try
//     {
//         using var conn = new OracleConnection(Infoconnect);
//         await conn.OpenAsync();

//         string grantRoleSql = $"GRANT \"{selectedRole}\" TO \"{selectedUser}\"";

//         using var cmd = new OracleCommand(grantRoleSql, conn);
//         await cmd.ExecuteNonQueryAsync();

//         await MessageBox.Show(this, $"Cấp Role '{selectedRole}' cho '{selectedUser}' thành công!", "Thành công", MessageBox.MessageBoxButtons.Ok);
//     }
//     catch (Exception ex)
//     {
//         await MessageBox.Show(this, $"Lỗi: {ex.Message}", "Error", MessageBox.MessageBoxButtons.Ok);
//     }
// }
//       private async void GrantPrivilege_Click(object? sender, RoutedEventArgs e)
// {
//     var selectedUser = (UserRoleComboBox.SelectedItem as string) ?? "";
//     var selectedPrivilege = (PrivilegeComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
//     var selectedObjectType = (ObjectTypeComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
//     var selectedObjectName = _selectedObjectName;
//     var selectedColumnName = (ColumnNameComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
//     var withGrantOption = WithGrantOptionCheckBox.IsChecked == true;

//     if (string.IsNullOrEmpty(selectedUser))
//     {
//         await MessageBox.Show(this, "Vui lòng chọn user hoặc role!", "Error", MessageBox.MessageBoxButtons.Ok);
//         return;
//     }
//     if (string.IsNullOrEmpty(selectedPrivilege))
//     {
//         await MessageBox.Show(this, "Vui lòng chọn quyền!", "Error", MessageBox.MessageBoxButtons.Ok);
//         return;
//     }
//     if (string.IsNullOrEmpty(selectedObjectType))
//     {
//         await MessageBox.Show(this, "Vui lòng chọn loại đối tượng!", "Error", MessageBox.MessageBoxButtons.Ok);
//         return;
//     }
//     if (string.IsNullOrEmpty(selectedObjectName))
//     {
//         await MessageBox.Show(this, "Vui lòng chọn tên đối tượng!", "Error", MessageBox.MessageBoxButtons.Ok);
//         return;
//     }

//     // Kiểm tra ràng buộc quyền với loại đối tượng
//     if ((selectedPrivilege == "SELECT" || selectedPrivilege == "INSERT" || selectedPrivilege == "UPDATE" || selectedPrivilege == "DELETE")
//         && selectedObjectType != "TABLE")
//     {
//         await MessageBox.Show(this, $"Quyền {selectedPrivilege} chỉ áp dụng cho TABLE!", "Lỗi", MessageBox.MessageBoxButtons.Ok);
//         return;
//     }

//     if (selectedPrivilege == "EXECUTE" && (selectedObjectType != "PROCEDURE" && selectedObjectType != "FUNCTION"))
//     {
//         await MessageBox.Show(this, "Quyền EXECUTE chỉ áp dụng cho PROCEDURE hoặc FUNCTION!", "Lỗi", MessageBox.MessageBoxButtons.Ok);
//         return;
//     }

//     // Nếu không lỗi thì thực hiện cấp quyền
//     try
//     {
//         await privilegeService.GrantPrivilegeAsync(
//             grantee: selectedUser,
//             objectType: selectedObjectType,
//             objectName: selectedObjectName,
//             privilege: selectedPrivilege,
//             withGrantOption: withGrantOption,
//             columnName: selectedColumnName
//         );

//         await MessageBox.Show(this,
//             $"Đã cấp quyền '{selectedPrivilege}' cho '{selectedUser}' trên '{selectedObjectName}' thành công!",
//             "Thành công",
//             MessageBox.MessageBoxButtons.Ok);
//     }
//     catch (Exception ex)
//     {
//         await MessageBox.Show(this,
//             $"Lỗi khi cấp quyền: {ex.Message}",
//             "Lỗi",
//             MessageBox.MessageBoxButtons.Ok);
//     }

// }

//     }
// }
