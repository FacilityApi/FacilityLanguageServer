using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Facility.Definition;
using Microsoft.Extensions.Logging;
using LangServer = OmniSharp.Extensions.LanguageServer.Server.LanguageServer;

namespace Facility.LanguageServer
{
	internal sealed class Program
	{
		private static async Task Main()
		{
			////while (!System.Diagnostics.Debugger.IsAttached)
			////{
			////	await Task.Delay(100);
			////}

			var server = new LangServer(Console.OpenStandardInput(), Console.OpenStandardOutput(), new LoggerFactory());

			var serviceInfos = new Dictionary<Uri, ServiceInfo>();
			server.AddHandler(new FsdSyncHandler(server, serviceInfos));
			server.AddHandler(new FsdDefinitionHandler(server, serviceInfos));
			server.AddHandler(new FsdHoverHandler(server, serviceInfos));

			await server.Initialize();
			await server.WaitForExit;
		}
	}
}
