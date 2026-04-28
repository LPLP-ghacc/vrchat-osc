namespace vrchat_osc;

public interface IStatusModule
{
    string Key { get; }
    bool IsEnabled { get; set; }
    Task<string> GetValueAsync();
}