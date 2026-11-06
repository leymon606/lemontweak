using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Threading;
using MoliTweak.Core;

namespace MoliTweak.Views;

public class DashboardTab : StackPanel
{
    public DashboardTab(MainWindow app, HardwareSpecs specs)
    {
        // ═══ HEADER ═══
        Children.Add(new TextBlock
        {
            Text = "Sistem hazır mı?", FontFamily = new FontFamily("Segoe UI"), FontSize = 26, FontWeight = FontWeights.Bold,
            Foreground = B(C("#f5f5f5")), Margin = new Thickness(0, 0, 0, 2)
        });
        Children.Add(new TextBlock
        {
            Text = "Makinenin FiveM için mevcut durumu. Optimizasyonu başlatmadan önce\nanticheat koruma durumunu kontrol edin.",
            FontFamily = new FontFamily("Segoe UI"), FontSize = 12, Foreground = B(C("#525252")),
            TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 20), LineHeight = 19
        });

        // ═══ INFO CARDS — SINGLE CONTAINER ═══
        var infoBorder = new Border
        {
            Background = B(C("#111111")), BorderBrush = B(C("#1e1e1e")), BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(10), Margin = new Thickness(0, 0, 0, 14), ClipToBounds = true
        };

        var infoOuter = new StackPanel();

        // Green top glow line
        var topGlow = new Border
        {
            Height = 2,
            Background = new LinearGradientBrush(
                new GradientStopCollection
                {
                    new GradientStop(C(MainWindow.CurrentAccentHex), 0),
                    new GradientStop(C("#16a34a"), 0.5),
                    new GradientStop(C(MainWindow.CurrentAccentHex), 1)
                }, 0)
        };
        infoOuter.Children.Add(topGlow);

        var infoGrid = new Grid();
        for (int i = 0; i < 4; i++)
            infoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        // Card 1: İŞLEMCİ
        var cpuSection = MakeSection("İŞLEMCİ", GetCpuName(specs), $"{specs.CpuPhysical} çekirdek · {specs.CpuCores} iş parçacığı");
        Grid.SetColumn(cpuSection, 0); infoGrid.Children.Add(cpuSection);
        infoGrid.Children.Add(MakeDivider(1));

        // Card 2: EKRAN KARTI
        var gpuSection = MakeSection("EKRAN KARTI", specs.GpuName, $"VRAM: {specs.VramMb / 1024.0:F1} GB");
        Grid.SetColumn(gpuSection, 1); infoGrid.Children.Add(gpuSection);
        infoGrid.Children.Add(MakeDivider(2));

        // Card 3: ZAMANLAMAYICI
        var timerSection = MakeSection("ZAMANLAMAYICI", "0.5 ms", "optimize edildi");
        Grid.SetColumn(timerSection, 2); infoGrid.Children.Add(timerSection);
        infoGrid.Children.Add(MakeDivider(3));

