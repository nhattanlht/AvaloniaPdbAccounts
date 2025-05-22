using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaPdbAccounts.ViewModels.TRGDV
{
    public partial class MainWindowTRGDVModel : ViewModelBase
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(EmployeesButtonIsActive))]
        [NotifyPropertyChangedFor(nameof(StudentsButtonIsActive))]
        [NotifyPropertyChangedFor(nameof(CoursesButtonIsActive))]
        [NotifyPropertyChangedFor(nameof(RegistrationsButtonIsActive))]
        private ViewModelBase _currentPage;

        public bool EmployeesButtonIsActive => CurrentPage == _employeesTRGDV;
        public bool StudentsButtonIsActive => CurrentPage == _studentsTRGDV;
        public bool CoursesButtonIsActive => CurrentPage == _coursesTRGDV;
        public bool RegistrationsButtonIsActive => CurrentPage == _registrationsTRGDV;    


        private readonly EmployeesTRGDVViewModel _employeesTRGDV = new();
        private readonly StudentsTRGDVViewModel _studentsTRGDV = new();
        private readonly CoursesTRGDVViewModel _coursesTRGDV = new();
        private readonly RegistrationsTRGDVViewModel _registrationsTRGDV = new();

        public MainWindowTRGDVModel()
        {
            CurrentPage = _employeesTRGDV;
        }

        [RelayCommand]
        private void ShowEmployeesPage()
        {
            CurrentPage = _employeesTRGDV;
        }

        [RelayCommand]
        private void ShowStudentsPage()
        {
            CurrentPage = _studentsTRGDV;
        }
        [RelayCommand]
        private void ShowCoursesPage()
        {
            CurrentPage = _coursesTRGDV;
        }
        [RelayCommand]
        private void ShowRegistrationsPage()
        {
            CurrentPage = _registrationsTRGDV;
        }
    }
} 