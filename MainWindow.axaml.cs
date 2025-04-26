using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Data;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using System.Threading.Tasks;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.VisualBasic;
using Avalonia.Data;   // for Binding


namespace AvaloniaPdbAccounts
{
    public partial class MainWindow : Window
    {
        private const string Infoconnect = "User Id=sys;Password=123;Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=orcl21)));DBA Privilege=SYSDBA;";
        public MainWindow()
        {
            InitializeComponent();
        }

      private async void LoadAccounts_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
{
    try
    {
        string connectionString = Infoconnect;

        using (var conn = new OracleConnection(connectionString))
        {
            await conn.OpenAsync();

            var users = await GetAllUsersAsync(conn);

            


            AccountsListBox.ItemsSource = users;
        }
    }
    catch (Exception ex)
    {
        await MessageBox.Show(this, $"Error: {ex.Message}", "Lỗi", MessageBox.MessageBoxButtons.Ok);
    }
}

        private async Task<ObservableCollection<string>> GetAccountsAsync()
        {
            var accounts = new ObservableCollection<string>();

            try
            {
                string connectionString = Infoconnect;

                using (var conn = new OracleConnection(connectionString))
                {
                    await conn.OpenAsync();

                    using (var cmd = new OracleCommand("SELECT * FROM ALL_USERS", conn))
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

        private async void ReloadUser()
        {
            var accounts = await GetAccountsAsync();
            AccountsListBox.ItemsSource = accounts;
        }
        private async void DeleteAccount_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (AccountsListBox.SelectedItem is string selectedAccount)
            {
                var confirm = await MessageBox.Show(this, $"Bạn có chắc muốn xóa user '{selectedAccount}'?", "Xác nhận", MessageBox.MessageBoxButtons.YesNo);

                if (confirm == MessageBox.MessageBoxResult.Yes)
                {
                    try
                    {
                        string connectionString = Infoconnect;

                        using (var conn = new OracleConnection(connectionString))
                        {
                            await conn.OpenAsync();
                            using (var cmd = new OracleCommand($"DROP USER {selectedAccount} CASCADE", conn))
                            {
                                await cmd.ExecuteNonQueryAsync();
                            }
                        }

                        ReloadUser();
                    }
                    catch (Exception ex)
                    {
                        await MessageBox.Show(this, $"Error: {ex.Message}", "Lỗi", MessageBox.MessageBoxButtons.Ok);
                    }
                }
            }
        }
        private async void EditAccount_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (AccountsListBox.SelectedItem is string selectedAccount)
            {
                var newPassword = await MessageBox.InputBox(this, $"Nhập mật khẩu mới cho '{selectedAccount}'", "Đổi mật khẩu");

                if (!string.IsNullOrEmpty(newPassword))
                {
                    try
                    {
                        string connectionString = Infoconnect;

                        using (var conn = new OracleConnection(connectionString))
                        {
                            await conn.OpenAsync();
                            using (var cmd = new OracleCommand($"ALTER USER {selectedAccount} IDENTIFIED BY \"{newPassword}\"", conn))
                            {
                                await cmd.ExecuteNonQueryAsync();
                            }
                        }

                        await MessageBox.Show(this, "Đổi mật khẩu thành công!", "Thành công", MessageBox.MessageBoxButtons.Ok);
                    }
                    catch (Exception ex)
                    {
                        await MessageBox.Show(this, $"Error: {ex.Message}", "Lỗi", MessageBox.MessageBoxButtons.Ok);
                    }
                }
            }
        }
        private async void AddUser_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var username = await MessageBox.InputBox(this, "Nhập tên user mới", "Tạo User");
            if (string.IsNullOrWhiteSpace(username))
                return;

            var password = await MessageBox.InputBox(this, $"Nhập mật khẩu cho user '{username}'", "Tạo User");
            if (string.IsNullOrWhiteSpace(password))
                return;

            try
            {
                string connectionString = Infoconnect;

                using (var conn = new OracleConnection(connectionString))
                {
                    await conn.OpenAsync();

                    // Tạo user
                    using (var cmdCreate = new OracleCommand($"CREATE USER {username} IDENTIFIED BY \"{password}\"", conn))
                    {
                        await cmdCreate.ExecuteNonQueryAsync();
                    }

                    // Grant quyền kết nối
                    using (var cmdGrant = new OracleCommand($"GRANT CONNECT, RESOURCE TO {username}", conn))
                    {
                        await cmdGrant.ExecuteNonQueryAsync();
                    }
                }

                await MessageBox.Show(this, $"Tạo user '{username}' thành công!", "Thành công", MessageBox.MessageBoxButtons.Ok);

                // Reload danh sách
                ReloadUser();
            }
            catch (Exception ex)
            {
                await MessageBox.Show(this, $"Error: {ex.Message}", "Lỗi", MessageBox.MessageBoxButtons.Ok);
            }
        }

        private async Task LoadAccountsListAsync()
{
    try
    {
        string connectionString = Infoconnect;

        using (var conn = new OracleConnection(connectionString))
        {
            await conn.OpenAsync();

            var users = await GetAllUsersAsync(conn);
            var roles = await GetAllRolesAsync(conn);

            var all = new List<string>();
            all.AddRange(users);
            all.AddRange(roles);

         


            UserRoleComboBox.ItemsSource = all;
            if (all.Count > 0)
                UserRoleComboBox.SelectedIndex = 0;

            
        }
    }
    catch (Exception ex)
    {
        await MessageBox.Show(this, $"Error: {ex.Message}", "Lỗi", MessageBox.MessageBoxButtons.Ok);
    }
}
      
