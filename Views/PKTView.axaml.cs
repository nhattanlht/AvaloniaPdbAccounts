// Views/PKTView.axaml.cs
using Avalonia.Controls;

namespace AvaloniaPdbAccounts.Views
{
    public partial class PKTView : Window
    {
        public PKTView()
        {
            InitializeComponent();
            DataContext = new AvaloniaPdbAccounts.ViewModels.MainWindowModel();
        }
    }
}
