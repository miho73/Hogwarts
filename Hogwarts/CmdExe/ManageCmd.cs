using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Text;
using System.Threading.Tasks;
using Hogwarts.CmdExe.Attributes;

//Command here will be applied globally

namespace Hogwarts.CmdExe
{
    public class ManageCmd : ModuleBase<SocketCommandContext>
    {
        [Command("whoami")]
        [Summary("Show who you are")]
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
        [Summary("Delete messages")]
        [Sudo("@Principle")]
        public async Task PurgeChat(int amount)
        {
            var messages = await Context.Channel.GetMessagesAsync(amount + 1).FlattenAsync();
            await (Context.Channel as SocketTextChannel).DeleteMessagesAsync(messages);
        }

        [Command("clear")]
        [Summary("Clear message")]
        [Sudo("@Principle")]
        public async Task ClearChat()
        {
            var messages = await Context.Channel.GetMessagesAsync(2147483647).FlattenAsync();
            await (Context.Channel as SocketTextChannel).DeleteMessagesAsync(messages);
        }

        public Task SendMsg(string text)
        {
            return Context.Channel.SendMessageAsync(text);
        }
    }
}
