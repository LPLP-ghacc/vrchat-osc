using System.Text.RegularExpressions;
using LibreHardwareMonitor.Hardware;

namespace vrchat_osc.Modules;

public record SensorPair(SensorType Type, string Name);

public class SensorInfo
{
    public readonly List<SensorPair> Pairs = [];

    public SensorInfo(SensorType type, params string[] names)
    {
        foreach (var name in names) Pairs.Add(new SensorPair(type, name));
    }
}

public abstract class HardwareComponent
{
    protected virtual SensorInfo? LoadInfo => null;

    public required string Name { get; init; }
    public float Usage { get; private set; }

    protected static bool GetIntValue(ISensor sensor, SensorInfo? info, out int value)
    {
        if (GetFloatValue(sensor, info, out var floatValue))
        {
            value = (int)MathF.Round(floatValue);
            return true;
        }

        value = 0;
        return false;
    }

    protected static bool GetFloatValue(ISensor sensor, SensorInfo? info, out float value)
    {
        if (info is null)
        {
            value = 0f;
            return false;
        }

        foreach (var innerValue in from pair in info.Pairs let innerValue = sensor.Value.GetValueOrDefault(0f) 
                 where sensor.SensorType == pair.Type && sensor.Name == pair.Name && innerValue != 0f select innerValue)
        {
            value = innerValue;
            return true;
        }

        value = 0f;
        return false;
    }

    public virtual void Update(ISensor sensor)
    {
        if (GetFloatValue(sensor, LoadInfo, out var loadValue)) Usage = loadValue;
    }
}

public abstract class Cpu : HardwareComponent
{
    protected override SensorInfo LoadInfo => new(SensorType.Load, "CPU Total");
    protected virtual SensorInfo? PowerInfo => null;
    protected virtual SensorInfo? TemperatureInfo => null;

    public int Power { get; private set; }
    public int Temperature { get; private set; }

    public override void Update(ISensor sensor)
    {
        base.Update(sensor);
        if (GetIntValue(sensor, PowerInfo, out var powerValue)) Power = powerValue;
        if (GetIntValue(sensor, TemperatureInfo, out var temperatureValue)) Temperature = temperatureValue;
    }
}

public class IntelCpu : Cpu
{
    protected override SensorInfo PowerInfo => new(SensorType.Power, "CPU Package");
    protected override SensorInfo TemperatureInfo => new(SensorType.Temperature, "CPU Package");
}

public class AmdСpu : Cpu
{
    protected override SensorInfo PowerInfo => new(SensorType.Power, "Package");
    protected override SensorInfo TemperatureInfo => new(SensorType.Temperature, "Core (Tdie)", "Core (Tctl/Tdie)", "CPU Cores");
}

public abstract class Gpu : HardwareComponent
{
    protected override SensorInfo LoadInfo => new(SensorType.Load, "GPU Core");
    private readonly SensorInfo _powerInfo = new(SensorType.Power, "GPU Package");
    private readonly SensorInfo _temperatureInfo = new(SensorType.Temperature, "GPU Core");
    private readonly SensorInfo _memoryFreeInfo = new(SensorType.SmallData, "GPU Memory Free");
    private readonly SensorInfo _memoryUsedInfo = new(SensorType.SmallData, "GPU Memory Used", "D3D Dedicated Memory Used");
    private readonly SensorInfo _memoryTotalInfo = new(SensorType.SmallData, "GPU Memory Total");

    public int Power { get; private set; }
    public int Temperature { get; private set; }
    public float MemoryFree { get; private set; }
    private float MemoryUsed { get; set; }
    private float MemoryTotal { get; set; }
    public float MemoryUsage => MemoryTotal == 0f ? 0f : MemoryUsed / MemoryTotal;

    public override void Update(ISensor sensor)
    {
        base.Update(sensor);
        if (GetIntValue(sensor, _powerInfo, out var powerValue)) Power = powerValue;
        if (GetIntValue(sensor, _temperatureInfo, out var temperatureValue)) Temperature = temperatureValue;
        if (GetFloatValue(sensor, _memoryFreeInfo, out var memoryFreeValue)) MemoryFree = memoryFreeValue;
        if (GetFloatValue(sensor, _memoryUsedInfo, out var memoryUsedValue)) MemoryUsed = memoryUsedValue;
        if (GetFloatValue(sensor, _memoryTotalInfo, out var memoryTotalValue)) MemoryTotal = memoryTotalValue;
    }
}

public class NvidiaGpu : Gpu
{
}

public class Amdgpu : Gpu
{
}

public class Ram : HardwareComponent
{
    protected override SensorInfo LoadInfo => new(SensorType.Load, "Memory");
    private readonly SensorInfo _memoryUsedInfo = new(SensorType.Data, "Memory Used");
    private readonly SensorInfo _memoryAvailableInfo = new(SensorType.Data, "Memory Available");

    public float Used { get; private set; }
    private float Available { get; set; }
    public float Total => Used + Available;

    public override void Update(ISensor sensor)
    {
        base.Update(sensor);
        if (GetFloatValue(sensor, _memoryUsedInfo, out var memoryUsedValue)) Used = memoryUsedValue;
        if (GetFloatValue(sensor, _memoryAvailableInfo, out var memoryAvailableValue)) Available = memoryAvailableValue;
    }
}

public sealed partial class HardwareStatsProvider
{
    private readonly Computer _computer = new()
    {
        IsCpuEnabled = true,
        IsGpuEnabled = true,
        IsMemoryEnabled = true
    };

