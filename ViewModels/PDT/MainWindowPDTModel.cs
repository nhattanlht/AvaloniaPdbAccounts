using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaPdbAccounts.ViewModels.PDT
{
    public partial class MainWindowPDTModel : ViewModelBase
    {

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(EmployeesButtonIsActive))]
        [NotifyPropertyChangedFor(nameof(StudentsButtonIsActive))]
        [NotifyPropertyChangedFor(nameof(CoursesButtonIsActive))]
        [NotifyPropertyChangedFor(nameof(RegistrationsButtonIsActive))]
        private ViewModelBase _currentPage;

        public bool EmployeesButtonIsActive => CurrentPage == _employeesPDT;
        public bool StudentsButtonIsActive => CurrentPage == _studentsPDT;
        public bool CoursesButtonIsActive => CurrentPage == _coursesPDT;
        public bool RegistrationsButtonIsActive => CurrentPage == _registrationsPDT;    


        private readonly EmployeesPDTViewModel _employeesPDT = new();
        private readonly StudentsPDTViewModel _studentsPDT = new();
        private readonly CoursesPDTViewModel _coursesPDT = new();
        private readonly RegistrationsPDTViewModel _registrationsPDT = new();

        public MainWindowPDTModel()
        {
            CurrentPage = _studentsPDT;
        }

        [RelayCommand]
        private void ShowEmployeesPage()
        {
            CurrentPage = _employeesPDT;
        }

        [RelayCommand]
        private void ShowStudentsPage()
        {
            CurrentPage = _studentsPDT;
        }
        [RelayCommand]
        private void ShowCoursesPage()
        {
            CurrentPage = _coursesPDT;
        }
        [RelayCommand]
        private void ShowRegistrationsPage()
        {
            CurrentPage = _registrationsPDT;
        }
    }
}
