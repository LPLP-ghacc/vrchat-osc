namespace vrchat_osc.Modules;

public class SoundpadModule : BaseModule
{
    public override string Name => "Soundpad";

    public string CurrentSound { get; set; } = "";

    public override Task<string> GetTextAsync()
        => Safe(() => string.IsNullOrEmpty(CurrentSound) ? "" : $"{CurrentSound}");
}