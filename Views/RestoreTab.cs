using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using MoliTweak.Core;

namespace MoliTweak.Views;

public class RestoreTab : StackPanel
{
    public RestoreTab(MainWindow app)
    {
        // ═══ HEADER ═══
        Children.Add(new TextBlock
        {
            Text = "Geri Yükleme", FontFamily = new FontFamily("Segoe UI"), FontSize = 26, FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f5f5f5")),
            Margin = new Thickness(0, 0, 0, 2)
        });
        Children.Add(new TextBlock
        {
            Text = "Tüm değişiklikleri tek tıkla geri alabilir veya ayrı ayrı kontrol edebilirsiniz.",
            FontFamily = new FontFamily("Segoe UI"), FontSize = 12,
            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#525252")),
            Margin = new Thickness(0, 0, 0, 18)
        });

        // ═══ BIG REVERT ALL CARD ═══
        var revertAllCard = new Border
        {
            CornerRadius = new CornerRadius(10), Padding = new Thickness(24, 20, 24, 20),
            Margin = new Thickness(0, 0, 0, 18), BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3b1a1a")),
            Background = new LinearGradientBrush(
                (Color)ColorConverter.ConvertFromString("#1a0f0f"),
                (Color)ColorConverter.ConvertFromString("#0a0a0a"), 0)
        };

        var revertGrid = new Grid();
        revertGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        revertGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var revertLeft = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
        revertLeft.Children.Add(new TextBlock
        {
            Text = "Tüm ayarları geri al", FontFamily = new FontFamily("Segoe UI"), FontSize = 16,
            FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f5f5f5"))
        });
        revertLeft.Children.Add(new TextBlock
        {
            Text = "lemonistan tarafından yapılan tüm değişiklikleri tek tıkla Windows varsayılanına döndürür.",
            FontFamily = new FontFamily("Segoe UI"), FontSize = 11.5,
            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#525252")),
            TextWrapping = TextWrapping.Wrap, MaxWidth = 500, Margin = new Thickness(0, 6, 20, 0), LineHeight = 18
        });
        Grid.SetColumn(revertLeft, 0);
        revertGrid.Children.Add(revertLeft);

        var revertBtn = new Button
        {
            Content = "Tümünü Geri Al",
            Style = (Style)Application.Current.FindResource("ActionButtonRose"),
            MinWidth = 140, MinHeight = 42, VerticalAlignment = VerticalAlignment.Center
        };
        revertBtn.Click += (_, _) => app.RevertAllOptimizations();
        Grid.SetColumn(revertBtn, 1);
        revertGrid.Children.Add(revertBtn);

        revertAllCard.Child = revertGrid;
        Children.Add(revertAllCard);

        // ═══ SECTION: TEK TEK GERİ AL ═══
        Children.Add(new TextBlock
        {
            Text = "TEK TEK GERİ AL", FontFamily = new FontFamily("Segoe UI"), FontSize = 9,
            FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3f3f3f")),
            Margin = new Thickness(0, 0, 0, 10)
        });

        Children.Add(CardBuilder.CreateActionCard(
            "Sistem Geri Yükleme Noktası",
            "Değişiklikler öncesi Windows üzerinde tam geri dönülebilir bir restore point oluşturur.",
            "Oluştur",
            () => app.RunAsync(() => RestoreManager.Instance.CreateSystemRestorePoint()),
            "Güvenlik", "ActionButtonAmber"));

        Children.Add(CardBuilder.CreateActionCard(
            "GTA V Ayarlarını Geri Yükle",
            "Daha önce alınan gta5_settings.xml yedeğini geri yükler.",
            "Geri Yükle",
            () => app.RunAsync(() => RestoreManager.Instance.RestoreGtaSettingsFromBackup())));

        Children.Add(CardBuilder.CreateActionCard(
            "Doku Fix Ayarlarını Kaldır",
            "Eklenen commandline.txt dosyalarını siler ve varsayılan bellek havuzuna döner.",
            "Kaldır",
            () => app.RunAsync(() => TextureLossFix.Instance.RevertTextureFix())));

        Children.Add(CardBuilder.CreateActionCard(
            "Telemetri & GameDVR Varsayılan",
            "Telemetri ve GameDVR kayıt defteri anahtarlarını orijinal Windows değerlerine çeker.",
            "Döndür",
            () => app.RunAsync(() => TelemetryDebloat.Instance.RevertToDefault())));

        Children.Add(CardBuilder.CreateActionCard(
            "Gecikme Ayarlarını Sıfırla",
            "BCDEDIT ve MMCSS ayarlarını Windows varsayılanına döndürür.",
            "Sıfırla",
            () => app.RunAsync(() => LatencyTweaks.Instance.RevertToDefault())));

        Children.Add(CardBuilder.CreateActionCard(
            "Nagle Ayarlarını Sıfırla",
            "TCPNoDelay ve TcpAckFrequency kayıtlarını siler, varsayılana döndürür.",
            "Sıfırla",
            () => app.RunAsync(() => NetworkTweaks.Instance.RevertNetwork())));

        Children.Add(CardBuilder.CreateActionCard(
            "Sistem İnce Ayarlarını Sıfırla",
            "Fullscreen, görsel efektler, arka plan uygulamaları, fare ivmesi ve bildirimleri varsayılana döndürür.",
            "Sıfırla",
            () => app.RunAsync(() => WindowsTweaks.Instance.RevertToDefault())));
    }
}
