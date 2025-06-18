using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaPdbAccounts.ViewModels.SV
{
    public partial class MainWindowSVModel : ViewModelBase
    {

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(EmployeesButtonIsActive))]
        [NotifyPropertyChangedFor(nameof(StudentsButtonIsActive))]
        [NotifyPropertyChangedFor(nameof(CoursesButtonIsActive))]
        [NotifyPropertyChangedFor(nameof(RegistrationsButtonIsActive))]
        private ViewModelBase _currentPage;

        public bool EmployeesButtonIsActive => CurrentPage == _employeesSV;
        public bool StudentsButtonIsActive => CurrentPage == _studentsSV;
        public bool CoursesButtonIsActive => CurrentPage == _coursesSV;
        public bool RegistrationsButtonIsActive => CurrentPage == _registrationsSV;    


        private readonly EmployeesSVViewModel _employeesSV = new();
        private readonly StudentsSVViewModel _studentsSV = new();
        private readonly CoursesSVViewModel _coursesSV = new();
        private readonly RegistrationsSVViewModel _registrationsSV = new();

        public MainWindowSVModel()
        {
            CurrentPage = _studentsSV;
        }

        [RelayCommand]
        private void ShowEmployeesPage()
        {
            CurrentPage = _employeesSV;
        }

        [RelayCommand]
        private void ShowStudentsPage()
        {
            CurrentPage = _studentsSV;
        }
        [RelayCommand]
        private void ShowCoursesPage()
        {
            CurrentPage = _coursesSV;
        }
        [RelayCommand]
        private void ShowRegistrationsPage()
        {
            CurrentPage = _registrationsSV;
        }
    }
}
