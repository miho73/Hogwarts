using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Hogwarts.CmdExe.Attributes
{
    public class SudoAttribute : PreconditionAttribute
    {
        private readonly string _name;
        public SudoAttribute(string name) => _name = name;

        public async override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (context.User is SocketGuildUser gUser)
            {
                if (gUser.Roles.Any(r => r.Name == _name)) return PreconditionResult.FromSuccess();
                else
                {
                    string retmsg = $"권한 거부: *{_name}*만이 이 명령을 수행할 수 있습니다.";
                    await context.Channel.SendMessageAsync(retmsg);
                    return PreconditionResult.FromError(retmsg);
                }
            }
            else
            {
                string retmsg = $"권한 거부: *{_name}*만이 이 명령을 수행할 수 있습니다.";
                await context.Channel.SendMessageAsync(retmsg);
                return PreconditionResult.FromError(retmsg);
            }
        }
    }
}
