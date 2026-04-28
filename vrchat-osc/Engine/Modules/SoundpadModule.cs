using System.Diagnostics;

namespace vrchat_osc.Modules;

public class SoundpadModule : BaseModule
{
    public override string Key => "soundpad";

    private static readonly string[] EmptyTitles =
    [
        "Soundpad",
        "Soundpad (trial)",
        "Soundpad - Soundpad"
    ];

    public override Task<string> GetValueAsync()
        => Safe(GetCurrentSound);

    private static string GetCurrentSound()
    {
        var process = Process
            .GetProcessesByName("Soundpad")
            .FirstOrDefault(static p => !string.IsNullOrWhiteSpace(p.MainWindowTitle));

        if (process == null)
            return "";

        var title = process.MainWindowTitle.Trim();
        if (string.IsNullOrWhiteSpace(title) || EmptyTitles.Contains(title, StringComparer.OrdinalIgnoreCase))
            return "";

        title = title.Replace(" - Soundpad", "", StringComparison.OrdinalIgnoreCase);
        title = title.Replace(" — Soundpad", "", StringComparison.OrdinalIgnoreCase);
        title = title.Trim();

        return EmptyTitles.Contains(title, StringComparer.OrdinalIgnoreCase) ? "" : title;
    }
}
