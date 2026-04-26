namespace vrchat_osc;

public interface IStatusModule
{
    string Name { get; }
    bool IsEnabled { get; set; }

    int Priority { get; }

    Task<string> GetTextAsync();
}