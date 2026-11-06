using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using MoliTweak.Core;
using MoliTweak.Views;

using System.Windows.Media.Animation;

namespace MoliTweak;

public partial class MainWindow : Window
{
    private readonly Dictionary<string, UIElement> _tabs = new();
    private readonly Dictionary<string, Button> _navButtons = new();

    private HardwareSpecs _specs;

    private readonly TranslateTransform _stripeTransform = new();
    private DispatcherTimer? _toastTimer;

    public static string CurrentAccentHex = "#22c55e"; // Default Green
    public static int CurrentBackgroundEffectIndex = 3; // Varsayılan: Kapalı

    private double _targetScrollOffset;
    private bool _isScrollAnimating;

    public MainWindow()
    {
        InitializeComponent();

        Logger.Instance.LogReceived += OnLogReceived;
        _specs = HardwareProfiler.Instance.ScanHardware();

        RegisterNavButtons();
        BuildTabs();
        SelectTab("dashboard");
        InitBackground();
        SetBackgroundEffect(3);
        SetupSmoothScrolling();
        InitDiscordProfile();
    }

    private void InitDiscordProfile()
    {
        Task.Run(async () =>
        {
            var profile = await DiscordHelper.GetProfileAsync();

            Dispatcher.Invoke(() =>
            {
                TxtDiscordUsername.Text = profile.Username;
                TxtModalDiscordUser.Text = profile.Username;
                TxtModalDiscordId.Text = string.IsNullOrEmpty(profile.Id) ? "ID: Bilinmiyor" : $"ID: {profile.Id}";

                string fallback = profile.Username.Length > 0 ? profile.Username[..1].ToUpper() : "D";
                TxtAvatarFallback.Text = fallback;
                TxtModalAvatarFallback.Text = fallback;

                string? avatarSource = profile.LocalAvatarPath ?? profile.AvatarUrl;
                if (!string.IsNullOrEmpty(avatarSource))
                {
                    LoadDiscordAvatar(avatarSource);
                }
            });
        });
    }

