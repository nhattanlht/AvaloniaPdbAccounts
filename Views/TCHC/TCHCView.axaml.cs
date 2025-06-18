// Views/TCHCView.axaml.cs
using Avalonia.Controls;

namespace AvaloniaPdbAccounts.Views.TCHC
{
    public partial class TCHCView : Window
    {
        public TCHCView()
        {
            InitializeComponent();
            DataContext = new AvaloniaPdbAccounts.ViewModels.TCHC.MainWindowTCHCModel();
        }
    }
}
