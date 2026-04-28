namespace vrchat_osc.Modules;

public class TwitchModule : BaseModule
{
    public override string Key => "viewers";

    public int Viewers { get; set; }

    public override Task<string> GetValueAsync()
        => Safe(() => Viewers > 0 ? Viewers.ToString() : "");
}