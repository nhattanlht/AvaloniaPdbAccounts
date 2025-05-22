using System.ComponentModel;

public class DangKyModel : INotifyPropertyChanged
{
    public string MaSV { get; set; }
    public string HoTenSV { get; set; }
    public string MaMM { get; set; }
    public string TenMonHoc { get; set; }

    private double? _diemTH;
    public double? DiemTH
    {
        get => _diemTH;
        set
        {
            _diemTH = value;
            OnPropertyChanged(nameof(DiemTH));
        }
    }

    private double? _diemQT;
    public double? DiemQT
    {
        get => _diemQT;
        set
        {
            _diemQT = value;
            OnPropertyChanged(nameof(DiemQT));
        }
    }

    private double? _diemCK;
    public double? DiemCK
    {
        get => _diemCK;
        set
        {
            _diemCK = value;
            OnPropertyChanged(nameof(DiemCK));
        }
    }

    private double? _diemTK;
    public double? DiemTK
    {
        get => _diemTK;
        set
        {
            _diemTK = value;
            OnPropertyChanged(nameof(DiemTK));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
