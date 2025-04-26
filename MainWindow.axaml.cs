using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using System.Threading.Tasks;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.VisualBasic;

namespace AvaloniaPdbAccounts
{
    public partial class MainWindow : Window
    {
        private const string Infoconnect = "User Id=sys;Password=123;Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=AuditDB)));DBA Privilege=SYSDBA;";
        public MainWindow()
        {
            InitializeComponent();
        }


        //CRUD USER
        private async void LoadAccounts_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var accounts = await GetAccountsAsync();
            AccountsListBox.ItemsSource = accounts;
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

        //CRUD ROLE
        private async void LoadRoles_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var accounts = await LoadRolesAsync();
            AccountsListBox.ItemsSource = accounts;
        }

        private async Task<ObservableCollection<string>> LoadRolesAsync()
        {
            var accounts = new ObservableCollection<string>();

            try
            {
                string connectionString = Infoconnect;

                using (var conn = new OracleConnection(connectionString))
                {
                    await conn.OpenAsync();

                    using (var cmd = new OracleCommand("SELECT role FROM dba_roles GROUP BY role", conn))
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

        private async void ReloadRole()
        {
            var accounts = await LoadRolesAsync();
            AccountsListBox.ItemsSource = accounts;
        }

        private async void DeleteRole_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (AccountsListBox.SelectedItem is string selectedAccount)
            {
                var confirm = await MessageBox.Show(this, $"Bạn có chắc muốn xóa role '{selectedAccount}'?", "Xác nhận", MessageBox.MessageBoxButtons.YesNo);

                if (confirm == MessageBox.MessageBoxResult.Yes)
                {
                    try
                    {
                        string connectionString = Infoconnect;

                        using (var conn = new OracleConnection(connectionString))
                        {
                            await conn.OpenAsync();
                            using (var cmd = new OracleCommand($"DROP ROLE {selectedAccount}", conn))
                            {
                                await cmd.ExecuteNonQueryAsync();
                            }
                        }

                        ReloadRole();
                    }
                    catch (Exception ex)
                    {
                        await MessageBox.Show(this, $"Error: {ex.Message}", "Lỗi", MessageBox.MessageBoxButtons.Ok);
                    }
                }
            }
        }
        private async void EditRole_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (AccountsListBox.SelectedItem is string selectedAccount)
            {
                var role = selectedAccount;
                var privileges = await GetRolePrivilegesAsync(role);
                var selectedPrivileges = new ObservableCollection<PrivilegeItem>(privileges);

                // Store the currently selected item
                var previouslySelectedItem = AccountsListBox.SelectedItem;

                var result = await ShowEditPrivilegesDialog(role, selectedPrivileges);
                if (result)
                {
                    await UpdateRolePrivilegesAsync(role, selectedPrivileges);
                    ReloadRole();
                }

                // Re-select the previously selected item
                AccountsListBox.SelectedItem = previouslySelectedItem;
            }
        }

        private async Task<ObservableCollection<PrivilegeItem>> GetRolePrivilegesAsync(string role)
        {
            var privileges = new ObservableCollection<PrivilegeItem>();

            try
            {
                string connectionString = Infoconnect;

                using (var conn = new OracleConnection(connectionString))
                {
                    await conn.OpenAsync();

                    // Define a list of basic system privileges
                    var basicPrivileges = new List<string>
                            {
                                "SELECT",
                                "INSERT",
                                "UPDATE",
                                "DELETE",
                                "EXECUTE",
                                "ALTER",
                                "CREATE",
                                "DROP",
                                "GRANT",
                                "REVOKE"
                            };

                    // Add basic privileges to the privileges collection
                    foreach (var privilegeName in basicPrivileges)
                    {
                        if (!privileges.Any(p => p.Name == privilegeName))
                        {
                            privileges.Add(new PrivilegeItem { Name = privilegeName, IsGranted = false });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await MessageBox.Show(this, $"Error: {ex.Message}", "Lỗi", MessageBox.MessageBoxButtons.Ok);
            }

            return privileges;
        }


        private async Task<bool> ShowEditPrivilegesDialog(string role, ObservableCollection<PrivilegeItem> privileges)
        {
            var dlg = new Window
            {
                Title = $"Chỉnh sửa quyền cho role '{role}'",
                Width = 300,
                Height = 400,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition(GridLength.Star)); // List chiếm phần lớn
            grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto)); // Button tự co nhỏ

            var listBox = new ListBox
            {
                SelectionMode = SelectionMode.Multiple,
                [ScrollViewer.VerticalScrollBarVisibilityProperty] = ScrollBarVisibility.Auto
            };

            foreach (var privilege in privileges)
            {
                var checkBox = new CheckBox
                {
                    Content = privilege.Name,
                    IsChecked = privilege.IsGranted
                };

                listBox.Items.Add(checkBox);
            }

            var okButton = new Button
            {
                Content = "OK",
                Width = 60,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            };

            okButton.Click += (_, __) =>
            {
                for (int i = 0; i < listBox.Items.Count; i++)
                {
                    if (listBox.Items[i] is CheckBox checkBox && privileges.Count > i)
                    {
                        privileges[i].IsGranted = checkBox.IsChecked == true;
                    }
                }
                dlg.Close();
            };

            // Add ListBox vào dòng 0
            Grid.SetRow(listBox, 0);
            grid.Children.Add(listBox);

            // Add OK Button vào dòng 1
            Grid.SetRow(okButton, 1);
            grid.Children.Add(okButton);

            dlg.Content = grid;

            await dlg.ShowDialog(this);
            return true;
        }
        private async Task UpdateRolePrivilegesAsync(string role, ObservableCollection<PrivilegeItem> privileges)
        {
            try
            {
                string connectionString = Infoconnect;

                using (var conn = new OracleConnection(connectionString))
                {
                    await conn.OpenAsync();

                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            foreach (var privilege in privileges)
                            {
                                Console.WriteLine($"Privilege: {privilege.Name}");
                                var sql = privilege.IsGranted
                                    ? $"GRANT \"{privilege.Name}\" TO \"{role}\""
                                    : $"REVOKE \"{privilege.Name}\" FROM \"{role}\"";

                                using (var cmd = new OracleCommand(sql, conn))
                                {
                                    cmd.Transaction = transaction;
                                    await cmd.ExecuteNonQueryAsync();
                                }
                            }

                            await transaction.CommitAsync();
                        }
                        catch (Exception innerEx)
                        {
                            await transaction.RollbackAsync();
                            throw new Exception($"Transaction failed: {innerEx.Message}", innerEx);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await MessageBox.Show(this, $"Error: {ex.Message}", "Lỗi", MessageBox.MessageBoxButtons.Ok);
            }
        }


        public class PrivilegeItem
        {
            public string? Name { get; set; }
            public bool IsGranted { get; set; }
        }
        private async void AddRole_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var role = await MessageBox.InputBox(this, "Nhập tên role mới", "Tạo Role");
            if (string.IsNullOrWhiteSpace(role))
                return;

            try
            {
                string connectionString = Infoconnect;

                using (var conn = new OracleConnection(connectionString))
                {
                    await conn.OpenAsync();

                    // Tạo user
                    using (var cmdCreate = new OracleCommand($"CREATE ROLE {role} ", conn))
                    {
                        await cmdCreate.ExecuteNonQueryAsync();
                    }

                    // Grant quyền kết nối
                    using (var cmdGrant = new OracleCommand($"GRANT CONNECT, RESOURCE TO {role}", conn))
                    {
                        await cmdGrant.ExecuteNonQueryAsync();
                    }
                }

                await MessageBox.Show(this, $"Tạo role '{role}' thành công!", "Thành công", MessageBox.MessageBoxButtons.Ok);

                // Reload danh sách
                ReloadRole();
            }
            catch (Exception ex)
            {
                await MessageBox.Show(this, $"Error: {ex.Message}", "Lỗi", MessageBox.MessageBoxButtons.Ok);
            }
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
