using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using AvaloniaPdbAccounts.Models;
using AvaloniaPdbAccounts.Services;
using Avalonia.Threading;

namespace AvaloniaPdbAccounts.ViewModels.TRGDV
{
    public partial class EmployeesTRGDVViewModel : ViewModelBase
    {
        public ObservableCollection<EmployeeModel> Employees { get; }

        private readonly UserService _userService;
        public EmployeesTRGDVViewModel()
        {
            _userService = new UserService();

            Employees = new ObservableCollection<EmployeeModel>();
            // Lấy dữ liệu hiển thị ra khi khởi tạo
            _ = LoadPersonalEmployee();
        }

        private async Task LoadPersonalEmployee()
        {
            try
            {
                var employees = await _userService.GetEmployeesForTRGDVAsync();

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