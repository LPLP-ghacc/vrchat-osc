namespace vrchat_osc.Modules;

public class SoundpadModule : BaseModule
{
    public override string Key => "sound";

    public string CurrentSound { get; set; } = "";

    public override Task<string> GetValueAsync()
        => Safe(() => CurrentSound);
}