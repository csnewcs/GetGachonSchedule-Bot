using Microsoft.Data.Sqlite;
using System.Text;

class Database {
    public static void InsertNewUser(SqliteConnection connection, ulong discordId, string gachonId, string gachonPw, string googleAuth, string calendarId) {
        byte[] base64 = Encoding.UTF8.GetBytes(gachonPw);
        string encode = Convert.ToBase64String(base64);
        connection.Open();
        string sql = $"INSERT INTO AccountInfo (discordId, gachonId, gachonPw, googleAuth, calendarId) VALUES ('{discordId}', '{gachonId}', '{encode}', '{googleAuth}', '{calendarId}');";
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
    public static void RemoveUser(SqliteConnection connection, ulong discordId) {
        connection.Open();
        string sql = $"DELETE FROM AccountInfo WHERE discordId = '{discordId}';";
        SqliteCommand command = new SqliteCommand(sql, connection);
        command.ExecuteNonQuery();
        connection.Close();
    }
    public static void UpdateGachonPassword(SqliteConnection connection, ulong discordId, string newPassword) {
        byte[] base64 = Encoding.UTF8.GetBytes(newPassword);
        string encode = Convert.ToBase64String(base64);
        string sql = $"UPDATE AccountInfo SET gachonPw = '{encode}' WHERE discordId = '{discordId}';";
        connection.Open();
        SqliteCommand command = new SqliteCommand(sql, connection);
        command.ExecuteNonQuery();
        connection.Close();
    }
}