        // Card 4: UYGULANAN AYAR (registry/dosya durumundan gerçek zamanlı hesaplanır)
        var status = TweakStatusChecker.GetStatus();
        int applied = status.Count(s => s.Applied);
        int total = status.Count;
        var appliedSection = new StackPanel { Margin = new Thickness(18, 14, 18, 14) };
        appliedSection.Children.Add(new TextBlock
        {
            Text = "UYGULANAN AYAR", FontFamily = new FontFamily("Segoe UI"), FontSize = 9, FontWeight = FontWeights.SemiBold,
            Foreground = B(C(MainWindow.CurrentAccentHex)), Margin = new Thickness(0, 0, 0, 6), Opacity = 0.7
        });
        var countRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 6) };
        var countNum = new TextBlock
        {
            Text = applied.ToString(), FontFamily = new FontFamily("Segoe UI"), FontSize = 24, FontWeight = FontWeights.Bold,
            Foreground = B(C("#f5f5f5"))
        };
        countRow.Children.Add(countNum);
        countRow.Children.Add(new TextBlock
        {
            Text = $" /{total}", FontFamily = new FontFamily("Segoe UI"), FontSize = 11, Foreground = B(C("#3f3f3f")),
            VerticalAlignment = VerticalAlignment.Bottom, Margin = new Thickness(0, 0, 0, 3)
        });
        appliedSection.Children.Add(countRow);

        // Progress bar
        var barBg = new Border { Height = 4, CornerRadius = new CornerRadius(2), Background = B(C("#1a1a1a")) };
        var barFill = new Border
        {
            Height = 4, CornerRadius = new CornerRadius(2), HorizontalAlignment = HorizontalAlignment.Left,
            Width = (applied / (double)total) * 160,
            Background = new LinearGradientBrush(C(MainWindow.CurrentAccentHex), C("#16a34a"), 0)
        };
        var barGrid = new Grid { MaxWidth = 160, HorizontalAlignment = HorizontalAlignment.Left };
        barGrid.Children.Add(barBg);
        barGrid.Children.Add(barFill);
        appliedSection.Children.Add(barGrid);

        Grid.SetColumn(appliedSection, 3); infoGrid.Children.Add(appliedSection);

        infoOuter.Children.Add(infoGrid);
        infoBorder.Child = infoOuter;
        Children.Add(infoBorder);

        // ═══ ANTICHEAT BANNER ═══
        var acBanner = new Border
        {
            CornerRadius = new CornerRadius(10), Padding = new Thickness(18, 14, 18, 14),
            Margin = new Thickness(0, 0, 0, 14), BorderThickness = new Thickness(1),
            BorderBrush = B(C("#1a2a1a")),
            Background = new LinearGradientBrush(C("#0d1a0d"), C("#0a0a0a"), 0)
        };
        var acRow = new StackPanel { Orientation = Orientation.Horizontal };
        acRow.Children.Add(new TextBlock
        {
            Text = "⊘", FontFamily = new FontFamily("Segoe UI"), FontSize = 18, Foreground = B(C(MainWindow.CurrentAccentHex)),
            VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 12, 0)
        });
        var acTextPanel = new StackPanel();
        acTextPanel.Children.Add(new TextBlock
        {
            Text = "Anticheat koruması etkin", FontFamily = new FontFamily("Segoe UI"), FontSize = 13, FontWeight = FontWeights.SemiBold,
            Foreground = B(C("#f5f5f5")), Margin = new Thickness(0, 0, 0, 3)
        });
        acTextPanel.Children.Add(new TextBlock
        {
            Text = "7 kritik servis izleniyor ve korunuyor. lemonistan bu servisleri hiçbir koşulda kapatmaz — FiveM sunucularında ban riskiniz yok.",
            FontFamily = new FontFamily("Segoe UI"), FontSize = 11, Foreground = B(C("#525252")),
            TextWrapping = TextWrapping.Wrap, LineHeight = 17
        });
        acRow.Children.Add(acTextPanel);
        acBanner.Child = acRow;
        Children.Add(acBanner);

        // ═══ CTA: ÖNERİLEN OPTİMİZASYON ═══
        var ctaBorder = new Border
        {
            CornerRadius = new CornerRadius(10), Padding = new Thickness(24, 20, 24, 20), Margin = new Thickness(0, 0, 0, 14),
            BorderThickness = new Thickness(1), BorderBrush = B(C("#1a2a1a")),
            Background = new LinearGradientBrush(
                new GradientStopCollection
                {
                    new GradientStop(C("#0f1a0f"), 0),
                    new GradientStop(C("#0a130a"), 0.6),
                    new GradientStop(C("#0d180d"), 1)
                }, new Point(0, 0), new Point(1, 1))
        };

        var ctaGrid = new Grid();
        ctaGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        ctaGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var ctaLeft = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
        ctaLeft.Children.Add(new TextBlock
        {
            Text = "Önerilen optimizasyonu uygula", FontFamily = new FontFamily("Segoe UI"), FontSize = 16, FontWeight = FontWeights.SemiBold,
            Foreground = B(C("#f5f5f5"))
        });
        ctaLeft.Children.Add(new TextBlock
        {
            Text = "Güvenli ve doğrulanmış ayarlar tek seferde uygulanır. İşlem öncesi sistem geri yükleme noktası oluşturulur, her adım geri alınabilir.",
            FontFamily = new FontFamily("Segoe UI"), FontSize = 11.5, Foreground = B(C("#525252")),
            TextWrapping = TextWrapping.Wrap, MaxWidth = 550, Margin = new Thickness(0, 6, 20, 0), LineHeight = 18
        });
        Grid.SetColumn(ctaLeft, 0);
        ctaGrid.Children.Add(ctaLeft);

        var ctaBtn = MakeGlowButton("Optimize Et");
        ctaBtn.Click += (_, _) => app.ApplyMasterUltraBoost();
        Grid.SetColumn(ctaBtn, 1);
        ctaGrid.Children.Add(ctaBtn);

        ctaBorder.Child = ctaGrid;
        Children.Add(ctaBorder);

        // ═══ TWO COLUMNS: HIZLI İŞLEMLER + UYGULANAN AYARLAR ═══
        var bottomGrid = new Grid();
        bottomGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        bottomGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        // LEFT: Hızlı İşlemler
        var quickCard = new Border
        {
            Background = B(C("#111111")), BorderBrush = B(C("#1a1a1a")), BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(10), Padding = new Thickness(20, 16, 20, 16), Margin = new Thickness(0, 0, 6, 0)
        };
        var quickPanel = new StackPanel();
        quickPanel.Children.Add(new TextBlock
        {
            Text = "Hızlı işlemler", FontFamily = new FontFamily("Segoe UI"), FontSize = 14, FontWeight = FontWeights.SemiBold,
            Foreground = B(C("#f5f5f5")), Margin = new Thickness(0, 0, 0, 2)
        });
        quickPanel.Children.Add(new TextBlock
        {
            Text = "Tek tıkla çalışan bakım araçları", FontFamily = new FontFamily("Segoe UI"), FontSize = 11,
            Foreground = B(C("#525252")), Margin = new Thickness(0, 0, 0, 14)
        });

        var pillWrap = new WrapPanel { Orientation = Orientation.Horizontal };
        pillWrap.Children.Add(MakePill("FiveM önbelleğini temizle", () => app.RunAsync(() => CacheCleaner.Instance.CleanAll())));
        pillWrap.Children.Add(MakePill("RAM temizleyici", () => app.RunAsync(() => RamCleaner.Instance.CleanRam())));
        pillWrap.Children.Add(MakePill("FiveM önceliğini yükselt", () => app.RunAsync(() => FiveMTweaks.Instance.ApplyHighCpuPriority())));
        pillWrap.Children.Add(MakePill("0.5ms Timer kilidi", () => app.RunAsync(() => TimerResolution.Instance.SetResolution(0.5))));
        quickPanel.Children.Add(pillWrap);
        quickCard.Child = quickPanel;
        Grid.SetColumn(quickCard, 0);
        bottomGrid.Children.Add(quickCard);

        // RIGHT: Uygulanan Ayarlar
        var settingsCard = new Border
        {
            Background = B(C("#111111")), BorderBrush = B(C("#1a1a1a")), BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(10), Padding = new Thickness(20, 16, 20, 16), Margin = new Thickness(6, 0, 0, 0)
        };
        var settingsPanel = new StackPanel();
        settingsPanel.Children.Add(new TextBlock
        {
            Text = "Uygulanan ayarlar", FontFamily = new FontFamily("Segoe UI"), FontSize = 14, FontWeight = FontWeights.SemiBold,
            Foreground = B(C("#f5f5f5")), Margin = new Thickness(0, 0, 0, 2)
        });
        settingsPanel.Children.Add(new TextBlock
        {
            Text = "Her değişiklik yedeklenir ve geri alınabilir", FontFamily = new FontFamily("Segoe UI"), FontSize = 11,
            Foreground = B(C("#525252")), Margin = new Thickness(0, 0, 0, 12)
        });

        var appliedList = status.Where(s => s.Applied).ToList();
        if (appliedList.Count == 0)
        {
            settingsPanel.Children.Add(new TextBlock
            {
                Text = "Henüz optimizasyon uygulanmadı. \"Optimize Et\" ile başlayın.",
                FontFamily = new FontFamily("Segoe UI"), FontSize = 11, Foreground = B(C("#525252")),
                TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 12)
            });
        }
        else
        {
            foreach (var item in appliedList.Take(5))
                settingsPanel.Children.Add(MakeAppliedRow(item.Name));

            int remaining = appliedList.Count - 5;
            if (remaining > 0)
            {
                var moreLink = new TextBlock
                {
                    Text = $"+{remaining} ayar daha", FontFamily = new FontFamily("Segoe UI"), FontSize = 11,
                    Foreground = B(C("#3f3f3f")), Margin = new Thickness(0, 8, 0, 12), Cursor = Cursors.Hand
                };
                moreLink.MouseLeftButtonDown += (_, _) => app.SelectTab("gameperf");
                settingsPanel.Children.Add(moreLink);
            }
        }

        // Bottom row: buttons
        var btnRow = new StackPanel { Orientation = Orientation.Horizontal };

        // "Tümünü geri al" red outline button
        var revertBtn = new Button { Cursor = Cursors.Hand, Margin = new Thickness(0, 0, 12, 0) };
        var revertTemplate = new ControlTemplate(typeof(Button));
        var revertBorder = new FrameworkElementFactory(typeof(Border), "Bd");
        revertBorder.SetValue(Border.CornerRadiusProperty, new CornerRadius(6));
        revertBorder.SetValue(Border.PaddingProperty, new Thickness(14, 6, 14, 6));
        revertBorder.SetValue(Border.BackgroundProperty, Brushes.Transparent);
        revertBorder.SetValue(Border.BorderBrushProperty, B(C("#ef4444")));
        revertBorder.SetValue(Border.BorderThicknessProperty, new Thickness(1));
        var revertTxt = new FrameworkElementFactory(typeof(TextBlock));
        revertTxt.SetValue(TextBlock.TextProperty, "Tümünü geri al");
        revertTxt.SetValue(TextBlock.FontFamilyProperty, new FontFamily("Segoe UI"));
        revertTxt.SetValue(TextBlock.FontSizeProperty, 11.0);
        revertTxt.SetValue(TextBlock.FontWeightProperty, FontWeights.SemiBold);
        revertTxt.SetValue(TextBlock.ForegroundProperty, B(C("#ef4444")));
        revertBorder.AppendChild(revertTxt);
        revertTemplate.VisualTree = revertBorder;
        var revertHover = new Trigger { Property = UIElement.IsMouseOverProperty, Value = true };
        revertHover.Setters.Add(new Setter(Border.BackgroundProperty, B(C("#1a0a0a")), "Bd"));
        revertTemplate.Triggers.Add(revertHover);
        revertBtn.Template = revertTemplate;
        revertBtn.Click += (_, _) => app.RevertAllOptimizations();
        btnRow.Children.Add(revertBtn);

        // "Detayları gör" link
        var detailLink = new TextBlock
        {
            Text = "Detayları gör", FontFamily = new FontFamily("Segoe UI"), FontSize = 11,
            Foreground = B(C("#525252")), Cursor = Cursors.Hand, VerticalAlignment = VerticalAlignment.Center
        };
        detailLink.MouseEnter += (_, _) => detailLink.Foreground = B(C("#a3a3a3"));
        detailLink.MouseLeave += (_, _) => detailLink.Foreground = B(C("#525252"));
        detailLink.MouseLeftButtonDown += (_, _) => app.SelectTab("gameperf");
        btnRow.Children.Add(detailLink);
        settingsPanel.Children.Add(btnRow);

        settingsCard.Child = settingsPanel;
        Grid.SetColumn(settingsCard, 1);
        bottomGrid.Children.Add(settingsCard);

        Children.Add(bottomGrid);
    }

    private static string GetCpuName(HardwareSpecs specs)
    {
        try
        {
            using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0");
            var name = key?.GetValue("ProcessorNameString") as string;
            if (!string.IsNullOrEmpty(name)) return name.Trim();
        }
        catch { }
        return $"{specs.CpuCores} Core İşlemci";
    }

    private static StackPanel MakeSection(string label, string value, string sub)
    {
        var panel = new StackPanel { Margin = new Thickness(18, 14, 18, 14) };
        panel.Children.Add(new TextBlock
        {
            Text = label, FontFamily = new FontFamily("Segoe UI"), FontSize = 9, FontWeight = FontWeights.SemiBold,
            Foreground = B(C(MainWindow.CurrentAccentHex)), Margin = new Thickness(0, 0, 0, 6), Opacity = 0.7
        });
        panel.Children.Add(new TextBlock
        {
            Text = value, FontFamily = new FontFamily("Segoe UI"), FontSize = 13, FontWeight = FontWeights.SemiBold,
            Foreground = B(C("#f5f5f5")), TextTrimming = TextTrimming.CharacterEllipsis, Margin = new Thickness(0, 0, 0, 4)
        });
        panel.Children.Add(new TextBlock
        {
            Text = sub, FontFamily = new FontFamily("Segoe UI"), FontSize = 10, Foreground = B(C("#525252"))
        });
        return panel;
    }

    private static Border MakeDivider(int col)
    {
        var div = new Border { Width = 1, Background = B(C("#1e1e1e")), HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(0, 10, 0, 10) };
        Grid.SetColumn(div, col);
        return div;
    }

    private static Grid MakeAppliedRow(string text)
    {
        var row = new Grid { Margin = new Thickness(0, 0, 0, 5) };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        row.Children.Add(new TextBlock
        {
            Text = text, FontFamily = new FontFamily("Segoe UI"), FontSize = 11, Foreground = B(C("#a3a3a3")),
            VerticalAlignment = VerticalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis
        });

        var badge = new Border
        {
            Background = B(C("#14532d")), CornerRadius = new CornerRadius(4), Padding = new Thickness(8, 2, 8, 2),
            VerticalAlignment = VerticalAlignment.Center
        };
        badge.Child = new TextBlock
        {
            Text = "uygulandı", FontFamily = new FontFamily("Segoe UI"), FontSize = 9, FontWeight = FontWeights.Bold,
            Foreground = B(C("#4ade80"))
        };
        Grid.SetColumn(badge, 1);
        row.Children.Add(badge);

        return row;
    }

    private static Button MakePill(string text, Action? action)
    {
        var btn = new Button { Cursor = Cursors.Hand, Margin = new Thickness(0, 0, 8, 8) };
        var template = new ControlTemplate(typeof(Button));
        var border = new FrameworkElementFactory(typeof(Border), "Bd");
        border.SetValue(Border.CornerRadiusProperty, new CornerRadius(6));
        border.SetValue(Border.PaddingProperty, new Thickness(14, 8, 14, 8));
        border.SetValue(Border.BackgroundProperty, B(C("#161616")));
        border.SetValue(Border.BorderBrushProperty, B(C("#262626")));
        border.SetValue(Border.BorderThicknessProperty, new Thickness(1));

        var txt = new FrameworkElementFactory(typeof(TextBlock));
        txt.SetValue(TextBlock.TextProperty, text);
        txt.SetValue(TextBlock.FontFamilyProperty, new FontFamily("Segoe UI"));
        txt.SetValue(TextBlock.FontSizeProperty, 11.0);
        txt.SetValue(TextBlock.ForegroundProperty, B(C("#a3a3a3")));
        border.AppendChild(txt);

        template.VisualTree = border;
        var hover = new Trigger { Property = UIElement.IsMouseOverProperty, Value = true };
        hover.Setters.Add(new Setter(Border.BackgroundProperty, B(C("#1e1e1e")), "Bd"));
        hover.Setters.Add(new Setter(Border.BorderBrushProperty, B(C(MainWindow.CurrentAccentHex)), "Bd"));
        template.Triggers.Add(hover);

        btn.Template = template;
        if (action != null) btn.Click += (_, _) => action();
        return btn;
    }

    private static Button MakeGlowButton(string text)
    {
        var btn = new Button { MinWidth = 140, MinHeight = 42, Cursor = Cursors.Hand };
        var template = new ControlTemplate(typeof(Button));
        var border = new FrameworkElementFactory(typeof(Border), "Bd");
        border.SetValue(Border.CornerRadiusProperty, new CornerRadius(8));
        border.SetValue(Border.PaddingProperty, new Thickness(24, 10, 24, 10));
        border.SetValue(Border.BackgroundProperty, B(C("#16a34a")));

        var txt = new FrameworkElementFactory(typeof(TextBlock));
        txt.SetValue(TextBlock.TextProperty, text);
        txt.SetValue(TextBlock.FontFamilyProperty, new FontFamily("Segoe UI"));
        txt.SetValue(TextBlock.FontSizeProperty, 13.0);
        txt.SetValue(TextBlock.FontWeightProperty, FontWeights.SemiBold);
        txt.SetValue(TextBlock.ForegroundProperty, B(Colors.White));
        txt.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        border.AppendChild(txt);

        template.VisualTree = border;
        var hover = new Trigger { Property = UIElement.IsMouseOverProperty, Value = true };
        hover.Setters.Add(new Setter(Border.BackgroundProperty, B(C(MainWindow.CurrentAccentHex)), "Bd"));
        template.Triggers.Add(hover);

        btn.Template = template;
        return btn;
    }

    private static Color C(string hex) => (Color)ColorConverter.ConvertFromString(hex);
    private static readonly Dictionary<Color, SolidColorBrush> _brushCache = new();
    private static SolidColorBrush B(Color c)
    {
        if (!_brushCache.TryGetValue(c, out var b))
        {
            b = new SolidColorBrush(c);
            b.Freeze();
            _brushCache[c] = b;
        }
        return b;
    }
}
