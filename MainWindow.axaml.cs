using Avalonia.Controls;
using System;
using System.Collections.ObjectModel;
using Oracle.ManagedDataAccess.Client;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace AvaloniaPdbAccounts
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

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
                string connectionString = "User Id=sys;Password=123;Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=AuditDB)));DBA Privilege=SYSDBA;";

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
                        string connectionString = "User Id=sys;Password=123;Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=AuditDB)));DBA Privilege=SYSDBA;";

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
                        string connectionString = "User Id=sys;Password=123;Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=AuditDB)));DBA Privilege=SYSDBA;";

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
                string connectionString = "User Id=sys;Password=123;Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=AuditDB)));DBA Privilege=SYSDBA;";

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
