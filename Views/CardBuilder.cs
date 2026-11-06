using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace MoliTweak.Views;

public static class CardBuilder
{
    private static readonly SolidColorBrush NormalBorder = FreezeBrush(new SolidColorBrush(C("#1e1e1e")));
    private static readonly SolidColorBrush HoverBorder = FreezeBrush(new SolidColorBrush(C("#2a2a2a")));
    private static readonly SolidColorBrush NormalBg = FreezeBrush(new SolidColorBrush(C("#111111")));
    private static readonly SolidColorBrush HoverBg = FreezeBrush(new SolidColorBrush(C("#141414")));

    private static SolidColorBrush FreezeBrush(SolidColorBrush b) { b.Freeze(); return b; }

    public static Border CreateActionCard(string title, string description, string btnText, Action command,
        string? badgeText = null, string buttonStyle = "ActionButtonCyan")
    {
        var card = new Border
        {
            Background = NormalBg,
            BorderBrush = NormalBorder,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(18, 14, 18, 14),
            Margin = new Thickness(0, 3, 0, 3),
            Cursor = Cursors.Arrow
        };

        // Ultra-fast hover with zero allocation
        card.MouseEnter += (_, _) => { card.BorderBrush = HoverBorder; card.Background = HoverBg; };
        card.MouseLeave += (_, _) => { card.BorderBrush = NormalBorder; card.Background = NormalBg; };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var infoPanel = new StackPanel { VerticalAlignment = VerticalAlignment.Center };

        var headerPanel = new StackPanel { Orientation = Orientation.Horizontal };
        headerPanel.Children.Add(new TextBlock
        {
            Text = title,
            FontFamily = new FontFamily("Segoe UI"), FontSize = 12.5, FontWeight = FontWeights.SemiBold,
            Foreground = (SolidColorBrush)Application.Current.FindResource("TextPrimaryBrush"),
            VerticalAlignment = VerticalAlignment.Center
        });

        if (badgeText != null)
        {
            var (bgColor, fgColor) = GetBadgeColors(badgeText);
            var badge = new Border
            {
                Background = new SolidColorBrush(bgColor),
                CornerRadius = new CornerRadius(5),
                Padding = new Thickness(7, 2, 7, 2),
                Margin = new Thickness(8, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            badge.Child = new TextBlock
            {
                Text = badgeText, FontFamily = new FontFamily("Segoe UI"), FontSize = 9, FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(fgColor)
            };
            headerPanel.Children.Add(badge);
        }

        infoPanel.Children.Add(headerPanel);
        infoPanel.Children.Add(new TextBlock
        {
            Text = description,
            FontFamily = new FontFamily("Segoe UI"), FontSize = 11,
            Foreground = (SolidColorBrush)Application.Current.FindResource("TextSecondaryBrush"),
            TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 5, 16, 0), LineHeight = 17
        });

        Grid.SetColumn(infoPanel, 0);
        grid.Children.Add(infoPanel);

        var btn = new Button
        {
            Content = btnText,
            Style = (Style)Application.Current.FindResource(buttonStyle),
            VerticalAlignment = VerticalAlignment.Center,
            MinWidth = 115
        };
        btn.Click += (_, _) => command();
        Grid.SetColumn(btn, 1);
        grid.Children.Add(btn);

        card.Child = grid;
        return card;
    }

    public static Border CreateStatCard(string title, string value, string subtext, string accentColorKey = "LiquidCyanBrush")
    {
        var card = new Border
        {
            Background = new SolidColorBrush(C("#111111")),
            BorderBrush = new SolidColorBrush(C("#1e1e1e")),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(16, 14, 16, 14),
            MinWidth = 180
        };

        var panel = new StackPanel();
        panel.Children.Add(new TextBlock
        {
            Text = title.ToUpper(), FontFamily = new FontFamily("Segoe UI"), FontSize = 9, FontWeight = FontWeights.SemiBold,
            Foreground = (SolidColorBrush)Application.Current.FindResource("TextMutedBrush"),
            Margin = new Thickness(0, 0, 0, 4)
        });
        panel.Children.Add(new TextBlock
        {
            Text = value, FontFamily = new FontFamily("Segoe UI"), FontSize = 22, FontWeight = FontWeights.Bold,
            Foreground = (SolidColorBrush)Application.Current.FindResource(accentColorKey),
            Margin = new Thickness(0, 0, 0, 4)
        });
        if (!string.IsNullOrEmpty(subtext))
        {
            panel.Children.Add(new TextBlock
            {
                Text = subtext, FontFamily = new FontFamily("Segoe UI"), FontSize = 10,
                Foreground = (SolidColorBrush)Application.Current.FindResource("TextSecondaryBrush")
            });
        }

        card.Child = panel;
        return card;
    }

    private static (Color bg, Color fg) GetBadgeColors(string badgeText)
    {
        if (badgeText.Contains("Kritik") || badgeText.Contains("Maksimum") || badgeText.Contains("İsteğe"))
            return (C("#4c0519"), C("#fda4af"));
        if (badgeText.Contains("Önerilen") || badgeText.Contains("Hesap") || badgeText.Contains("0 Input") || badgeText.Contains("Dengeli") || badgeText.Contains("Ultra Netlik"))
            return (C("#14532d"), C("#86efac"));
        if (badgeText.Contains("Crash") || badgeText.Contains("Lag") || badgeText.Contains("Stutter"))
            return (C("#3b0764"), C("#c4b5fd"));
        if (badgeText.Contains("Güvenlik"))
            return (C("#451a03"), C("#fde68a"));
        return (C("#1e3a5f"), C("#93c5fd"));
    }

    private static Color C(string hex) => (Color)ColorConverter.ConvertFromString(hex);
}
