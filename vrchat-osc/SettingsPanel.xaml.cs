using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace vrchat_osc;

public partial class SettingsPanel
{
    public static SettingsPanel? Instance;

    public SettingsPanel()
    {
        InitializeComponent();

        Instance = this;
    }

    private void OpenGitHub(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "https://github.com/LPLP-ghacc/vrchat-osc",
            UseShellExecute = true
        });
    }
}