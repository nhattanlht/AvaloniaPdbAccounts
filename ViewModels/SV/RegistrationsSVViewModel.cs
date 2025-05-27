using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Threading;
using AvaloniaPdbAccounts.Models;
using AvaloniaPdbAccounts.Services;
using System.Windows.Input;
using AvaloniaPdbAccounts.Utilities;

namespace AvaloniaPdbAccounts.ViewModels.SV
{
    public partial class RegistrationsSVViewModel : ViewModelBase
    {
        public ObservableCollection<RegistrationModel> Registrations { get; } = new();

        private readonly UserService _userService = new();

        public RegistrationModel NewRegistration
        {
            get => _newRegistration;
            set
            {
                if (_newRegistration != value)
                {
                    _newRegistration = value;
                    OnPropertyChanged(nameof(NewRegistration));
                }
            }
        }
        private RegistrationModel _newRegistration = new RegistrationModel();

        public RegistrationModel SelectedRegistration
        {
            get => _selectedRegistration;
            set
            {
                if (_selectedRegistration != value)
                {
                    _selectedRegistration = value;
                    OnPropertyChanged(nameof(SelectedRegistration));
                }
            }
        }
        private RegistrationModel _selectedRegistration;

        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }

        public RegistrationsSVViewModel()
        {
            AddCommand = new RelayCommand(async () => await AddRegistrationAsync(NewRegistration));
            EditCommand = new RelayCommand(async () =>
            {
                if (SelectedRegistration != null)
                    await UpdateRegistrationAsync(SelectedRegistration);
            });
            DeleteCommand = new RelayCommand(async () =>
            {
                if (SelectedRegistration != null)
                    await DeleteRegistrationAsync(SelectedRegistration);
            });
            _ = LoadRegistrationsAsync();
        }

        public async Task AddRegistrationAsync(RegistrationModel model)
        {
            await _userService.AddRegistrationAsync(model);
            Registrations.Add(model); // cập nhật UI
        }

        public async Task UpdateRegistrationAsync(RegistrationModel model)
        {
            await _userService.UpdateRegistrationAsync(model);
            // Optionally refresh danh sách hoặc cập nhật item cụ thể nếu cần
        }

        public async Task DeleteRegistrationAsync(RegistrationModel model)
        {
            await _userService.DeleteRegistrationAsync(model.StudentID, model.CourseID);
            Registrations.Remove(model); // cập nhật UI
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
