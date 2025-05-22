// Views/PKTView.axaml.cs
using Avalonia.Controls;

namespace AvaloniaPdbAccounts.Views.PKT
{
    public partial class PKTView : Window
    {
        public PKTView()
        {
            InitializeComponent();
            DataContext = new AvaloniaPdbAccounts.ViewModels.PKT.MainWindowPKTModel();
        }
    }
}
