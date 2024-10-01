using Discord.Net;
using Discord;
using Discord.WebSocket;
using System.Collections;

namespace GetGachonScheduleBot
{
  class SlashCommands
  {
    public static Dictionary<string, Func<SocketSlashCommand, Task>> Commands = new Dictionary<string, Func<SocketSlashCommand, Task>>
    {
      {
        "help", async (SocketSlashCommand command) => {
          await command.RespondAsync("TEST", ephemeral: true);
        }
      },
      {
        "Enroll", async (SocketSlashCommand command) => {
          Console.WriteLine(command.Data);
        }
      }
    };
  }
}
