using Avalonia.Controls;
using System;
using System.Collections.ObjectModel;
using Oracle.ManagedDataAccess.Client;
using System.Threading.Tasks;

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
                string connectionString = "User Id=sys;Password=new_password;Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=FREE)));DBA Privilege=SYSDBA;";
                
                using (var conn = new OracleConnection(connectionString))
                {
                    await conn.OpenAsync();

                    using (var cmd = new OracleCommand("SELECT username FROM dba_users", conn))
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
    }
}
