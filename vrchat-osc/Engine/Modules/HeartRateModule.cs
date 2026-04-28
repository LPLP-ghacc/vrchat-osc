namespace vrchat_osc.Modules;

public class HeartRateModule : BaseModule
{
    public override string Key => "bpm";

    public int Bpm { get; set; }

    public override Task<string> GetValueAsync()
        => Safe(() => Bpm > 0 ? Bpm.ToString() : "");
}