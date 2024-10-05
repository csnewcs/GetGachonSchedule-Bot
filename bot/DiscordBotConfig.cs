using System.IO.MemoryMappedFiles;
using Microsoft.Data.Sqlite;

struct DiscordBotConfig
{
  public readonly string Token { get; }
  public readonly string DBFilePath { get; }
  public readonly KeyValuePair<string, string> GoogleOauth { get; }
  public readonly string RedirectUrl {get;}
  public SqliteConnection DBConnection { get; private set; }
  public DiscordBotConfig()
  { //환경변수 사용
    Exception invalidEnvironmentValiable = new Exception("The environment variable is not set.");
    string? variables = Environment.GetEnvironmentVariable("GETGACHONBOT");
    if (variables == null) throw invalidEnvironmentValiable;
    string[] values = variables.Split(';'); //[0]: Token, [1]: SharedMemoryPath, [2]: DBFilePath
    if (values.Length != 5) throw invalidEnvironmentValiable;
    Token = values[0];
    DBFilePath = values[1];
    GoogleOauth = new KeyValuePair<string, string>(values[2], values[3]);
    RedirectUrl = values[4];
    makeFileIfNotExist();
  }
  public static DiscordBotConfig MakeConfig()
  {
    string? token = "", dbFilePath = Directory.GetCurrentDirectory() + "/db.sqlite", googleOauthId = "", googleOauthSecret = "", redirectUrl = "";
    Console.WriteLine("봇 환경 설정을 시작합니다.");
    string environmentVariable = "";

    while(string.IsNullOrEmpty(token)) {
      Console.Write("봇 토큰을 입력해주세요.");
      token = Console.ReadLine();
    }
    environmentVariable += token + ";";

    string? temp;
    Console.Write($"봇의 데이터베이스 파일 경로를 입력해주세요.(기본값: {dbFilePath})\n>");
    temp = Console.ReadLine();
    dbFilePath = string.IsNullOrEmpty(temp) ? dbFilePath : temp;
    environmentVariable += dbFilePath + ";";

    //Google Ouath2 인증 정보 입력
    while(googleOauthId == "") {
      Console.Write($"구글 API OAuth2.0 ID를 입력해주세요.\n>");
      googleOauthId = Console.ReadLine();
    }
    environmentVariable += googleOauthId + ";";

    while(googleOauthSecret == "") {
      Console.Write($"구글 API OAuth2.0 Secret을 입력해주세요.\n>");
      googleOauthSecret = Console.ReadLine();
    }
    environmentVariable += googleOauthSecret + ";";

    while(redirectUrl == "") {
      Console.Write($"구글 API OAuth2.0 Redirect URL을 입력해주세요.\n>");
      redirectUrl = Console.ReadLine();
    }
    environmentVariable += redirectUrl;

    if (OperatingSystem.IsLinux())
    {
      setEnvironmentVariableOnLinux("GETGACHONBOT", environmentVariable);
    }
    Environment.SetEnvironmentVariable("GETGACHONBOT", environmentVariable);

    Console.WriteLine("환경변수 설정이 완료되었습니다. " + environmentVariable);
    return new DiscordBotConfig();
  }
  private void makeFileIfNotExist()
  {
    DBConnection = new SqliteConnection($"Data Source={DBFilePath};");
    if (!File.Exists(DBFilePath))
    {
      DBConnection.Open();
      string sql = "CREATE TABLE AccountInfo (discordId VARCHAR(20) PRIMARY KEY, gachonId TEXT, gachonPw TEXT, googleId TEXT, googleAuth TEXT, calendarId TEXT);";
      SqliteCommand command = new SqliteCommand(sql, DBConnection);
      command.ExecuteNonQuery();
      DBConnection.Close();
      Console.WriteLine("SQLite File was created.");
    }
  }
  private static void setEnvironmentVariableOnLinux(string name, string value)
  {
    string? shell = Environment.GetEnvironmentVariable("SHELL");
    string command = $"\nexport {name}=\"{value}\"";
    string? home = Environment.GetEnvironmentVariable("HOME");
    if (shell == null)
    {
      Console.WriteLine("Shell is not set.");
    }
    else if (shell.Contains("bash"))
    {
      File.AppendAllText($"{home}/.bashrc", command);
    }
    else if (shell.Contains("zsh"))
    {
      File.AppendAllText($"{home}/.zshrc", command);
    }
    else
    {
      Console.WriteLine("Shell is not supported.");
    }
  }
}
