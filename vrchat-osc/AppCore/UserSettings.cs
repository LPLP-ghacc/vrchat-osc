using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace vrchat_osc.AppCore;

[AttributeUsage(AttributeTargets.Property)]
public class HeaderAttribute(string value) : Attribute
{
    public string Title { get; } = value;
}

[AttributeUsage(AttributeTargets.Property)]
public class DisplayNameAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}

[AttributeUsage(AttributeTargets.Property)]
public class SeparatorAttribute() : Attribute {}

public class UserSettings : INotifyPropertyChanged
{
    [Header("App Settings")]
    [DisplayName("Run at Windows startup")]
    public bool RunAtWindowsStartup
    {
        get;
        init => Set(ref field, value);
    }
    
    [DisplayName("Enable logging")]
    public bool IsEnableLog
    {
        get;
        init => Set(ref field, value);
    }
    
    [DisplayName("Collapse into the tray when closing")]
    public bool MinimizeToTrayOnClose 
    {
        get;
        init => Set(ref field, value);
    }

    [DisplayName("Font of output blocks\ndefault Consolas")]
    public string? FontFamily
    {
        get;
        init => Set(ref field, value);
    }

    public static UserSettings Default =>
        new()
        {
            RunAtWindowsStartup = false,
            IsEnableLog = true,
            MinimizeToTrayOnClose = false,
            FontFamily = "Consolas",
        };

    public event PropertyChangedEventHandler? PropertyChanged;

    private void Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (Equals(field, value))
            return;

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}