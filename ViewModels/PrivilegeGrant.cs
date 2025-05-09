using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Data;
using System.Collections.ObjectModel;
using Oracle.ManagedDataAccess.Client;
using System.Threading.Tasks;
using System.Linq;
using Avalonia.Interactivity;
using System.Diagnostics;
using AvaloniaPdbAccounts.Models; // Import model
using AvaloniaPdbAccounts.Services; // Import service
using static AvaloniaPdbAccounts.Utilities.Helpers;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AvaloniaPdbAccounts.ViewModels
{
    public class PrivilegeGrant : INotifyPropertyChanged
    {
        public ObservableCollection<string> ObjectTypes { get; set; } = new() { "TABLE", "VIEW", "PROCEDURE", "FUNCTION" };
        public ObservableCollection<string> ObjectNames { get; set; } = new();
        public ObservableCollection<string> ColumnNames { get; set; } = new();


        private string _selectedObjectType;
        public string SelectedObjectType
        {
            get => _selectedObjectType;
            set
            {
                if (_selectedObjectType != value)
                {
                    _selectedObjectType = value;
                    OnPropertyChanged();
                    _ = LoadObjectNamesAsync(); // Load bảng/tên đối tượng khi loại thay đổi
                }
            }
        }

        private string _selectedObjectName;
        public string SelectedObjectName
        {
            get => _selectedObjectName;
            set
            {
                if (_selectedObjectName != value)
                {
                    _selectedObjectName = value;
                    OnPropertyChanged();
                    _ = LoadColumnsAsync(); // Nếu là TABLE thì load cột
                }
            }
        }

        private string _selectedPrivilege;
        public string SelectedPrivilege
        {
            get => _selectedPrivilege;
            set
            {
                if (_selectedPrivilege != value)
                {
                    _selectedPrivilege = value;
                    OnPropertyChanged();
                    IsColumnComboBoxVisible = _selectedPrivilege is "SELECT" or "UPDATE";
                }
            }
        }

        private bool _isColumnComboBoxVisible;
        public bool IsColumnComboBoxVisible
        {
            get => _isColumnComboBoxVisible;
            set
            {
                _isColumnComboBoxVisible = value;
                OnPropertyChanged();
            }
        }

        private const string Infoconnect = DatabaseSettings.ConnectionString;

        public async Task LoadObjectNamesAsync()
        {
            ObjectNames.Clear();
            if (string.IsNullOrWhiteSpace(SelectedObjectType)) return;

            using var conn = new Oracle.ManagedDataAccess.Client.OracleConnection(Infoconnect);
            await conn.OpenAsync();

            string sql = SelectedObjectType switch
            {
                "TABLE" => "SELECT table_name FROM user_tables",
                "VIEW" => "SELECT view_name FROM user_views",
                "PROCEDURE" => "SELECT object_name FROM user_procedures WHERE object_type = 'PROCEDURE'",
                "FUNCTION" => "SELECT object_name FROM user_procedures WHERE object_type = 'FUNCTION'",
                _ => ""
            };

            if (string.IsNullOrEmpty(sql)) return;

            using var cmd = new Oracle.ManagedDataAccess.Client.OracleCommand(sql, conn);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                ObjectNames.Add(reader.GetString(0));
            }

            ColumnNames.Clear(); // Reset column list
        }

        public async Task LoadColumnsAsync()
        {
            if (SelectedObjectType != "TABLE") return;

            using var conn = new Oracle.ManagedDataAccess.Client.OracleConnection(Infoconnect);
            await conn.OpenAsync();

            string sql = "SELECT column_name FROM user_tab_columns WHERE table_name = :tableName";
            using var cmd = new Oracle.ManagedDataAccess.Client.OracleCommand(sql, conn);
            cmd.Parameters.Add(":tableName", SelectedObjectName);
            using var reader = await cmd.ExecuteReaderAsync();
            ColumnNames.Clear();
            ColumnNames.Add("Tất cả");
            while (await reader.ReadAsync())
            {
                ColumnNames.Add(reader.GetString(0));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
