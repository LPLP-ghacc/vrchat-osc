namespace vrchat_osc.Modules;

public abstract class BaseModule : IStatusModule
{
    public abstract string Name { get; }
    public virtual bool IsEnabled { get; set; } = false;
    public virtual int Priority => 5;

    public abstract Task<string> GetTextAsync();

    protected static Task<string> Safe(Func<string> func)
    {
        try
        {
            return Task.FromResult(func());
        }
        catch
        {
            return Task.FromResult("");
        }
    }
}