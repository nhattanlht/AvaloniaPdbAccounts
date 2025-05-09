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


namespace   AvaloniaPdbAccounts{
    public partial class MainWindow {
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
    }
}    
