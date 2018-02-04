using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using LangServer = OmniSharp.Extensions.LanguageServer.Server.LanguageServer;

namespace Facility.LanguageServer
{
	class Program
	{
		static async Task Main(string[] args)
		{
			//while (!System.Diagnostics.Debugger.IsAttached)
			//{
			//	await Task.Delay(100);
			//}

			var server = new LangServer(Console.OpenStandardInput(), Console.OpenStandardOutput(), new LoggerFactory());

			server.AddHandler(new FacilityServiceDefinitionDocumentHandler(server));

			await server.Initialize();
			await server.WaitForExit;
		}
	}
}
