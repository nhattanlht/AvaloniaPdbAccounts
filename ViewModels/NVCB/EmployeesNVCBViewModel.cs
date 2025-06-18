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

namespace AvaloniaPdbAccounts.ViewModels.NVCB
{
    public partial class EmployeesNVCBViewModel : ViewModelBase
    {
        public ObservableCollection<EmployeeModel> Employees { get; }
        public ReactiveCommand<EmployeeModel, Unit> EditCommand { get; }

        private readonly UserService _userService;
        public EmployeesNVCBViewModel()
        {
            _userService = new UserService();

            Employees = new ObservableCollection<EmployeeModel>();

            // Command để cập nhật số điện thoại
            EditCommand = ReactiveCommand.Create<EmployeeModel>(employee =>
            {
                Dispatcher.UIThread.Post(async () =>
                {
                    try
                    {
                        await _userService.UpdateEmployeePhoneNumberAsync(employee.EmployeeID, employee.Phone);
                        Console.WriteLine($"Updated employee: {employee.EmployeeID} - {employee.FullName}");


                        await LoadPersonalEmployee();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error updating employee: " + ex.Message);
                    }
                });
            });

            // Lấy dữ liệu hiển thị ra khi khởi tạo
            _ = LoadPersonalEmployee();
        }

        private async Task LoadPersonalEmployee()
        {
            try
            {
                var employees = await _userService.PersonalEmployeeAsync();

                Dispatcher.UIThread.Post(() =>
                {
                    Employees.Clear();
                    foreach (var employee in employees)
                    {
                        Employees.Add(employee);
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("LoadPersonalEmployee ERROR: " + ex.Message);
            }
        }
    }
}