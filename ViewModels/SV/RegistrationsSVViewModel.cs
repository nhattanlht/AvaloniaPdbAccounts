using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Threading;
using AvaloniaPdbAccounts.Models;
using AvaloniaPdbAccounts.Services;

namespace AvaloniaPdbAccounts.ViewModels.SV
{
    public partial class RegistrationsSVViewModel : ViewModelBase
    {
        public ObservableCollection<RegistrationModel> Registrations { get; } = new();

        private readonly UserService _userService = new();

        public RegistrationsSVViewModel()
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
