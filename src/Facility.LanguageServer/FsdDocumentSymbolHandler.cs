using Facility.Definition;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;

namespace Facility.LanguageServer
{
	internal sealed class FsdDocumentSymbolHandler : FsdRequestHandler, IDocumentSymbolHandler
	{
		public FsdDocumentSymbolHandler(ILanguageServerFacade router, ILanguageServerConfiguration configuration, IDictionary<DocumentUri, ServiceInfo> serviceInfos)
			: base(router, configuration, serviceInfos)
		{
		}

		public async Task<SymbolInformationOrDocumentSymbolContainer> Handle(DocumentSymbolParams request, CancellationToken cancellationToken)
		{
			var documentUri = request.TextDocument.Uri;
			var service = GetService(documentUri);
			if (service == null)
				return null;

			var symbols = service.GetServiceSymbols();
			return symbols;
		}

		public DocumentSymbolRegistrationOptions GetRegistrationOptions(DocumentSymbolCapability capability, ClientCapabilities clientCapabilities)
			=> new()
			{
				DocumentSelector = DocumentSelector.ForLanguage("fsd"),
			};
	}
}
