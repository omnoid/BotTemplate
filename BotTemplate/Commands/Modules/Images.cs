using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BotTemplate.Commands.Modules
{
	class Images : ModuleBase<SocketCommandContext>
	{
		[Command("Shinoblob")]
		public async Task PostShinoblob()
		{
			await ReplyAsync("https://cdn.discordapp.com/attachments/199979364501159936/721425843775078532/1592067550003.png");
		}
	}
}
