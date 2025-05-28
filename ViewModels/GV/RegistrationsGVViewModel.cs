using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using AvaloniaPdbAccounts.Models;
using AvaloniaPdbAccounts.Services;
using Avalonia.Threading;

namespace AvaloniaPdbAccounts.ViewModels.GV
{
    public partial class RegistrationsGVViewModel : ViewModelBase
    {
        public ObservableCollection<RegistrationModel> Registrations { get; } = new();

        private readonly UserService _userService = new();

        public RegistrationsGVViewModel()
        {
            _ = LoadRegistrationsAsync();
        }

        private async Task LoadRegistrationsAsync()
        {
            try
            {
                var list = await _userService.GetRegistrationModelDataAsync();

                Dispatcher.UIThread.Post(() =>
                {
                    Registrations.Clear();
                    foreach (var r in list)
                    {
                        Registrations.Add(r);
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LoadRegistrationsAsync ERROR: {ex.Message}");
            }
        }
    }
} 