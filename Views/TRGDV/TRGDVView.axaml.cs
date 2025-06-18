using Avalonia.Controls;

namespace AvaloniaPdbAccounts.Views.TRGDV
{
    public partial class TRGDVView : Window
    {
        public TRGDVView()
        {
            InitializeComponent();
            DataContext = new AvaloniaPdbAccounts.ViewModels.TRGDV.MainWindowTRGDVModel();
        }
    }
} 