using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Threading;
using AvaloniaPdbAccounts.Models;
using AvaloniaPdbAccounts.Services;
using ReactiveUI;

namespace AvaloniaPdbAccounts.ViewModels.PKT
{
    public partial class RegistrationsPKTViewModel : ViewModelBase
    {
        public ObservableCollection<RegistrationModel> Registrations { get; }
        public ReactiveCommand<RegistrationModel, Unit> EditCommand { get; }
        private readonly UserService _userService;

        public RegistrationsPKTViewModel()
        {
            _userService = new UserService();

            Registrations = new ObservableCollection<RegistrationModel>();

            EditCommand = ReactiveCommand.Create<RegistrationModel>(registration =>
            {
                Dispatcher.UIThread.Post(async () =>
                {
                    await _userService.UpdateRegistrationScoreAsync(registration.StudentID, registration.CourseID, registration.PracticeScore, registration.ProcessScore, registration.FinalScore, registration.TotalScore);
                    Console.WriteLine($"Updated score for {registration.StudentID} {registration.CourseID} to {registration.PracticeScore}, {registration.ProcessScore}, {registration.FinalScore}, {registration.TotalScore}");
                });
            });

            // Load when ViewModel is created
            _ = LoadRegistrationsAsync();
        }

        private async Task LoadRegistrationsAsync()
        {
            var registrations = await _userService.GetRegistrationModelDataAsync();

            // Update ObservableCollection on UI thread
            Dispatcher.UIThread.Post(() =>
            {
                Registrations.Clear();
                foreach (var registration in registrations)
                {
                    Registrations.Add(registration);
                }
            });
        }
    }
}
