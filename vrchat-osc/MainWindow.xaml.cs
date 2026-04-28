using vrchat_osc.Modules;
using vrchat_osc.Services;

namespace vrchat_osc;

public partial class MainWindow
{
    public static MainWindow Instance { get; private set; } = null!;
    private readonly CancellationTokenSource _cts = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
        Instance = this;
        
        Loaded += OnLoaded;
        
        _mainFieldElements.Add(MainGrid);
        _mainFieldElements.Add(UserSettingsGrid);
        OpenUiElement(MainGrid, MainCanvasShowButton);

        var osc = new OscClient();
        var vr = new VrChatService(osc);

        var engine = new StatusEngine(vr)
        {
            Mode = StatusMode.Template,
            Template = "пишу OSC для врчата\n{afk}",
            DelayMs = 3000
        };

        engine.AddModule(new MusicModule { IsEnabled = true }); //track 
        engine.AddModule(new TimeModule { IsEnabled = true }); //time
        engine.AddModule(new NetworkModule { IsEnabled = true }); //ping
        engine.AddModule(new HardwareModule() { IsEnabled = true }); //hardware
        engine.AddModule(new WindowActivityModule() { IsEnabled = true }); //window
        engine.AddModule(new SoundpadModule() { IsEnabled = true }); //window
        engine.AddModule(new AfkModule() { IsEnabled = true }); //afk
        
        _ = engine.StartAsync(_cts.Token);
    }

    public void Log(string message)
    {
        Console.WriteLine(message);

        if (_settings is { IsEnableLog: false }) return;
    }
}