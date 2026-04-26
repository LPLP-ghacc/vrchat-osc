using vrchat_osc.Services;

namespace vrchat_osc.Modules;

public class MusicModule : BaseModule
{
    public override string Name => "Music";

    public string CurrentTrack { get; set; } = "";

    public override Task<string> GetTextAsync()
        => Safe(() => string.IsNullOrEmpty(CurrentTrack) ? "" : $"🎶 {CurrentTrack}");
}