namespace vrchat_osc.Services;

public class VrChatService(OscClient osc)
{
    /// <summary>
    /// Send a message to the VRChat chat
    /// </summary>
    public async Task SendChatMessageAsync(string message)
    {
        await Task.Run(() =>
        {
            osc.Send("/chatbox/input", message, true, false);
        });
    }

    /// <summary>
    /// Clear the chatbox
    /// </summary>
    public void ClearChat()
    {
        osc.Send("/chatbox/input", "", true, false);
    }

    /// <summary>
    /// Set the avatar parameter
    /// </summary>
    public void SetAvatarParameter(string name, object value)
    {
        osc.Send($"/avatar/parameters/{name}", value);
    }

    /// <summary>
    /// Enable/disable AFK status
    /// </summary>
    public void SetAfk(bool isAfk)
    {
        osc.Send("/avatar/parameters/AFK", isAfk);
    }
    
    public void ShowNotification(string message)
    {
        osc.Send("/chatbox/input", message, true, true);
    }

    public void SetTyping(bool isTyping)
    {
        osc.Send("/chatbox/typing", isTyping);
    }
}