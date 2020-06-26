using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace BotTemplate.Commands.Modules
{
	[Group("info"), Summary("Bot information")]
	public class BotInfo : ModuleBase<SocketCommandContext>
	{

		[Command("say")]
		[Summary("Echoes a message.")]
		public Task SayAsync([Remainder][Summary("The text to echo")] string echo)
			=> ReplyAsync(echo);

		[Command("uptime")]
		[Summary("Time program has been running.")]
		[Alias("ut")]
		public async Task Uptime()
		{
			var embed = new EmbedBuilder();

			TimeSpan uptime = DateTime.Now - Program.StartupTime;
			uptime.ToString("g");

			embed.WithAuthor(Context.Client.CurrentUser)
				.WithTitle("Bot uptime")
				.WithDescription($"Bot has been running for following length of time (dd.hh:mm:ss):\n{uptime:dd\\.hh\\:mm\\:ss}");

			await ReplyAsync(embed: embed.Build());
		}
	}
}
