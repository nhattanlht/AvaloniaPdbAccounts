// Views/PDTView.axaml.cs
using Avalonia.Controls;

namespace AvaloniaPdbAccounts.Views.PDT
{
    public partial class PDTView : Window
    {
        public PDTView()
        {
            InitializeComponent();
            DataContext = new AvaloniaPdbAccounts.ViewModels.PDT.MainWindowPDTModel();
        }
    }
}
