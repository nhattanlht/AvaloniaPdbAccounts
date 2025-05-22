using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AvaloniaPdbAccounts.Views.PKT;

public partial class EmployeesPKTView : UserControl
{
    public EmployeesPKTView()
    {
        InitializeComponent();
        DataContext = new AvaloniaPdbAccounts.ViewModels.PKT.EmployeesPKTViewModel();
    }
}