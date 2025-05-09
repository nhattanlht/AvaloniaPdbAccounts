// using System.Threading.Tasks;
// using Avalonia.Controls;
// using AvaloniaPdbAccounts.Models;

// namespace AvaloniaPdbAccounts.Services;
// public class DialogService : IDialogService
// {
//     private readonly Window _parentWindow;

//     public DialogService(Window parentWindow)
//     {
//         _parentWindow = parentWindow;
//     }

//     public async Task<string?> ShowInputDialogAsync(string title, string message)
//     {
//         return await MessageBox.InputBox(_parentWindow, message, title);
//     }
// }


// DialogService.cs
using System.Threading.Tasks;
using Avalonia.Controls;
using AvaloniaPdbAccounts.Models;

namespace AvaloniaPdbAccounts.Services
{
    public class DialogService : IDialogService
    {
        private readonly Window _parentWindow;

        public DialogService()
        {
        }

        public DialogService(Window parentWindow)
        {
            _parentWindow = parentWindow;
        }

        public async Task<string?> ShowInputDialogAsync(string title, string message)
        {
            return await MessageBox.InputBox(_parentWindow, message, title);
        }
    }
}