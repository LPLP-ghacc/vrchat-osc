namespace vrchat_osc.Models;

public class AppConfig(double windowHeight, double windowWidth, List<(int, string)> templates, int lastTemplateId)
{
    public double WindowWidth { get; set; } = windowWidth;
    public double WindowHeight { get; set; } = windowHeight;
    public List<(int, string)> Templates { get; init; } = templates;
    public int LastTemplateId = lastTemplateId;
    public static AppConfig Default => new(1200, 800, [], 0);
}