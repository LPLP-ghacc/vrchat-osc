using vrchat_osc.Services;

namespace vrchat_osc;

public enum StatusMode
{
    Manual,
    Template
}

public class StatusEngine(VrChatService vrChat)
{
    public readonly List<IStatusModule> Modules = [];

    public StatusMode Mode { get; init; } = StatusMode.Template;

    private static string ManualText => "";

    public int DelayMs { get; init; } = 3000;

    public string Template { get; set; } = "";
    
    public void AddModule(IStatusModule module)
    {
        Modules.Add(module);
    }

    public async Task StartAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            var text = Mode switch
            {
                StatusMode.Manual => ManualText,
                StatusMode.Template => await BuildTemplateText(),
                _ => ""
            };

            if (!string.IsNullOrWhiteSpace(text))
            {
                await vrChat.SendChatMessageAsync(text);
            }

            await Task.Delay(DelayMs, token);
        }
    }
    
    private async Task<string> BuildTemplateText()
    {
        var enabled = Modules.Where(m => m.IsEnabled);

        var result = Template;

        foreach (var module in enabled)
        {
            var value = await module.GetValueAsync();
            result = result.Replace($"{{{module.Key}}}", value ?? "").Trim();
        }
        if (result.Length > 144)
            result = result[..144];
        
        MainWindow.Instance.ActualText.Text = result.Trim();
        return result;
    }
}
