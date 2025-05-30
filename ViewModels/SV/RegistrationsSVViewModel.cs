using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Threading;
using AvaloniaPdbAccounts.Models;
using AvaloniaPdbAccounts.Services;
using System.Windows.Input;
using AvaloniaPdbAccounts.Utilities;
using System.Linq;
using System.Collections.Generic;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace AvaloniaPdbAccounts.ViewModels.SV
{
    public partial class RegistrationsSVViewModel : ViewModelBase
    {
        public ObservableCollection<RegistrationModel> Registrations { get; } = new();
        public ObservableCollection<CourseModel> AvailableCourses { get; } = new();

        private readonly UserService _userService = new();

        private CourseModel _selectedCourse;
        public CourseModel SelectedCourse
        {
            get => _selectedCourse;
            set
            {
                if (_selectedCourse != value)
                {
                    _selectedCourse = value;
                    OnPropertyChanged(nameof(SelectedCourse));
                    if (value != null)
                    {
                        NewRegistration.CourseID = value.CourseID;
                    }
                }
            }
        }

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
        public ICommand DeleteCommand { get; }

        public IEnumerable<RegistrationModel> RegistrationsWithoutScore => Registrations.Where(r => !r.HasScore);
        public IEnumerable<RegistrationModel> RegistrationsWithScore => Registrations.Where(r => r.HasScore);

        public RegistrationsSVViewModel()
        {
            AddCommand = new RelayCommand(async () => await AddRegistrationAsync(NewRegistration), () => SelectedCourse != null);
            DeleteCommand = new RelayCommand<RegistrationModel>(async (model) =>
            {
                if (model != null)
                    await DeleteRegistrationAsync(model);
            });
            _ = LoadRegistrationsAsync();
            _ = LoadAvailableCoursesAsync();
        }

        private async Task LoadAvailableCoursesAsync()
        {
            try
            {
                var courses = await _userService.GetAvailableCoursesForRegistrationAsync();
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    AvailableCourses.Clear();
                    foreach (var course in courses)
                    {
                        AvailableCourses.Add(course);
                    }
                });
            }
            catch (Exception ex)
            {
                await MessageBoxManager.GetMessageBoxStandard(
                    "Lỗi",
                    "Không thể tải danh sách môn học: " + ex.Message,
                    ButtonEnum.Ok,
                    Icon.Error).ShowAsync();
            }
        }

        public async Task AddRegistrationAsync(RegistrationModel model)
        {
            try
            {
                if (SelectedCourse == null)
                {
                    await MessageBoxManager.GetMessageBoxStandard(
                        "Lỗi",
                        "Vui lòng chọn môn học",
                        ButtonEnum.Ok,
                        Icon.Error).ShowAsync();
                    return;
                }

                // Lấy mã số sinh viên từ role hiện tại
                var svRole = AvaloniaPdbAccounts.Services.DatabaseService.CurrentRoles
                    .FirstOrDefault(r => r.RoleName.StartsWith("SV"))?.RoleName;
                var studentId = svRole?.Split('_').LastOrDefault();
                if (string.IsNullOrEmpty(studentId))
                {
                    await MessageBoxManager.GetMessageBoxStandard(
                        "Lỗi",
                        "Không tìm thấy mã số sinh viên",
                        ButtonEnum.Ok,
                        Icon.Error).ShowAsync();
                    return;
                }

                model.StudentID = studentId;
                model.CourseID = SelectedCourse.CourseID;
                await _userService.AddRegistrationAsync(model);
                
                // Cập nhật UI
                Registrations.Add(model);
                OnPropertyChanged(nameof(RegistrationsWithoutScore));
                OnPropertyChanged(nameof(RegistrationsWithScore));
                
                // Reset form
                NewRegistration = new RegistrationModel();
                SelectedCourse = null;

                // Hiển thị thông báo thành công
                await MessageBoxManager.GetMessageBoxStandard(
                    "Thành công",
                    "Đăng ký môn học thành công",
                    ButtonEnum.Ok,
                    Icon.Success).ShowAsync();
            }
            catch (Exception ex)
            {
                await MessageBoxManager.GetMessageBoxStandard(
                    "Lỗi",
                    ex.Message,
                    ButtonEnum.Ok,
                    Icon.Error).ShowAsync();
            }
        }

        public async Task DeleteRegistrationAsync(RegistrationModel model)
        {
            try
            {
                var success = await _userService.DeleteRegistrationAsync(model.StudentID, model.CourseID);
                if (success)
                {
                    // Xóa khỏi ObservableCollection
                    Registrations.Remove(model);
                    
                    // Cập nhật các view collections
                    OnPropertyChanged(nameof(RegistrationsWithoutScore));
                    OnPropertyChanged(nameof(RegistrationsWithScore));

                    // Tải lại toàn bộ dữ liệu để đảm bảo đồng bộ
                    await LoadRegistrationsAsync();
                }
            }
            catch (Exception ex)
            {
                var box = MessageBoxManager.GetMessageBoxStandard(
                    "Lỗi",
                    ex.Message,
                    ButtonEnum.Ok,
                    Icon.Error);
                await box.ShowAsync();
            }
        }

        private async Task LoadRegistrationsAsync()
        {
            try
            {
                var list = await _userService.GetRegistrationModelDataAsync();

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Registrations.Clear();
                    foreach (var r in list)
                    {
                        Registrations.Add(r);
                    }
                    OnPropertyChanged(nameof(RegistrationsWithoutScore));
                    OnPropertyChanged(nameof(RegistrationsWithScore));
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LoadRegistrationsAsync ERROR: {ex.Message}");
                var box = MessageBoxManager.GetMessageBoxStandard(
                    "Lỗi",
                    "Không thể tải danh sách đăng ký: " + ex.Message,
                    ButtonEnum.Ok,
                    Icon.Error);
                await box.ShowAsync();
            }
        }
    }
}
