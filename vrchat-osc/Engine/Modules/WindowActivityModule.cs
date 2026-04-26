using System.Runtime.InteropServices;
using System.Text;

namespace vrchat_osc.Modules;

public class WindowActivityModule : BaseModule
{
    public override string Name => "Window";

    [DllImport("user32.dll")]
    static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

    public override Task<string> GetTextAsync()
        => Safe(() =>
        {
            var handle = GetForegroundWindow();
            var sb = new StringBuilder(256);
            GetWindowText(handle, sb, sb.Capacity);

            var title = sb.ToString();
            return string.IsNullOrWhiteSpace(title) ? "" : $"🧭 {title}";
        });
}