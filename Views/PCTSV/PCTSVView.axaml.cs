using Avalonia.Controls;

namespace AvaloniaPdbAccounts.Views.PCTSV
{
    public partial class PCTSVView : Window
    {
        public PCTSVView()
        {
            InitializeComponent();
            DataContext = new AvaloniaPdbAccounts.ViewModels.PCTSV.MainWindowPCTSVModel();
        }
    }
} 