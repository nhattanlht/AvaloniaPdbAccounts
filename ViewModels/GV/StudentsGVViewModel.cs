using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Threading;
using AvaloniaPdbAccounts.Models;
using AvaloniaPdbAccounts.Services;
using System.Reactive;
using ReactiveUI;

namespace AvaloniaPdbAccounts.ViewModels.GV
{
    public partial class StudentsGVViewModel : ViewModelBase
    {
        public string Test { get; set; } = "Students";

        // Collection of student records (should contain only the current student due to VPD policy)
        public ObservableCollection<StudentModel> Students { get; }

        private readonly UserService _userService;

        public StudentsGVViewModel()
        {
            _userService = new UserService();
            Students = new ObservableCollection<StudentModel>();

            _ = LoadStudentsAsync();
        }

        private async Task LoadStudentsAsync()
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
                Console.WriteLine($"LoadStudentsAsync ERROR: {ex.Message}");
            }
        }
    }
}