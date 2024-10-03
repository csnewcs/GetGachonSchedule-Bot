using Discord;
using Discord.WebSocket;

namespace GetGachonScheduleBot
{
  class SlashCommands
  {
    public static Dictionary<ulong, Enroll> EnrollQueue = new Dictionary<ulong, Enroll>();

    public static Dictionary<string, Tuple<SlashCommandBuilder, Func<SocketSlashCommand, Task>>> Commands = new Dictionary<string, Tuple<SlashCommandBuilder, Func<SocketSlashCommand, Task>>>
    {
      {
        "도움", new Tuple<SlashCommandBuilder, Func<SocketSlashCommand, Task>>(new SlashCommandBuilder().WithName("도움").WithDescription("show commands"), async (SocketSlashCommand command) => {
          await command.RespondAsync("TEST", ephemeral: true);
        })
      },
      {
        "가입", new Tuple<SlashCommandBuilder, Func<SocketSlashCommand, Task>>(new SlashCommandBuilder().WithName("가입").WithDescription("서비스에 가입합니다").AddOption(new SlashCommandOptionBuilder().WithName("gachon_id").WithDescription("가천대학교 ID").WithType(ApplicationCommandOptionType.String).WithRequired(true)).AddOption(new SlashCommandOptionBuilder().WithName("gachon_pw").WithDescription("가천대학교 비밀번호(주의! DB에 저장됨!)").WithType(ApplicationCommandOptionType.String).WithRequired(true)), async (SocketSlashCommand command) => {
          if(Database.IsExistUser(Program.Config.DBConnection, command.User.Id)) {
            await command.RespondAsync("이미 가입되어 있습니다.", ephemeral: true);
            return;
          }
          var options = command.Data.Options.ToDictionary(x => x.Name, x => x.Value.ToString());
          // if()
          EnrollQueue.Add(command.User.Id, new Enroll(command.User.Id, options["gachon_id"], options["gachon_pw"]));
          var returnMessageButton = ButtonBuilder.CreateDangerButton("정말 가입하시겠습니까?", $"EnrollApply {command.User.Id}");
          await command.RespondAsync("정말 가입하시겠습니까? 당신의 가천대학교 비밀번호는 데이터베이스에 **평문 혹은 복호화 가능한 암호화가 적용되어** 저장됩니다. 이는 보안상 굉장히 취약하니 반드시 고려해주시길 바랍니다.", components: new ComponentBuilder().WithButton(returnMessageButton).Build(), ephemeral: true);
        })
      },
      {
        "탈퇴", new Tuple<SlashCommandBuilder, Func<SocketSlashCommand, Task>>(new SlashCommandBuilder().WithName("탈퇴").WithDescription("서비스에서 탈퇴합니다"), async (SocketSlashCommand command) => {
          Console.WriteLine(command.Data);
        })
      },
      {
        "수정", new Tuple<SlashCommandBuilder, Func<SocketSlashCommand, Task>>(new SlashCommandBuilder().WithName("수정").WithDescription("비밀번호 등 정보를 수정합니다"), async (SocketSlashCommand command) => {
          Console.WriteLine(command.Data);
        })
      }
    };
  }
}
