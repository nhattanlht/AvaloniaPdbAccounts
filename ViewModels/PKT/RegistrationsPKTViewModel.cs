using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using AvaloniaPdbAccounts.Models;
using AvaloniaPdbAccounts.Services;
using ReactiveUI;
using Avalonia.Threading;

namespace AvaloniaPdbAccounts.ViewModels.PKT
{
    public class RegistrationsPKTViewModel : ViewModelBase
    {
        private readonly PKTService _pktService = new();
        private ObservableCollection<DangKyModel> _registrations = new();
        private DangKyModel _selectedRegistration;
        private string _searchText;
        private bool _isLoading;

        public RegistrationsPKTViewModel()
        {
            // Khởi tạo commands
            LoadRegistrationsCommand = ReactiveCommand.CreateFromTask(LoadRegistrationsAsync);
            UpdateScoresCommand = ReactiveCommand.CreateFromTask(UpdateScoresAsync, 
                this.WhenAnyValue(x => x.SelectedRegistration).Select(x => x != null));
            
            // Cách 1: Sử dụng command không tham số
            SearchCommand = ReactiveCommand.CreateFromTask(SearchRegistrationsAsync);

            // Tự động load dữ liệu khi khởi tạo
            Dispatcher.UIThread.Post(() => LoadRegistrationsCommand.Execute().Subscribe());

            // Cách 1: Thiết lập search với debounce (không dùng InvokeCommand)
            this.WhenAnyValue(x => x.SearchText)
                .Where(x => x != null)
                .Throttle(TimeSpan.FromMilliseconds(300))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => SearchCommand.Execute().Subscribe());
        }

        public ObservableCollection<DangKyModel> Registrations
        {
            get => _registrations;
            set => SetProperty(ref _registrations, value);
        }

        public DangKyModel SelectedRegistration
        {
            get => _selectedRegistration;
            set => SetProperty(ref _selectedRegistration, value);
        }

        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ReactiveCommand<Unit, Unit> LoadRegistrationsCommand { get; }
        public ReactiveCommand<Unit, Unit> UpdateScoresCommand { get; }
        public ReactiveCommand<Unit, Unit> SearchCommand { get; }

        private async Task LoadRegistrationsAsync()
        {
            try
            {
                IsLoading = true;
                var registrations = await Task.Run(() => _pktService.GetDanhSachDangKy());

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Registrations = new ObservableCollection<DangKyModel>(registrations);
                });
            }
            catch (Exception ex)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Console.WriteLine($"Lỗi khi tải danh sách đăng ký: {ex.Message}");
                });
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task UpdateScoresAsync()
        {
            if (SelectedRegistration == null) return;

            try
            {
                IsLoading = true;
                var success = await Task.Run(() => _pktService.CapNhatDiem(
                    SelectedRegistration.MaSV,
                    SelectedRegistration.MaMM,
                    SelectedRegistration.DiemTH,
                    SelectedRegistration.DiemQT,
                    SelectedRegistration.DiemCK));

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    if (success)
                    {
                        SelectedRegistration.DiemTK = CalculateFinalScore(
                            SelectedRegistration.DiemTH,
                            SelectedRegistration.DiemQT,
                            SelectedRegistration.DiemCK);

                        Console.WriteLine("Cập nhật điểm thành công");
                    }
                    else
                    {
                        Console.WriteLine("Cập nhật điểm thất bại");
                    }
                });
            }
            catch (Exception ex)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Console.WriteLine($"Lỗi khi cập nhật điểm: {ex.Message}");
                });
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SearchRegistrationsAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                await LoadRegistrationsAsync();
                return;
            }

            try
            {
                IsLoading = true;
                var searchTerm = SearchText.ToLowerInvariant();
                var filtered = await Task.Run(() => 
                    _registrations.Where(r =>
                        (r.MaSV?.ToLowerInvariant().Contains(searchTerm) ?? false) ||
                        (r.HoTenSV?.ToLowerInvariant().Contains(searchTerm) ?? false) ||
                        (r.TenMonHoc?.ToLowerInvariant().Contains(searchTerm) ?? false))
                    .ToList());

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Registrations = new ObservableCollection<DangKyModel>(filtered);
                });
            }
            catch (Exception ex)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Console.WriteLine($"Lỗi khi tìm kiếm: {ex.Message}");
                });
            }
            finally
            {
                IsLoading = false;
            }
        }

        private static double? CalculateFinalScore(double? diemTH, double? diemQT, double? diemCK)
        {
            if (diemTH == null || diemQT == null || diemCK == null)
                return null;

            return (diemTH.Value * 0.3) + (diemQT.Value * 0.3) + (diemCK.Value * 0.4);
        }
    }
}