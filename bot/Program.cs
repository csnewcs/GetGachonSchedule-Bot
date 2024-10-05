using Discord;
using Discord.Net;
using Discord.WebSocket;
using System.Data;

namespace GetGachonScheduleBot
{
  class Program
  {
    DiscordSocketClient client;
    public static DiscordBotConfig Config {get; set;}
    static void Main(string[] args) => new Program().StartBot().GetAwaiter().GetResult();
    public async Task StartBot()
    {
      try
      {
        Config = new DiscordBotConfig();
      }
      catch (Exception e)
      {
        if (e.Message == "The environment variable is not set.")
        {
          Config = DiscordBotConfig.MakeConfig();
        }
        else
        {
          Console.WriteLine(e.Message);
          return;
        }
      }
      setClientEvent();
      await client.LoginAsync(TokenType.Bot, Config.Token);
      await client.StartAsync();
      await Task.Delay(-1);
    }
    public static Task Log(LogMessage msg)
    {
      if (msg.Severity == LogSeverity.Critical || msg.Severity == LogSeverity.Error)
      {
        Console.ForegroundColor = ConsoleColor.Red;
      }
      else if (msg.Severity == LogSeverity.Warning)
      {
        Console.ForegroundColor = ConsoleColor.Yellow;
      }
      else
      {
        Console.ForegroundColor = ConsoleColor.White;
      }
      Console.WriteLine($"[{DateTime.Now}] {msg.Message} {msg.Exception}");
      return Task.CompletedTask;
    }
    private void setClientEvent()
    {
      client = new DiscordSocketClient();
      client.Log += Log;
      client.SlashCommandExecuted += async (SocketSlashCommand command) =>
      {
        if (SlashCommands.Commands.ContainsKey(command.CommandName))
        {
          await SlashCommands.Commands[command.CommandName].Item2(command);
        }
      };
      client.InteractionCreated += interactionCreated;
      client.ModalSubmitted += modalSubmitted;
      client.Ready += addCommands;
    }
    private async Task addCommands()
    {
      foreach (var guild in client.Guilds)
      {
        foreach(var c in await guild.GetApplicationCommandsAsync())
        {
          await c.DeleteAsync();
          await Log(new LogMessage(LogSeverity.Info, "MakeCommand", $"{guild.Name} command {c.Name} deleted"));
        }
        foreach (var c in SlashCommands.Commands.Values)
        {
          await guild.CreateApplicationCommandAsync(c.Item1.Build());
          await Log(new LogMessage(LogSeverity.Info, "MakeCommand", $"{guild.Name} command {c.Item1.Name} created"));
        }
      }
    }
    private async Task interactionCreated (SocketInteraction interaction)
    {
      if (interaction is SocketMessageComponent component)
      {
        if (component.Data.CustomId.StartsWith("EnrollApply"))
        {
          var userId = ulong.Parse(component.Data.CustomId.Split(" ")[1]);
          if(SlashCommands.EnrollQueue.ContainsKey(userId))
          {
            await SlashCommands.EnrollQueue[userId].Apply(Config, interaction);
          }
        }
        else if (component.Data.CustomId.StartsWith("GoogleCode"))
        {
          var userId = ulong.Parse(component.Data.CustomId.Split(" ")[1]);
          if(SlashCommands.EnrollQueue.ContainsKey(userId))
          {
            await SlashCommands.EnrollQueue[userId].OpenGoogleCodeModal(interaction);
          }
        }
      }
    }
    private async Task modalSubmitted(SocketModal modal)
    {
      if (modal.Data.CustomId.StartsWith("GoogleCode"))
      {
        var userId = ulong.Parse(modal.Data.CustomId.Split(" ")[1]);
        if(SlashCommands.EnrollQueue.ContainsKey(userId))
        {
          await SlashCommands.EnrollQueue[userId].InputedGoogleCode(Config, modal);
        }
      }
    }
  }
}
