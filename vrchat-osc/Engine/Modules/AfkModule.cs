using System.IO;
using System.Text.RegularExpressions;

namespace vrchat_osc.Modules;

public class AfkModule : BaseModule
{
    public override string Key => "afk";

    private static readonly Regex AfkLineRegex = new(
        @"AFK enabled:\s*(True|False)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private readonly SemaphoreSlim _sync = new(1, 1);

    private DateTime _lastUpdateUtc = DateTime.MinValue;
    private string _cached = "";

    private const int CacheSeconds = 2;

    public override async Task<string> GetValueAsync()
    {
        if ((DateTime.UtcNow - _lastUpdateUtc).TotalSeconds < CacheSeconds)
            return _cached;

        await _sync.WaitAsync();
        try
        {
            if ((DateTime.UtcNow - _lastUpdateUtc).TotalSeconds < CacheSeconds)
                return _cached;

            _cached = await Safe(ReadAfkState);
            _lastUpdateUtc = DateTime.UtcNow;
            return _cached;
        }
        finally
        {
            _sync.Release();
        }
    }

    private static string ReadAfkState()
    {
        var logPath = GetLatestVrChatLogPath();
        if (string.IsNullOrWhiteSpace(logPath) || !File.Exists(logPath))
            return "";

        var latestState = TryReadLatestAfkState(logPath);
        return latestState == true ? "AFK" : "";
    }

    private static string? GetLatestVrChatLogPath()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrWhiteSpace(localAppData))
            return null;

        var logDirectory = Path.GetFullPath(Path.Combine(localAppData, "..", "LocalLow", "VRChat", "VRChat"));
        if (!Directory.Exists(logDirectory))
            return null;

        return Directory
            .EnumerateFiles(logDirectory, "output_log_*.txt", SearchOption.TopDirectoryOnly)
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault();
    }

    private static bool? TryReadLatestAfkState(string logPath)
    {
        using var stream = new FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        using var reader = new StreamReader(stream);

        bool? latestState = null;
        while (reader.ReadLine() is { } line)
        {
            var match = AfkLineRegex.Match(line);
            if (!match.Success)
                continue;

            latestState = bool.Parse(match.Groups[1].Value);
        }

        return latestState;
    }
}
