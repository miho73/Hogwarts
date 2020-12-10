using System;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace Hogwarts.Database.Log
{
    public enum LOG_LEVEL
    {
        DEBUG,
        INFO,
        WARNING,
        ERROR,
        FATAL
    }

    namespace Dormitory
    {
        public class LogLine
        {
            public string Message { get; set; }
            public LOG_LEVEL Log_lv { get; set; }
            public DateTimeOffset Time { get; set; }
        }

        public class Log
        {
            private readonly string DB_LOCATION = "Log/log.db";

            public Log()
            {
                ExecuteNonquery("CREATE TABLE IF NOT EXISTS Dormitory (" +
                    "ID INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL," +
                    "Message TEXT NOT NULL," +
                    "Log_lv INTEGER NOT NULL," +
                    "Time TEXT NOT NULL);");
            }

            public async Task AddDormitoryLog(LogLine line)
            {
                using SQLiteConnection con = new SQLiteConnection($"Data Source={DB_LOCATION};Version=3;");
                await con.OpenAsync();

                SQLiteCommand command = new SQLiteCommand("INSERT INTO Dormitory " +
                    "(Message, Log_lv, Time) VALUES" +
                    "(@msg, @loglv, @time)", con);
                command.Parameters.AddWithValue("@msg", line.Message);
                command.Parameters.AddWithValue("@loglv", (int)line.Log_lv);
                command.Parameters.AddWithValue("@time", line.Time.ToString("yyyy/MM/dd HH:mm:ss"));
                await command.ExecuteNonQueryAsync();
            }

            private async void ExecuteNonquery(string cmd)
            {
                using SQLiteConnection con = new SQLiteConnection($"Data Source={DB_LOCATION};Version=3;");
                await con.OpenAsync();

                SQLiteCommand command = new SQLiteCommand(cmd, con);
                await command.ExecuteNonQueryAsync();
            }
        }
    }

    namespace Students
    {
        public class LogLine
        {
            public string Message { get; set; }
            public LOG_LEVEL Log_lv { get; set; }
            public DateTimeOffset Time { get; set; }
        }

        public class Log
        {
            private readonly string DB_LOCATION = "Log/log.db";

            public Log()
            {
                ExecuteNonquery("CREATE TABLE IF NOT EXISTS Students (" +
                    "ID INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL," +
                    "Message TEXT NOT NULL," +
                    "Log_lv INTEGER NOT NULL," +
                    "Time TEXT NOT NULL);");
            }

            public async Task AddStudentLog(LogLine line)
            {
                using SQLiteConnection con = new SQLiteConnection($"Data Source={DB_LOCATION};Version=3;");
                await con.OpenAsync();

                SQLiteCommand command = new SQLiteCommand("INSERT INTO Students " +
                    "(Message, Log_lv, Time) VALUES" +
                    "(@msg, @loglv, @time)", con);
                command.Parameters.AddWithValue("@msg", line.Message);
                command.Parameters.AddWithValue("@loglv", (int)line.Log_lv);
                command.Parameters.AddWithValue("@time", line.Time.ToString("yyyy/MM/dd HH:mm:ss"));
                await command.ExecuteNonQueryAsync();
            }

            private async void ExecuteNonquery(string cmd)
            {
                using SQLiteConnection con = new SQLiteConnection($"Data Source={DB_LOCATION};Version=3;");
                await con.OpenAsync();

                SQLiteCommand command = new SQLiteCommand(cmd, con);
                await command.ExecuteNonQueryAsync();
            }
        }
    }
}
