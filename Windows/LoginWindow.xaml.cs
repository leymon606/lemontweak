using System.Windows;
using MoliTweak.Core;

namespace MoliTweak;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
        TxtHwid.Text = HwidHelper.GetHwid();
    }

    private void Login_Click(object sender, RoutedEventArgs e)
    {
        var key = TxtKey.Text.Trim();
        if (LicenseManager.Instance.ValidateKey(key))
        {
            LicenseManager.Instance.SaveLicense(key);
            var main = new MainWindow();
            main.Show();
            Close();
        }
        else
        {
            TxtError.Text = "Geçersiz lisans anahtarı. Lütfen kontrol edin.";
            TxtError.Visibility = Visibility.Visible;
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }
}
