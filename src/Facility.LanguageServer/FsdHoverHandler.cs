using Facility.Definition;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;

namespace Facility.LanguageServer
{
	internal sealed class FsdHoverHandler : FsdRequestHandler, IHoverHandler
	{
		public FsdHoverHandler(
			ILanguageServerFacade router,
			ILanguageServerConfiguration configuration,
			IDictionary<DocumentUri, ServiceInfo> serviceInfos)
			: base(router, configuration, serviceInfos)
		{
		}

		public HoverRegistrationOptions GetRegistrationOptions(HoverCapability capability, ClientCapabilities clientCapabilities)
		{
			return new HoverRegistrationOptions()
			{
				DocumentSelector = DocumentSelector,
			};
		}

		public void SetCapability(HoverCapability capability)
		{
		}

		public async Task<Hover> Handle(HoverParams request, CancellationToken cancellationToken)
		{
			var documentUri = request.TextDocument.Uri;
			var service = GetService(documentUri);
			if (service == null)
				return null;

			var member = service.GetMemberReferencedAtPosition(new Position(request.Position));
			if (member != null)
			{
				return new Hover
				{
					Contents = GetMarkup(member),
				};
			}

			return null;
		}

		private static MarkedStringsOrMarkupContent GetMarkup(ServiceMemberInfo member)
		{
			return new MarkedStringsOrMarkupContent(
				new MarkupContent { Kind = MarkupKind.PlainText, Value = member.Summary });
		}
	}
}
