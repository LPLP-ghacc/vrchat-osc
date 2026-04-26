using System.Net.NetworkInformation;
using vrchat_osc.Services;

namespace vrchat_osc.Modules;

using System.Net.NetworkInformation;

public class NetworkModule : BaseModule
{
    public override string Name => "Network";

    public override async Task<string> GetTextAsync()
    {
        try
        {
            var ping = new Ping();
            var reply = await ping.SendPingAsync("8.8.8.8");
            return $"{reply.RoundtripTime}ms";
        }
        catch
        {
            return "";
        }
    }
}