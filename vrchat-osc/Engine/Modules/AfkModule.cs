using vrchat_osc.Services;

namespace vrchat_osc.Modules;

public class AfkModule : BaseModule
{
    public override string Key => "afk";

    public bool IsAfk { get; set; }

    public override Task<string> GetValueAsync()
        => Safe(() => IsAfk ? "AFK" : "");
}