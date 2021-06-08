// IMessageHandler.cs

using System.Threading.Tasks;
using Discord.WebSocket;

namespace UwUinator
{
    public interface IMessageHandler
    {
        Task HandleMessageAsync(SocketMessage message, DiscordSocketClient client);
    }
}