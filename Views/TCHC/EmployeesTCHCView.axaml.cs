using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaPdbAccounts.Models;
using AvaloniaPdbAccounts.ViewModels.TCHC;

namespace AvaloniaPdbAccounts.Views.TCHC;

public partial class EmployeesTCHCView : UserControl
{
    public EmployeesTCHCView()
    {
        InitializeComponent();
        DataContext = new AvaloniaPdbAccounts.ViewModels.TCHC.EmployeesTCHCViewModel();
    }

}