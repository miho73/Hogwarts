﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Hogwarts.Database;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Hogwarts
{
    public partial class Hogwarts
    {
        private readonly DiscordSocketClient discordSocket;
        private readonly CommandService commands;
        private readonly MessageHistory messageHistory;
        private readonly JObject settingsObject;

        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            new Hogwarts().MainAsync().GetAwaiter().GetResult();
        }
        public Hogwarts()
        {
            if (!File.Exists("settings.json")) goto Setup;
            string settingsJson = File.ReadAllText("settings.json");
            settingsObject = JObject.Parse(settingsJson);

            Setup:
            if(settingsObject != null)
            {
                discordSocket = new DiscordSocketClient();
                commands = new CommandService();
                discordSocket.Log += Log;
                discordSocket.Ready += Ready;
                discordSocket.MessageReceived += MessageReceivedAsync;
                discordSocket.ReactionAdded += ReactionReceivedAsync;
                commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: null);
                messageHistory = new MessageHistory();
            }
            else
            {
                Console.WriteLine("No settings file founds");
                Environment.Exit(0);
            }
        }

        public async Task MainAsync()
        {
            await MakeInfoLog("Starter", "Starting server");
            await MakeInfoLog("Starter", "Loading settings");
            await discordSocket.LoginAsync(TokenType.Bot, settingsObject.Value<string>("Token"));
            await discordSocket.StartAsync();
            await Task.Delay(-1);
        }

        private static Task Log(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }

        public static Task MakeInfoLog(string from, string msg)
        {
            return Log(new LogMessage(LogSeverity.Info, from, msg));
        }
        public static Task MakeWarnLog(string from, string msg)
        {
            return Log(new LogMessage(LogSeverity.Warning, from, msg));
        }
        public static Task MakeErrLog(string from, string msg)
        {
            return Log(new LogMessage(LogSeverity.Error, from, msg));
        }
        public static Task MakeFatalLog(string from, string msg)
        {
            return Log(new LogMessage(LogSeverity.Critical, from, msg));
        }

        private Task Ready()
        {
            Console.WriteLine($"{discordSocket.CurrentUser} Connected!");
            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(SocketMessage message)
        {
            if (message.Author.Id == discordSocket.CurrentUser.Id) return;

            await messageHistory.AddDatum(new History()
            {
                Message = message.Content,
                MsgID = message.Id,
                UsrID = message.Author.Id,
                UsrName = message.Author.Username,
                ChannelID = message.Channel.Id,
                Time = DateTimeOffset.Now,
                Channel_name = message.Channel.Name
            });

            var context = new SocketCommandContext(discordSocket, message as SocketUserMessage);
            await commands.ExecuteAsync(
                context: context,
                argPos: 0,
                services: null
           );
        }

        private async Task ReactionReceivedAsync(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            Console.WriteLine(channel.Id);

            if (channel.Id == 784088929351630858)
            {
                var msg = await message.GetOrDownloadAsync();
                if (msg != null && reaction.User.IsSpecified)
                    Console.WriteLine($"{reaction.User.Value} just added a reaction '{reaction.Emote}' " +
                                      $"to {msg.Author}'s message ({msg.Id}).");
            }
        }
    }
}