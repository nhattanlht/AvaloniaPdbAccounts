using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaPdbAccounts.ViewModels.GV
{
    public partial class MainWindowGVModel : ViewModelBase
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(EmployeesButtonIsActive))]
        [NotifyPropertyChangedFor(nameof(StudentsButtonIsActive))]
        [NotifyPropertyChangedFor(nameof(CoursesButtonIsActive))]
        [NotifyPropertyChangedFor(nameof(RegistrationsButtonIsActive))]
        private ViewModelBase _currentPage;

        public bool EmployeesButtonIsActive => CurrentPage == _employeesGV;
        public bool StudentsButtonIsActive => CurrentPage == _studentsGV;
        public bool CoursesButtonIsActive => CurrentPage == _coursesGV;
        public bool RegistrationsButtonIsActive => CurrentPage == _registrationsGV;    


        private readonly EmployeesGVViewModel _employeesGV = new();
        private readonly StudentsGVViewModel _studentsGV = new();
        private readonly CoursesGVViewModel _coursesGV = new();
        private readonly RegistrationsGVViewModel _registrationsGV = new();

        public MainWindowGVModel()
        {
            CurrentPage = _coursesGV;
        }

        [RelayCommand]
        private void ShowEmployeesPage()
        {
            CurrentPage = _employeesGV;
        }

        [RelayCommand]
        private void ShowStudentsPage()
        {
            CurrentPage = _studentsGV;
        }
        [RelayCommand]
        private void ShowCoursesPage()
        {
            CurrentPage = _coursesGV;
        }
        [RelayCommand]
        private void ShowRegistrationsPage()
        {
            CurrentPage = _registrationsGV;
        }
    }
} 