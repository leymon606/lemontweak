using System.Windows;
using System.Windows.Threading;
using MoliTweak.Core;

namespace MoliTweak;

public partial class SplashWindow : Window
{
    private readonly DispatcherTimer _timer;
    private int _progress;

    private static readonly string[] StatusMessages =
    [
        "Donanım kimliği doğrulanıyor...",
        "Sistem profili analiz ediliyor...",
        "Optimizasyon planı hazırlanıyor...",
        "Bağlanıyor..."
    ];

    public SplashWindow()
    {
        InitializeComponent();

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(25) };
        _timer.Tick += OnTick;

        Loaded += (_, _) =>
        {
            Task.Run(() => HwidHelper.GetHwid());
            _timer.Start();
        };
    }

    private void OnTick(object? sender, EventArgs e)
    {
        _progress += 1;
        if (_progress > 100) _progress = 100;

        TxtPercent.Text = $"{_progress}%";
        ProgressFill.Width = _progress * 4.0; // 400px track width at 100%

        int msgIndex = _progress switch
        {
            < 30 => 0,
            < 55 => 1,
            < 85 => 2,
            _ => 3
        };
        TxtStatus.Text = StatusMessages[msgIndex];

        if (_progress >= 100)
        {
            _timer.Stop();

            if (LicenseManager.Instance.HasSavedLicense())
            {
                var main = new MainWindow();
                main.Show();
            }
            else
            {
                var login = new LoginWindow();
                login.Show();
            }

            Close();
        }
    }
}
