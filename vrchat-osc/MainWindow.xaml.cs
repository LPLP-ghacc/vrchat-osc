using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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
        
        InitTextBoxes(6);
        InitEmojisButtons();
        
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

        _engine.AddModule(new MusicModule { IsEnabled = false }); //track 
        _engine.AddModule(new TimeModule { IsEnabled = false }); //time
        _engine.AddModule(new NetworkModule { IsEnabled = false }); //ping
        _engine.AddModule(new HardwareModule() { IsEnabled = false }); //hardware
        _engine.AddModule(new WindowActivityModule() { IsEnabled = false }); //window
        _engine.AddModule(new SoundpadModule() { IsEnabled = false }); //soundpad
        _engine.AddModule(new AfkModule() { IsEnabled = false }); //afk
        _engine.AddModule(new ProgressModule() { IsEnabled = false }); //progress
        
        _ = _engine.StartAsync(_cts.Token);
    }

    private void InitTextBoxes(int size)
    {
        for (var i = 0; i < size; i++)
        {
            var box = new OneStringTextBox(i + 1, string.Empty, (sender, _) =>
            {
                var box = sender as TextBox;
                Debug.Assert(box != null, nameof(box) + " != null");
                if (string.IsNullOrEmpty(box.Text)) return;

                SetText();
            });
            MainPanel.Children.Add(box);
        }
    }

    private void SetText()
    {
        var text = string.Empty;

        foreach (UIElement mainPanelChild in MainPanel.Children)
        {
            if (mainPanelChild is not OneStringTextBox mainTextBox) continue;
            if (!string.IsNullOrEmpty(mainTextBox.GetText()))
                text += mainTextBox.GetText() + Environment.NewLine;
        }

        if (text.Length > 144)
            text = text[..144];
        
        //set enable modules by keywords 
        _engine.Modules.ForEach(module => module.IsEnabled = text.Contains($"{{{module.Key}}}"));

        _engine.Template = text;
    }
    
    public static void Log(string message) => Console.WriteLine(message);
}