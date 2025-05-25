using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Threading;
using AvaloniaPdbAccounts.Models;
using AvaloniaPdbAccounts.Services;
using ReactiveUI;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AvaloniaPdbAccounts.Utilities;

namespace AvaloniaPdbAccounts.ViewModels.PCTSV
{
    public partial class AddStudentPCTSVViewModel : ViewModelBase
    {
        private DateTimeOffset? _uiBirthday;
        public DateTimeOffset? UiBirthday
        {
            get => _uiBirthday;
            set
            {
                SetProperty(ref _uiBirthday, value);
                if (value.HasValue)
                    NewStudent.BIRTHDAY = value.Value.Date; // Lấy phần Date, bỏ time/offset
            }
        }
        private readonly UserService _userService;

        private StudentModel _NewStudent = new();
        public StudentModel NewStudent
        {
            get => _NewStudent;
            set => SetProperty(ref _NewStudent, value);
        }

        public IAsyncRelayCommand AddCommand { get; }

        public AddStudentPCTSVViewModel()
        {
            _userService = new UserService();
            AddCommand = new AsyncRelayCommand(AddStudentAsync);
        }

        private async Task AddStudentAsync()
        {
            try
            {

                await _userService.AddStudentAsync(NewStudent);
                NewStudent = new StudentModel();  // clears the form
            }
            catch (Exception ex)
            {
                Console.WriteLine("AddStudentAsync error:");
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine($"Source: {ex.Source}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine("Inner Exception:");
                    Console.WriteLine($"Message: {ex.InnerException.Message}");
                    Console.WriteLine($"StackTrace: {ex.InnerException.StackTrace}");
                }
            }
        }

    }

}


