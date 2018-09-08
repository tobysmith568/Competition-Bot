using Discord;
using Discord.Commands;
using Discord.Net.Providers.WS4Net;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using Discord.Rest;

namespace Competition_Bot
{
    class Program
    {
        #region Trap application termination
        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

        private delegate bool EventHandler(CtrlType sig);
        static EventHandler _handler;

        enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        private static bool Handler(CtrlType sig)
        {
            Logger(new LogMessage(LogSeverity.Info, "Program", "Exiting system due to external CTRL-C, or process kill, or shutdown"));

            Thread.Sleep(1000);

            Logger(new LogMessage(LogSeverity.Info, "Program", "Cleanup complete - now closing."));

            Environment.Exit(0);

            return true;
        }

        #endregion

        public readonly DiscordSocketClient _client;

        public static Program program;

        private readonly IServiceCollection _map = new ServiceCollection();
        private readonly CommandService _commands = new CommandService();
        private IServiceProvider _services;

        static void Main(string[] args)
        {
            Console.Title = "";

            _handler += new EventHandler(Handler);
            SetConsoleCtrlHandler(_handler, true);

            program = new Program();
            program.MainAsync().GetAwaiter().GetResult();
        }

        private Program()
        {
            if (File.Exists("Bot.json"))
                ConfigFile.FromJson(File.ReadAllText("Bot.json"));

            if (ConfigFile.Token == null)
            {
                Logger(LogSeverity.Error, "Program", "Open the file \"Bot.json\" and add your bot account's token to the line \"Token:\".");
                File.WriteAllText("Bot.json", JsonConvert.SerializeObject(new ConfigFile(), Formatting.Indented));
                Console.ReadKey();
                Environment.Exit(0);
            }

            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Debug,
                MessageCacheSize = 50,
                WebSocketProvider = WS4NetProvider.Instance
            });
        }

        public static Task Logger(LogSeverity severity, string source, string message)
        {
            return Logger(new LogMessage(severity, source, message));
        }

        public static Task Logger(LogMessage message)
        {
            var cc = Console.ForegroundColor;
            switch (message.Severity)
            {
                case LogSeverity.Critical:
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogSeverity.Verbose:
                case LogSeverity.Debug:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
            }
            Console.WriteLine($"{DateTime.Now,-19} [{message.Severity,8}] {message.Source}: {message.Message}");
            Console.ForegroundColor = cc;

            return Task.CompletedTask;
        }

        private async Task MainAsync()
        {
            _client.Log += Logger;

            await InitCommands();

            await _client.LoginAsync(TokenType.Bot, ConfigFile.Token);
            await _client.StartAsync();

            await _client.SetGameAsync(ConfigFile.Game);

            await Task.Delay(-1);
        }

        private async Task InitCommands()
        {
            _services = _map.BuildServiceProvider();

            await _commands.AddModuleAsync<Commands>(null);

            _client.MessageReceived += HandleCommandAsync;
            _client.GuildAvailable += _client_GuildAvailable;
            _client.UserJoined += _client_UserJoined;
        }

        private async Task _client_UserJoined(SocketGuildUser user)
        {
            string username = user.Username;
            username = username.Replace("💰", "");
            username = username + " 💰 100";
            await ((IGuildUser)user).ModifyAsync(u => u.Nickname = username);
        }

        private async Task _client_GuildAvailable(SocketGuild arg)
        {
            //Check all the users have some points
            foreach (IUser user in arg.Users.Where(u => u.IsBot == false)) 
            {
                if (((IGuildUser)user).Guild.OwnerId != user.Id)
                    if (((IGuildUser)user).Nickname == null)
                        await((IGuildUser)user).ModifyAsync(u => u.Nickname = user.Username + " 💰 100");
            }
        }

        private async Task HandleCommandAsync(SocketMessage message)
        {
            var msg = message as SocketUserMessage;
            if (msg == null) return;

            int pos = 0;
            //Replace the ! below with your command prefix
            if (msg.HasCharPrefix('!', ref pos) || msg.HasMentionPrefix(_client.CurrentUser, ref pos))
            {
                var context = new SocketCommandContext(_client, msg);
                var result = await _commands.ExecuteAsync(context, pos, _services);

                if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                    await msg.Channel.SendMessageAsync(result.ErrorReason);
            }
        }
    }
}