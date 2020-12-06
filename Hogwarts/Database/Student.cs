using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading.Tasks;
using Hogwarts.CmdExe.Attributes;

namespace Hogwarts.Database
{
    [ExclusiveCmd(784077924776017980)]
    public class Student
    {
        private static int studying = 0;
        private string DB_LOCATION = "Data/Students/";
        public async Task InitDB(ulong id)
        {
            await ExecuteNonQuery(id, "CREATE TABLE IF NOT EXISTS StudyRecord (" +
                "ID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," +
                "Start TEXT NOT NULL," +
                "Finish TEXT," +
                "Comment TEXT," +
                "Span INTEGER);");
        }

        public enum StudyTransactionResult
        {
            SUCESS,
            CLOSE_BEFORE_START,
            START_BEFORE_CLOSE,
            ERROR
        }

        public async Task<StudyTransactionResult> BeginStudy(ulong id, DateTimeOffset start, string comment)
        {
            await InitDB(id);
            StudyTransactionResult result = StudyTransactionResult.ERROR;
            using (SQLiteConnection con = new SQLiteConnection($"Data Source={DB_LOCATION + id}.db;Version=3;"))
            {
                await con.OpenAsync();

                SQLiteCommand checkIfStudying = new SQLiteCommand(
                    "SELECT Finish FROM StudyRecord WHERE ID=(SELECT MAX(ID) FROM StudyRecord);", con);
                bool isStudying = false;
                using (SQLiteDataReader studyingRd = (SQLiteDataReader)await checkIfStudying.ExecuteReaderAsync())
                {
                    while (await studyingRd.ReadAsync())
                    {
                        if (studyingRd["Finish"] == DBNull.Value) isStudying = true;
                    }
                }
                if (isStudying)
                {
                    result = StudyTransactionResult.CLOSE_BEFORE_START;
                }
                else
                {
                    SQLiteCommand command = new SQLiteCommand("INSERT INTO StudyRecord " +
                    "(Start, Comment) VALUES (@start, @comment);", con);
                    command.Parameters.AddWithValue("@start", start.ToString("yyyy/MM/dd HH:mm:ss"));
                    command.Parameters.AddWithValue("@comment", comment);
                    await command.ExecuteNonQueryAsync();
                    studying++;
                    result = StudyTransactionResult.SUCESS;
                }
            }
            return result;
        }

        public async Task<StudyTransactionResult> FinishStudy(ulong id)
        {
            StudyTransactionResult result = StudyTransactionResult.ERROR;
            using (SQLiteConnection con = new SQLiteConnection($"Data Source={DB_LOCATION + id}.db;Version=3;"))
            {
                await con.OpenAsync();

                SQLiteCommand checkIfStudying = new SQLiteCommand(
                    "SELECT Finish FROM StudyRecord WHERE ID=(SELECT MAX(ID) FROM StudyRecord);", con);
                bool isStarted = false;
                using (SQLiteDataReader studyingRd = (SQLiteDataReader)await checkIfStudying.ExecuteReaderAsync())
                {
                    while (await studyingRd.ReadAsync())
                    {
                        if (studyingRd["Finish"] == DBNull.Value) isStarted = true;
                    }
                }
                if (isStarted)
                {
                    DateTimeOffset start = DateTimeOffset.ParseExact(await ReadStudyTable<string>(id, "Start"), "yyyy/MM/dd HH:mm:ss", null);
                    DateTimeOffset finished = DateTimeOffset.Now;
                    TimeSpan gap = finished.Subtract(start);

                    SQLiteCommand write = new SQLiteCommand("UPDATE StudyRecord SET Finish=@finish, Span=@span " +
                        "WHERE ID=(SELECT MAX(ID) FROM StudyRecord);", con);
                    write.Parameters.AddWithValue("@finish", finished.ToString("yyyy/MM/dd HH:mm:ss"));
                    write.Parameters.AddWithValue("@span", gap.TotalSeconds);
                    await write.ExecuteNonQueryAsync();

                    studying--;
                    result = StudyTransactionResult.SUCESS;
                }
                else
                {
                    result = StudyTransactionResult.START_BEFORE_CLOSE;
                }
            }
            return result;
        }

