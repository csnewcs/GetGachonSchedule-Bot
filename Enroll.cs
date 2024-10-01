using System.Net;
using Discord.WebSocket;

class Enroll
{
    readonly ulong userId;
    readonly string gachonID;
    readonly string gachonPW;
    readonly string googleID;

    public Enroll(ulong userId, string gachonID, string gachonPW, string googleID)
    {
        this.userId = userId;
        this.gachonID = gachonID;
        this.gachonPW = gachonPW;
        this.googleID = googleID;
    }
    public async Task Apply(DiscordBotConfig config, SocketInteraction interaction) {
        await interaction.DeferAsync();
        var originalRespond = await interaction.GetOriginalResponseAsync();
        await originalRespond.ModifyAsync(m => {
            m.Components = null;
            m.Content = "가천대학교 계정 정보 확인 중...";
        });
        if(await isValidateGachonAuth(gachonID, gachonPW)) {
            await originalRespond.ModifyAsync(m => {
                m.Content = "가천대학교 계정 정보가 확인되었습니다. 구글 계정 요청 중...";
            });
        } else {
            await interaction.FollowupAsync("가천대학교 아이디 또는 비밀번호를 잘못 입력하였습니다.", ephemeral: true);

        }
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
            Console.WriteLine(str);
            return !str.Contains("아이디 또는 패스워드가 잘못 입력되었습니다.");
        }
    }
}