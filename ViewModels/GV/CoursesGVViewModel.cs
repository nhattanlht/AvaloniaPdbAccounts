using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Threading;
using AvaloniaPdbAccounts.Models;
using AvaloniaPdbAccounts.Services;
using System.Linq;

namespace AvaloniaPdbAccounts.ViewModels.GV
{
    public class CoursesGVViewModel : ViewModelBase
    {
        public ObservableCollection<CourseModel> Courses { get; } = new();
        private readonly UserService _userService = new();

        public CoursesGVViewModel()
        {
            _ = LoadCoursesAsync();
        }

        private async Task LoadCoursesAsync()
        {
            try
            {
                var list = await _userService.GetCoursesForGVAsync();

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Courses.Clear();
                    foreach (var course in list)
                    {
                        Courses.Add(course);
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LoadCoursesAsync ERROR: {ex.Message}");
            }
        }
    }
} 