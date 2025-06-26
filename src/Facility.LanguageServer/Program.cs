using Facility.Definition;
using Facility.LanguageServer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Server;

////while (!System.Diagnostics.Debugger.IsAttached)
////{
////	await Task.Delay(100);
////}

var serviceInfos = new Dictionary<DocumentUri, ServiceInfo>();

var server = await LanguageServer.From(
	options =>
		options
			.WithInput(Console.OpenStandardInput())
			.WithOutput(Console.OpenStandardOutput())
			.ConfigureLogging(
				x => x
					.AddLanguageProtocolLogging()
					.SetMinimumLevel(LogLevel.Information))
			.WithServices(
				services => services
					.AddSingleton<IDictionary<DocumentUri, ServiceInfo>>(serviceInfos))
			.WithHandler<FsdSyncHandler>()
			.WithHandler<FsdDefinitionHandler>()
			.WithHandler<FsdReferenceHandler>()
			.WithHandler<FsdRenameHandler>()
			.WithHandler<FsdHoverHandler>()
			.WithHandler<FsdDocumentSymbolHandler>()).ConfigureAwait(false);

await server.WaitForExit.ConfigureAwait(false);
