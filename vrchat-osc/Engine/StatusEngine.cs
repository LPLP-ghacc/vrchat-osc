using vrchat_osc.Services;

namespace vrchat_osc;

public enum StatusMode
{
    Cycle,
    Combined,
    Manual,
    Template
}

public class StatusEngine(VrChatService vrChat)
{
    private readonly List<IStatusModule> _modules = [];

    private int _index = 0;

    public StatusMode Mode { get; set; } = StatusMode.Cycle;

    public string ManualText { get; set; } = "";
    public string PersistentText { get; set; } = "";

    public int DelayMs { get; init; } = 3000;
    public int MaxCombinedLength { get; set; } = 120;

    public string Template { get; set; } = "";
    
    public void AddModule(IStatusModule module)
    {
        _modules.Add(module);
    }

    public async Task StartAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            var text = Mode switch
            {
                StatusMode.Cycle => await BuildCycleText(),
                StatusMode.Combined => await BuildCombinedText(),
                StatusMode.Manual => ManualText,
                StatusMode.Template => await BuildTemplateText(), // ← ВОТ ЭТО
                _ => ""
            };

            if (!string.IsNullOrWhiteSpace(PersistentText))
            {
                text = $"{PersistentText}\n{text}";
            }

            if (!string.IsNullOrWhiteSpace(text))
            {
                await vrChat.SendChatMessageAsync(text);
            }

            await Task.Delay(DelayMs, token);
        }
    }

    private async Task<string> BuildCycleText()
    {
        var enabled = _modules.Where(m => m.IsEnabled).ToList();
        if (enabled.Count == 0) return "";

        var module = enabled[_index % enabled.Count];
        _index++;

        return await module.GetValueAsync();
    }

    private async Task<string> BuildCombinedText()
    {
        var enabled = _modules
            .Where(m => m.IsEnabled)
            .ToList();

        var parts = new List<string>();

        foreach (var module in enabled)
        {
            var value = await module.GetValueAsync();

            if (string.IsNullOrWhiteSpace(value))
                continue;

            parts.Add(value);

            var combined = string.Join(" | ", parts);

            if (combined.Length > MaxCombinedLength)
                break;
        }

        return string.Join(" | ", parts);
    }
    
    private async Task<string> BuildTemplateText()
    {
        if (string.IsNullOrWhiteSpace(Template))
            return "";

        var enabled = _modules.Where(m => m.IsEnabled);

        var result = Template;

        foreach (var module in enabled)
        {
            var value = await module.GetValueAsync();
            result = result.Replace($"{{{module.Key}}}", value ?? "");
        }

        return result;
    }
}
