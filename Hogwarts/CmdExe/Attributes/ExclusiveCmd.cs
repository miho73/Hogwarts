using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hogwarts.CmdExe.Attributes
{
    public class ExclusiveCmd : PreconditionAttribute
    {
        private readonly List<ulong> AllowedChannelList;
        public ExclusiveCmd(params ulong[] channelsId)
        {
            AllowedChannelList = channelsId.ToList();
        }

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (AllowedChannelList.Contains(context.Channel.Id)) return await Task.FromResult(PreconditionResult.FromSuccess());
            else return await Task.FromResult(PreconditionResult.FromError("You cannot run such command in current room"));
        }
    }
}
