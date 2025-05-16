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

namespace AvaloniaPdbAccounts.Views
{
    public partial class LoginView : Window
    {
        private bool systemRole = true;
        private UserService _userService = new();
        public LoginView()
        {
            InitializeComponent();
            var loginVM = new LoginViewModel();
            DataContext = loginVM;

            // Create an instance of SymmetricEncryptionService
            var encryptionService = new SymmetricEncryptionService();

           loginVM.OnLoginSuccess += async () =>
            {
                Console.WriteLine(DatabaseService.CurrentRole.RoleName);
                if (systemRole)
                {
                    var mainWindow = new MainWindow();
                    mainWindow.Show();
                    this.Close();
                }
                // else if (DatabaseService.CurrentRole.RoleName == "GV")
                // {
                //     var gvWindow = new GVView();
                //     gvWindow.Show();
                //     this.Close();

                // }
            };
        }


    }
}