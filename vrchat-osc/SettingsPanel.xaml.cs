using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace vrchat_osc;

public partial class SettingsPanel
{
    public SettingsPanel()
    {
        InitializeComponent();
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