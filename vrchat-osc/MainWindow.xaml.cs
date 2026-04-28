using System.Windows.Input;
using vrchat_osc.Modules;
using vrchat_osc.Services;

namespace vrchat_osc;

public partial class MainWindow
{
    public static MainWindow Instance { get; private set; } = null!;
    private readonly CancellationTokenSource _cts = new();
    
    private readonly StatusEngine _engine;
    
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

        _engine = new StatusEngine(vr)
        {
            Mode = StatusMode.Template,
            DelayMs = 3000
        };

        _engine.AddModule(new MusicModule { IsEnabled = true }); //track 
        _engine.AddModule(new TimeModule { IsEnabled = true }); //time
        _engine.AddModule(new NetworkModule { IsEnabled = true }); //ping
        _engine.AddModule(new HardwareModule() { IsEnabled = true }); //hardware
        _engine.AddModule(new WindowActivityModule() { IsEnabled = true }); //window
        _engine.AddModule(new SoundpadModule() { IsEnabled = true }); //soundpad
        _engine.AddModule(new AfkModule() { IsEnabled = true }); //afk
        
        _ = _engine.StartAsync(_cts.Token);
    }

    private void InputBox_KeyDown(object sender, KeyEventArgs e)
    {
        if(!string.IsNullOrEmpty(InputBox.Text))
            _engine.Template = InputBox.Text;
    }
    
    public static void Log(string message) => Console.WriteLine(message);
}