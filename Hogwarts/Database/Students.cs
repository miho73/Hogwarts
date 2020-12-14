using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;
using System.Threading.Tasks;

namespace Hogwarts.Database
{
    public class StudentRecord
    {
        public ulong ID { get; set; }
        public string Name { get; set; }
        public string Dormitory { get; set; }
        public DateTimeOffset JoinedDate { get; set; }
        public string Grade { get; set; }
    }
    public class Students
    {
        private readonly string DB_LOCATION = "Data/Students.db";

        public Students()
        {
            _ = ExecuteNonquery("CREATE TABLE IF NOT EXISTS Students (" +
                "ID INTEGER PRIMARY KEY NOT NULL," +
                "Name TEXT NOT NULL," +
                "Dormitory INTEGER NOT NULL," +
                "JoinedDate TEXT NOT NULL," +
                "Grade TEXT NOT NULL);");
        }

        public async Task AddDatum(StudentRecord record)
        {
            using SQLiteConnection con = new SQLiteConnection($"Data Source={DB_LOCATION};Version=3;");
            await con.OpenAsync();

            SQLiteCommand command = new SQLiteCommand("INSERT INTO Students " +
                "(ID, Name, Dormitory, JoinedDate, Grade) VALUES" +
                "(@uid, @uname, @dorm, @joined, @grade)", con);
            command.Parameters.AddWithValue("@uid", record.ID);
            command.Parameters.AddWithValue("@uname", record.Name);
            command.Parameters.AddWithValue("@dorm", TxtDormToDormCode(record.Dormitory));
            command.Parameters.AddWithValue("@joined", record.JoinedDate.ToString("yyyy/MM/dd HH:mm:ss"));
            command.Parameters.AddWithValue("@grade", record.Grade);
            await command.ExecuteNonQueryAsync();
        }

        //TODO: Change to ReadColum function
        public async Task<bool> IsRegistered(ulong id)
        {
            bool ret = false;
            using (SQLiteConnection con = new SQLiteConnection($"Data Source={DB_LOCATION};Version=3;"))
            {
                await con.OpenAsync();

                SQLiteCommand command = new SQLiteCommand("SELECT * FROM Students WHERE ID=@id", con);
                command.Parameters.AddWithValue("@id", id);
                SQLiteDataReader reader = (SQLiteDataReader)await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    ret = true;
                    goto End;
                }
                ret = false;
            }
            End:
            return ret;
        }

        public string DormCodeToTxtDorm(long dormCode)
        {
            return dormCode switch
            {
                0 => "Gryffindor",
                1 => "Hufflepuff",
                2 => "Ravenclaw",
                3 => "Slytherin",
                _ => "정해지지 않음"
            };
        }
        public long TxtDormToDormCode(string dormName)
        {
            return dormName switch
            {
                "Gryffindor" => 0,
                "Hufflepuff" => 1,
                "Ravenclaw" => 2,
                "Slytherin" => 3,
                _ => -1
            };
        }

        public async Task UpdateDormitory(string dormName, ulong id)
        {
            await ExecuteNonquery("UPDATE Students SET Dormitory=" + TxtDormToDormCode(dormName) + " WHERE ID=" + id);
        }

        public async Task<T> ReadColum<T>(ulong id, string colum)
        {
            T ret = default;
            using (SQLiteConnection con = new SQLiteConnection($"Data Source={DB_LOCATION};Version=3;"))
            {
                await con.OpenAsync();

                SQLiteCommand command = new SQLiteCommand("SELECT * FROM Students WHERE ID=@id", con);
                command.Parameters.AddWithValue("@id", id);
                SQLiteDataReader reader = (SQLiteDataReader)await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    ret = (T)reader[colum];
                }
            }
            return ret;
        }

        public async Task<StudentRecord> ReadDatum(ulong id)
        {
            StudentRecord record = new StudentRecord();
            using (SQLiteConnection con = new SQLiteConnection($"Data Source={DB_LOCATION};Version=3;"))
            {
                await con.OpenAsync();

                SQLiteCommand command = new SQLiteCommand("SELECT * FROM Students WHERE ID=@id", con);
                command.Parameters.AddWithValue("@id", id);
                SQLiteDataReader reader = (SQLiteDataReader)await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    record.ID = id;
                    record.Name = reader["Name"].ToString();
                    record.Dormitory = DormCodeToTxtDorm((long)reader["Dormitory"]);
                    record.JoinedDate = DateTimeOffset.ParseExact(reader["JoinedDate"].ToString(), "yyyy/MM/dd HH:mm:ss", null);
                    record.Grade = reader["Grade"].ToString();
                }
            }
            return record;
        }

        public async Task RenameStudent(ulong id, string NewName)
        {
            using SQLiteConnection con = new SQLiteConnection($"Data Source={DB_LOCATION};Version=3;");
            await con.OpenAsync();
            try
            {
                SQLiteCommand command = new SQLiteCommand("UPDATE Students SET Name=@name WHERE ID=@id;", con);
                command.Parameters.AddWithValue("@id", id);
                command.Parameters.AddWithValue("@name", NewName);
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
            }
        }

        private async Task ExecuteNonquery(string cmd)
        {
            using SQLiteConnection con = new SQLiteConnection($"Data Source={DB_LOCATION};Version=3;");
            await con.OpenAsync();

            SQLiteCommand command = new SQLiteCommand(cmd, con);
            await command.ExecuteNonQueryAsync();
        }
    }
}
