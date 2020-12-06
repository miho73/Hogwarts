using Discord.Commands;
using Hogwarts.Database;
using Hogwarts.CmdExe.Attributes;
using System.Threading.Tasks;
using Hogwarts.Database.Log.Students;
using Hogwarts.Database.Log;
using System;
using Discord;
using Hogwarts.Module;
using System.Collections.Generic;
using static Hogwarts.Database.Student;

namespace Hogwarts.CmdExe
{
    [ExclusiveCmd(784244294445301790)]
    public class Ministry : ModuleBase<SocketCommandContext>
    {
        private readonly Log log = new Log();
        private readonly Students student = new Students();

        [Command("register")]
        [Summary("마법부에 학적을 등록합니다.")]
        public async Task Register()
        {
            if (await student.IsRegistered(Context.User.Id))
            {
                await SendMsg(Context.User.Username + "님은 이미 등록되어 있습니다.");
                return;
            }
            await student.AddDatum(new StudentRecord()
            {
                ID = Context.User.Id,
                Name = Context.User.Username,
                Dormitory = "Null",
                JoinedDate = DateTimeOffset.Now,
                Grade = "Silver"
            });
            await log.AddStudentLog(new LogLine()
            {
                Message = "Added " + Context.User + "(" + Context.User.Id + ") into registration",
                Log_lv = LOG_LEVEL.INFO,
                Time = DateTimeOffset.Now
            });
            await SendMsg(Context.User.Username + "님, 등록되었습니다. ");
        }

        [Command("rename")]
        [Summary("이름을 변경합니다.")]
        public async Task Rename()
        {
            string Before = await student.ReadColum<string>(Context.User.Id, "Name");
            string currentName = Context.User.Username;
            Console.WriteLine(Before + " " + currentName);
            await student.RenameStudent(Context.User.Id, currentName);
            await log.AddStudentLog(new LogLine()
            {
                Message = "Updated user name from \"" + Before + "\" to \"" + currentName + "\"",
                Log_lv = LOG_LEVEL.INFO,
                Time = DateTimeOffset.Now
            });
            await SendMsg($"{Before}→{currentName}으로 이름을 변경했습니다.");
        }

        [Command("student-record")]
        [Summary("학적사항을 개인적으로 보내드립니다.")]
        public async Task StudentRecord()
        {
            ulong sID = Context.User.Id;
            string sc = StudentCode.GetStudentCode(sID);
            StudentRecord record = await student.ReadDatum(sID);
            IDMChannel dmc = await Context.User.GetOrCreateDMChannelAsync();
            await dmc.SendMessageAsync($"학번: {sc}\n" +
                                       $"이름: {record.Name}\n" +
                                       $"기숙사: {record.Dormitory}\n" +
                                       $"학급: {record.Grade}\n" +
                                       $"입학일: {record.JoinedDate}");
        }

        public Task SendMsg(string text)
        {
            return Context.Channel.SendMessageAsync(text);
        }
    }
}
