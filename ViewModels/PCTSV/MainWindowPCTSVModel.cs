using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaPdbAccounts.ViewModels.PCTSV
{
    public partial class MainWindowPCTSVModel : ViewModelBase
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(AddStudentButtonIsActive))]
        [NotifyPropertyChangedFor(nameof(StudentsButtonIsActive))]
        [NotifyPropertyChangedFor(nameof(CoursesButtonIsActive))]
        [NotifyPropertyChangedFor(nameof(RegistrationsButtonIsActive))]
        private ViewModelBase _currentPage;

        public bool AddStudentButtonIsActive => CurrentPage == _addStudentPCTSV;
        public bool StudentsButtonIsActive => CurrentPage == _studentsPCTSV;
        public bool CoursesButtonIsActive => CurrentPage == _coursesPCTSV;
        public bool RegistrationsButtonIsActive => CurrentPage == _registrationsPCTSV;


        private readonly AddStudentPCTSVViewModel _addStudentPCTSV = new();
        private readonly StudentsPCTSVViewModel _studentsPCTSV = new();
        private readonly CoursesPCTSVViewModel _coursesPCTSV = new();
        private readonly RegistrationsPCTSVViewModel _registrationsPCTSV = new();

        public MainWindowPCTSVModel()
        {
            CurrentPage = _coursesPCTSV;
        }

        [RelayCommand]
        private void ShowAddStudentPage()
        {
            CurrentPage = _addStudentPCTSV;
        }

        [RelayCommand]
        private void ShowStudentsPage()
        {
            CurrentPage = _studentsPCTSV;
        }
        [RelayCommand]
        private void ShowCoursesPage()
        {
            CurrentPage = _coursesPCTSV;
        }
        [RelayCommand]
        private void ShowRegistrationsPage()
        {
            CurrentPage = _registrationsPCTSV;
        }
    }
}