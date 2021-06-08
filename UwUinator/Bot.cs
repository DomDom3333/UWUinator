// Bot.cs

using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace UwUinator
{
    public class Bot
    {
        private readonly DiscordSocketClient _client;
        private readonly ILogger<Bot> _logger;
        private readonly IMessageHandler _messageHandler;
        private readonly Config _config;

        public Bot(DiscordSocketClient client, ILogger<Bot> logger, IMessageHandler messageHandler, Config config)
        {
            _client = client;
            _logger = logger;
            _messageHandler = messageHandler;
            _config = config;

            _client.Log += LogAsync;
            _client.Ready += OnReadyAsync;
            _client.MessageReceived += message => _messageHandler.HandleMessageAsync(message, _client);
        }

        public async Task RunAsync()
        {
            string token = _config.Token;

            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogError("Bot token is not configured.");
                return;
            }

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private Task LogAsync(LogMessage logMessage)
        {
            _logger.Log(logMessage.Severity.ToLogLevel(), logMessage.Exception, logMessage.Message);
            return Task.CompletedTask;
        }

        private Task OnReadyAsync()
        {
            _logger.LogInformation("Bot is connected and ready!");
            return Task.CompletedTask;
        }
    }
}