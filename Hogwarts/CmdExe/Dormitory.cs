using Discord.Commands;
using Discord.WebSocket;
using Hogwarts.Database;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hogwarts.CmdExe
{
    public partial class Dormitory : ModuleBase<SocketCommandContext>
    {
        private readonly Dictionary<string, ulong> DormitoryRoles = new Dictionary<string, ulong>()
        {
            { "Gryffindor", 784202186686332968 },
            { "Hufflepuff", 784202500793171978 },
            { "Ravenclaw", 784202571060346941 },
            { "Slytherin", 784202928369303582 }
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

        private readonly Student student = new Student();
        [Command("mhat")]
        [Summary("Assign your dormitory.")]
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
            if (dat == -1) return false;
            return true;
        }

        public Task SendMsg(string text)
        {
            return Context.Channel.SendMessageAsync(text);
        }
    }
}
