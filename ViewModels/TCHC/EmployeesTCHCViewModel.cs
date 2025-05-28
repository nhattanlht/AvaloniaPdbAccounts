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

namespace AvaloniaPdbAccounts.ViewModels.TCHC
{
    public partial class EmployeesTCHCViewModel : ViewModelBase
    {
        public ObservableCollection<EmployeeModel> Employees { get; }
        public ReactiveCommand<EmployeeModel, Unit> EditCommand { get; }
        public ReactiveCommand<EmployeeModel, Unit> DeleteCommand { get; }

        private readonly UserService _userService;

        public EmployeesTCHCViewModel()
        {
            _userService = new UserService();

            Employees = new ObservableCollection<EmployeeModel>();

            EditCommand = ReactiveCommand.Create<EmployeeModel>(employee =>
            {
                Dispatcher.UIThread.Post(async () =>
                {
                    await _userService.UpdateEmployeeAsync(
                        employee.EmployeeID,
                        employee.Salary,
                        employee.Allowance,
                        employee.Phone,
                        employee.Department
                    );

                    Console.WriteLine($"Updated employee: {employee.EmployeeID} - {employee.FullName}");
                });
            });
            DeleteCommand = ReactiveCommand.Create<EmployeeModel>(employee =>
            {
               
                _ = Task.Run(async () =>
                {
                   

                    bool success = await _userService.DeleteEmployeeAsync(employee.EmployeeID);
                    if (success)
                    {
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            Employees.Remove(employee);
                            Console.WriteLine("Deleted employee: " + employee.EmployeeID + " - " + employee.FullName);
                        });
                    }
                    else
                    {
                        Console.WriteLine("Deleted falied employee " + employee.EmployeeID + " - " + employee.FullName);
                    }
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
