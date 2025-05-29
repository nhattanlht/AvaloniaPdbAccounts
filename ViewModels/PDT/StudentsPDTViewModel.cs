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
using Avalonia.Metadata;

namespace AvaloniaPdbAccounts.ViewModels.PDT
{
    public partial class StudentsPDTViewModel : ViewModelBase
    {
        public ReactiveCommand<StudentModel, Unit> EditCommand { get; }
        public ObservableCollection<string> AvailableStatus { get; }
        private readonly UserService _userService;

        public StudentsPDTViewModel()
        {
            _userService = new UserService();

            AvailableStatus = new ObservableCollection<string>
            {
                "Active",
                "Drop out",
                "Graduated",
                "Reservation"
            };

            EditCommand = ReactiveCommand.Create<StudentModel>(student =>
            {
                Dispatcher.UIThread.Post(async () =>
                {
                    await _userService.UpdateStudentStatusForNVPDTAsync(InputStudentID, SelectedStatus);
                    Console.WriteLine($"Updated status for {InputStudentID} with status {SelectedStatus}");
                });
            });
        }
        private string _inputStudentID;
        public string InputStudentID
        {
            get => _inputStudentID;
            set => this.SetProperty(ref _inputStudentID, value);
        }
        private string _studentStatus;
        public string StudentStatus
        {
            get => _studentStatus;
            set => this.SetProperty(ref _studentStatus, value);
        }
        private string _selectedStatus;
        public string SelectedStatus
        {
            get => _selectedStatus;
            set => this.SetProperty(ref _selectedStatus, value);
        }
    }
}
