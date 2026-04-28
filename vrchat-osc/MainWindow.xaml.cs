using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using vrchat_osc.Modules;
using vrchat_osc.Services;

namespace vrchat_osc;

public partial class MainWindow
{
    private readonly CancellationTokenSource _cts = new();

    public MainWindow()
    {
        InitializeComponent();

        var osc = new OscClient();
        var vr = new VrChatService(osc);

        var engine = new StatusEngine(vr)
        {
            Mode = StatusMode.Template,
            Template = "{time}\n{hardware}\n{track}",
            DelayMs = 3000
        };

        engine.AddModule(new MusicModule { IsEnabled = true }); //track 
        engine.AddModule(new TimeModule { IsEnabled = true }); //time
        engine.AddModule(new NetworkModule { IsEnabled = true }); //ping
        engine.AddModule(new HardwareModule() { IsEnabled = true }); //hardware
        engine.AddModule(new WindowActivityModule() { IsEnabled = true }); //window

        _ = engine.StartAsync(_cts.Token);
    }
}