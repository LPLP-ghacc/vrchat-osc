namespace vrchat_osc.Modules;

public class RadarModule : BaseModule
{
    public override string Name => "Radar";

    public int NearbyUsers { get; set; }

    public override Task<string> GetTextAsync()
        => Safe(() => $"📶 {NearbyUsers} nearby");
}