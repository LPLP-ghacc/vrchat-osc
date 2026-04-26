namespace vrchat_osc.Modules;

public class TimeModule : BaseModule
{
    public override string Name => "Time";
    public override int Priority => 1;

    public override Task<string> GetTextAsync()
        => Safe(() => $"{DateTime.Now:HH:mm}");
}