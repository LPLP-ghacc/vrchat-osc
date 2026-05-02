using System.Diagnostics;
using System.Text;

namespace vrchat_osc.Modules;

public class ProgressModule : BaseModule
{
    public override string Key => "progress";

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

            var current = await Task.Run(QueryProgress);
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

    private static string QueryProgress()
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{BuildPowerShellCommand()}\"",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8
        };

        process.Start();

        if (!process.WaitForExit(QueryTimeoutMs))
        {
            try { process.Kill(true); }
            catch
            {
                // ignored
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
                              
                              $managerType = [Windows.Media.Control.GlobalSystemMediaTransportControlsSessionManager, Windows, ContentType=WindowsRuntime]
                              
                              $asTask = [System.WindowsRuntimeSystemExtensions].GetMethods() | Where-Object {
                                  $_.Name -eq 'AsTask' -and $_.IsGenericMethodDefinition
                              } | Select-Object -First 1
                              
                              $managerTask = $asTask.MakeGenericMethod($managerType).Invoke($null, @($managerType::RequestAsync()))
                              $manager = $managerTask.GetAwaiter().GetResult()
                              
                              if ($null -eq $manager) { return }
                              
                              $session = $manager.GetCurrentSession()
                              if ($null -eq $session) { return }
                              
                              $timeline = $session.GetTimelineProperties()
                              if ($null -eq $timeline) { return }
                              
                              $pos = $timeline.Position
                              $max = $timeline.EndTime
                              
                              function Format-Time($t) {
                                  return "{0:D2}:{1:D2}" -f $t.Minutes, $t.Seconds
                              }
                              
                              if ($max.TotalSeconds -le 0) { return }
                              
                              $result = (Format-Time($pos)) + " / " + (Format-Time($max))
                              [Console]::Write($result)
                              """;

        return script
            .Replace("\"", "\\\"")
            .Replace(Environment.NewLine, "; ");
    }
}