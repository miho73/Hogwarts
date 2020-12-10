using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Text;
using System.Threading.Tasks;
using Hogwarts.CmdExe.Attributes;
using System;
using Newtonsoft.Json.Linq;
using Hogwarts.Database;

//Command here will be applied globally

namespace Hogwarts.CmdExe
{
    public class ManageCmd : ModuleBase<SocketCommandContext>
    {
        [Command("whoami")]
        [Summary("여러분이 아는 whoami 맞습니다.")]
        public async Task Whoami()
        {
            bool isSU = false;
            StringBuilder roleNames = new StringBuilder();
            foreach (SocketRole role in (Context.User as SocketGuildUser).Roles)
            {
                if (role.IsEveryone) continue;
                roleNames.Append(role.ToString() + " ");
                if (role.ToString() == "@Principle")
                {
                    isSU = true;
                }
            }
            if (roleNames.ToString() == "") roleNames = new StringBuilder("*지위 없음*");
            else roleNames.Remove(roleNames.Length - 1, 1);
            string rep;
            if (isSU)
            {
                rep = "** *" + Context.User.Username + "* **  (" + roleNames + ")\n";
                await SendMsg(rep);
            }
            else
            {
                rep = Context.User.Username + "  (" + roleNames + ")\n";
                await SendMsg(rep);
            }
        }

        [Command("purge")]
        [Summary("일정 갯수의 메시지를 지웁니다.")]
        [Sudo("@Principle")]
        public async Task PurgeChat(int amount)
        {
            var messages = await Context.Channel.GetMessagesAsync(amount + 1).FlattenAsync();
            await (Context.Channel as SocketTextChannel).DeleteMessagesAsync(messages);
        }

        [Command("clear")]
        [Summary("메시지를 모두 지웁니다.")]
        [DM]
        [Sudo("@Principle")]
        public async Task ClearChat()
        {
            var messages = await Context.Channel.GetMessagesAsync(2147483647).FlattenAsync();
            await (Context.Channel as SocketTextChannel).DeleteMessagesAsync(messages);
        }

        [Command("help")]
        [Summary("사용 가능한 명령어에 대한 설명을 표시합니다.")]
        [ExclusiveCmd(784077924776017980)]
        public async Task Help([Remainder] string cmd)
        {
            string[] cmdPt = cmd.Split(' ');
            JObject current = Hogwarts.CmdHelp;
            foreach(string pt in cmdPt)
            {
                current = (JObject)current.GetValue(pt);
                if(current == null)
                {
                    await SendMsg($"{pt}명령어를 찾을 수 없습니다.");
                    return;
                }
                if((bool)current.GetValue("isEnd"))
                {
                    break;
                }
            }

            EmbedBuilder embed = new EmbedBuilder()
            {
                Title = current.GetValue("DisplayCmd").ToString(),
                Color = Color.Purple,
                Description = "*" + current.GetValue("at").ToString() + "*에서 사용할 수 있습니다."
            };
            embed.AddField(current.GetValue("DisplayCmd").ToString(), current.GetValue("explain").ToString(), true);
            embed.AddField("사용법", current.GetValue("usage").ToString(), true);
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("help")]
        [Summary("사용 가능한 명령어들을 열거합니다.")]
        public async Task HelpEnum()
        {
            EmbedBuilder embed = new EmbedBuilder()
            {
                Title = "명령어 목록",
                Color = Color.Purple,
                Description = "각 명령에 대한 자세한 설명이 필요하면 `help <명령어>`를 입력하세요."
            };
            JArray cmds = (JArray)Hogwarts.CmdHelp.GetValue("commands");
            string cmdRow = "";
            foreach(string cmd in cmds)
            {
                cmdRow += $"`{cmd}`\n";
            }
            embed.AddField("Hogwarts", cmdRow);
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Group("sudo")]
        [Sudo("@Principle")]
        [ExclusiveCmd(785220297422667846)]
        [DM]
        public class Manager : ModuleBase<SocketCommandContext>
        {
            [Command("UpdateCommandHelp")]
            public async Task UpdateCmdHelp()
            {
                try
                {
                    Hogwarts.UpdateCmdHelp();
                    await SendMsg("Sucess");
                }
                catch(Exception e)
                {
                    await SendMsg(e.Message + "\n" + e.StackTrace);
                }
            }

            [Command("studentsql")]
            public async Task StudentSql(ulong id, [Remainder] string query)
            {
                try
                {
                    Student student = new Student();
                    await student.InitDB(id);
                    await SendMsg((await student.ExecuteNonQuery(id, query)).ToString());
                }
                catch(Exception e)
                {
                    await SendMsg(e.Message + "\n" + e.StackTrace);
                }
            }
            
            [Command("msg_log_sql")]
            public async Task MsgLogSql([Remainder] string query)
            {
                try
                {
                    MessageHistory messageHistory = new MessageHistory();
                    await SendMsg((await messageHistory.ExecuteNonquery(query)).ToString());
                }
                catch(Exception e)
                {
                    await SendMsg(e.Message + "\n" + e.StackTrace);
                }
            }

            [Command("shutdown")]
            public async Task Shutdown()
            {
                await SendMsg($"{Context.User.Username}({Context.User.Id})이(가) shutdown을 실행했습니다.");
                Environment.Exit(0);
            }

            [Command("help")]
            public async Task Help()
            {
                await SendMsg("`UpdateCommandHelp`\n`studentsql`\n`msg_log_sql`\n`shutdown`");
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