    private async void CheckPermission_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
{
    try
    {
        await LoadAccountsListAsync(); // Load User + Role vào UserRoleComboBox

        CheckArea.IsVisible = true;

        await MessageBox.Show(this, "Vui lòng chọn User/Role và loại quyền, sau đó bấm 'Check'!", "Thông báo", MessageBox.MessageBoxButtons.Ok);
    }
    catch (Exception ex)
    {
        await MessageBox.Show(this, $"Error: {ex.Message}", "Lỗi", MessageBox.MessageBoxButtons.Ok);
    }
}
private async void ConfirmCheckButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
{
    try
    {
        if (UserRoleComboBox.SelectedItem == null ||
            PrivilegeTypeComboBox.SelectedItem is not ComboBoxItem selectedTypeItem)
        {
            await MessageBox.Show(this,
                "Vui lòng chọn User hoặc Role và loại quyền!",
                "Thông báo",
                MessageBox.MessageBoxButtons.Ok);
            return;
        }

        var selectedName = UserRoleComboBox.SelectedItem.ToString()!;
        var selectedType = selectedTypeItem.Content!.ToString()!;

        using var conn = new OracleConnection(Infoconnect);
        await conn.OpenAsync();
        var dataTable = await QueryPrivilegesAsync(conn, selectedType, selectedName);

        if (dataTable == null || dataTable.Rows.Count == 0)
        {
            await MessageBox.Show(this,
                "Không có dữ liệu quyền!",
                "Thông báo",
                MessageBox.MessageBoxButtons.Ok);
            return;
        }

        // flatten each DataRow into a single string without LINQ/Cast
        var lines = new List<string>(dataTable.Rows.Count);
        foreach (DataRow row in dataTable.Rows)
        {
            var parts = new List<string>(dataTable.Columns.Count);
            foreach (DataColumn col in dataTable.Columns)
            {
                parts.Add($"{col.ColumnName}:{row[col]}");
            }
            lines.Add(string.Join(" | ", parts));
        }

        // bind to your ListBox
        PermissionListBox.ItemsSource = lines;
    }
    catch (Exception ex)
    {
        await MessageBox.Show(this,
            $"Error: {ex.Message}",
            "Lỗi",
            MessageBox.MessageBoxButtons.Ok);
    }
}


       private async Task<List<string>> GetAllUsersAsync(OracleConnection conn)
{
    var users = new List<string>();
    using (var cmd = new OracleCommand("SELECT USERNAME FROM DBA_USERS WHERE ACCOUNT_STATUS = 'OPEN'", conn))
    using (var reader = await cmd.ExecuteReaderAsync())
    {
        while (await reader.ReadAsync())
        {
            users.Add(reader.GetString(0));
        }
    }
    return users;
}

private async Task<List<string>> GetAllRolesAsync(OracleConnection conn)
{
    var roles = new List<string>();
    using (var cmd = new OracleCommand("SELECT ROLE FROM DBA_ROLES", conn))
    using (var reader = await cmd.ExecuteReaderAsync())
    {
        while (await reader.ReadAsync())
        {
            roles.Add(reader.GetString(0));
        }
    }
    return roles;
}

private async Task<DataTable?> QueryPrivilegesAsync(OracleConnection conn, string selectedType, string selectedName)
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


        public static class MessageBox
        {
            public enum MessageBoxButtons { Ok, YesNo }
            public enum MessageBoxResult { Ok, Yes, No }

            public static async Task<MessageBoxResult> Show(Window owner, string text, string caption, MessageBoxButtons buttons)
            {
                var dlg = new Window
                {
                    Title = caption,
                    Width = 300,
                    Height = 150,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                var textBlock = new TextBlock { Text = text, Margin = new Avalonia.Thickness(10) };
                var okButton = new Button { Content = "OK", Width = 60 };
                var yesButton = new Button { Content = "Yes", Width = 60 };
                var noButton = new Button { Content = "No", Width = 60 };

                var buttonPanel = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, Spacing = 10 };

                if (buttons == MessageBoxButtons.Ok)
                    buttonPanel.Children.Add(okButton);
                else
                {
                    buttonPanel.Children.Add(yesButton);
                    buttonPanel.Children.Add(noButton);
                }

                var stack = new StackPanel();
                stack.Children.Add(textBlock);
                stack.Children.Add(buttonPanel);

                dlg.Content = stack;

                MessageBoxResult result = MessageBoxResult.Ok;

                okButton.Click += (_, __) => { result = MessageBoxResult.Ok; dlg.Close(); };
                yesButton.Click += (_, __) => { result = MessageBoxResult.Yes; dlg.Close(); };
                noButton.Click += (_, __) => { result = MessageBoxResult.No; dlg.Close(); };

                await dlg.ShowDialog(owner);
                return result;
            }

            public static async Task<string?> InputBox(Window owner, string text, string caption)
            {
                var dlg = new Window
                {
                    Title = caption,
                    Width = 300,
                    Height = 200,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                var textBlock = new TextBlock { Text = text, Margin = new Avalonia.Thickness(10) };
                var textBox = new TextBox { Margin = new Avalonia.Thickness(10) };
                var okButton = new Button { Content = "OK", Width = 60, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center };

                var stack = new StackPanel();
                stack.Children.Add(textBlock);
                stack.Children.Add(textBox);
                stack.Children.Add(okButton);

                dlg.Content = stack;

                string? result = null;

                okButton.Click += (_, __) => { result = textBox.Text; dlg.Close(); };

                await dlg.ShowDialog(owner);
                return result;
            }
        }

    }
}
