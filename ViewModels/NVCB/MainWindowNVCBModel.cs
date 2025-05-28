using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaPdbAccounts.ViewModels.NVCB
{
    public partial class MainWindowNVCBModel : ViewModelBase
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(EmployeesButtonIsActive))]
        [NotifyPropertyChangedFor(nameof(StudentsButtonIsActive))]
        [NotifyPropertyChangedFor(nameof(CoursesButtonIsActive))]
        [NotifyPropertyChangedFor(nameof(RegistrationsButtonIsActive))]
        private ViewModelBase _currentPage;

        public bool EmployeesButtonIsActive => CurrentPage == _employeesNVCB;
        public bool StudentsButtonIsActive => CurrentPage == _studentsNVCB;
        public bool CoursesButtonIsActive => CurrentPage == _coursesNVCB;
        public bool RegistrationsButtonIsActive => CurrentPage == _registrationsNVCB;


        private readonly EmployeesNVCBViewModel _employeesNVCB = new();
        private readonly StudentsNVCBViewModel _studentsNVCB = new();
        private readonly CoursesNVCBViewModel _coursesNVCB = new();
        private readonly RegistrationsNVCBViewModel _registrationsNVCB = new();

        public MainWindowNVCBModel()
        {
            CurrentPage = _coursesNVCB;
        }

        [RelayCommand]
        private void ShowEmployeesPage()
        {
            CurrentPage = _employeesNVCB;
        }

        [RelayCommand]
        private void ShowStudentsPage()
        {
            CurrentPage = _studentsNVCB;
        }
        [RelayCommand]
        private void ShowCoursesPage()
        {
            CurrentPage = _coursesNVCB;
        }
        [RelayCommand]
        private void ShowRegistrationsPage()
        {
            CurrentPage = _registrationsNVCB;
        }
    }
}