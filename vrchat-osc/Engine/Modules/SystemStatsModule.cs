using System.Diagnostics;

namespace vrchat_osc.Modules;


public class SystemStatsModule : BaseModule
{
    public override string Name => "System";

    public override Task<string> GetTextAsync()
        => Safe(() =>
        {
            var proc = Process.GetCurrentProcess();
            var ram = proc.WorkingSet64 / 1024 / 1024;

            return $"🎛️ RAM {ram}MB";
        });
}