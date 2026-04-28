namespace vrchat_osc.Modules;

public abstract class BaseModule : IStatusModule
{
    public abstract string Key { get; }
    public virtual bool IsEnabled { get; set; }

    public abstract Task<string> GetValueAsync();

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