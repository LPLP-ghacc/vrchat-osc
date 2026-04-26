namespace vrchat_osc.Modules;

public class VrBatteryModule : BaseModule
{
    public override string Name => "VR Battery";

    public int Battery { get; set; } = 100;

    public override Task<string> GetTextAsync()
        => Safe(() => $"{Battery}%");
}