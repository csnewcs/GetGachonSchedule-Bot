using System.Text;
using System.Web;
using Discord;
using Discord.WebSocket;

using Google.Apis.Services;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using GetGachonScheduleBot;

class Enroll
{
    readonly ulong userId;
    readonly string gachonID;
    readonly string gachonPW;
    public Enroll(ulong userId, string gachonID, string gachonPW)
    {
        this.userId = userId;
        this.gachonID = gachonID;
        this.gachonPW = gachonPW;
    }
    public async Task Apply(DiscordBotConfig config, SocketInteraction interaction) {
        await interaction.DeferAsync();
        var originalRespond = await interaction.GetOriginalResponseAsync();
        await originalRespond.ModifyAsync(m => {
            m.Components = null;
            m.Content = "가천대학교 계정 정보 확인 중...";
        });
        //1. 가천대학교 계정 정보 확인
        if(await isValidateGachonAuth(gachonID, gachonPW)) {
            await originalRespond.ModifyAsync(m => {
                m.Content = "가천대학교 계정 정보가 확인되었습니다. 구글 계정 요청 중...";
            });
        } else {
            await interaction.FollowupAsync("가천대학교 아이디 또는 비밀번호를 잘못 입력하였습니다.", ephemeral: true);
            SlashCommands.EnrollQueue.Remove(userId);
            return;
        }
        //2. 구글 계정 API 요청
        // GoogleCredential.FromJson

        string oauthID = config.GoogleOauth.Key;
    
        const string calendarScope = "https://www.googleapis.com/auth/calendar";
        const string emailScope = "https://www.googleapis.com/auth/userinfo.email";
        string variables = $"scope={HttpUtility.UrlEncode($"{calendarScope} {emailScope}", Encoding.UTF8)}&response_type=code&client_id={oauthID}&prompt=consent&access_type=offline&redirect_uri={config.RedirectUrl}";
        string auth = $"https://accounts.google.com/o/oauth2/v2/auth?{variables}";

        await interaction.ModifyOriginalResponseAsync(m => {
            m.Content = $"아래 링크로 들어가 구글 계정에 로그인하고 나온 코드를 입력해주세요.\n## 만약 캘린더에 해당하는 체크박스가 해제되어 있다면 체크해주세요\n주의 문구가 나올 수 있는데 무시하셔도 무방합니다.\n[구글 계정 요청]({auth})";
            m.Components = new ComponentBuilder().WithButton("구글 코드 입력", $"GoogleCode {interaction.User.Id} Open", ButtonStyle.Primary).Build();
        });
    }
    private async Task<bool> isValidateGachonAuth(string gachonID, string gachonPW)
    {
        const string LOGIN_URL = "https://cyber.gachon.ac.kr/login/index.php"; //가천대학교 사이버캠퍼스 로그인 페이지
        using (HttpClient client = new HttpClient()) {
            HttpContent content = new FormUrlEncodedContent(new Dictionary<string, string> {
                {"username", gachonID},
                {"password", gachonPW}
            });
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");
            var result = await client.PostAsync(LOGIN_URL, content);
            string str = await result.Content.ReadAsStringAsync();
            return !str.Contains("아이디 또는 패스워드가 잘못 입력되었습니다.");
        }
    }
    public async Task OpenGoogleCodeModal(SocketInteraction interaction) {
        await interaction.RespondWithModalAsync(new ModalBuilder()
        .WithTitle("구글 코드 입력")
        .WithCustomId($"GoogleCode {interaction.User.Id}")
        .AddTextInput("로그인 후 구글 코드 입력", "GoogleCodeText", required: true).Build());
    }
    public async Task InputedGoogleCode(DiscordBotConfig config, SocketModal interaction) {
        const string TOKEN_URL = "https://oauth2.googleapis.com/token";
        var code = interaction.Data.Components.ToList();
        await interaction.DeferLoadingAsync();
        HttpClient client = new HttpClient();
        var token = await client.PostAsync(TOKEN_URL, new FormUrlEncodedContent(new Dictionary<string, string> {
            {"code", code[0].Value},
            {"client_id", config.GoogleOauth.Key},
            {"client_secret", config.GoogleOauth.Value},
            {"redirect_uri", config.RedirectUrl},
            {"grant_type", "authorization_code"}
        }));
        string credentialString = await token.Content.ReadAsStringAsync();

        GoogleAuthorizationCodeFlow googleAuthFlow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer() {
            ClientSecrets = new ClientSecrets {
                ClientId = config.GoogleOauth.Key,
                ClientSecret = config.GoogleOauth.Value
            },
            Scopes = [CalendarService.Scope.Calendar],
        });
        var tokenResponse = System.Text.Json.JsonSerializer.Deserialize<TokenResponse>(credentialString);
        GoogleCredential googleCredential = GoogleCredential.FromAccessToken(tokenResponse.access_token);
        var service = new CalendarService(new BaseClientService.Initializer() {
            HttpClientInitializer = googleCredential
        });
        Calendar calendar = new Calendar {
            Summary = "가천대학교 일정",
            TimeZone = "Asia/Seoul"
        };
        
        var upload = service.Calendars.Insert(calendar).Execute();
        
        Database.InsertNewUser(config.DBConnection, userId, gachonID, gachonPW, credentialString, upload.Id);
        
        await interaction.ModifyOriginalResponseAsync(m => {
            m.Content = "가입이 완료되었습니다.";
            m.Components = null;
        });
        await Program.Log(new LogMessage(LogSeverity.Info, "Enroll", $"User {userId} enrolled"));
    }
}
struct TokenResponse {
    public string access_token {get; set;}
    public string token_type {get; set;}
    public int expires_in {get; set;}
    public string refresh_token {get; set;}
    public string scope {get; set;}
    public string id_token {get; set;}
}
