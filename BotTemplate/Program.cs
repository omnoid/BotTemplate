using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BotTemplate.Commands;
using BotTemplate.Commands.Modules;
using BotTemplate.Settings;
using System;
using System.Threading.Tasks;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace BotTemplate
{
	class Program
	{
		
		private DiscordSocketClient _client;
		private readonly IConfiguration _configuration;
		private readonly IServiceProvider _services;
		public static DateTime StartupTime { get; private set; }
		public static DateTime LastLogin { get; private set; }


		//Entrypoint
		static void Main(string[] args)
		{
			Log.Logger = new LoggerConfiguration()
				.WriteTo.Console()
				.MinimumLevel.Information()
				.CreateLogger();

			try
			{
				//Starts Program in async context, calling on Program constructor.
				new Program().MainAsync().GetAwaiter().GetResult();
			}
			catch(Exception e)
			{
				Log.Logger.Error("Program exited unexpectedly {Exception}", e);
			}
			finally
			{
				//flush logger
			}
		}

		public Program()
		{
			//Reads configuration from appsettings.json
			_configuration = new ConfigurationBuilder()
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
				.AddEnvironmentVariables()
				.Build();

			//Sets up Dependency Injection
			_services = BuildServiceProvider();

			StartupTime = DateTime.Now;
			Log.Logger.Information("Program initalized, starting connection with Discord.");
		}

		public async Task MainAsync()
		{
			//Fetches global discord client from dependency injection system (ServiceCollection)
			var client = _services.GetRequiredService<DiscordSocketClient>();
			_client = client;

			//Sets up logging of Discord events, and function for when client logs in
			client.Log += LogAsync;
			client.Ready += ReadyAsync;

			_services.GetRequiredService<CommandService>().Log += LogAsync;

			//Logs into Discord as a bot using globale client
			await client.LoginAsync(TokenType.Bot, _configuration.GetSection("Bot:Token").Value);
			await client.StartAsync();

			await _services.GetRequiredService<CommandHandler>().InitializeAsync();

			await Task.Delay(-1);
		}

		private Task LogAsync(LogMessage log)
		{
			Log.Logger.Information(log.ToString());
			return Task.CompletedTask;
		}

		private Task ReadyAsync()
		{
			LastLogin = DateTime.Now;
			Log.Logger.Information("Connected as -> [{CurrentUser}]", _client.CurrentUser);
			return Task.CompletedTask;
		}

		private IServiceProvider BuildServiceProvider() => new ServiceCollection()
			.Configure<CommandSettings>(options => options = _configuration.GetSection(CommandSettings.Command).Get<CommandSettings>())
			.AddSingleton<DiscordSocketClient>()
			.AddSingleton<CommandService>()
			.AddSingleton<CommandHandler>()
			.BuildServiceProvider();
	}
}
