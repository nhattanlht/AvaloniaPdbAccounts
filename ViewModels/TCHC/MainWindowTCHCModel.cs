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
        [NotifyPropertyChangedFor(nameof(AddEmployeeButtonIsActive))]
        private ViewModelBase _currentPage;

        public bool EmployeesButtonIsActive => CurrentPage == _employeesTCHC;
        public bool StudentsButtonIsActive => CurrentPage == _studentsTCHC;
        public bool CoursesButtonIsActive => CurrentPage == _coursesTCHC;
        public bool AddEmployeeButtonIsActive => CurrentPage == _AddEmployeeTCHC;    


        private readonly EmployeesTCHCViewModel _employeesTCHC = new();
        private readonly StudentsTCHCViewModel _studentsTCHC = new();
        private readonly CoursesTCHCViewModel _coursesTCHC = new();
        private readonly AddEmployeeTCHCViewModel _AddEmployeeTCHC = new();

        public MainWindowTCHCModel()
        {
            CurrentPage = _studentsTCHC;
        }

        [RelayCommand]
        private async Task ShowEmployeesPage()
        {
            await _employeesTCHC.LoadEmployeesAsync();
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
        private void ShowAddEmployeePage()
        {
            CurrentPage = _AddEmployeeTCHC;
        }
    }
}
