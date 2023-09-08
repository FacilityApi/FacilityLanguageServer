using Facility.Definition;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;

namespace Facility.LanguageServer
{
	internal abstract class FsdRequestHandler
	{
		protected FsdRequestHandler(
			ILanguageServerFacade router,
			ILanguageServerConfiguration configuration,
			IDictionary<DocumentUri, ServiceInfo> serviceInfos)
		{
			Router = router;
			Configuration = configuration;
			m_services = serviceInfos;
		}

		protected ILanguageServerFacade Router { get; }
		protected ILanguageServerConfiguration Configuration { get; }

		protected DocumentSelector DocumentSelector { get; } =
			new DocumentSelector(
				new DocumentFilter
				{
					Pattern = "**/*.fsd",
					Language = "fsd",
				});

		protected void SetService(DocumentUri documentUri, ServiceInfo service)
		{
			m_services[documentUri] = service;
		}

		protected ServiceInfo GetService(DocumentUri documentUri)
		{
			return m_services.TryGetValue(documentUri, out var service) ? service : null;
		}

		private readonly IDictionary<DocumentUri, ServiceInfo> m_services;
	}
}
