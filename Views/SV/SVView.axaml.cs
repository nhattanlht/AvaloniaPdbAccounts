// Views/SVView.axaml.cs
using Avalonia.Controls;

namespace AvaloniaPdbAccounts.Views.SV
{
    public partial class SVView : Window
    {
        public SVView()
        {
            InitializeComponent();
            DataContext = new AvaloniaPdbAccounts.ViewModels.SV.MainWindowSVModel();
        }
    }
}
