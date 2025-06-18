using Avalonia.Markup.Xaml.MarkupExtensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AvaloniaPdbAccounts.Services;
using System;
using System.Threading.Tasks;

namespace AvaloniaPdbAccounts.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    [ObservableProperty]
    private string username;

    [ObservableProperty]
    private string password;

    [ObservableProperty]
    private string errorMessage;

    public LoginViewModel()
    {
        LoginCommand = new AsyncRelayCommand(LoginAsync);
    }

    public IAsyncRelayCommand LoginCommand { get; }

    public event Action? OnLoginSuccess;

    private async Task LoginAsync()
    {
        var success = await AccessService.Login(Username, Password);
        if (success)
        {
            OnLoginSuccess?.Invoke();  // Gọi event khi đăng nhập thành công
        }
        else
        {
            ErrorMessage = "Invalid username or password.";
        }
    }

    // public event Action OnLoginSuccess;
}