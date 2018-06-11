using Discord.Commands;
using System.Threading.Tasks;
using Discord;

namespace DiscordBot
{
    [Group("Schedule")]
    public class InfoModule : ModuleBase<SocketCommandContext>
    {
        // ~say hello -> hello
        [Command("add")]
        [Summary("Add to schedule.")]

        public async Task addScheduleAsync([Summary("Name of this Schedule to Add")] string name,[Summary("Days in mon-thu style")] string days, [Summary("Time in HH:MM format with 00:00-00:00 formatting")] string time, [Summary("Message to be displayed")] string message)
        {
            // ReplyAsync is a method on ModuleBase
            await ReplyAsync("stuff");
        }

        [Command("remove")]
        [Summary("Add to schedule.")]

        public async Task removeScheduleAsync([Summary("Name of this Schedule to Remove")] string name)
        {
            // ReplyAsync is a method on ModuleBase
            await ReplyAsync("stuff");
        }
    }
}
