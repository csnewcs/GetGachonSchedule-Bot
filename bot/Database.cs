using Microsoft.Data.Sqlite;

class Database {
    public static void InsertNewUser(SqliteConnection connection, ulong discordId, string gachonId, string gachonPw, string googleAuth, string calendarId) {
        connection.Open();
        string sql = $"INSERT INTO AccountInfo (discordId, gachonId, gachonPw, googleAuth, calendarId) VALUES ('{discordId}', '{gachonId}', '{gachonPw}', '{googleAuth}', '{calendarId}');";
        SqliteCommand command = new SqliteCommand(sql, connection);
        command.ExecuteNonQuery();
        connection.Close();
    }
    public static bool IsExistUser(SqliteConnection connection, ulong discordId) {
        connection.Open();
        string sql = $"SELECT * FROM AccountInfo WHERE discordId = '{discordId}';";
        SqliteCommand command = new SqliteCommand(sql, connection);
        SqliteDataReader reader = command.ExecuteReader();
        bool result = reader.Read();
        connection.Close();
        return result;
    }
}