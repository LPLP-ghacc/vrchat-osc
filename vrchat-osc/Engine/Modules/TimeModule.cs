namespace vrchat_osc.Modules;

public class TimeModule : BaseModule
{
    public override string Key => "time";

    public override Task<string> GetValueAsync()
        => Safe(() => DateTime.Now.ToString("HH:mm"));
}