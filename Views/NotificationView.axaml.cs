using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaPdbAccounts.ViewModels;

namespace AvaloniaPdbAccounts.Views
{
    public partial class NotificationView : Window
    {
        public NotificationView()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            DataContext = new NotificationViewModel();
        }
        
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}