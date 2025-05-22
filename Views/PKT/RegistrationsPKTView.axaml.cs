using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaPdbAccounts.ViewModels.PKT;
namespace AvaloniaPdbAccounts.Views.PKT;

public partial class RegistrationsPKTView : UserControl
{
    public RegistrationsPKTView()
    {
        InitializeComponent();
        DataContext = new RegistrationsPKTViewModel();
    }

}