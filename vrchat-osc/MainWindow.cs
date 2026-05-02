using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using vrchat_osc.AppCore;
using vrchat_osc.Extensions;

namespace vrchat_osc;

public partial class MainWindow
{
    private readonly List<UIElement> _mainFieldElements = [];
    private UserSettings? _settings;
   
    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            _settings = await SettingsManager.Load();
        
            _settings.PropertyChanged += (_, propertyChangedEventArgs) =>
            {
                SettingsManager.Save(_settings);
                ShowNotificationButton();
                
            };

            if (SettingsPanel.Instance != null)
                SettingsManager.GenerateUI(SettingsPanel.Instance.UserSettingsField, _settings);
        }
        catch (Exception ex)
        {
            ex.Message.Log();
        }
    }

        private void InitEmojisButtons()
    {
        Emojis.Children.Clear();

        var categories = new Dictionary<string, string[]>
        {
            ["🙂 Basic"] =
            [
                "😀","😁","😂","🤣","😃","😄","😅","😆","😉","😊","😋","😎",
                "😍","😘","😗","😙","😚","🙂","🤗","🤩","🤔","🤨","😐","😑",
                "😶","🙄","😏","😣","😥","😮","🤐","😯","😪","😫","🥱","😴"
            ],

            ["❤️ Emotions"] =
            [
                "❤️","💔","💖","💘","💝","💞","💕","💓","💗","💙","💚","💛",
                "💜","🖤","🤍","🤎","💢","💥","💫","💦","💨","🫶","❣️","💟"
            ],

            ["👍 Gestures"] =
            [
                "👍","👎","👌","🤌","🤏","✌️","🤞","🤟","🤘","👏","🙌",
                "👐","🤲","🙏","👋","🤝","💪","🖕","✋","🤚","🫱","🫲"
            ],

            ["🔥 Popular ones"] =
            [
                "🔥","💀","👀","🎉","💯","✨","⚡","🚀","🎶","🎵",
                "🏆","🥇","🎯","📢","💡","📌","⭐","🌟","✔️","❌"
            ],

            ["😂 Reactions"] =
            [
                "😭","😡","😱","🥶","🥵","🤯","😳","🥴","🤮","🤢",
                "😈","👿","🤡","💩","👻","👽","🤖","😺","😸","😹"
            ],

            ["🐾 Animals"] =
            [
                "🐶","🐱","🐭","🐹","🐰","🦊","🐻","🐼","🐨","🐯",
                "🦁","🐸","🐵","🐧","🐦","🐤","🐺","🐗","🐴","🦄"
            ],

            ["🍔 Meal"] =
            [
                "🍎","🍌","🍇","🍓","🍒","🍑","🍍","🥝","🍅","🥑",
                "🍔","🍟","🍕","🌭","🍿","🥓","🍗","🍩","🍪","🎂"
            ],

            ["⚙️ Objects"] =
            [
                "⌚","📱","💻","⌨️","🖥️","🖨️","🖱️","💾","📷","📹",
                "🎧","📡","🔋","🔌","💡","🔦","🧯","🛠️","⚙️","🔧"
            ],

            ["🧠 Other"] =
            [
                "🧠","👁️","🗿","⚠️","❗","❓","🔴","🟢","🔵","🟡",
                "⬆️","⬇️","⬅️","➡️","🔺","🔻","◼️","◻️","▪️","▫️"
            ]
        };
        
        var block = new TextBlock
        {
            Text = "click on the emoji to copy",
            FontSize = 16,
            TextAlignment = TextAlignment.Center
        };
        Emojis.Children.Add(block);
        
        foreach (var category in categories)
        {
            var categoryPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(3)
            };

            var header = new TextBlock
            {
                Text = category.Key,
                FontSize = 14,
                Margin = new Thickness(3),
                Foreground = System.Windows.Media.Brushes.White,
                FontFamily = new FontFamily("Segoe UI Emoji")
            };

            categoryPanel.Children.Add(header);

            var wrap = new WrapPanel
            {
                Margin = new Thickness(3)
            };

            foreach (var emoji in category.Value)
            {
                var btn = new Button
                {
                    Content = emoji,
                    Width = 35,
                    Height = 35,
                    Margin = new Thickness(3),
                    FontSize = 18,
                    FontFamily = new FontFamily("Segoe UI Emoji")
                };

                btn.Click += (s, e) =>
                {
                    Clipboard.SetText(emoji);
                };

                wrap.Children.Add(btn);
            }
            
            categoryPanel.Children.Add(wrap);
            Emojis.Children.Add(categoryPanel);
        }
    }
    
    private void ShowNotificationButton() => NotificationButton.Visibility = Visibility.Visible;

    private void Minimize_OnClick_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void Fullscreen_Click(object sender, RoutedEventArgs e)
    {
        if (WindowState == WindowState.Maximized)
        {
            WindowState = WindowState.Normal;
            WindowStyle = WindowStyle.SingleBorderWindow;
            ResizeMode = ResizeMode.CanResizeWithGrip;
            FullscreenEnter.Visibility = Visibility.Visible;
            FullscreenExit.Visibility = Visibility.Collapsed;
        }
        else
        {
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            MaxWidth = SystemParameters.WorkArea.Width;
            MaxHeight = SystemParameters.WorkArea.Height;
            WindowState = WindowState.Maximized;

            FullscreenEnter.Visibility = Visibility.Collapsed;
            FullscreenExit.Visibility = Visibility.Visible;
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!_settings!.MinimizeToTrayOnClose)
            {
                //save settings here
                Environment.Exit(0);
            }
            else
            {
                WindowState = WindowState.Minimized;
                Hide();
            }
        }
        catch (Exception exception) { exception.Message.Log(); }
    }

    private void OpenUiElement(UIElement? obj, object sender)
    {
        var brush = ((Brush?)Application.Current.FindResource("SidebarButtonActiveBackground"))! ?? throw new InvalidOperationException();
        
        foreach (var element in _mainFieldElements)
        {
            if (element == obj)
            {
                element.Visibility = Visibility.Visible;
                if (element is UserControl uc) uc.IsEnabled = true;
            }
            else
            {
                element.Visibility = Visibility.Collapsed;
                if (element is UserControl uc) uc.IsEnabled = false;
            }
        }

        var buttons = new List<Button>()
        {
            MainCanvasShowButton,
            SettingsShowButton
        };

        foreach (var button in buttons)
        {
            var border = VisualTreeHelper.GetParent(button) as Border;

            border?.Background = button != sender ? Brushes.Transparent : brush;
        }
    }

    private void MainCanvasShowButton_OnClick(object sender, RoutedEventArgs e) => OpenUiElement(MainGrid, sender);

    private void NotificationButton_OnClick(object sender, RoutedEventArgs e)
    {
        var wind = new NotificationWindow("Applying the changes", "To apply the changed settings, the application needs to be restarted.",
            () =>
            {
                Application.Current.MainWindow!.Close();

                var window = new MainWindow();
                Application.Current.MainWindow = window;
                window.Show();
                
                NotificationButton.Visibility = Visibility.Hidden;
            },
            () =>
            {
                NotificationButton.Visibility = Visibility.Hidden;
            });
        
        wind.ShowDialog();
    }

    private void SettingsShowButton_OnClick(object sender, RoutedEventArgs e) => OpenUiElement(UserSettingsGrid, sender);

    private void TopBorder_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left) 
            DragMove();
    }
}