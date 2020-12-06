using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Text;
using System.Threading.Tasks;
using Hogwarts.CmdExe.Attributes;
using System.Linq;
using System.Collections.Generic;
using System;
using Newtonsoft.Json.Linq;

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
