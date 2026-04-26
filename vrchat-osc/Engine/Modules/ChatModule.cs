using vrchat_osc.Services;

namespace vrchat_osc.Modules;

public class ChatModule(VrChatService vr)
{
    public Task Send(string text)
        => vr.SendChatMessageAsync(text);
}