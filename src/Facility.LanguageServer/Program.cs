using System.Globalization;
using Facility.LanguageServer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Server;
using Serilog;

////while (!System.Diagnostics.Debugger.IsAttached)
////{
////	await Task.Delay(100);
////}

Log.Logger = new LoggerConfiguration()
	.Enrich.FromLogContext()
	.WriteTo.File(path: "log.txt", rollingInterval: RollingInterval.Day, formatProvider: new DateTimeFormatInfo())
	.MinimumLevel.Verbose()
	.CreateLogger();

var server = await LanguageServer.From(
	options =>
		options
			.WithInput(Console.OpenStandardInput())
			.WithOutput(Console.OpenStandardOutput())
			.ConfigureLogging(
				x => x
					.AddSerilog(Log.Logger)
					.AddLanguageProtocolLogging()
					.SetMinimumLevel(LogLevel.Debug))
			.WithHandler<FsdSyncHandler>()
			.WithHandler<FsdDefinitionHandler>()
			.WithHandler<FsdHoverHandler>()
			.WithServices(x => x.AddLogging(b => b.SetMinimumLevel(LogLevel.Trace)))).ConfigureAwait(false);

await server.WaitForExit.ConfigureAwait(false);
