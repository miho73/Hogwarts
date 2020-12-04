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
    }
    public class Student
    {
        private readonly string DB_LOCATION = "Data/Students.db";

        public Student()
        {
            ExecuteNonquery("CREATE TABLE IF NOT EXISTS Students (" +
                "ID INTEGER PRIMARY KEY NOT NULL," +
                "Name TEXT NOT NULL," +
                "Dormitory INTEGER NOT NULL);");
        }

        public async Task AddDatum(StudentRecord record)
        {
            using (SQLiteConnection con = new SQLiteConnection($"Data Source={DB_LOCATION};Version=3;"))
            {
                await con.OpenAsync();

                SQLiteCommand command = new SQLiteCommand("INSERT INTO Students " +
                    "(ID, Name, Dormitory) VALUES" +
                    "(@uid, @uname, @dorm)", con);
                command.Parameters.AddWithValue("@uid", record.ID);
                command.Parameters.AddWithValue("@uname", record.Name);
                command.Parameters.AddWithValue("@dorm", TxtDormToDormCode(record.Dormitory));
                await command.ExecuteNonQueryAsync();
            }
        }

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
                _ => "Null"
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
            object ret = null;
            using (SQLiteConnection con = new SQLiteConnection($"Data Source={DB_LOCATION};Version=3;"))
            {
                await con.OpenAsync();

                SQLiteCommand command = new SQLiteCommand("SELECT * FROM Students WHERE ID=@id", con);
                command.Parameters.AddWithValue("@id", id);
                SQLiteDataReader reader = (SQLiteDataReader)await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    if ((long)reader[colum] == -1)
                    {
                        ret = default;
                    }
                    ret = (T)reader[colum];
                }
            }
            if (ret == null) throw new ArgumentException("Cannot find data you are looking for");
            else return (T)ret;
        }

        private async Task ExecuteNonquery(string cmd)
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
