using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MoliTweak.Core;

namespace MoliTweak.Views;

public class GamePerfTab : StackPanel
{
    public GamePerfTab(MainWindow app)
    {
        // ═══ FiveM SECTION ═══
        Children.Add(CreateSectionHeader("FiveM Optimizasyonu"));

        Children.Add(CardBuilder.CreateActionCard(
            "Doku Kaybı Düzeltme",
            "Yolların ve binaların kaybolmasını CitizenFX streaming_memory_budget ve güvenli parametrelerle çözer.",
            "Uygula",
            () => app.RunAsync(() => TextureLossFix.Instance.ApplyTextureLossFix()),
            "Crash-Proof", "ActionButtonPurple"));

        Children.Add(CardBuilder.CreateActionCard(
            "FiveM Güvenli Önbellek Temizliği",
            "Bozuk shader, log ve çökme raporlarını siler. Hesap, giriş bilgileri ve sunucu modları korunur.",
            "Temizle",
            () => app.RunAsync(() => CacheCleaner.Instance.CleanFiveMCache()),
            "Hesap & Modları Korur", "ActionButtonEmerald"));

        Children.Add(CardBuilder.CreateActionCard(
            "FiveM CPU & G/Ç Önceliği",
            "FiveM ve alt işlemlerine Registry üzerinden yüksek işlemci ve G/Ç önceliği tanımlar.",
            "Öncelik Ata",
            () => app.RunAsync(() => FiveMTweaks.Instance.ApplyHighCpuPriority()),
            "Stutter Fix"));

        Children.Add(CardBuilder.CreateActionCard(
            "CitizenFX.ini Optimizasyonu",
            "ShadowPlay kanca çakışmalarını ve gereksiz render çökmelerini engelleyen bayrakları yazar.",
            "Yapılandır",
            () => app.RunAsync(() => FiveMTweaks.Instance.OptimizeCitizenFxIni())));

        Children.Add(CardBuilder.CreateActionCard(
            "GTA V Preset: Ultra FPS",
            "Gölge, çimen, parçacık ve şehir yoğunluklarını optimize ederek 1% Low FPS droplarını sıfırlar.",
            "Uygula",
            () => app.RunAsync(() => FiveMTweaks.Instance.ApplyGtaPreset("ultra_fps"))));

        Children.Add(CardBuilder.CreateActionCard(
            "GTA V Preset: Dengeli",
            "Görsel netliği korurken yüksek ve stabil kare hızları sağlayan grafik ayarları.",
            "Uygula",
            () => app.RunAsync(() => FiveMTweaks.Instance.ApplyGtaPreset("balanced"))));

        // ═══ DONANIM PROFİLLERİ ═══
        Children.Add(CreateSectionHeader("Donanım Profilleri"));

        Children.Add(CardBuilder.CreateActionCard(
            "Low-End PC",
            "8GB ve altı RAM, dahili grafik veya eski GTX serisi. Gölgeler kapalı, 2GB texture budget, agresif debloat.",
            "Uygula",
            () => app.RunAsync(() => HardwareProfiler.Instance.ApplyProfile("low")),
            "Maksimum FPS", "ActionButtonRose"));

        Children.Add(CardBuilder.CreateActionCard(
            "Mid-End PC",
            "16GB RAM, GTX 1660 / RTX 2060 / RTX 3050-3060 / RX 6600. Yüksek dokular, 4GB texture pool, 0.5ms timer.",
            "Uygula",
            () => app.RunAsync(() => HardwareProfiler.Instance.ApplyProfile("mid")),
            "Dengeli 100+ FPS", "ActionButtonCyan"));

        Children.Add(CardBuilder.CreateActionCard(
            "High-End PC",
            "32GB+ RAM, RTX 3070-4090 / RX 7800+. Ultra netlik, 6-8GB texture pool, 0 gecikme, DPC kilidi.",
            "Uygula",
            () => app.RunAsync(() => HardwareProfiler.Instance.ApplyProfile("high")),
            "Ultra Netlik & 0 Lag", "ActionButtonPurple"));

        // ═══ GECİKME & GPU ═══
        Children.Add(CreateSectionHeader("Gecikme & GPU"));

        Children.Add(CardBuilder.CreateActionCard(
            "0.5ms Timer Resolution",
            "Windows 15.6ms varsayılan gecikmesini NtSetTimerResolution ile 0.5ms'ye kilitler.",
            "Kilitle",
            () => app.RunAsync(() => TimerResolution.Instance.SetResolution(0.5)),
            "0 Input Lag", "ActionButtonPurple"));

        Children.Add(CardBuilder.CreateActionCard(
            "MSI Modu",
            "GPU ve ağ bağdaştırıcısını IRQ'dan MSI moduna geçirir. DPC gecikmesini %80 düşürür.",
            "Etkinleştir",
            () => app.RunAsync(() => MsiMode.Instance.EnableMsiModeForGamingDevices()),
            "DPC Latency"));

        Children.Add(CardBuilder.CreateActionCard(
            "GPU Zorlama",
            "FiveM'in yalnızca harici güçlü ekran kartında çalışmasını kilitler (GpuPreference=2).",
            "Zorla",
            () => app.RunAsync(() => GpuTweaks.Instance.ForceDedicatedGpu())));

        Children.Add(CardBuilder.CreateActionCard(
            "GPU Sürücü Optimizasyonu",
            "HAGS, TDR kararlılık ve DWM pencereli ekran gecikmesini Registry ile çözer.",
            "Optimize Et",
            () => app.RunAsync(() => GpuTweaks.Instance.ApplyGpuDriverTweaks())));

        Children.Add(CardBuilder.CreateActionCard(
            "BCDEDIT Gecikme Ayarları",
            "disabledynamictick, useplatformclock ve timer ayarlarıyla CPU zamanlayıcı gecikmesini minimuma indirir.",
            "Uygula",
            () => app.RunAsync(() => LatencyTweaks.Instance.ApplyBcdeditTweaks())));

        Children.Add(CardBuilder.CreateActionCard(
            "MMCSS Oyun Önceliklendirme",
            "NetworkThrottlingIndex'i kaldırarak oyun sırasında ağ paketlerinin gecikmesini engeller.",
            "Uygula",
            () => app.RunAsync(() => LatencyTweaks.Instance.ApplyMmcssTweaks())));

        Children.Add(CardBuilder.CreateActionCard(
            "Nagle & TCPNoDelay",
            "TcpAckFrequency ve TCPNoDelay ayarlarıyla oyun paketlerinin anında iletilmesini sağlar.",
            "Uygula",
            () => app.RunAsync(() => NetworkTweaks.Instance.DisableNagleAlgorithm()),
            "Düşük Ping"));

        // ═══ WINDOWS OPTİMİZASYONU ═══
        Children.Add(CreateSectionHeader("Windows Optimizasyonu"));

        Children.Add(CardBuilder.CreateActionCard(
            "Ultimate Performance Güç Planı",
            "Windows'un gizli Nihai Performans güç planını açar ve CPU Core Parking'i kapatır.",
            "Etkinleştir",
            () => app.RunAsync(() => PowerTweaks.Instance.ApplyAll()),
            buttonStyle: "ActionButtonEmerald"));

        Children.Add(CardBuilder.CreateActionCard(
            "Telemetri ve Veri Toplamayı Kapat",
            "Windows telemetri, tanılama verileri, reklam kimliği ve etkinlik geçmişini devre dışı bırakır.",
            "Kapat",
            () => app.RunAsync(() => TelemetryDebloat.Instance.DisableTelemetryRegistry())));

        Children.Add(CardBuilder.CreateActionCard(
            "GameDVR ve Arka Plan Kaydı",
            "Oyun sırasında stutter'a yol açan arka plan ekran ve ses kaydını kapatır.",
            "Kapat",
            () => app.RunAsync(() => TelemetryDebloat.Instance.DisableGameDvr()),
            "Stutter Fix"));

        Children.Add(CardBuilder.CreateActionCard(
            "RAM Temizleyici",
            "Windows'un serbest bırakmadığı önbellek RAM sayfalarını boşaltarak bellek darboğazını çözer.",
            "Temizle",
            () => app.RunAsync(() => RamCleaner.Instance.CleanRam())));

        // ═══ SİSTEM İNCE AYARLARI ═══
        Children.Add(CreateSectionHeader("Sistem İnce Ayarları"));

        Children.Add(CardBuilder.CreateActionCard(
            "Fullscreen Optimizations Kapat",
            "Windows'un oyunları gizlice borderless windowed yapmasını engeller — gerçek tam ekran modunu zorlar.",
            "Kapat",
            () => app.RunAsync(() => WindowsTweaks.Instance.DisableFullscreenOptimizations()),
            "3-8 FPS", "ActionButtonPurple"));

        Children.Add(CardBuilder.CreateActionCard(
            "Görsel Efektleri Kapat",
            "Animasyon, şeffaflık, pencere gölgesi ve menü gecikmesini kapatarak GPU'ya nefes aldırır.",
            "Kapat",
            () => app.RunAsync(() => WindowsTweaks.Instance.DisableVisualEffects()),
            "GPU Tasarruf"));

        Children.Add(CardBuilder.CreateActionCard(
            "Arka Plan Uygulamaları Kapat",
            "UWP ve Microsoft Store uygulamalarının arka planda sessizce çalışmasını tamamen engeller.",
            "Kapat",
            () => app.RunAsync(() => WindowsTweaks.Instance.DisableBackgroundApps()),
            "CPU/RAM Tasarruf"));

        Children.Add(CardBuilder.CreateActionCard(
            "Power Throttling Kapat",
            "Windows'un CPU'yu akıllı güç yönetimiyle kısmasını devre dışı bırakır — tam güç her zaman aktif.",
            "Kapat",
            () => app.RunAsync(() => WindowsTweaks.Instance.DisablePowerThrottling()),
            "Tam Güç"));

        Children.Add(CardBuilder.CreateActionCard(
            "Fare İvmesini Kapat",
            "Enhance Pointer Precision özelliğini kapatarak 1:1 ham fare hareketi sağlar.",
            "Kapat",
            () => app.RunAsync(() => WindowsTweaks.Instance.DisableMouseAcceleration()),
            "Raw Input"));

        Children.Add(CardBuilder.CreateActionCard(
            "Bildirimleri Kapat",
            "Toast bildirimleri ve bildirim merkezini kapatarak oyun sırasında popup stutter'ını engeller.",
            "Kapat",
            () => app.RunAsync(() => WindowsTweaks.Instance.DisableNotifications()),
            "Stutter Fix"));
    }

    private static TextBlock CreateSectionHeader(string text)
    {
        return new TextBlock
        {
            Text = text, FontFamily = new FontFamily("Segoe UI"), FontSize = 13, FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#a3a3a3")),
            Margin = new Thickness(0, 12, 0, 8)
        };
    }
}
