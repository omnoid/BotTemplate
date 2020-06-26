using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using BotTemplate.Settings;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace BotTemplate.Commands
{
	class CommandHandler
	{
		private readonly DiscordSocketClient _client;
		private readonly CommandService _commands;
		private readonly IServiceProvider _services;
        private readonly CommandSettings _settings;

		public CommandHandler(DiscordSocketClient client, CommandService commands, IServiceProvider services, IOptions<CommandSettings> settings)
		{
			_client = client;
			_commands = commands;
			_services = services;
            _settings = settings.Value;
		}

		public async Task InitializeAsync()
		{
			await _commands.AddModulesAsync(
			assembly: Assembly.GetEntryAssembly(),
			services: _services);

            _commands.CommandExecuted += CommandExecutedAsync;
            _client.MessageReceived += HandleCommandsAsync;
		}

		public async Task HandleCommandsAsync(SocketMessage msg)
		{
			if (!(msg is SocketUserMessage message)) return;

			int argPos = 0;

            if (!(message.HasCharPrefix(_settings.CommandPrefix ?? '!', ref argPos) ||
                message.HasMentionPrefix(_client.CurrentUser, ref argPos)) ||
                message.Author.IsBot)
                return;

            var context = new SocketCommandContext(_client, message);

            await _commands.ExecuteAsync(
                context: context,
                argPos: argPos,
                services: _services);
        }

        public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            // if a command isn't found, log that info to console and exit this method
            if (!command.IsSpecified)
            {
                System.Console.WriteLine($"Command failed to execute for [{context.User.Username}] <-> [{result.ErrorReason}]!");
                return;
            }


            // log success to the console and exit this method
            if (result.IsSuccess)
            {
                System.Console.WriteLine($"Command [{command.Value.Name}] executed for -> [{context.User.Username}]");
                return;
            }


            // failure scenario, let's let the user know
            //await context.Channel.SendMessageAsync($"Sorry, {context.User.Username}... something went wrong -> [{result}]!");
        }
    }
}

