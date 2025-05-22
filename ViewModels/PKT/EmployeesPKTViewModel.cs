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

namespace AvaloniaPdbAccounts.ViewModels.PKT
{
    public partial class EmployeesPKTViewModel : ViewModelBase
    {
        public ObservableCollection<EmployeeModel> Employees { get; }
        public ReactiveCommand<EmployeeModel, Unit> EditCommand { get; }
        private readonly UserService _userService;

        public EmployeesPKTViewModel()
        {
            _userService = new UserService();

            Employees = new ObservableCollection<EmployeeModel>();

            EditCommand = ReactiveCommand.Create<EmployeeModel>(employee =>
            {
                Dispatcher.UIThread.Post(async () =>
                {
                    await _userService.UpdateEmployeePhoneNumberAsync(employee.EmployeeID, employee.Phone);
                    Console.WriteLine($"Updated phone number for {employee.FullName} to {employee.Phone}");
                });
            });

            // Load when ViewModel is created
            _ = LoadEmployeesAsync();
        }

        private async Task LoadEmployeesAsync()
        {
            var employees = await _userService.GetEmployeeModelDataAsync();

            // Update ObservableCollection on UI thread
            Dispatcher.UIThread.Post(() =>
            {
                Employees.Clear();
                foreach (var employee in employees)
                {
                    Employees.Add(employee);
                }
            });
        }
    }
}
