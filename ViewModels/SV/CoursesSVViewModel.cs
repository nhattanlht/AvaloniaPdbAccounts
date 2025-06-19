using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Threading;
using AvaloniaPdbAccounts.Models;
using AvaloniaPdbAccounts.Services;
using System.Linq;

namespace AvaloniaPdbAccounts.ViewModels.SV
{
    public class CoursesSVViewModel : ViewModelBase
    {
        public ObservableCollection<CourseModel> Courses { get; } = new();
        private readonly UserService _userService = new();
        private readonly DatabaseService _databaseService;

        public CoursesSVViewModel()
        {
            _databaseService = DatabaseService.Instance;
            _ = LoadCoursesAsync();
        }

        private async Task LoadCoursesAsync()
        {
            try
            {
                // Lấy role SV hiện tại
                var svRole = DatabaseService.CurrentRoles
                    .FirstOrDefault(r => r.RoleName.StartsWith("SV"))?.RoleName;

                if (svRole == null)
                {
                    Console.WriteLine("Không tìm thấy role SV");
                    return;
                }

                // Lấy mã số sinh viên từ tên role (ví dụ: SV_A536166 -> A536166)
                var studentId = svRole.Split('_').LastOrDefault();
                if (studentId == null)
                {
                    Console.WriteLine("Không thể xác định mã sinh viên từ role");
                    return;
                }

                var list = await _userService.GetCoursesByStudentDepartmentAsync(studentId);

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
