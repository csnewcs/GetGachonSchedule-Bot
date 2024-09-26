using Discord;
using Discord.Net;
using Discord.WebSocket;
using System.Data;

namespace GetGachonScheduleBot
{
    class Program
    {
        DiscordSocketClient client;
        static void Main(string[] args) => new Program().StartBot().GetAwaiter().GetResult();
        public async Task StartBot() {
            
            DiscordBotConfig config;
            try {
                config = new DiscordBotConfig();
            } catch(Exception e) {
                if(e.Message == "The environment variable is not set.") {
                    config = DiscordBotConfig.MakeConfig();
                } else {
                    Console.WriteLine(e.Message);
                    return;
                }
            }
            client = new DiscordSocketClient();
            client.Log += Log;
            client.Ready += async () => {
                await Log(new LogMessage(LogSeverity.Info, "Bot", "Bot is ready."));
            };
            await client.LoginAsync(TokenType.Bot, config.Token);
            await client.StartAsync();
            await Task.Delay(-1);
        }
        private async Task Log(LogMessage msg) {
            if(msg.Severity == LogSeverity.Critical || msg.Severity == LogSeverity.Error) {
                Console.ForegroundColor = ConsoleColor.Red;
            } else if(msg.Severity == LogSeverity.Warning) {
                Console.ForegroundColor = ConsoleColor.Yellow;
            } else {
                Console.ForegroundColor = ConsoleColor.White;
            }
            Console.WriteLine($"[{DateTime.Now}] {msg.Message}");
        }
        
    }
}