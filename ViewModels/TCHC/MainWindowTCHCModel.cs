using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvaloniaPdbAccounts.ViewModels.TCHC;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaPdbAccounts.ViewModels.TCHC
{
    public partial class MainWindowTCHCModel : ViewModelBase
    {

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(EmployeesButtonIsActive))]
        [NotifyPropertyChangedFor(nameof(StudentsButtonIsActive))]
        [NotifyPropertyChangedFor(nameof(CoursesButtonIsActive))]
        [NotifyPropertyChangedFor(nameof(RegistrationsButtonIsActive))]
        private ViewModelBase _currentPage;

        public bool EmployeesButtonIsActive => CurrentPage == _employeesTCHC;
        public bool StudentsButtonIsActive => CurrentPage == _studentsTCHC;
        public bool CoursesButtonIsActive => CurrentPage == _coursesTCHC;
        public bool RegistrationsButtonIsActive => CurrentPage == _registrationsTCHC;    


        private readonly EmployeesTCHCViewModel _employeesTCHC = new();
        private readonly StudentsTCHCViewModel _studentsTCHC = new();
        private readonly CoursesTCHCViewModel _coursesTCHC = new();
        private readonly RegistrationsTCHCViewModel _registrationsTCHC = new();

        public MainWindowTCHCModel()
        {
            CurrentPage = _studentsTCHC;
        }

        [RelayCommand]
        private void ShowEmployeesPage()
        {
            CurrentPage = _employeesTCHC;
        }

        [RelayCommand]
        private void ShowStudentsPage()
        {
            CurrentPage = _studentsTCHC;
        }
        [RelayCommand]
        private void ShowCoursesPage()
        {
            CurrentPage = _coursesTCHC;
        }
        [RelayCommand]
        private void ShowRegistrationsPage()
        {
            CurrentPage = _registrationsTCHC;
        }
    }
}
