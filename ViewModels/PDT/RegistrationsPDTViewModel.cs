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
    public partial class RegistrationsPDTViewModel : ViewModelBase
    {
        public ObservableCollection<RegistrationModel> Registrations { get; }
        public ReactiveCommand<RegistrationModel, Unit> EditCommand { get; }
        public ReactiveCommand<RegistrationModel, Unit> DeleteCommand { get; }
        private readonly UserService _userService;

        public RegistrationsPDTViewModel()
        {
            var view = new AddRegistrationPopupView();
            _userService = new UserService();

            Registrations = new ObservableCollection<RegistrationModel>();

            EditCommand = ReactiveCommand.Create<RegistrationModel>(registration =>
            {
                Dispatcher.UIThread.Post(async () =>
                {
                    await _userService.UpdateRegistrationNVPDTAsync(registration);
                    Console.WriteLine($"Updated registration for student {registration.StudentID}, course {registration.CourseID}");
                });
            });

            DeleteCommand = ReactiveCommand.Create<RegistrationModel>(registration =>
            {
                Dispatcher.UIThread.Post(async () =>
                {
                    await _userService.DeleteRegistrationNVPDTAsync(registration.StudentID, registration.CourseID);
                    Console.WriteLine($"Deleted registration for student: {registration.StudentID}, course: {registration.CourseID}");
                    await LoadRegistrationsAsync();
                });
            });

            // Initialize commands
            ShowAddPopupCommand = ReactiveCommand.Create(ShowAddPopup);
            AddRegistrationCommand = ReactiveCommand.CreateFromTask(AddRegistrationAsync);
            CancelCommand = ReactiveCommand.Create(CancelPopup);

            // Load initial data
            _ = LoadRegistrationsAsync();
            _ = LoadCoursesAsync();
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

        // For the add registration form
        private RegistrationModel _newRegistation = new();
        public RegistrationModel NewRegistation
        {
            get => _newRegistation;
            set => this.SetProperty(ref _newRegistation, value);
        }

        // Available for dropdown
        private ObservableCollection<CourseModel> _availableCourses = new();
        public ObservableCollection<CourseModel> AvailableCourses
        {
            get => _availableCourses;
            set => this.SetProperty(ref _availableCourses, value);
        }
        private CourseModel? _selectedCourse;
        public CourseModel? SelectedCourse
        {
            get => _selectedCourse;
            set
            {
                this.SetProperty(ref _selectedCourse, value);
                if (value != null)
                {
                    NewRegistation.CourseID = value.CourseID; // Gán ID vào model cần lưu
                }
            }
        }
        // Thêm các properties cho Selected items
        private int _selectedCourseIndex = -1;
        public int SelectedCourseIndex
        {
            get => _selectedCourseIndex;
            set => this.SetProperty(ref _selectedCourseIndex, value);
        }

        private string _inputStudentID = string.Empty;
        public string InputStudentID
        {
            get => _inputStudentID;
            set
            {
                this.SetProperty(ref _inputStudentID, value);
                if(!string.IsNullOrEmpty(value))
                {
                    NewRegistation.StudentID = value; // Gán ID vào model cần lưu
                }
            }
        }

        // Commands
        public ReactiveCommand<Unit, Unit> ShowAddPopupCommand { get; }
        public ReactiveCommand<Unit, Unit> AddRegistrationCommand { get; }
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }

        private void ShowAddPopup()
        {
            NewRegistation = new RegistrationModel(); // Reset form
            PopupContent = new AddRegistrationPopupView { DataContext = this };
            IsPopupVisible = true;
        }

        private void CancelPopup()
        {
            IsPopupVisible = false;
            PopupContent = null;
        }
        private async Task AddRegistrationAsync()
        {
            try
            {

                // Add to database
                await _userService.AddRegistrationAsync(NewRegistation);

                // Refresh the list
                await LoadRegistrationsAsync();

                // Close popup
                IsPopupVisible = false;
                PopupContent = null;
            }
            catch (Exception ex)
            {
                // Handle error (show to user)
                Console.WriteLine($"Error adding registration: {ex.Message}");
            }
        }
        private async Task LoadCoursesAsync()
        {
            var courses = await _userService.GetCoursesNVPDTAsync();

            Dispatcher.UIThread.Post(() =>
            {
                AvailableCourses.Clear();
                foreach (var course in courses)
                {
                    AvailableCourses.Add(course);
                }
            });
        }
        private async Task LoadRegistrationsAsync()
        {
            var registrations = await _userService.GetRegistrationsForNVPDTAsync();

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
