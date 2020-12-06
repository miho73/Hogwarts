using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Hogwarts.CmdExe.Attributes
{
    public class DM : PreconditionAttribute
    {
        public async override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if(typeof(SocketDMChannel).IsInstanceOfType(context.Channel))
            {
                string retmsg = $"DM에서는 그 명령을 사용할 수 없습니다.";
                await context.Channel.SendMessageAsync(retmsg);
                return PreconditionResult.FromError(retmsg);
            }
            return PreconditionResult.FromSuccess();
        }
    }
}
