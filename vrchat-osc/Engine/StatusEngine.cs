using vrchat_osc.Services;

namespace vrchat_osc;

public enum StatusMode
{
    Cycle,
    Combined,
    Manual
}

public class StatusEngine(VrChatService vrChat)
{
    private readonly List<IStatusModule> _modules = [];

    private int _index = 0;

    public StatusMode Mode { get; set; } = StatusMode.Cycle;

    public string ManualText { get; set; } = "";
    public string PersistentText { get; set; } = "";

    public int DelayMs { get; set; } = 3000;
    public int MaxCombinedLength { get; set; } = 120;

    public void AddModule(IStatusModule module)
    {
        _modules.Add(module);
    }

    public async Task StartAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            string text = Mode switch
            {
                StatusMode.Cycle => await BuildCycleText(),
                StatusMode.Combined => await BuildCombinedText(),
                StatusMode.Manual => ManualText,
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

        return await module.GetTextAsync();
    }

    private async Task<string> BuildCombinedText()
    {
        var enabled = _modules
            .Where(m => m.IsEnabled)
            .OrderBy(m => m.Priority)
            .ToList();

        var parts = new List<string>();

        foreach (var module in enabled)
        {
            var text = await module.GetTextAsync();

            if (string.IsNullOrWhiteSpace(text))
                continue;

            parts.Add(text);

            var combined = string.Join(" | ", parts);

            if (combined.Length > MaxCombinedLength)
                break;
        }

        return string.Join(" | ", parts);
    }
}