using Discord.Commands;
using Hogwarts.Database;
using Hogwarts.Module;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using static Hogwarts.Database.Student;

namespace Hogwarts.CmdExe
{
    [Group("study")]
    [Summary("공부와 관련된 것들입니다.")]
    public class Study : ModuleBase<SocketCommandContext>
    {
        private Student student = new Student();

        [Command("start")]
        [Summary("공부를 시작합니다. 사용법: `study start <공부 내용>`")]
        public async Task StartStudy(string comment)
        {
            ulong id = Context.User.Id;
            Student.StudyTransactionResult result = await student.BeginStudy(id, DateTimeOffset.Now, comment);
            if(result == Student.StudyTransactionResult.SUCESS) await SendMsg(comment + "공부를 시작했습니다");
            else if(result == Student.StudyTransactionResult.CLOSE_BEFORE_START)
            {
                string From = await student.ReadStudyTable<string>(id, "Start");
                await SendMsg($"이미 공부중입니다. ({From}-)");
            }
        }

        [Command("finish")]
        [Summary("공부를 끝내고 기록을 저장합니다.")]
        public async Task FinishAndSave()
        {
            ulong id = Context.User.Id;
            Student.StudyTransactionResult result = await student.FinishStudy(id);
            if (result == Student.StudyTransactionResult.SUCESS) await SendMsg("공부를 마쳤습니다.");
            else if (result == Student.StudyTransactionResult.START_BEFORE_CLOSE)
            {
                await SendMsg($"공부를 일단 시작해야 합니다..");
            }
        }

        [Command("dispose")]
        [Summary("공부기록을 저장하지 않고 공부를 끝냅니다.")]
        public async Task DisposeStudy()
        {
            ulong id = Context.User.Id;
            StudyTransactionResult result = await student.DisposeStudy(id);
            if (result == StudyTransactionResult.SUCESS) await SendMsg("공부를 마쳤습니다. 공부시간: 없음");
            else if (result == StudyTransactionResult.START_BEFORE_CLOSE)
            {
                await SendMsg($"공부를 일단 시작해야 합니다.");
            }
        }

        [Command("record")]
        [Summary("공부 기록을 알려드립니다. 사용법: `record [today/week/month/year/total]`")]
        public async Task StudyRecord(string span)
        {
            ulong id = Context.User.Id;
            Dictionary<DateTimeOffset, StudyRecord> records = await student.GetStudyTime(id);
            switch (span)
            {
                case "today":
                    StudyRecord record = records.GetValueOrDefault(DateTimeOffset.Now.Date);
                    if (record == null)
                    {
                        await SendMsg("오늘 공부한 기록이 없습니다...");
                    }
                    else await SendMsg($"오늘 {TimeFormat.SpanToProper(record.span)} 공부했습니다.");
                    break;
                case "week":
                    StudyRecord sum = new StudyRecord()
                    {
                        comment = new List<string>(),
                        span = TimeSpan.FromSeconds(0)
                    };
                    DateTimeOffset dt = DateTimeOffset.Now;
                    int diff = (7 + (dt.DayOfWeek - DayOfWeek.Monday)) % 7;
                    DateTimeOffset BeginOfWeek = dt.AddDays(-1 * diff).Date;
                    for(int i=0; i<7; i++)
                    {
                        StudyRecord dayRecord = records.GetValueOrDefault(BeginOfWeek.AddDays(i));
                        if (dayRecord == null) continue;
                        sum += dayRecord;
                    }
                    if (sum.span.TotalSeconds == 0)
                    {
                        await SendMsg("이번주 공부한 기록이 없습니다...");
                    }
                    else await SendMsg($"이번주 {TimeFormat.SpanToProper(sum.span)} 공부했습니다.");
                    break;
                case "month":

                default:
                    await SendMsg($"{span}?");
                    break;
            }
        }

        public Task SendMsg(string text)
        {
            return Context.Channel.SendMessageAsync(text);
        }
    }
}
