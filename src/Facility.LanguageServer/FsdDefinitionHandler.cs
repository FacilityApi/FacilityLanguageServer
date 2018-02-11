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
	sealed class FsdDefinitionHandler : FsdRequestHandler, IDefinitionHandler
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

			Position? position = null;

			Position requestPosition = new Position(request.Position);
			var infoAtPosition = service.Find(requestPosition);
			if (infoAtPosition is ServiceFieldInfo field)
			{
				if (requestPosition >= field.TypeNamePosition)
				{
					var fieldType = service.GetFieldType(field);
					var typeName = fieldType.GetMemberTypeName();
					if (typeName != null)
					{
						var typeInfo = service.FindMember(typeName);
						if (typeInfo?.Position != null)
							position = new Position(typeInfo.Position);
					}
				}
			}

			return position != null ?
				new LocationOrLocations(new Location { Uri = documentUri, Range = new Range(position, position) }) :
				new LocationOrLocations();
		}
	}
}
