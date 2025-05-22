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
using System.Linq;

namespace AvaloniaPdbAccounts.Views
{
    public partial class LoginView : Window
    {
        private bool system;
        public LoginView()
        {
            try
            {
                RunAllSql();
                Console.WriteLine("Runned script");

            }
            catch
            {
                Console.WriteLine("Run script error");
            }
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
                else if (DatabaseService.CurrentRoles != null &&
                DatabaseService.CurrentRoles.Any(r => string.Equals(r.RoleName, "GV", StringComparison.OrdinalIgnoreCase)))
                    {
                        var gvWindow = new GVView();
                        gvWindow.Show();
                        this.Close();
                      }
                else if (DatabaseService.CurrentRoles != null &&
                DatabaseService.CurrentRoles.Any(r => string.Equals(r.RoleName, "NVPKT", StringComparison.OrdinalIgnoreCase)))
                    {
                        var pktWindow = new PKT.PKTView();
                        pktWindow.Show();
                        this.Close();
                      }

            };
        }


    }
}