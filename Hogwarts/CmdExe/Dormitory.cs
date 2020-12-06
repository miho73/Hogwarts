using Discord.Commands;
using Discord.WebSocket;
using Hogwarts.Database;
using Hogwarts.Database.Log;
using Hogwarts.Database.Log.Dormitory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hogwarts.CmdExe.Attributes;

namespace Hogwarts.CmdExe
{
    [ExclusiveCmd(784077924776017980)]
    public class Dormitory : ModuleBase<SocketCommandContext>
    {
        private static readonly ulong GRIFFINDOR_CHANNEL_ID = 784202186686332968;
        private static readonly ulong HUFFLEPUFF_CHANNEL_ID = 784202500793171978;
        private static readonly ulong RAVENCLAW_CHANNEL_ID = 784202571060346941;
        private static readonly ulong SLYTHERIN_CHANNEL_ID = 784202928369303582;
        private readonly Dictionary<string, ulong> DormitoryRoles = new Dictionary<string, ulong>()
        {
            { "Gryffindor", GRIFFINDOR_CHANNEL_ID },
            { "Hufflepuff", HUFFLEPUFF_CHANNEL_ID },
            { "Ravenclaw", RAVENCLAW_CHANNEL_ID },
            { "Slytherin", SLYTHERIN_CHANNEL_ID }
        };
        private readonly string[] Dormitories = { "Gryffindor", "Hufflepuff", "Ravenclaw", "Slytherin" };
        private readonly Log log = new Log();

        public async Task AddLogD(string message, LOG_LEVEL lv)
        {
            await log.AddDormitoryLog(new LogLine()
            {
                Message = message,
                Log_lv = lv,
                Time = DateTimeOffset.Now
            });
        }

        private readonly Students student = new Students();
        [Command("mhat")]
        [Summary("기숙사를 배정합니다.")]
        public async Task AssignAffiliation()
        {
            try
            {
                //Allowed in auditorium only.
                if (Context.Channel.Id != 784077924776017980) return;

                SocketGuildUser user = Context.User as SocketGuildUser;
                if(await IsAlreadyHasAffiliation(user))
                {
                    await SendMsg("이미 기숙사에 배정받았습니다!");
                    await AddLogD("Already Assigned: " + Context.User.Username + "(" + Context.User.Id + ")", LOG_LEVEL.INFO);
                    return;
                }
                int dorCode = new Random().Next(0, 3);
                string dorName = Dormitories[dorCode];
                var role = Context.Guild.GetRole(DormitoryRoles[dorName]);

                await user.AddRoleAsync(role);
                await student.UpdateDormitory(dorName, Context.User.Id);
                await AddLogD("Added " + dorName + " to user " + Context.User.Username + "(" + Context.User.Id + ")", LOG_LEVEL.INFO);
                await SendMsg(dorName + "!");
            }
            catch(Exception e)
            {
                await AddLogD("Cannot add role to user " + Context.User.Username + "(" + Context.User.Id + "): " + e.Message, LOG_LEVEL.ERROR);
                await SendMsg("...");
            }
        }

        public async Task<bool> IsAlreadyHasAffiliation(SocketGuildUser user)
        {
            long dat = await student.ReadColum<long>(user.Id, "Dormitory");
            if (dat == default) throw new Exception("User is not registered");
            else if (dat == -1) return false;
            return true;
        }

        public Task SendMsg(string text)
        {
            return Context.Channel.SendMessageAsync(text);
        }
    }
}
