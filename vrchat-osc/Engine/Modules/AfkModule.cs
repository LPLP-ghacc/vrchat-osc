using vrchat_osc.Services;

namespace vrchat_osc.Modules;

public class AfkModule : BaseModule
{
    public override string Name => "AFK";
    public bool IsAfk { get; set; }

    public override Task<string> GetTextAsync()
        => Safe(() => IsAfk ? "💤 AFK" : "");
}