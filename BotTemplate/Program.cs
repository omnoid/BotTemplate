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
			_configuration = new ConfigurationBuilder()
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
				.AddEnvironmentVariables()
				.Build();
			_services = BuildServiceProvider();
			StartupTime = DateTime.Now;
			Log.Logger.Information("Program initalized, starting connection with Discord.");
		}

		public async Task MainAsync()
		{
			var client = _services.GetRequiredService<DiscordSocketClient>();
			_client = client;

			client.Log += LogAsync;
			client.Ready += ReadyAsync;
			_services.GetRequiredService<CommandService>().Log += LogAsync;

			await client.LoginAsync(TokenType.Bot, _configuration.GetSection("Omnic:Token").Value);
			await client.StartAsync();

			await _services.GetRequiredService<CommandHandler>().InitializeAsync();

			await Task.Delay(-1);
		}

		private Task LogAsync(LogMessage log)
		{
			Console.WriteLine(log.ToString());
			return Task.CompletedTask;
		}

		private Task ReadyAsync()
		{
			LastLogin = DateTime.Now;
			Log.Information("Connected as -> [{CurrentUser}]", _client.CurrentUser);
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