        public async Task<StudyTransactionResult> DisposeStudy(ulong id)
        {
            StudyTransactionResult result = StudyTransactionResult.ERROR;
            using (SQLiteConnection con = new SQLiteConnection($"Data Source={DB_LOCATION + id}.db;Version=3;"))
            {
                await con.OpenAsync();

                SQLiteCommand checkIfStudying = new SQLiteCommand(
                    "SELECT Finish FROM StudyRecord WHERE ID=(SELECT MAX(ID) FROM StudyRecord);", con);
                bool isStarted = false;
                using (SQLiteDataReader studyingRd = (SQLiteDataReader)await checkIfStudying.ExecuteReaderAsync())
                {
                    while (await studyingRd.ReadAsync())
                    {
                        if (studyingRd["Finish"] == DBNull.Value) isStarted = true;
                    }
                }
                if (isStarted)
                {
                    SQLiteCommand command = new SQLiteCommand("DELETE FROM StudyRecord WHERE ID=(SELECT MAX(ID) FROM StudyRecord);", con);
                    await command.ExecuteNonQueryAsync();
                    studying--;
                    result = StudyTransactionResult.SUCESS;
                }
                else
                {
                    result = StudyTransactionResult.START_BEFORE_CLOSE;
                }
            }
            return result;
        }

        public async Task<T> ReadStudyTable<T>(ulong id, string colum)
        {
            T ret = default;
            using (SQLiteConnection con = new SQLiteConnection($"Data Source={DB_LOCATION + id}.db;Version=3;"))
            {
                await con.OpenAsync();

                SQLiteCommand command = new SQLiteCommand("SELECT * FROM StudyRecord WHERE ID=(SELECT MAX(ID) FROM StudyRecord)", con);
                SQLiteDataReader reader = (SQLiteDataReader)await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    
                    ret = (T)reader[colum];
                }
            }
            return ret;
        }

        public class StudyRecord
        {
            public List<string> comment { get; set; }
            public TimeSpan span { get; set; }
            public static StudyRecord operator +(StudyRecord a, StudyRecord b)
            {
                List<string> comments = a.comment;
                foreach(string cmt in b.comment)
                {
                    comments.Add(cmt);
                }

                return new StudyRecord()
                {
                    comment = comments,
                    span = a.span + b.span
                };
            }
        }
        public async Task<Dictionary<DateTimeOffset, StudyRecord>> GetStudyTime(ulong id)
        {
            Dictionary<DateTimeOffset, StudyRecord> record = new Dictionary<DateTimeOffset, StudyRecord>();
            using (SQLiteConnection con = new SQLiteConnection($"Data Source={DB_LOCATION + id}.db;Version=3;"))
            {
                await con.OpenAsync();

                SQLiteCommand command = new SQLiteCommand("SELECT Start, Finish, Span, Comment FROM StudyRecord;", con);
                SQLiteDataReader reader = (SQLiteDataReader)await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    //Republic of Korea, UTC+9
                    DateTimeOffset at = new DateTimeOffset(
                        (
                            DateTimeOffset.ParseExact(reader["Start"].ToString(), "yyyy/MM/dd HH:mm:ss", null).Ticks +
                            DateTimeOffset.ParseExact(reader["Finish"].ToString(), "yyyy/MM/dd HH:mm:ss", null).Ticks
                        ) / 2, new TimeSpan(9, 0, 0)).Date;
                    List<string> commt = new List<string>();
                    commt.Add(reader["Comment"].ToString());
                    StudyRecord rec = new StudyRecord()
                    {
                        comment = commt,
                        span = TimeSpan.FromSeconds((long)reader["Span"])
                    };

                    if(record.ContainsKey(at))
                    {
                        record[at] = record[at] + rec;
                    }
                    else record.Add(at, rec);
                }
            }
            return record;
        }

        private async Task ExecuteNonQuery(ulong id, string cmd)
        {
            using (SQLiteConnection con = new SQLiteConnection($"Data Source={DB_LOCATION+id}.db;Version=3;"))
            {
                await con.OpenAsync();

                SQLiteCommand command = new SQLiteCommand(cmd, con);
                await command.ExecuteNonQueryAsync();
            }
        }
    }
}
