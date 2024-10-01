using System.IO.MemoryMappedFiles;
using Microsoft.Data.Sqlite;

struct DiscordBotConfig
{
  public readonly string Token { get; }
  public readonly string SharedMemoryPath { get; }
  public readonly string DBFilePath { get; }
  public SqliteConnection DBConnection { get; private set; }
  public MemoryMappedFile SharedMemory { get; private set; }
  public DiscordBotConfig()
  { //환경변수 사용
    Exception invalidEnvironmentValiable = new Exception("The environment variable is not set.");
    string? variables = Environment.GetEnvironmentVariable("GETGACHONBOT");
    if (variables == null) throw invalidEnvironmentValiable;
    string[] values = variables.Split(';'); //[0]: Token, [1]: SharedMemoryPath, [2]: DBFilePath
    if (values.Length != 3) throw invalidEnvironmentValiable;
    Token = values[0];
    SharedMemoryPath = values[1];
    DBFilePath = values[2];
    makeFileIfNotExist();
    SharedMemory = MemoryMappedFile.CreateFromFile(SharedMemoryPath, FileMode.OpenOrCreate, null, 4096);
  }
  public static DiscordBotConfig MakeConfig()
  {
    string token, sharedMemoryPath = "/dev/shm/GetGachonBot", dbFilePath = Directory.GetCurrentDirectory() + "/db.sqlite";
    Console.WriteLine("봇 환경 설정을 시작합니다.");
    Console.Write("봇 토큰을 입력해주세요.");
    string environmentVariable = "";
    token = Console.ReadLine();
    environmentVariable += token + ";";
    Console.Write($"스케줄 가져오기 프로그램과의 공유 메모리 경로를 입력해주세요.(기본값: {sharedMemoryPath})\n>");
    string? temp = Console.ReadLine();
    sharedMemoryPath = string.IsNullOrEmpty(temp) ? sharedMemoryPath : temp;
    environmentVariable += sharedMemoryPath + ";";
    Console.Write($"봇의 데이터베이스 파일 경로를 입력해주세요.(기본값: {dbFilePath})\n>");
    temp = Console.ReadLine();
    dbFilePath = string.IsNullOrEmpty(temp) ? dbFilePath : temp;
    environmentVariable += dbFilePath;
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
      string sql = "CREATE TABLE AccountInfo (discordId VARCHAR(20) PRIMARY KEY, gachonId TEXT, gachonPw TEXT, googleId TEXT)";
      SqliteCommand command = new SqliteCommand(sql, DBConnection);
      command.ExecuteNonQuery();
      DBConnection.Close();
      Console.WriteLine("SQLite File was created.");
    }
  }
  private static void setEnvironmentVariableOnLinux(string name, string value)
  {
    string shell = Environment.GetEnvironmentVariable("SHELL");
    string command = $"\nexport {name}=\"{value}\"";
    string home = Environment.GetEnvironmentVariable("HOME");
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
