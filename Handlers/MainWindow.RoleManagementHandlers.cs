using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaPdbAccounts.Services;
using AvaloniaPdbAccounts.ViewModels;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Threading.Tasks;
using  AvaloniaPdbAccounts.Utilities;
using  AvaloniaPdbAccounts.Models;
using System.Collections.Generic;
using System.Data;
using static AvaloniaPdbAccounts.Utilities.Helpers;
using System.Linq;


namespace AvaloniaPdbAccounts{
    public partial class MainWindow {
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
    }
}