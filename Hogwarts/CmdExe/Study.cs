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
        private readonly Student student = new Student();

        [Command("start")]
        [Summary("공부를 시작합니다. 사용법: `study start <공부 내용>`")]
        public async Task StartStudy([Remainder] string comment)
        {
            ulong id = Context.User.Id;
            StudyTransactionResult result = await student.BeginStudy(id, DateTimeOffset.Now, comment);
            if(result == StudyTransactionResult.SUCESS) await SendMsg(comment + "공부를 시작했습니다");
            else if(result == StudyTransactionResult.CLOSE_BEFORE_START)
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
            StudyTransactionResult result = await student.FinishStudy(id);
            if (result == StudyTransactionResult.SUCESS) await SendMsg("공부를 마쳤습니다.");
            else if (result == StudyTransactionResult.START_BEFORE_CLOSE)
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

        [Group("record")]
        [Summary("공부 기록을 알려드립니다. 사용법: `record [today/week/month/year/total/avg]`")]
        public class ShowRecord : ModuleBase<SocketCommandContext>
        {
            private enum Range
            {
                WEEK,
                MONTH,
                YEAR,
                TOTAL
            }
            private async Task<TimeSpan> GetSpanOfRange(Range range)
            {
                ulong id = Context.User.Id;
                Dictionary<DateTimeOffset, StudyRecord> records = await student.GetStudyTime(id);
                DateTimeOffset today = DateTimeOffset.Now;
                StudyRecord sum = new StudyRecord()
                {
                    comment = new List<string>(),
                    span = TimeSpan.FromSeconds(0)
                };

                switch (range)
                {
                    case Range.WEEK:
                        int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
                        DateTimeOffset BeginOfWeek = today.AddDays(-1 * diff).Date;
                        for (int i = 0; i < 7; i++)
                        {
                            if (BeginOfWeek.CompareTo(today) > 0) break;
                            StudyRecord dayRecord = records.GetValueOrDefault(BeginOfWeek);
                            BeginOfWeek.AddDays(1);
                            if (dayRecord == null) continue;
                            sum += dayRecord;
                        }
                        break;

                    case Range.MONTH:
                        DateTimeOffset date = DateTimeOffset.Now;
                        DateTimeOffset firstDayOfMonth = new DateTime(date.Year, date.Month, 1).Date;
                        DateTimeOffset lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1).Date;
                        do
                        {
                            if (records.ContainsKey(firstDayOfMonth))
                            {
                                sum += records[firstDayOfMonth];
                            }
                            firstDayOfMonth = firstDayOfMonth.AddDays(1);
                        } while (firstDayOfMonth.CompareTo(lastDayOfMonth) <= 0 && firstDayOfMonth.CompareTo(today) <= 0);
                        break;

                    case Range.YEAR:
                        DateTimeOffset firstDayOfYear = new DateTime(today.Year, 1, 1).Date;
                        DateTimeOffset lastDayOfYear = firstDayOfYear.AddYears(1).AddDays(-1).Date;
                        do
                        {
                            if (records.ContainsKey(firstDayOfYear))
                            {
                                sum += records[firstDayOfYear];
                            }
                            firstDayOfYear = firstDayOfYear.AddDays(1);
                        } while (firstDayOfYear.CompareTo(lastDayOfYear) <= 0 && firstDayOfYear.CompareTo(today) <= 0);
                        break;

                    case Range.TOTAL:
                        foreach (StudyRecord rec in records.Values) sum += rec;
                        break;

                    default:
                        throw new ArgumentException($"Unexpected range: {range}");
                }
                return sum.span;
            }

            private readonly Student student = new Student();
            private readonly Students students = new Students();

            [Command("today")]
            public async Task TodayRecord()
            {
                ulong id = Context.User.Id;
                Dictionary<DateTimeOffset, StudyRecord> records = await student.GetStudyTime(id);
                StudyRecord record = records.GetValueOrDefault(DateTimeOffset.Now.Date);
                if (record == null)
                {
                    await SendMsg("오늘 공부한 기록이 없습니다.");
                }
                else await SendMsg($"오늘 {TimeFormat.SpanToProper(record.span)} 공부했습니다.");
            }

            [Command("week")]
            private async Task WeeklyRecord()
            {
                TimeSpan sum = await GetSpanOfRange(Range.WEEK);
                if (sum.TotalSeconds == 0)
                {
                    await SendMsg("이번주 공부한 기록이 없습니다.");
                }
                else await SendMsg($"이번주 {TimeFormat.SpanToProper(sum)} 공부했습니다.");
            }

            [Command("month")]
            public async Task MonthlyRecord()
            {
                TimeSpan sum = await GetSpanOfRange(Range.MONTH);
                if (sum.TotalSeconds == 0)
                {
                    await SendMsg("이번달 공부한 기록이 없습니다.");
                }
                else await SendMsg($"이번달 {TimeFormat.SpanToProper(sum)} 공부했습니다.");
            }

            [Command("year")]
            public async Task YearRecord()
            {
                TimeSpan sum = await GetSpanOfRange(Range.YEAR);
                if (sum.TotalSeconds == 0)
                {
                    await SendMsg("이번년에 공부한 기록이 없습니다.");
                }
                else await SendMsg($"이번년에 {TimeFormat.SpanToProper(sum)} 공부했습니다.");
            }

            [Command("total")]
            public async Task TotalRecord()
            {
                TimeSpan sum = await GetSpanOfRange(Range.TOTAL);
                if (sum.TotalSeconds == 0)
                {
                    await SendMsg("공부를 한 적이 없습니다.");
                }
                else await SendMsg($"{TimeFormat.SpanToProper(sum)}만큼 공부했습니다.");
            }

            [Command("avg")]
            [Summary("전체 공부시간 하루 평균을 알려드립니다.")]
            public async Task Avg()
            {
                TimeSpan avgSpan = TimeSpan.FromSeconds(
                    (await GetSpanOfRange(Range.TOTAL)).TotalSeconds
                    /
                    DateTimeOffset.Now.Subtract(
                    DateTimeOffset.ParseExact(
                        await students.ReadColum<string>(Context.User.Id, "JoinedDate"), "yyyy/MM/dd HH:mm:ss", null)
                    ).TotalDays);
                await SendMsg($"하루평균 {TimeFormat.SpanToProper(avgSpan)}만큼 공부했습니다.");
            }

            [Command]
            public async Task NoArg()
            {
                await TotalRecord();
            }

            public Task SendMsg(string text)
            {
                return Context.Channel.SendMessageAsync(text);
            }
        }

        public Task SendMsg(string text)
        {
            return Context.Channel.SendMessageAsync(text);
        }
    }
}
