using Facility.Definition;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

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

			var symbols = new List<SymbolInformationOrDocumentSymbol>();
			foreach (var member in service.Members)
			{
				var memberNamePart = member.GetPart(ServicePartKind.Name);
				if (memberNamePart == null)
					continue;

				var symbolKind = member switch
				{
					ServiceMethodInfo => SymbolKind.Method,
					ServiceDtoInfo => SymbolKind.Interface,
					ServiceEnumInfo => SymbolKind.Enum,
					ServiceExternalDtoInfo => SymbolKind.Interface,
					ServiceExternalEnumInfo => SymbolKind.Enum,
					_ => SymbolKind.Null,
				};

				var maxColumn = memberNamePart.EndPosition.ColumnNumber;
				var maxLine = memberNamePart.EndPosition.LineNumber;

				var childSymbols = new List<DocumentSymbol>();
				foreach (var child in member.GetDescendants().OfType<ServiceFieldInfo>())
				{
					var childNamePart = child.GetPart(ServicePartKind.Name);
					if (childNamePart == null)
						continue;

					maxColumn = Math.Max(maxColumn, childNamePart.EndPosition.ColumnNumber);
					maxLine = Math.Max(maxLine, childNamePart.EndPosition.LineNumber);

					var childTypePart = child.GetPart(ServicePartKind.TypeName);

					var childSymbol = new DocumentSymbol
					{
						Name = child.Name,
						Kind = SymbolKind.Field,
						Range = new Range(
							new Position(childNamePart.Position),
							new Position(
								new ServiceDefinitionPosition(
									child.Name,
									childTypePart?.EndPosition.LineNumber ?? childNamePart.EndPosition.LineNumber,
									childTypePart?.EndPosition.ColumnNumber ?? childNamePart.EndPosition.ColumnNumber))),
						SelectionRange = new Range(
							new Position(childNamePart.Position),
							new Position(childNamePart.EndPosition)),
					};
					childSymbols.Add(childSymbol);
				}

				foreach (var child in member.GetDescendants().OfType<ServiceEnumValueInfo>())
				{
					var childNamePart = child.GetPart(ServicePartKind.Name);
					if (childNamePart == null)
						continue;

					maxColumn = Math.Max(maxColumn, childNamePart.EndPosition.ColumnNumber);
					maxLine = Math.Max(maxLine, childNamePart.EndPosition.LineNumber);
					var childSymbol = new DocumentSymbol
					{
						Name = child.Name,
						Kind = SymbolKind.EnumMember,
						Range = new Range(
							new Position(childNamePart.Position),
							new Position(childNamePart.EndPosition)),
						SelectionRange = new Range(
							new Position(childNamePart.Position),
							new Position(childNamePart.EndPosition)),
					};
					childSymbols.Add(childSymbol);
				}

				var keywordPart = member.GetPart(ServicePartKind.Keyword);
				var minColumn = keywordPart?.Position.ColumnNumber ?? memberNamePart.Position.ColumnNumber;
				var minLine = keywordPart?.Position.LineNumber ?? memberNamePart.Position.LineNumber;

				var symbol = new DocumentSymbol
				{
					Name = member.Name,
					Kind = symbolKind,

					Range = new Range(
						new Position(
							new ServiceDefinitionPosition(member.Name, minLine, minColumn)),
						new Position(
							new ServiceDefinitionPosition(member.Name, maxLine + 1, maxColumn))),
					SelectionRange = new Range(
						new Position(memberNamePart.Position),
						new Position(memberNamePart.EndPosition)),
					Children = childSymbols,
				};
				symbols.Add(symbol);
			}

			return symbols;
		}

		public DocumentSymbolRegistrationOptions GetRegistrationOptions(DocumentSymbolCapability capability, ClientCapabilities clientCapabilities)
			=> new DocumentSymbolRegistrationOptions
			{
				DocumentSelector = DocumentSelector.ForLanguage("fsd"),
			};
	}
}
