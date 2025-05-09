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
        private async Task ShowInfo(string message){
            await MessageBox.Show(this, message, "Thông báo", MessageBox.MessageBoxButtons.Ok);
        }

        private async Task ShowInfo(string message, string messageType){
            await MessageBox.Show(this, message, messageType, MessageBox.MessageBoxButtons.Ok);
        }
    }

}
