using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AvaloniaPdbAccounts.ViewModels.PDT
{
    public partial class PDTViewModel : ViewModelBase
    {
        // Đây là ViewModel sau khi đăng nhập thành công
        public string WelcomeMessage => "Welcome to the PDTViewModel!";
    }
}
