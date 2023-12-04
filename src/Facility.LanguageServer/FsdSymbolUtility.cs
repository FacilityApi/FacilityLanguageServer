using Facility.Definition;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace Facility.LanguageServer;

internal static class FsdSymbolUtility
{
	public static List<SymbolInformationOrDocumentSymbol> GetServiceSymbols(this ServiceInfo service)
	{
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
				ServiceErrorSetInfo => SymbolKind.Interface,
				_ => SymbolKind.Null,
			};

			var maxColumn = memberNamePart.EndPosition.ColumnNumber;
			var maxLine = memberNamePart.EndPosition.LineNumber;

			var childSymbols = new List<DocumentSymbol>();
			foreach (var child in member.GetDescendants().OfType<ServiceElementWithAttributesInfo>())
			{
				var childNamePart = child.GetPart(ServicePartKind.Name);
				if (childNamePart == null)
					continue;

				maxColumn = Math.Max(maxColumn, childNamePart.EndPosition.ColumnNumber);
				maxLine = Math.Max(maxLine, childNamePart.EndPosition.LineNumber);

				var childTypePart = child.GetPart(ServicePartKind.TypeName);

				var childName = child switch
				{
					ServiceFieldInfo field => field.Name,
					ServiceErrorInfo error => error.Name,
					_ => null,
				};

				if (childName == null)
					continue;

				var childSymbol = new DocumentSymbol
				{
					Name = childName,
					Kind = SymbolKind.Field,
					Range = new Range(
						new Position(childNamePart.Position),
						new Position(
							new ServiceDefinitionPosition(
								childName,
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
}
