using Avalonia.Controls;

namespace AvaloniaPdbAccounts.Views.GV
{
    public partial class GVView : Window
    {
        public GVView()
        {
            InitializeComponent();
            DataContext = new AvaloniaPdbAccounts.ViewModels.GV.MainWindowGVModel();
        }
    }
} 