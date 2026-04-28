using System.Windows;
using System.Windows.Controls;
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
                
                if (propertyChangedEventArgs.PropertyName == nameof(UserSettings.RunAtWindowsStartup))
                    ApplyStartupSetting();
            };
        }
        catch (Exception ex)
        {
            ex.Message.Log();
        }
    }
    
    private void ApplyStartupSetting()
    {
        var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
        if (_settings!.RunAtWindowsStartup)
            key?.SetValue(App.APPNAME, $"\"{System.Reflection.Assembly.GetExecutingAssembly().Location}\"");
        else
            key?.DeleteValue(App.APPNAME, false);
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
}