    private void LoadDiscordAvatar(string pathOrUrl)
    {
        try
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(pathOrUrl, UriKind.RelativeOrAbsolute);
            bitmap.EndInit();
            bitmap.Freeze();

            ImgDiscordAvatar.Source = bitmap;
            ImgModalDiscordAvatar.Source = bitmap;
            TxtAvatarFallback.Visibility = Visibility.Collapsed;
            TxtModalAvatarFallback.Visibility = Visibility.Collapsed;
        }
        catch { }
    }

    private void UserProfileCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        OpenLicenseModal();
    }

    private void OpenLicenseModal()
    {
        var lic = LicenseManager.Instance.GetLicenseDetails();
        TxtModalPlan.Text = lic.PlanName;
        TxtModalDuration.Text = lic.Duration;
        TxtModalActivationDate.Text = lic.ActivationDate;
        TxtModalExpirationDate.Text = lic.ExpirationDate;
        TxtModalKey.Text = lic.MaskedKey;
        TxtModalHwid.Text = HwidHelper.GetHwid();

        LicenseModalHost.Opacity = 0;
        LicenseModalHost.Visibility = Visibility.Visible;

        var fadeAnim = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(180))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        var scaleAnim = new DoubleAnimation(0.95, 1.0, TimeSpan.FromMilliseconds(180))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        LicenseModalHost.BeginAnimation(UIElement.OpacityProperty, fadeAnim);
        LicenseModalScale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
        LicenseModalScale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);
    }

    private void CloseLicenseModal_Click(object sender, RoutedEventArgs e)
    {
        CloseLicenseModal();
    }

    private void LicenseModalHost_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource == LicenseModalHost)
        {
            CloseLicenseModal();
        }
    }

    private void LicenseModalCard_MouseDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
    }

    private void CloseLicenseModal()
    {
        LicenseModalHost.Visibility = Visibility.Collapsed;
    }

    private void CopyKey_Click(object sender, RoutedEventArgs e)
    {
        var key = LicenseManager.Instance.GetSavedKey() ?? "1234";
        Clipboard.SetText(key);
        ShowToast("Lisans anahtarı panoya kopyalandı.", LogLevel.SUCCESS);
    }

    private void CopyHwid_Click(object sender, RoutedEventArgs e)
    {
        Clipboard.SetText(HwidHelper.GetHwid());
        ShowToast("Donanım kimliği (HWID) panoya kopyalandı.", LogLevel.SUCCESS);
    }

    private void SetupSmoothScrolling()
    {
        ContentScroll.PreviewMouseWheel += (sender, e) =>
        {
            e.Handled = true;

            // Hızlı ve anında tepki veren kaydırma miktarı
            double scrollDelta = -e.Delta * 0.85;

            if (!_isScrollAnimating)
            {
                _targetScrollOffset = ContentScroll.VerticalOffset;
                _isScrollAnimating = true;
                CompositionTarget.Rendering += OnSmoothScrollFrame;
            }

            _targetScrollOffset += scrollDelta;
            _targetScrollOffset = Math.Max(0, Math.Min(_targetScrollOffset, ContentScroll.ScrollableHeight));
        };
    }

    private void OnSmoothScrollFrame(object? sender, EventArgs e)
    {
        double current = ContentScroll.VerticalOffset;
        double diff = _targetScrollOffset - current;

        if (Math.Abs(diff) < 1.0)
        {
            ContentScroll.ScrollToVerticalOffset(Math.Round(_targetScrollOffset));
            CompositionTarget.Rendering -= OnSmoothScrollFrame;
            _isScrollAnimating = false;
            return;
        }

        // Hızlı, kaymayan ve anında duran lerp (0.38) + Math.Round ile piksel hizalama (WPF ClearType font cache'ini korur)
        double next = current + (diff * 0.38);
        ContentScroll.ScrollToVerticalOffset(Math.Round(next));
    }

    private void InitBackground()
    {
        var dotColor = Color.FromArgb(25, 255, 255, 255);
        var brush = new SolidColorBrush(dotColor);
        brush.Freeze();

        var drawingGroup = new DrawingGroup();
        drawingGroup.Children.Add(new GeometryDrawing(
            Brushes.Transparent, null,
            new RectangleGeometry(new Rect(0, 0, 20, 20))));
        
        drawingGroup.Children.Add(new GeometryDrawing(
            brush, null,
            new EllipseGeometry(new Point(10, 10), 1.1, 1.1)));

        var patternBrush = new DrawingBrush
        {
            Drawing = drawingGroup,
            TileMode = TileMode.Tile,
            Viewport = new Rect(0, 0, 20, 20),
            ViewportUnits = BrushMappingMode.Absolute
        };

        var rect = new Rectangle
        {
            Fill = patternBrush,
            IsHitTestVisible = false
        };

        BgCanvas.SizeChanged += (_, _) =>
        {
            rect.Width = BgCanvas.ActualWidth;
            rect.Height = BgCanvas.ActualHeight;
        };

        BgCanvas.Children.Add(rect);
    }

    private static readonly SolidColorBrush NavHoverBrush = FreezeBrush(Color.FromRgb(0xbd, 0xbd, 0xc4));
    private static readonly SolidColorBrush NavInactiveBrush = FreezeBrush(Color.FromRgb(0x78, 0x78, 0x80));
    private static readonly SolidColorBrush NavActiveBrush = FreezeBrush(Color.FromRgb(0xf5, 0xf5, 0xf7));

    private static SolidColorBrush FreezeBrush(Color c)
    {
        var b = new SolidColorBrush(c);
        b.Freeze();
        return b;
    }

    private void RegisterNavButtons()
    {
        _navButtons["dashboard"] = NavDashboard;
        _navButtons["gameperf"] = NavGamePerf;
        _navButtons["restore"] = NavRestore;
        _navButtons["settings"] = NavSettings;

        foreach (var (id, btn) in _navButtons)
        {
            btn.ApplyTemplate();

            // Dokunsal mikro-tıklama geri bildirimi
            var scale = new ScaleTransform(1.0, 1.0);
            btn.RenderTransformOrigin = new Point(0.5, 0.5);
            btn.RenderTransform = scale;

            btn.PreviewMouseDown += (s, e) =>
            {
                var pressAnim = new DoubleAnimation(0.975, TimeSpan.FromMilliseconds(50))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                scale.BeginAnimation(ScaleTransform.ScaleXProperty, pressAnim);
                scale.BeginAnimation(ScaleTransform.ScaleYProperty, pressAnim);
            };

            btn.PreviewMouseUp += (s, e) =>
            {
                var releaseAnim = new DoubleAnimation(1.0, TimeSpan.FromMilliseconds(80))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                scale.BeginAnimation(ScaleTransform.ScaleXProperty, releaseAnim);
                scale.BeginAnimation(ScaleTransform.ScaleYProperty, releaseAnim);
            };

            // Akıcı & Hafif Hover
            btn.MouseEnter += (s, e) =>
            {
                if (btn.Tag?.ToString() == _currentTabId) return;

                var slide = btn.Template.FindName("BtnSlide", btn) as TranslateTransform;
                if (slide != null)
                {
                    var slideAnim = new DoubleAnimation(2.0, TimeSpan.FromMilliseconds(100))
                    {
                        EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                    };
                    slide.BeginAnimation(TranslateTransform.XProperty, slideAnim);
                }

                btn.Foreground = NavHoverBrush;
            };

            btn.MouseLeave += (s, e) =>
            {
                if (btn.Tag?.ToString() == _currentTabId) return;

                var slide = btn.Template.FindName("BtnSlide", btn) as TranslateTransform;
                if (slide != null)
                {
                    var slideAnim = new DoubleAnimation(0, TimeSpan.FromMilliseconds(100))
                    {
                        EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                    };
                    slide.BeginAnimation(TranslateTransform.XProperty, slideAnim);
                }

                btn.Foreground = NavInactiveBrush;
            };
        }
    }

    private void UpdateNavStyles(bool animated = true)
    {
        foreach (var (id, btn) in _navButtons)
        {
            btn.ApplyTemplate();
            var root = btn.Template.FindName("BtnRoot", btn) as Border;
            var pill = btn.Template.FindName("LeftPill", btn) as Border;
            var slide = btn.Template.FindName("BtnSlide", btn) as TranslateTransform;

            bool isActive = (id == _currentTabId);

            if (isActive)
            {
                btn.Foreground = NavActiveBrush;
                btn.FontWeight = FontWeights.SemiBold;

                if (root != null)
                {
                    root.Background = Brushes.Transparent;
                    root.BorderBrush = Brushes.Transparent;
                }

                if (slide != null)
                {
                    var slideAnim = new DoubleAnimation(0, TimeSpan.FromMilliseconds(100))
                    {
                        EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                    };
                    slide.BeginAnimation(TranslateTransform.XProperty, slideAnim);
                }

                if (pill != null)
                {
                    if (animated)
                    {
                        var heightAnim = new DoubleAnimation(16, TimeSpan.FromMilliseconds(130))
                        {
                            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                        };
                        var opacityAnim = new DoubleAnimation(1.0, TimeSpan.FromMilliseconds(130))
                        {
                            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                        };
                        pill.BeginAnimation(FrameworkElement.HeightProperty, heightAnim);
                        pill.BeginAnimation(UIElement.OpacityProperty, opacityAnim);
                    }
                    else
                    {
                        pill.Height = 16;
                        pill.Opacity = 1.0;
                    }
                }
            }
            else
            {
                btn.Foreground = NavInactiveBrush;
                btn.FontWeight = FontWeights.Normal;

                if (root != null)
                {
                    root.Background = Brushes.Transparent;
                    root.BorderBrush = Brushes.Transparent;
                }

                if (pill != null)
                {
                    if (animated)
                    {
                        var heightAnim = new DoubleAnimation(0, TimeSpan.FromMilliseconds(100))
                        {
                            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                        };
                        var opacityAnim = new DoubleAnimation(0, TimeSpan.FromMilliseconds(100))
                        {
                            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                        };
                        pill.BeginAnimation(FrameworkElement.HeightProperty, heightAnim);
                        pill.BeginAnimation(UIElement.OpacityProperty, opacityAnim);
                    }
                    else
                    {
                        pill.Height = 0;
                        pill.Opacity = 0;
                    }
                }
            }
        }
    }

    private void BuildTabs()
    {
        _tabs["dashboard"] = new DashboardTab(this, _specs);
        _tabs["gameperf"] = new GamePerfTab(this);
        _tabs["restore"] = new RestoreTab(this);
        _tabs["settings"] = new SettingsTab(this, _specs);
    }

    private string? _currentTabId;

    public void SelectTab(string tabId)
    {
        if (_currentTabId == tabId) return;
        _currentTabId = tabId;

        ContentPanel.Children.Clear();

        if (_tabs.TryGetValue(tabId, out var content))
        {
            ContentPanel.Children.Add(content);
        }

        if (_isScrollAnimating)
        {
            CompositionTarget.Rendering -= OnSmoothScrollFrame;
            _isScrollAnimating = false;
        }
        _targetScrollOffset = 0;
        ContentScroll.ScrollToTop();

        var tt = new TranslateTransform(0, 6);
        ContentPanel.RenderTransform = tt;
        ContentPanel.Opacity = 0;

        var slideAnim = new DoubleAnimation(6, 0, TimeSpan.FromMilliseconds(110))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        var fadeAnim = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(110))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        tt.BeginAnimation(TranslateTransform.YProperty, slideAnim);
        ContentPanel.BeginAnimation(UIElement.OpacityProperty, fadeAnim);

        UpdateNavStyles(animated: true);
    }

    private void NavButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string tabId)
            SelectTab(tabId);
    }

    public void RunAsync(Action action)
    {
        Task.Run(() =>
        {
            try { action(); }
            catch (Exception ex) { Logger.Instance.Error($"İşlem hatası: {ex.Message}"); }
        });
    }

    private void OnLogReceived(object? sender, LogEventArgs e)
    {
        Dispatcher.BeginInvoke(() => ShowToast(e.Message, e.Level));
    }

    private void ShowToast(string message, LogLevel level)
    {
        var (icon, iconColor, title) = level switch
        {
            LogLevel.SUCCESS => ("✓", "#4ade80", "BAŞARILI"),
            LogLevel.WARNING => ("⚠", "#f59e0b", "UYARI"),
            LogLevel.ERROR => ("✕", "#ef4444", "HATA"),
            _ => ("ℹ", "#a1a1aa", "BİLDİRİM")
        };

        ToastIcon.Text = icon;
        ToastIcon.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(iconColor));
        ToastTitle.Text = title;
        ToastText.Text = message;

        ToastHost.Visibility = Visibility.Visible;

        // Telefon bildirimi tarzı yukarıdan yaylanarak inme animasyonu
        var slideDown = new DoubleAnimation(-40, 0, TimeSpan.FromMilliseconds(280))
        {
            EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.25 }
        };
        var fadeIn = new DoubleAnimation(0, 1.0, TimeSpan.FromMilliseconds(200))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        ToastSlide.BeginAnimation(TranslateTransform.YProperty, slideDown);
        ToastHost.BeginAnimation(UIElement.OpacityProperty, fadeIn);

        _toastTimer?.Stop();
        _toastTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3.2) };
        _toastTimer.Tick += (_, _) =>
        {
            _toastTimer.Stop();

            var slideUp = new DoubleAnimation(0, -35, TimeSpan.FromMilliseconds(200))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };
            var fadeOut = new DoubleAnimation(1.0, 0, TimeSpan.FromMilliseconds(180))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };

            fadeOut.Completed += (_, _) =>
            {
                ToastHost.Visibility = Visibility.Collapsed;
            };

            ToastSlide.BeginAnimation(TranslateTransform.YProperty, slideUp);
            ToastHost.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        };
        _toastTimer.Start();
    }

    public void ApplyMasterUltraBoost()
    {
        RunAsync(() =>
        {
            RestoreManager.Instance.BackupAllCriticalKeys();
            CacheCleaner.Instance.CleanAll();
            HardwareProfiler.Instance.ApplyProfile(_specs.Tier);
            FiveMTweaks.Instance.ApplyHighCpuPriority();
            FiveMTweaks.Instance.OptimizeCitizenFxIni();
            Logger.Instance.Success($"Tüm optimizasyonlar uygulandı — [{_specs.Tier.ToUpper()}-END] profili aktif.");
        });
    }

    public void RevertAllOptimizations()
    {
        RunAsync(() =>
        {
            Logger.Instance.Info("Tüm ayarlar geri alınıyor...");
            try { RestoreManager.Instance.RestoreGtaSettingsFromBackup(); } catch { }
            try { TextureLossFix.Instance.RevertTextureFix(); } catch { }
            try { ServicesManager.Instance.RevertServicesProfile(); } catch { }
            try { TelemetryDebloat.Instance.RevertToDefault(); } catch { }
            try { LatencyTweaks.Instance.RevertToDefault(); } catch { }
            try { NetworkTweaks.Instance.RevertNetwork(); } catch { }
            try { WindowsTweaks.Instance.RevertToDefault(); } catch { }
            Logger.Instance.Success("Tüm ayarlar Windows varsayılanına döndürüldü.");
        });
    }

    private void AnimateElementOpacity(UIElement element, double targetOpacity)
    {
        if (element == null) return;
        element.Visibility = Visibility.Visible;
        var anim = new DoubleAnimation
        {
            To = targetOpacity,
            Duration = TimeSpan.FromMilliseconds(400),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        element.BeginAnimation(UIElement.OpacityProperty, anim);
    }

    private void AnimateGlowColor(GradientStop stop, Color targetColor)
    {
        if (stop == null) return;
        var anim = new ColorAnimation
        {
            To = targetColor,
            Duration = TimeSpan.FromMilliseconds(450),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        stop.BeginAnimation(GradientStop.ColorProperty, anim);
    }

    public void SetBackgroundEffect(int effectIndex)
    {
        CurrentBackgroundEffectIndex = effectIndex;

        if (effectIndex == 3)
        {
            GlowGrid.Visibility = Visibility.Collapsed;
            BgCanvas.Visibility = Visibility.Collapsed;
            return;
        }

        double glowTarget = (effectIndex == 0 || effectIndex == 1) ? 1.0 : 0.0;
        double bgTarget = (effectIndex == 0 || effectIndex == 2) ? 1.0 : 0.0;

        GlowGrid.Visibility = (glowTarget > 0) ? Visibility.Visible : Visibility.Collapsed;
        BgCanvas.Visibility = (bgTarget > 0) ? Visibility.Visible : Visibility.Collapsed;

        AnimateElementOpacity(GlowGrid, glowTarget);
        AnimateElementOpacity(BgCanvas, bgTarget);
    }

    public void ChangeThemeColor(string hex)
    {
        if (CurrentAccentHex == hex) return;

        CurrentAccentHex = hex;
        string hoverHex, dimHex, bgHex;
        switch (hex)
        {
            case "#3b82f6": hoverHex = "#60a5fa"; dimHex = "#1e40af"; bgHex = "#1e3a8a"; break; // Blue
            case "#ef4444": hoverHex = "#f87171"; dimHex = "#991b1b"; bgHex = "#450a0a"; break; // Red
            case "#a855f7": hoverHex = "#c084fc"; dimHex = "#6b21a8"; bgHex = "#3b0764"; break; // Purple
            default: hoverHex = "#4ade80"; dimHex = "#166534"; bgHex = "#14532d"; break; // Green
        }

        Application.Current.Resources["AccentColor"] = (Color)ColorConverter.ConvertFromString(hex);
        Application.Current.Resources["AccentBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
        Application.Current.Resources["AccentHoverBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hoverHex));
        Application.Current.Resources["AccentDimBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(dimHex));
        Application.Current.Resources["AccentBgBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(bgHex));

        if (GlowStop1 != null) AnimateGlowColor(GlowStop1, (Color)ColorConverter.ConvertFromString("#45" + hoverHex.Substring(1)));
        if (GlowStop2 != null) AnimateGlowColor(GlowStop2, (Color)ColorConverter.ConvertFromString("#48" + bgHex.Substring(1)));
        
        BuildTabs();
        UpdateNavStyles(animated: false);
        
        ContentPanel.Children.Clear();
        if (_currentTabId != null && _tabs.TryGetValue(_currentTabId, out var content)) ContentPanel.Children.Add(content);

        var fadeAnim = new DoubleAnimation(0.6, 1.0, TimeSpan.FromMilliseconds(300))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        ContentPanel.BeginAnimation(UIElement.OpacityProperty, fadeAnim);
    }

    private void Titlebar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left) DragMove();
    }

    private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    private void Maximize_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }
    private void Close_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
}
