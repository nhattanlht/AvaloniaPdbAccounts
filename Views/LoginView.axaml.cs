using Avalonia.Controls;
using AvaloniaPdbAccounts.ViewModels;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System;
using AvaloniaPdbAccounts.Services;
using static AvaloniaPdbAccounts.Utilities.Helpers;
using AvaloniaPdbAccounts.Models;
using System.Collections.Generic;
using static AvaloniaPdbAccounts.Utilities.RunSQLScriptUtility;

namespace AvaloniaPdbAccounts.Views
{
    public partial class LoginView : Window
    {
        private bool system;
        public LoginView()
        {
            RunAllSql();
            InitializeComponent();

            var loginVM = new LoginViewModel();
            DataContext = loginVM;

            // Create an instance of SymmetricEncryptionService
            loginVM.OnLoginSuccess += async () =>
            {
                system = DatabaseService.system;
                if (system)
                {
                    var mainWindow = new MainWindow();
                    mainWindow.Show();
                    this.Close();
                }
                else if (DatabaseService.CurrentRole != null && DatabaseService.CurrentRole.RoleName == "GV")
                {
                    var gvWindow = new GVView();
                    gvWindow.Show();
                    this.Close();
                }
                // else if (DatabaseService.CurrentRole != null && DatabaseService.CurrentRole.RoleName == "GV")
                // {
                //     var gvWindow = new GVView();
                //     gvWindow.Show();
                //     this.Close();
                // }
                // else if (DatabaseService.CurrentRole != null && DatabaseService.CurrentRole.RoleName == "GV")
                // {
                //     var gvWindow = new GVView();
                //     gvWindow.Show();
                //     this.Close();
                // }
                // else if (DatabaseService.CurrentRole != null && DatabaseService.CurrentRole.RoleName == "GV")
                // {
                //     var gvWindow = new GVView();
                //     gvWindow.Show();
                //     this.Close();
                // }
            };
        }


    }
}