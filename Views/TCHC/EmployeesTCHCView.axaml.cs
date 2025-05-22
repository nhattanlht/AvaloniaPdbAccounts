using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AvaloniaPdbAccounts.Views.TCHC;

public partial class EmployeesTCHCView : UserControl
{
    public EmployeesTCHCView()
    {
        InitializeComponent();
        DataContext = new AvaloniaPdbAccounts.ViewModels.TCHC.EmployeesTCHCViewModel();
    }
}