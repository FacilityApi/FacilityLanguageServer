using Facility.Definition;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Server;

namespace Facility.LanguageServer
{
	internal abstract class FsdRequestHandler
	{
		protected FsdRequestHandler(ILanguageServer router, IDictionary<Uri, ServiceInfo> serviceInfos)
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

		protected void SetService(Uri documentUri, ServiceInfo service)
		{
			m_services[documentUri] = service;
		}

		protected ServiceInfo GetService(Uri documentUri)
		{
			return m_services.TryGetValue(documentUri, out ServiceInfo service) ? service : null;
		}

		private readonly IDictionary<Uri, ServiceInfo> m_services;
	}
}
