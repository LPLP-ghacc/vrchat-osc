using vrchat_osc.Services;

namespace vrchat_osc.Modules;

public class StatusModule(VrChatService vrChat)
{
    public async Task UpdateStatusAsync()
    {
        while (true)
        {
            var time = DateTime.Now.ToString("HH:mm");
            var status = $"{time}";

            await vrChat.SendChatMessageAsync(status);

            await Task.Delay(5000);
        }
    }
}