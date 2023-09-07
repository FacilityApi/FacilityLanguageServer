using Facility.Definition;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;

namespace Facility.LanguageServer
{
	internal abstract class FsdRequestHandler
	{
		protected FsdRequestHandler(ILanguageServer router, IDictionary<DocumentUri, ServiceInfo> serviceInfos)
		{
			Router = router;
			m_services = serviceInfos;
		}

		protected ILanguageServer Router { get; }

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
