using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Facility.Definition;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Server;

namespace Facility.LanguageServer
{
	internal sealed class FsdDefinitionHandler : FsdRequestHandler, IDefinitionHandler
	{
		public FsdDefinitionHandler(ILanguageServer router, IDictionary<Uri, ServiceInfo> serviceInfos)
			: base(router, serviceInfos)
		{
		}

		public TextDocumentRegistrationOptions GetRegistrationOptions()
		{
			return new TextDocumentRegistrationOptions
			{
				DocumentSelector = DocumentSelector
			};
		}

		public void SetCapability(DefinitionCapability capability)
		{
		}

		public async Task<LocationOrLocations> Handle(TextDocumentPositionParams request, CancellationToken token)
		{
			Uri documentUri = request.TextDocument.Uri;
			ServiceInfo service = GetService(documentUri);
			if (service == null)
				return null;

			var member = service.GetMemberReferencedAtPosition(new Position(request.Position));
			if (member?.Position != null)
			{
				var position = new Position(member.GetPart(ServicePartKind.Name)?.Position ?? member.Position);
				return new LocationOrLocations(new Location { Uri = documentUri, Range = new Range(position, position) });
			}

			return new LocationOrLocations();
		}
	}
}
