using System;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace Hogwarts.Database
{
    public class History
    {
        public string Message { get; set; }
        public ulong MsgID { get; set; }
        public ulong UsrID { get; set; }
        public ulong ChannelID { get; set; }
        public DateTimeOffset Time { get; set; }
        public string UsrName { get; set; }
        public string Channel_name { get; set; }
    }

    public class MessageHistory
    {
        private readonly string DB_LOCATION = "Log/msg_history.db";

        public MessageHistory()
        {
            ExecuteNonquery("CREATE TABLE IF NOT EXISTS MsgHistory (" +
                "Key INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL," +
                "Message TEXT NOT NULL," +
                "Message_id INTEGER NOT NULL," +
                "User_id INTEGER NOT NULL," +
                "Time TEXT NOT NULL," +
                "User_name TEXT NOT NULL," +
                "Channel_id INTEGER NOT NULL," +
                "Channel_name TEXT NOT NULL);");
        }

        public async Task AddDatum(History history)
        {
            using (SQLiteConnection con = new SQLiteConnection($"Data Source={DB_LOCATION};Version=3;"))
            {
                await con.OpenAsync();

                SQLiteCommand command = new SQLiteCommand("INSERT INTO MsgHistory " +
                    "(Message, Message_id, User_id, Time, User_name, Channel_id, Channel_name) VALUES" +
                    "(@msg, @msgId, @uId, @time, @uName, @rId, @cName)", con);
                command.Parameters.AddWithValue("@msg", history.Message);
                command.Parameters.AddWithValue("@msgId", history.MsgID);
                command.Parameters.AddWithValue("@uId", history.UsrID);
                command.Parameters.AddWithValue("@time", history.Time.ToString("yyyy/MM/dd HH:mm:ss"));
                command.Parameters.AddWithValue("@uName", history.UsrName);
                command.Parameters.AddWithValue("@rId", history.ChannelID);
                command.Parameters.AddWithValue("@cName", history.Channel_name);
                await command.ExecuteNonQueryAsync();
            }
        }

        private async void ExecuteNonquery(string cmd)
        {
            using (SQLiteConnection con = new SQLiteConnection($"Data Source={DB_LOCATION};Version=3;"))
            {
                await con.OpenAsync();

                SQLiteCommand command = new SQLiteCommand(cmd, con);
                await command.ExecuteNonQueryAsync();
            }
        }
    }
}
