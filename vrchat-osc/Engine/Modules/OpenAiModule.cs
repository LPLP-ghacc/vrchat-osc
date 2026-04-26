namespace vrchat_osc.Modules;

public class OpenAiModule
{
    public async Task<string> Generate(string input)
    {
        await Task.Delay(100);
        return $"{input}";
    }
}