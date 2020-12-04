using Discord.Commands;
using Hogwarts.Database;
using System;
using System.Threading.Tasks;

namespace Hogwarts.CmdExe
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        private readonly Student student = new Student();

        [Command("register")]
        [Summary("마법부에 신원을 등록합니다.")]
        public async Task Register()
        {
            //Check the channel
            if (Context.Channel.Id != 784244294445301790) return;

            if (await student.IsRegistered(Context.User.Id))
            {
                await SendMsg(Context.User.Username + "님은 이미 등록되어 있습니다.");
                return;
            }
            await student.AddDatum(new StudentRecord()
            {
                ID = Context.User.Id,
                Name = Context.User.Username,
                Dormitory = "Null"
            });
            await SendMsg(Context.User.Username + "님, 등록되었습니다. ");
        }

        public Task SendMsg(string text)
        {
            return Context.Channel.SendMessageAsync(text);
        }
    }
}
