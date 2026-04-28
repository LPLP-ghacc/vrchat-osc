namespace vrchat_osc.Modules;

public class VrBatteryModule : BaseModule
{
    public override string Key => "battery";

    public int Battery { get; set; } = 100;

    public override Task<string> GetValueAsync()
        => Safe(() => Battery.ToString());
}