using System;
using System.Threading.Tasks;
using System.Reflection;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot
{

    class Program
    {
        private CommandService _commands;
        private DiscordSocketClient _client;
        private IServiceProvider _services;

        private static void Main(string[] args) => new Program().StartAsync().GetAwaiter().GetResult();

        public async Task StartAsync()
        {
            _client = new DiscordSocketClient();
            _commands = new CommandService();

            // Avoid hard coding your token. Use an external source instead in your code.
            string token = "MjkyMjM5Mzk0MTM4NDg4ODMz.DfF7VQ.DIgoNjOlAU_0qMD8hNFjFTBqsCI";

            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .BuildServiceProvider();

            await InstallCommandsAsync();
            try
            {
                await _client.LoginAsync(TokenType.Bot, token);
                await _client.StartAsync();

                _client.MessageUpdated += MessageUpdated;
                _client.UserJoined += _client_UserJoined;
                _client.GuildMemberUpdated += _client_GuildMemberUpdated;
                _client.Ready += () =>
                {
                    Console.WriteLine("Bot is connected!");

                    return Task.CompletedTask;
                };
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        

            await Task.Delay(-1);
        }

        public async Task InstallCommandsAsync()
        {
            // Hook the MessageReceived Event into our Command Handler
            _client.MessageReceived += HandleCommandAsync;
            // Discover all of the commands in this assembly and load them.
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            // Don't process the command if it was a System Message
            var message = messageParam as SocketUserMessage;

            if (message == null)
                return;

            var context = new SocketCommandContext(_client, message);

            if (message.Content.ToLower().Contains("i accept") && message.Author != _client.CurrentUser)
            {
                var user = context.User as SocketGuildUser;
                await user.AddRoleAsync(context.Guild.Roles.First(r => r.Name == "Member"));
                //await user.RemoveRoleAsync(context.Guild.Roles.First(r => r.Name == "everyone"));
                await context.Channel.SendMessageAsync("Welcome to the community " + user.Mention);
                return;
            }

            int argPos = 0;
            // Determine if the message is a command, based on if it starts with '!' or a mention prefix, or if 
            if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos)))
                return;
            else
            {

                // Create a Command Context

                // Execute the command. (result does not indicate a return value, 
                // rather an object stating if the command executed successfully)
                var result = await _commands.ExecuteAsync(context, argPos, _services);

                if (!result.IsSuccess)
                    await context.Channel.SendMessageAsync(result.ErrorReason);
            }
        }




        private async Task MessageUpdated(Cacheable<IMessage, ulong> before, SocketMessage after, ISocketMessageChannel channel)
        {
            // If the message was not in the cache, downloading it will result in getting a copy of `after`.
            var message = await before.GetOrDownloadAsync();
            Console.WriteLine($"{message} -> {after}");
        }

        private async Task _client_UserJoined(SocketGuildUser arg)
        {
            await sendUserAcceptance(arg);
        }

        private async Task _client_GuildMemberUpdated(SocketGuildUser arg1, SocketGuildUser arg2)
        {
            if(arg1.Status == UserStatus.Offline && (arg2.Status == UserStatus.Idle || arg2.Status == UserStatus.Online))
                await sendUserAcceptance(arg1);
        }


        private async Task sendUserAcceptance(SocketGuildUser user)
        {
            IRole toIgnore = user.Guild.Roles.First(r => r.Name == "Member");
            if (!user.Roles.Contains(toIgnore))
            {
                var chat = _client.GroupChannels.First(c => c.Name == "welcome");
                await chat.SendMessageAsync("Welcome to the server " + user.Mention + " In order to proceed you will need to accept the following rules " + Environment.NewLine + "If you are happy with the above please type  **I Accept**");
            }
            else
                return;
        }

        private async Task performDatabaseStuff()
        {
            using (var db = new BloggingContext())
            {
                var blogs = db.Blogs
                .Include(blog => blog.Posts)
                .ToList();

                db.Blogs.Add(new Blog { Url = "http://blogs.msdn.com/adonet", Posts = new List<Post>() { new Post() { } } });

                var count = db.SaveChanges();
                Console.WriteLine("{0} records saved to database", count);

                Console.WriteLine();
                Console.WriteLine("All blogs in database:");
                foreach (var blog in db.Blogs)
                {
                    Console.WriteLine(" - {0}", blog.Url);
                }
            }
        }
    }


}
