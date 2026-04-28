using System.Diagnostics;
using System.Text;

namespace vrchat_osc.Modules;

public class MusicModule : BaseModule
{
    public override string Key => "track";

    private readonly SemaphoreSlim _sync = new(1, 1);

    private DateTime _lastUpdate = DateTime.MinValue;
    private string _cached = "";

    private const int CacheSeconds = 2;
    private const int QueryTimeoutMs = 2500;

    public override async Task<string> GetValueAsync()
    {
        if ((DateTime.Now - _lastUpdate).TotalSeconds < CacheSeconds)
            return _cached;

        await _sync.WaitAsync();
        try
        {
            if ((DateTime.Now - _lastUpdate).TotalSeconds < CacheSeconds)
                return _cached;

            var current = await Task.Run(QueryNowPlaying);
            if (!string.IsNullOrWhiteSpace(current))
                _cached = current;

            _lastUpdate = DateTime.Now;
            return _cached;
        }
        catch
        {
            _lastUpdate = DateTime.Now;
            return _cached;
        }
        finally
        {
            _sync.Release();
        }
    }

    private static string QueryNowPlaying()
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{BuildPowerShellCommand()}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        process.Start();

        if (!process.WaitForExit(QueryTimeoutMs))
        {
            try
            {
                process.Kill(true);
            }
            catch
            {
                // ignore process termination failures
            }

            return "";
        }

        var output = process.StandardOutput.ReadToEnd().Trim();
        return process.ExitCode == 0 ? output : "";
    }

    private static string BuildPowerShellCommand()
    {
        const string script = """
            $ErrorActionPreference='Stop'
            Add-Type -AssemblyName System.Runtime.WindowsRuntime
            [Windows.Media.Control.GlobalSystemMediaTransportControlsSessionManager, Windows, ContentType=WindowsRuntime] > $null
            $managerType = [Windows.Media.Control.GlobalSystemMediaTransportControlsSessionManager, Windows, ContentType=WindowsRuntime]
            $propsType = [Windows.Media.Control.GlobalSystemMediaTransportControlsSessionMediaProperties, Windows, ContentType=WindowsRuntime]
            $asTask = [System.WindowsRuntimeSystemExtensions].GetMethods() | Where-Object {
                $_.Name -eq 'AsTask' -and $_.IsGenericMethodDefinition -and $_.GetGenericArguments().Count -eq 1 -and $_.GetParameters().Count -eq 1
            } | Select-Object -First 1
            if ($null -eq $asTask) { return }
            $managerTask = $asTask.MakeGenericMethod($managerType).Invoke($null, @($managerType::RequestAsync()))
            $manager = $managerTask.GetAwaiter().GetResult()
            if ($null -eq $manager) { return }
            $candidates = New-Object System.Collections.Generic.List[object]
            $readSession = {
                param($session, $isCurrent)
                if ($null -eq $session) { return }
                $propsTask = $asTask.MakeGenericMethod($propsType).Invoke($null, @($session.TryGetMediaPropertiesAsync()))
                $props = $propsTask.GetAwaiter().GetResult()
                if ($null -eq $props -or [string]::IsNullOrWhiteSpace($props.Title)) { return }
                $info = $session.GetPlaybackInfo()
                $artist = if ([string]::IsNullOrWhiteSpace($props.Artist)) { $props.AlbumArtist } else { $props.Artist }
                $subtitle = $props.Subtitle
                $score = switch ([int]$info.PlaybackStatus) { 4 { 100 } 5 { 40 } default { 10 } }
                if ($isCurrent) { $score += 25 }
                if (-not [string]::IsNullOrWhiteSpace($artist)) { $score += 10 }
                if (-not [string]::IsNullOrWhiteSpace($subtitle) -and $subtitle -ne $props.Title) { $score += 5 }
                $candidates.Add([pscustomobject]@{
                    Title = $props.Title.Trim()
                    Artist = if ($artist) { $artist.Trim() } else { '' }
                    Subtitle = if ($subtitle) { $subtitle.Trim() } else { '' }
                    Score = $score
                    AppId = $session.SourceAppUserModelId
                }) | Out-Null
            }
            & $readSession ($manager.GetCurrentSession()) $true
            foreach ($session in $manager.GetSessions()) { & $readSession $session $false }
            $best = $candidates |
                Sort-Object -Property @{Expression='Score';Descending=$true}, @{Expression='Title';Descending=$true} |
                Select-Object -First 1
            if ($null -eq $best) { return }
            if (-not [string]::IsNullOrWhiteSpace($best.Artist)) { [Console]::Write($best.Artist + ' - ' + $best.Title); return }
            if (-not [string]::IsNullOrWhiteSpace($best.Subtitle) -and $best.Subtitle -ne $best.Title) { [Console]::Write($best.Title + ' - ' + $best.Subtitle); return }
            [Console]::Write($best.Title)
            """;

        return script.Replace("\"", "\\\"").Replace(Environment.NewLine, " ");
    }
}
