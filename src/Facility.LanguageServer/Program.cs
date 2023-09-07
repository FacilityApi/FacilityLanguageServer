using Facility.LanguageServer;
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
			.WithHandler<FsdSyncHandler>()
			.WithHandler<FsdDefinitionHandler>()
			.WithHandler<FsdHoverHandler>()).ConfigureAwait(false);

await server.WaitForExit.ConfigureAwait(false);
