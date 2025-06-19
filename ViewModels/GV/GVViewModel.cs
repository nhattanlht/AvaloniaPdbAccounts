using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AvaloniaPdbAccounts.ViewModels.GV
{
    public partial class PCTSVViewModel : ViewModelBase
    {
        // Đây là ViewModel sau khi đăng nhập thành công
        public string WelcomeMessage => "Welcome to the GVViewModel!";
    }
} 