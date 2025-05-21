using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaPdbAccounts.ViewModels
{
    public partial class MainWindowModel : ViewModelBase
    {

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(EmployeesButtonIsActive))]
        [NotifyPropertyChangedFor(nameof(StudentsButtonIsActive))]
        [NotifyPropertyChangedFor(nameof(CoursesButtonIsActive))]
        [NotifyPropertyChangedFor(nameof(RegistrationsButtonIsActive))]
        private ViewModelBase _currentPage;

        public bool EmployeesButtonIsActive => CurrentPage == _employeesPKT;
        public bool StudentsButtonIsActive => CurrentPage == _studentsPKT;
        public bool CoursesButtonIsActive => CurrentPage == _coursesPKT;
        public bool RegistrationsButtonIsActive => CurrentPage == _registrationsPKT;    


        private readonly EmployeesPKTViewModel _employeesPKT = new();
        private readonly StudentsPKTViewModel _studentsPKT = new();
        private readonly CoursesPKTViewModel _coursesPKT = new();
        private readonly RegistrationsPKTViewModel _registrationsPKT = new();

        public MainWindowModel()
        {
            CurrentPage = _studentsPKT;
        }

        [RelayCommand]
        private void ShowEmployeesPage()
        {
            CurrentPage = _employeesPKT;
        }

        [RelayCommand]
        private void ShowStudentsPage()
        {
            CurrentPage = _studentsPKT;
        }
        [RelayCommand]
        private void ShowCoursesPage()
        {
            CurrentPage = _coursesPKT;
        }
        [RelayCommand]
        private void ShowRegistrationsPage()
        {
            CurrentPage = _registrationsPKT;
        }
    }
}
