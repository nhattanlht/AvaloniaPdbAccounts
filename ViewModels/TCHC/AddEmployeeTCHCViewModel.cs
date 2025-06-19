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

namespace AvaloniaPdbAccounts.ViewModels.TCHC
{
    public partial class AddEmployeeTCHCViewModel : ViewModelBase
    {
        private DateTimeOffset? _uiBirthDate;
        public DateTimeOffset? UIBirthDate
        {
            get => _uiBirthDate;
            set
            {
                SetProperty(ref _uiBirthDate, value);
                if (value.HasValue)
                    NewEmployee.BirthDate = value.Value.DateTime;
            }
        }
        private readonly UserService _userService;

        private EmployeeModel _newEmployee = new();
        public EmployeeModel NewEmployee
        {
            get => _newEmployee;
            set => SetProperty(ref _newEmployee, value);
        }

        public IAsyncRelayCommand AddCommand { get; }

        public AddEmployeeTCHCViewModel()
        {
            _userService = new UserService();
            AddCommand = new AsyncRelayCommand(AddEmployeeAsync);
        }

        private async Task AddEmployeeAsync()
        {
            try
            {
                
                await _userService.AddEmployeeAsync(NewEmployee);
                NewEmployee = new EmployeeModel();  // clears the form
            }
            catch (Exception ex)
            {
                Console.WriteLine("AddEmployeeAsync error:");
                Console.WriteLine(ex.Message);

            }
        }
      
    }

}


