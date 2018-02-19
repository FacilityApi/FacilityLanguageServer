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
	sealed class FsdHoverHandler : FsdRequestHandler, IHoverHandler
	{
		public FsdHoverHandler(ILanguageServer router, IDictionary<Uri, ServiceInfo> serviceInfos)
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

		public void SetCapability(HoverCapability capability)
		{
		}

		public async Task<Hover> Handle(TextDocumentPositionParams request, CancellationToken token)
		{
			Uri documentUri = request.TextDocument.Uri;
			ServiceInfo service = GetService(documentUri);
			if (service == null)
				return null;

			var member = service.GetMemberReferencedAtPosition(new Position(request.Position));
			if (member != null)
			{
				var position = new Position(member.Position);
				return new Hover
				{
					Contents = GetMarkup(member),
					Range = new Range(position, position)
				};
			}

			return null;
		}

		private static MarkedStringsOrMarkupContent GetMarkup(IServiceMemberInfo member)
		{
			return new MarkedStringsOrMarkupContent(
				new MarkedString(member.Summary)
			);
		}
	}
}
