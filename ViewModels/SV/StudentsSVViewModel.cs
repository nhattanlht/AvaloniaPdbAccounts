using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Reactive;
using Avalonia.Threading;
using AvaloniaPdbAccounts.Models;
using AvaloniaPdbAccounts.Services;
using ReactiveUI;

namespace AvaloniaPdbAccounts.ViewModels.SV
{
    public partial class StudentsSVViewModel : ViewModelBase
    {
        public string Test { get; set; } = "Students";

        // Collection of student records (should contain only the current student due to VPD policy)
        public ObservableCollection<StudentModel> Students { get; }

        // Command triggered from the UI to persist changes to ADDRESS or PHONE
        public ReactiveCommand<StudentModel, Unit> EditCommand { get; }

        private readonly UserService _userService;

        public StudentsSVViewModel()
        {
            _userService = new UserService();
            Students = new ObservableCollection<StudentModel>();

            // Command implementation
            EditCommand = ReactiveCommand.CreateFromTask<StudentModel>(async student =>
            {
                if (student == null) return;

                try
                {
                    await _userService.UpdateStudentAsync(student.ID, student.ADDRESS, student.PHONE, null);
                    // Reload data after successful update to reflect any DB-level changes
                    await LoadPersonalStudent();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error updating student {student.ID}: {ex.Message}");
                }
            });

            // Initial load
            _ = LoadPersonalStudent();
        }

        // Fetches the student row(s) visible to the current logged-in account.
        private async Task LoadPersonalStudent()
        {
            try
            {
                var list = await _userService.GetStudentModelDataAsync();

                Dispatcher.UIThread.Post(() =>
                {
                    Students.Clear();
                    foreach (var s in list)
                    {
                        Students.Add(s);
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LoadPersonalStudent ERROR: {ex.Message}");
            }
        }
    }
}
