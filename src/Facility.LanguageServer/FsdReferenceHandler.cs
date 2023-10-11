using Facility.Definition;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace Facility.LanguageServer
{
	internal sealed class FsdReferenceHandler : FsdRequestHandler, IReferencesHandler
	{
		public FsdReferenceHandler(
			ILanguageServerFacade router,
			ILanguageServerConfiguration configuration,
			IDictionary<DocumentUri, ServiceInfo> serviceInfos)
			: base(router, configuration, serviceInfos)
		{
		}

		public ReferenceRegistrationOptions GetRegistrationOptions(ReferenceCapability capability, ClientCapabilities clientCapabilities) =>
			new ReferenceRegistrationOptions()
			{
				DocumentSelector = DocumentSelector,
			};

		public async Task<LocationContainer> Handle(ReferenceParams request, CancellationToken cancellationToken)
		{
			var documentUri = request.TextDocument.Uri;
			var service = GetService(documentUri);
			if (service == null)
				return null;

			var position = new Position(request.Position);

			var serviceParts = service.GetReferencedServicePartsAtPosition(position);

			var locations = serviceParts.Select(part => new Location()
			{
				Uri = documentUri,
				Range = new Range(
						new OmniSharp.Extensions.LanguageServer.Protocol.Models
							.Position(part!.Position.LineNumber - 1, part.Position.ColumnNumber - 1),
						new OmniSharp.Extensions.LanguageServer.Protocol.Models
							.Position(part.EndPosition.LineNumber - 1, part.EndPosition.ColumnNumber - 1)),
			});

			return new LocationContainer(locations);
		}
	}
}
