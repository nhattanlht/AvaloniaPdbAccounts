using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using AvaloniaPdbAccounts.Models;
using AvaloniaPdbAccounts.Services;
using Oracle.ManagedDataAccess.Client;
using AvaloniaPdbAccounts.Utilities;
using AvaloniaPdbAccounts.ViewModels;


public class UserManagementViewModel : ViewModelBase
{
    private readonly UserService _userService = new();
    private readonly DialogService _dialogService = new();
    private string _connectionString = DatabaseSettings.ConnectionString;

    // Properties
    public ObservableCollection<string> Users { get; } = new();
    public string SelectedUser { get; set; }

    // Commands
    public ICommand LoadUsersCommand { get; }
    public ICommand AddUserCommand { get; }
    public ICommand EditUserCommand { get; }
    public ICommand DeleteUserCommand { get; }

 public UserManagementViewModel(IDialogService dialogService)
    {
        _dialogService = (DialogService)(dialogService ?? throw new ArgumentNullException(nameof(dialogService)));
        LoadUsersCommand = new RelayCommand(async () => await LoadUsersAsync());
        AddUserCommand = new RelayCommand(async () => 
        {
            var username = string.Empty;
            var password = string.Empty;
            // Cần xử lý nhập username/password từ UI
            username = await _dialogService.ShowInputDialogAsync("Tạo User", $"Nhập tên user mới {username}");
            password = await _dialogService.ShowInputDialogAsync("Tạo User", $"Nhập mật khẩu cho {password}");
            if (!string.IsNullOrEmpty(username) )
                await AddUserAsync(username, password);
        });
        
        EditUserCommand = new RelayCommand(async () => 
        {
            if (string.IsNullOrEmpty(SelectedUser)) return;
            var newPassword = await _dialogService.ShowInputDialogAsync("Sửa User", $"Nhập mật khẩu mới cho {SelectedUser}");
            if (!string.IsNullOrEmpty(newPassword))
                await EditUserAsync(newPassword);
        }, 
        () => !string.IsNullOrEmpty(SelectedUser)); // Chỉ enable khi có user được chọn
        
        DeleteUserCommand = new RelayCommand(async () => 
        {
            if (!string.IsNullOrEmpty(SelectedUser))
                await DeleteUserAsync();
        },
        () => !string.IsNullOrEmpty(SelectedUser));
    }

    public async Task LoadUsersAsync()
    {
        try
        {
            using var conn = new OracleConnection(_connectionString);
            await conn.OpenAsync();
            var users = await _userService.GetAllUsersAsync(conn);
            
            Users.Clear();
            foreach (var user in users)
                Users.Add(user);
        }
        catch (Exception ex)
        {
            // Handle error
        }
    }

public async Task AddUserAsync(string username, string password)
{
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException("Username cannot be empty or whitespace");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password cannot be empty or whitespace");
        }

        await _userService.CreateUserAsync(username, password);
        await LoadUsersAsync();
}

    public async Task EditUserAsync(string newPassword)
    {
        if (string.IsNullOrEmpty(SelectedUser)) return;
        await _userService.ChangePasswordAsync(SelectedUser, newPassword);
    }

    public async Task DeleteUserAsync()
    {
        if (string.IsNullOrEmpty(SelectedUser)) return;
        await _userService.DeleteUserAsync(SelectedUser);
        await LoadUsersAsync();
    }
}

