using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Hogwarts.CmdExe
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        public bool Sudo(SocketGuildUser auther)
        {
            bool flag = false;
            foreach (SocketRole role in auther.Roles)
            {
                if (role.ToString() == "@Principle")
                {
                    flag = true;
                    break;
                }
            }
            return flag;
        }

        public string GetUniqueProfile(SocketUser user)
        {
            return user.Username + "(" + user.Id + ")";
        }

        public async void MakePermissionError(string from, SocketUser issuer)
        {
            await Hogwarts.MakeWarnLog(from, "Permission denied: " + GetUniqueProfile(issuer));
        }
    }
}
