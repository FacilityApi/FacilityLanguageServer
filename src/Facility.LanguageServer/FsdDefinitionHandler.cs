using Facility.Definition;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace Facility.LanguageServer
{
	internal sealed class FsdDefinitionHandler : FsdRequestHandler, IDefinitionHandler
	{
		public FsdDefinitionHandler(
			ILanguageServerFacade router,
			ILanguageServerConfiguration configuration,
			IDictionary<DocumentUri, ServiceInfo> serviceInfos)
			: base(router, configuration, serviceInfos)
		{
		}

		public DefinitionRegistrationOptions GetRegistrationOptions(DefinitionCapability capability, ClientCapabilities clientCapabilities)
		{
			return new DefinitionRegistrationOptions()
			{
				DocumentSelector = DocumentSelector,
			};
		}

		public void SetCapability(DefinitionCapability capability)
		{
		}

		public async Task<LocationOrLocationLinks> Handle(DefinitionParams request, CancellationToken cancellationToken)
		{
			var documentUri = request.TextDocument.Uri;
			var service = GetService(documentUri);
			if (service == null)
				return null;

			var member = service.GetMemberReferencedAtPosition(new Position(request.Position));
			if (member?.Position != null)
			{
				var position = new Position(member.GetPart(ServicePartKind.Name)?.Position ?? member.Position);
				return new LocationOrLocationLinks(new Location { Uri = documentUri, Range = new Range(position, position) });
			}

			return new LocationOrLocationLinks();
		}
	}
}
