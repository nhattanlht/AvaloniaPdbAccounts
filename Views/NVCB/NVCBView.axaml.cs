using Avalonia.Controls;

namespace AvaloniaPdbAccounts.Views.NVCB
{
    public partial class NVCBView : Window
    {
        public NVCBView()
        {
            InitializeComponent();
            DataContext = new AvaloniaPdbAccounts.ViewModels.NVCB.MainWindowNVCBModel();
        }
    }
} 