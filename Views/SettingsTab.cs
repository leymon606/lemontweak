using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using MoliTweak.Core;

namespace MoliTweak.Views;

public class SettingsTab : StackPanel
{
    public SettingsTab(MainWindow app, HardwareSpecs specs)
    {
        var mutedBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#525252"));
        var textBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f5f5f5"));
        var secondaryBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#a3a3a3"));
        var cardBg = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#161616"));
        var borderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#262626"));
        var accentBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(MainWindow.CurrentAccentHex));

        // ═══════════════════════════════════════════════════════════════
        // 1. TEMA RENGI (MINIMAL COLOR PALETTE DOTS WITH MICRO-ANIMATIONS)
        // ═══════════════════════════════════════════════════════════════
        var colorCard = new Border 
        { 
            Background = cardBg, 
            BorderBrush = borderBrush, 
            BorderThickness = new Thickness(1), 
            CornerRadius = new CornerRadius(8), 
            Padding = new Thickness(18, 14, 18, 14), 
            Margin = new Thickness(0, 0, 0, 8) 
        };

        var colorGrid = new Grid();
        colorGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        colorGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var colorInfo = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
        colorInfo.Children.Add(new TextBlock { Text = "Tema Rengi", FontFamily = new FontFamily("Segoe UI"), FontSize = 13, FontWeight = FontWeights.SemiBold, Foreground = textBrush });
        colorInfo.Children.Add(new TextBlock { Text = "Programın ana vurgu rengini değiştirin.", FontFamily = new FontFamily("Segoe UI"), FontSize = 11, Foreground = secondaryBrush, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 4, 16, 0) });
        Grid.SetColumn(colorInfo, 0);
        colorGrid.Children.Add(colorInfo);

        var colorSwatches = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
        string[] colorHexes = { "#22c55e", "#3b82f6", "#ef4444", "#a855f7" };
        string[] colorNames = { "Zümrüt Yeşili", "Okyanus Mavisi", "Yakut Kırmızısı", "Ametist Moru" };

        for (int i = 0; i < colorHexes.Length; i++)
        {
            int index = i;
            bool isSelected = (colorHexes[i] == MainWindow.CurrentAccentHex);

            var scaleTransform = new ScaleTransform(1.0, 1.0);
            var swatch = new Border
            {
                Width = 26,
                Height = 26,
                CornerRadius = new CornerRadius(13),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorHexes[i])),
                BorderThickness = new Thickness(isSelected ? 2.5 : 1),
                BorderBrush = isSelected ? Brushes.White : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333333")),
                Margin = new Thickness(4, 0, 4, 0),
                Cursor = Cursors.Hand,
                ToolTip = colorNames[i],
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = scaleTransform
            };

            swatch.MouseEnter += (s, e) => {
                var popAnim = new DoubleAnimation(1.18, TimeSpan.FromMilliseconds(150)) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
                scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, popAnim);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, popAnim);

                if (colorHexes[index] != MainWindow.CurrentAccentHex)
                    swatch.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#777777"));
            };
            swatch.MouseLeave += (s, e) => {
                var shrinkAnim = new DoubleAnimation(1.0, TimeSpan.FromMilliseconds(150)) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
                scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, shrinkAnim);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, shrinkAnim);

                if (colorHexes[index] != MainWindow.CurrentAccentHex)
                    swatch.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333333"));
            };
            swatch.MouseDown += (s, e) => {
                app.ChangeThemeColor(colorHexes[index]);
            };

            colorSwatches.Children.Add(swatch);
        }

        Grid.SetColumn(colorSwatches, 1);
        colorGrid.Children.Add(colorSwatches);
        colorCard.Child = colorGrid;
        Children.Add(colorCard);

        // ═══════════════════════════════════════════════════════════════
        // 2. ARKA PLAN EFEKTI (MINIMAL SEGMENTED PILLS WITH ANIMATION)
        // ═══════════════════════════════════════════════════════════════
        var bgCard = new Border 
        { 
            Background = cardBg, 
            BorderBrush = borderBrush, 
            BorderThickness = new Thickness(1), 
            CornerRadius = new CornerRadius(8), 
            Padding = new Thickness(18, 14, 18, 14), 
            Margin = new Thickness(0, 0, 0, 8) 
        };

        var bgGrid = new Grid();
        bgGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        bgGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var bgInfo = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
        bgInfo.Children.Add(new TextBlock { Text = "Arka Plan Efekti", FontFamily = new FontFamily("Segoe UI"), FontSize = 13, FontWeight = FontWeights.SemiBold, Foreground = textBrush });
        bgInfo.Children.Add(new TextBlock { Text = "Ana sayfadaki arka plan akıcılığını ve efektlerini değiştirin.", FontFamily = new FontFamily("Segoe UI"), FontSize = 11, Foreground = secondaryBrush, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 4, 16, 0) });
        Grid.SetColumn(bgInfo, 0);
        bgGrid.Children.Add(bgInfo);

        var bgPills = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
        string[] effectNames = { "Tümü", "Parlama", "Noktalar", "Kapalı" };
        var pillBorders = new List<Border>();

        for (int i = 0; i < effectNames.Length; i++)
        {
            int index = i;
            bool isCurrent = (index == MainWindow.CurrentBackgroundEffectIndex);

            var pill = new Border
            {
                Background = isCurrent ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#222222")) : Brushes.Transparent,
                BorderBrush = isCurrent ? accentBrush : borderBrush,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(5),
                Padding = new Thickness(11, 6, 11, 6),
                Margin = new Thickness(2, 0, 2, 0),
                Cursor = Cursors.Hand
            };
            var pillTxt = new TextBlock
            {
                Text = effectNames[i],
                FontSize = 11,
                FontFamily = new FontFamily("Segoe UI"),
                Foreground = isCurrent ? textBrush : secondaryBrush,
                FontWeight = isCurrent ? FontWeights.SemiBold : FontWeights.Normal
            };
            pill.Child = pillTxt;

            pill.MouseEnter += (s, e) => {
                if (index != MainWindow.CurrentBackgroundEffectIndex)
                {
                    pill.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1a1a1a"));
                    pillTxt.Foreground = textBrush;
                }
            };
            pill.MouseLeave += (s, e) => {
                if (index != MainWindow.CurrentBackgroundEffectIndex)
                {
                    pill.Background = Brushes.Transparent;
                    pillTxt.Foreground = secondaryBrush;
                }
            };
            pill.MouseDown += (s, e) => {
                if (MainWindow.CurrentBackgroundEffectIndex == index) return;
                app.SetBackgroundEffect(index);
                
                for (int j = 0; j < pillBorders.Count; j++)
                {
                    var p = pillBorders[j];
                    var t = (TextBlock)p.Child;
                    bool active = (j == index);
                    p.Background = active ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#222222")) : Brushes.Transparent;
                    p.BorderBrush = active ? new SolidColorBrush((Color)ColorConverter.ConvertFromString(MainWindow.CurrentAccentHex)) : borderBrush;
                    t.Foreground = active ? textBrush : secondaryBrush;
                    t.FontWeight = active ? FontWeights.SemiBold : FontWeights.Normal;
                }
            };

            pillBorders.Add(pill);
            bgPills.Children.Add(pill);
        }

        Grid.SetColumn(bgPills, 1);
        bgGrid.Children.Add(bgPills);
        bgCard.Child = bgGrid;
        Children.Add(bgCard);

        // ═══════════════════════════════════════════════════════════════
        // 3. GAME WATCHER
        // ═══════════════════════════════════════════════════════════════
        var watcherCard = new Border
        {
            Background = cardBg, BorderBrush = borderBrush, BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8), Padding = new Thickness(18, 14, 18, 14), Margin = new Thickness(0, 0, 0, 8)
        };
        var watcherGrid = new Grid();
        watcherGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        watcherGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var watcherInfo = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
        watcherInfo.Children.Add(new TextBlock
        {
            Text = "Game Watcher", FontFamily = new FontFamily("Segoe UI"), FontSize = 13, FontWeight = FontWeights.SemiBold, Foreground = textBrush
        });
        watcherInfo.Children.Add(new TextBlock
        {
            Text = "FiveM açıldığında otomatik olarak timer, RAM ve öncelik optimizasyonu uygular.",
            FontFamily = new FontFamily("Segoe UI"), FontSize = 11, Foreground = secondaryBrush,
            TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 4, 16, 0), LineHeight = 17
        });
        Grid.SetColumn(watcherInfo, 0);
        watcherGrid.Children.Add(watcherInfo);

        var chkWatcher = new CheckBox
        {
            Style = (Style)Application.Current.FindResource("DarkCheckBox"),
            Content = "Etkin",
            IsChecked = GameWatcher.Instance.IsRunning,
            VerticalAlignment = VerticalAlignment.Center
        };
        chkWatcher.Checked += (_, _) => GameWatcher.Instance.StartWatcher();
        chkWatcher.Unchecked += (_, _) => GameWatcher.Instance.StopWatcher();
        Grid.SetColumn(chkWatcher, 1);
        watcherGrid.Children.Add(chkWatcher);

        watcherCard.Child = watcherGrid;
        Children.Add(watcherCard);

        // ═══════════════════════════════════════════════════════════════
        // 4. CACHE CLEANING
        // ═══════════════════════════════════════════════════════════════
        Children.Add(CardBuilder.CreateActionCard(
            "Tüm Önbellekleri Temizle",
            "FiveM, GPU shader önbelleği ve Windows geçici dosyalarını tek tuşla temizler.",
            "Temizle",
            () => app.RunAsync(() => CacheCleaner.Instance.CleanAll()),
            "Önerilen"));

        Children.Add(CardBuilder.CreateActionCard(
            "GPU Shader Önbelleği",
            "DirectX ve Vulkan shader derleme önbelleğini siler. İlk yüklemede shader derleme süresi artar.",
            "Temizle",
            () => app.RunAsync(() => CacheCleaner.Instance.CleanGpuShaderCache())));

        // ═══════════════════════════════════════════════════════════════
        // 5. GUNCELLEME KONTROLU
        // ═══════════════════════════════════════════════════════════════
        var updateCard = new Border
        {
            Background = cardBg, BorderBrush = borderBrush, BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8), Padding = new Thickness(18, 14, 18, 14), Margin = new Thickness(0, 12, 0, 0)
        };
        var updateGrid = new Grid();
        updateGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        updateGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var updateInfo = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
        updateInfo.Children.Add(new TextBlock
        {
            Text = "Güncelleme Kontrolü", FontFamily = new FontFamily("Segoe UI"), FontSize = 13, FontWeight = FontWeights.SemiBold, Foreground = textBrush
        });
        var updateStatus = new TextBlock
        {
            Text = $"Mevcut sürüm: v{UpdateChecker.CurrentVersion}",
            FontFamily = new FontFamily("Segoe UI"), FontSize = 11, Foreground = secondaryBrush,
            Margin = new Thickness(0, 4, 16, 0)
        };
        updateInfo.Children.Add(updateStatus);
        Grid.SetColumn(updateInfo, 0);
        updateGrid.Children.Add(updateInfo);

        var updateBtn = new Button
        {
            Content = "Kontrol Et",
            Style = (Style)Application.Current.FindResource("ActionButtonOutline"),
            FontSize = 11, MinWidth = 100, MinHeight = 34, VerticalAlignment = VerticalAlignment.Center
        };
        updateBtn.Click += async (_, _) =>
        {
            updateStatus.Text = "Kontrol ediliyor...";
            updateStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f59e0b"));
            var info = await UpdateChecker.Instance.CheckAsync();
            if (info.Available)
            {
                updateStatus.Text = $"Yeni sürüm mevcut: v{info.LatestVersion} (mevcut: v{info.CurrentVersion})";
                updateStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f59e0b"));
            }
            else
            {
                updateStatus.Text = $"lemonistan güncel - v{info.CurrentVersion}";
                updateStatus.Foreground = accentBrush;
            }
        };
        Grid.SetColumn(updateBtn, 1);
        updateGrid.Children.Add(updateBtn);

        updateCard.Child = updateGrid;
        Children.Add(updateCard);

        // ═══════════════════════════════════════════════════════════════
        // 6. UYGULAMA BILGILERI
        // ═══════════════════════════════════════════════════════════════
        var aboutCard = new Border
        {
            Background = cardBg, BorderBrush = borderBrush, BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8), Padding = new Thickness(18, 16, 18, 16), Margin = new Thickness(0, 12, 0, 0)
        };
        var aboutPanel = new StackPanel();
        aboutPanel.Children.Add(new TextBlock
        {
            FontFamily = new FontFamily("Segoe UI"), FontSize = 16, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 0, 8),
            Inlines =
            {
                new System.Windows.Documents.Run("lemonistan") { Foreground = textBrush }
            }
        });

        aboutPanel.Children.Add(CreateInfoRow("Sürüm", "1.0.0"));
        aboutPanel.Children.Add(CreateInfoRow("Donanım", $"{(specs.GpuName)} - {(specs.TotalRamGb)} GB RAM - {(specs.CpuCores)}C"));
        aboutPanel.Children.Add(CreateInfoRow("Profil", $"{(specs.Tier.ToUpper())}-END"));
        aboutPanel.Children.Add(CreateInfoRow("HWID", HwidHelper.GetHwid()));

        aboutCard.Child = aboutPanel;
        Children.Add(aboutCard);
    }

    private static Grid CreateInfoRow(string label, string value)
    {
        var grid = new Grid { Margin = new Thickness(0, 3, 0, 3) };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var lbl = new TextBlock
        {
            Text = label, FontFamily = new FontFamily("Segoe UI"), FontSize = 11, FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#525252"))
        };
        Grid.SetColumn(lbl, 0);
        grid.Children.Add(lbl);

        var val = new TextBlock
        {
            Text = value, FontFamily = new FontFamily("Consolas"), FontSize = 11,
            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#a3a3a3")),
            TextWrapping = TextWrapping.Wrap
        };
        Grid.SetColumn(val, 1);
        grid.Children.Add(val);

        return grid;
    }
}