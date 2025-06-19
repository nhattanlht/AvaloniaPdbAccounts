using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Threading;
using AvaloniaPdbAccounts.Models;
using AvaloniaPdbAccounts.Services;
using ReactiveUI;
using AvaloniaPdbAccounts.Views.PDT; // Add this at the top

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
            var view = new AddCoursePopupView();
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
                    await LoadCoursesAsync();
                });
            });

            // Initialize commands
            ShowAddPopupCommand = ReactiveCommand.Create(ShowAddPopup);
            AddCourseCommand = ReactiveCommand.CreateFromTask(AddCourseAsync);
            CancelCommand = ReactiveCommand.Create(CancelPopup);

            // Load initial data
            _ = LoadCoursesAsync();
            _ = LoadInstructorsAsync();
            _ = LoadModulesAsync();
        }

        // Popup properties
        private bool _isPopupVisible;
        public bool IsPopupVisible
        {
            get => _isPopupVisible;
            set => this.SetProperty(ref _isPopupVisible, value);
        }

        private object? _popupContent;
        public object? PopupContent
        {
            get => _popupContent;
            set => this.SetProperty(ref _popupContent, value);
        }

        // For the add course form
        private CourseOfferingModel _newCourse = new();
        public CourseOfferingModel NewCourse
        {
            get => _newCourse;
            set => this.SetProperty(ref _newCourse, value);
        }

        // Available for dropdown
        private ObservableCollection<ModuleModel> _availableModules = new();
        public ObservableCollection<ModuleModel> AvailableModules
        {
            get => _availableModules;
            set => this.SetProperty(ref _availableModules, value);
        }
        private ObservableCollection<EmployeeModel> _availableInstructors = new();
        public ObservableCollection<EmployeeModel> AvailableInstructors
        {
            get => _availableInstructors;
            set => this.SetProperty(ref _availableInstructors, value);
        }
        private ModuleModel? _selectedModule;
        public ModuleModel? SelectedModule
        {
            get => _selectedModule;
            set
            {
                this.SetProperty(ref _selectedModule, value);
                if (value != null)
                {
                    NewCourse.ModuleID = value.ModuleID; // Gán ID vào model cần lưu
                }
            }
        }
        // Thêm các properties cho Selected items
        private int _selectedModuleIndex = -1;
        public int SelectedModuleIndex
        {
            get => _selectedModuleIndex;
            set => this.SetProperty(ref _selectedModuleIndex, value);
        }
        private EmployeeModel? _selectedInstructor;
        public EmployeeModel? SelectedInstructor
        {
            get => _selectedInstructor;
            set
            {
                this.SetProperty(ref _selectedInstructor, value);
                if (value != null)
                {
                    NewCourse.InstructorID = value.EmployeeID; // Cập nhật InstructorID trong NewCourse
                }
            }
        }

        private int? _selectedSemester;
        public int? SelectedSemester
        {
            get => _selectedSemester;
            set
            {
                this.SetProperty(ref _selectedSemester, value);
                if (value.HasValue)
                {
                    NewCourse.Semester = value.Value; // Cập nhật Semester trong NewCourse
                }
            }
        }

        private int? _selectedYear;
        public int? SelectedYear
        {
            get => _selectedYear;
            set
            {
                this.SetProperty(ref _selectedYear, value);
                if (value.HasValue)
                {
                    NewCourse.Year = value.Value; // Cập nhật Year trong NewCourse
                }
            }
        }


        // Semester options
        public List<int> Semesters { get; } = new() { 1, 2, 3 };
        public List<int> Years { get; } = Enumerable.Range(DateTime.Now.Year-1, 10).ToList();


        // Commands
        public ReactiveCommand<Unit, Unit> ShowAddPopupCommand { get; }
        public ReactiveCommand<Unit, Unit> AddCourseCommand { get; }
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }

        private void ShowAddPopup()
        {
            NewCourse = new CourseOfferingModel(); // Reset form
            PopupContent = new AddCoursePopupView { DataContext = this };
            IsPopupVisible = true;
        }

        private void CancelPopup()
        {
            IsPopupVisible = false;
            PopupContent = null;
        }
        private async Task AddCourseAsync()
        {
            try
            {
                //Handle OfferingId before adding
                if (NewCourse != null)
                {
                    NewCourse.OfferingID = $"{NewCourse.ModuleID}_{NewCourse.Semester}_{NewCourse.Year}";
                }
                // Add to database
                await _userService.AddCourseOfferingNVPDTAsync(NewCourse);

                // Refresh the list
                await LoadCoursesAsync();

                // Close popup
                IsPopupVisible = false;
                PopupContent = null;
            }
            catch (Exception ex)
            {
                // Handle error (show to user)
                Console.WriteLine($"Error adding course: {ex.Message}");
            }
        }
        private async Task LoadInstructorsAsync()
        {
            var instructors = await _userService.GetInstructorsAsync();

            Dispatcher.UIThread.Post(() =>
            {
                AvailableInstructors.Clear();
                foreach (var instructor in instructors)
                {
                    AvailableInstructors.Add(instructor);
                }
            });
        }
        private async Task LoadModulesAsync()
        {
            var modules = await _userService.GetModulesAsync();

            Dispatcher.UIThread.Post(() =>
            {
                AvailableModules.Clear();
                foreach (var module in modules)
                {
                    AvailableModules.Add(module);
                }
            });
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
