using Facility.LanguageServer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Server;

////while (!System.Diagnostics.Debugger.IsAttached)
////{
////	await Task.Delay(100);
////}

var server = await LanguageServer.From(
	options =>
		options
			.WithInput(Console.OpenStandardInput())
			.WithOutput(Console.OpenStandardOutput())
			.ConfigureLogging(
				x => x
					.AddLanguageProtocolLogging()
					.SetMinimumLevel(LogLevel.Debug))
			.WithHandler<FsdSyncHandler>()
			.WithHandler<FsdDefinitionHandler>()
			.WithHandler<FsdHoverHandler>()
			.WithServices(x => x.AddLogging(b => b.SetMinimumLevel(LogLevel.Trace)))).ConfigureAwait(false);

await server.WaitForExit.ConfigureAwait(false);
