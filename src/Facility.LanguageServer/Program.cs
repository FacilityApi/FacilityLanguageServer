using Facility.Definition;
using Facility.LanguageServer;
using Microsoft.Extensions.Logging;
using LangServer = OmniSharp.Extensions.LanguageServer.Server.LanguageServer;
////while (!System.Diagnostics.Debugger.IsAttached)
////{
////	await Task.Delay(100);
////}

var server = new LangServer(Console.OpenStandardInput(), Console.OpenStandardOutput(), new LoggerFactory());

var serviceInfos = new Dictionary<Uri, ServiceInfo>();
server.AddHandler(new FsdSyncHandler(server, serviceInfos));
server.AddHandler(new FsdDefinitionHandler(server, serviceInfos));
server.AddHandler(new FsdHoverHandler(server, serviceInfos));

await server.Initialize().ConfigureAwait(false);
await server.WaitForExit.ConfigureAwait(false);