    private readonly Regex _hardwareIdRegex = HardwareIdRegex();
    private readonly Regex _sensorIdRegex = SensorIdRegex();

    public bool CanAcceptQueries { get; private set; }

    public readonly Dictionary<int, Cpu> Cpus = new();
    public readonly Dictionary<int, Gpu> Gpus = new();
    public Ram? Ram { get; private set; }

    public void Init()
    {
        _computer.Open();
        CanAcceptQueries = true;
    }

    public void Shutdown()
    {
        CanAcceptQueries = false;
        Cpus.Clear();
        Gpus.Clear();
        Ram = null;

        _computer.Close();
    }

    public void Update()
    {
        foreach (var hardware in _computer.Hardware)
        {
            UpdateHardware(hardware);
            AuditHardware(hardware);

            foreach (var sensor in hardware.Sensors)
            {
                var identifier = sensor.Identifier.ToString()!;

                if (identifier.Contains("ram", StringComparison.InvariantCultureIgnoreCase))
                {
                    Ram?.Update(sensor);
                    continue;
                }

                var sensorIdMatch = _sensorIdRegex.Match(identifier);
                if (!sensorIdMatch.Success) continue;

                var sensorId = int.Parse(sensorIdMatch.Groups[1].Value);

                if (identifier.Contains("cpu", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (Cpus.TryGetValue(sensorId, out var cpu))
                        cpu.Update(sensor);

                    continue;
                }

                if (!identifier.Contains("gpu", StringComparison.InvariantCultureIgnoreCase)) continue;
                if (Gpus.TryGetValue(sensorId, out var gpu))
                    gpu.Update(sensor);
            }
        }
    }

    private static void UpdateHardware(IHardware hardware)
    {
        hardware.Update();
    
        foreach (var sub in hardware.SubHardware)
        {
            UpdateHardware(sub);
        }
    }

    private void AuditHardware(IHardware hardware)
    {
        var identifier = hardware.Identifier.ToString()!;

        if (identifier.Contains("ram", StringComparison.InvariantCultureIgnoreCase))
        {
            Ram ??= new Ram
            {
                Name = string.Empty
            };
            return;
        }

        var hardwareIdMatch = _hardwareIdRegex.Match(identifier);
        if (!hardwareIdMatch.Success) return;

        var hardwareId = int.Parse(hardwareIdMatch.Groups[1].Value);

        if (identifier.Contains("cpu", StringComparison.InvariantCultureIgnoreCase))
        {
            if (identifier.Contains("intel", StringComparison.InvariantCultureIgnoreCase))
            {
                Cpus.TryAdd(hardwareId, new IntelCpu
                {
                    Name = hardware.Name
                });
                return;
            }

            if (identifier.Contains("amd", StringComparison.InvariantCultureIgnoreCase))
            {
                Cpus.TryAdd(hardwareId, new AmdСpu
                {
                    Name = hardware.Name
                });
                return;
            }
        }

        if (!identifier.Contains("gpu", StringComparison.InvariantCultureIgnoreCase)) return;
        if (identifier.Contains("nvidia", StringComparison.InvariantCultureIgnoreCase))
        {
            Gpus.TryAdd(hardwareId, new NvidiaGpu
            {
                Name = hardware.Name
            });
            return;
        }

        if (identifier.Contains("amd", StringComparison.InvariantCultureIgnoreCase))
        {
            Gpus.TryAdd(hardwareId, new Amdgpu
            {
                Name = hardware.Name
            });
        }
    }

    [GeneratedRegex(".+/([0-9])")]
    private static partial Regex HardwareIdRegex();
    [GeneratedRegex(".+/([0-9])/.+")]
    private static partial Regex SensorIdRegex();
}

public class HardwareModule : BaseModule
{
    public override string Key => "hardware";

    private readonly HardwareStatsProvider _provider = new();
    private DateTime _lastUpdate = DateTime.MinValue;

    public HardwareModule()
    {
        _provider.Init();
    }

    public override Task<string> GetValueAsync()
    {
        if (!_provider.CanAcceptQueries)
            return Task.FromResult("");
    
        if ((DateTime.Now - _lastUpdate).TotalSeconds > 2)
        {
            _provider.Update();
            _lastUpdate = DateTime.Now;
        }
    
        try
        {
            var cpu = _provider.Cpus.Values.FirstOrDefault();
            var gpu = _provider.Gpus.Values.FirstOrDefault();
            var ram = _provider.Ram;
    
            var parts = new List<string>();
    
            if (cpu != null)
            {
                var cpuParts = new List<string>();
    
                if (cpu.Usage > 0)
                    cpuParts.Add($"{cpu.Usage:0}%");
    
                if (cpu.Temperature > 0)
                    cpuParts.Add($"{cpu.Temperature}°C");
    
                if (cpuParts.Count > 0)
                    parts.Add($"CPU {string.Join(" ", cpuParts)}");
            }
    
            if (gpu != null)
            {
                var gpuParts = new List<string>();
    
                if (gpu.Usage > 0)
                    gpuParts.Add($"{gpu.Usage:0}%");
    
                if (gpu.Temperature > 0)
                    gpuParts.Add($"{gpu.Temperature}°C");
    
                if (gpuParts.Count > 0)
                    parts.Add($"GPU {string.Join(" ", gpuParts)}");
            }
    
            if (ram == null) return Task.FromResult(string.Join(" | ", parts));
            if (ram.Used > 0)
                parts.Add($"RAM {ram.Used:0.0}GB");
    
            return Task.FromResult(string.Join(" | ", parts));
        }
        catch
        {
            return Task.FromResult("");
        }
    }
}