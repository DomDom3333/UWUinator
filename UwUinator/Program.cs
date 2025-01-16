using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.EnvironmentVariables;

namespace UwUinator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            ServiceProvider serviceProvider = ConfigureServices();
            Bot bot = serviceProvider.GetRequiredService<Bot>();
            await bot.RunAsync();
        }

        private static ServiceProvider ConfigureServices()
        {
            Config config = LoadConfiguration();

            return new ServiceCollection()
                .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
                {
                    GatewayIntents = GatewayIntents.AllUnprivileged |
                                     GatewayIntents.Guilds |
                                     GatewayIntents.GuildMessages |
                                     GatewayIntents.MessageContent
                }))
                .AddSingleton<Bot>()
                .AddSingleton<IMessageHandler, UwUifyMessageHandler>()
                .AddSingleton(config)
                .AddLogging(builder =>
                {
                    builder.AddConsole(); // Include the console as a logging output
                    builder.SetMinimumLevel(LogLevel.Information); // Set the minimum log level
                })
                .BuildServiceProvider();
        }

        private static Config LoadConfiguration()
        {
            // Use ConfigurationBuilder to read environment variables or other sources
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables() // Read from environment variables
                .Build();

            // Map to the configuration object
            Config config = new Config();
            configuration.Bind(config);

            return config;
        }
    }

    // Configuration class to hold environment variables or other settings
    public class Config
    {
        public string Token { get; set; } = string.Empty; // Example: Discord Bot Token
        public string EnvironmentName { get; set; } = "Development"; // Example env variable
    }
}