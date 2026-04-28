using System.Net.NetworkInformation;
using vrchat_osc.Services;

namespace vrchat_osc.Modules;

using System.Net.NetworkInformation;

public class NetworkModule : BaseModule
{
    public override string Key => "ping";

    private readonly Ping _ping = new();
    private DateTime _lastUpdate = DateTime.MinValue;
    private string _cachedValue = "";

    private const int CacheSeconds = 3;
    private const int TimeoutMs = 1000;

    private readonly string[] _hosts =
    [
        "1.1.1.1",   // Cloudflare
        "8.8.8.8",   // Google
        "google.com" // fallback DNS
    ];

    public override async Task<string> GetValueAsync()
    {
        if ((DateTime.Now - _lastUpdate).TotalSeconds < CacheSeconds)
            return _cachedValue;

        foreach (var host in _hosts)
        {
            try
            {
                var reply = await _ping.SendPingAsync(host, TimeoutMs);

                if (reply is not { Status: IPStatus.Success, RoundtripTime: > 0 }) continue;
                _cachedValue = reply.RoundtripTime.ToString();
                _lastUpdate = DateTime.Now;
                return _cachedValue;
            }
            catch
            {
                // ignored
            }
        }
        
        return string.IsNullOrEmpty(_cachedValue) ? "-" : _cachedValue;
    }
}