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

namespace AvaloniaPdbAccounts.ViewModels.PDT
{
    public partial class CoursesPDTViewModel : ViewModelBase
    {
        public ObservableCollection<CourseOfferingModel> Courses { get; }
        public ReactiveCommand<CourseOfferingModel, Unit> EditCommand { get; }
        public ReactiveCommand<CourseOfferingModel, Unit> DeleteCommand { get; }
        private readonly UserService _userService;

        public CoursesPDTViewModel()
        {
            _userService = new UserService();

            Courses = new ObservableCollection<CourseOfferingModel>();

            EditCommand = ReactiveCommand.Create<CourseOfferingModel>(course =>
            {
                Dispatcher.UIThread.Post(async () =>
                {
                    await _userService.UpdateCourseOfferingNVPDTAsync(course);
                    Console.WriteLine($"Updated course offering for {course.OfferingID}");
                });
            });

            DeleteCommand = ReactiveCommand.Create<CourseOfferingModel>(course =>
            {
                Dispatcher.UIThread.Post(async () =>
                {
                    await _userService.DeleteCourseOfferingNVPDTAsync(course.OfferingID);
                    Console.WriteLine($"Deleted course offering {course.OfferingID}");
                });
            });

            // Load when ViewModel is created
            _ = LoadCoursesAsync();
        }

        private async Task LoadCoursesAsync()
        {
            var courses = await _userService.GetCourseOfferingNVPDTAsync();

            // Update ObservableCollection on UI thread
            Dispatcher.UIThread.Post(() =>
            {
                Courses.Clear();
                foreach (var course in courses)
                {
                    Courses.Add(course);
                }
            });
        }
    }
}
