using Avalonia.Controls;

namespace AvaloniaPdbAccounts.Views
{
public partial class GVView : Window
{
        public GVView()
        {
            InitializeComponent();
        }
    
        private void InitializeComponent()
        {
            Avalonia.Markup.Xaml.AvaloniaXamlLoader.Load(this);
        }
}
}
