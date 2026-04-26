namespace vrchat_osc.Modules;

public class HeartRateModule : BaseModule
{
    public override string Name => "HeartRate";

    public int BPM { get; set; }

    public override Task<string> GetTextAsync()
        => Safe(() => BPM > 0 ? $"{BPM} BPM" : "");
}