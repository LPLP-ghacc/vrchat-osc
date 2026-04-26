namespace vrchat_osc.Modules;

public class TwitchModule : BaseModule
{
    public override string Name => "Twitch";

    public int Viewers { get; set; }

    public override Task<string> GetTextAsync()
        => Safe(() => Viewers > 0 ? $"{Viewers} viewers" : "");
}