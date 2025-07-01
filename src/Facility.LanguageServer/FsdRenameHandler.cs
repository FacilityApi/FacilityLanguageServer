using Facility.Definition;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace Facility.LanguageServer;

internal sealed class FsdRenameHandler : FsdRequestHandler, IRenameHandler, IPrepareRenameHandler
{
	public FsdRenameHandler(
		ILanguageServerFacade router,
		ILanguageServerConfiguration configuration,
		IDictionary<DocumentUri, ServiceInfo> serviceInfos)
		: base(router, configuration, serviceInfos)
	{
	}

	public RenameRegistrationOptions GetRegistrationOptions(RenameCapability capability, ClientCapabilities clientCapabilities) =>
		new()
		{
			DocumentSelector = DocumentSelector,
			PrepareProvider = true,
		};

	public Task<RangeOrPlaceholderRange> Handle(PrepareRenameParams request, CancellationToken cancellationToken)
	{
		var documentUri = request.TextDocument.Uri;
		var service = GetService(documentUri);
		if (service == null)
			return Task.FromResult<RangeOrPlaceholderRange>(null);

		var members = new List<ServiceMemberInfo>();
		members.AddRange(service.GetDescendants().OfType<ServiceDtoInfo>());
		members.AddRange(service.GetDescendants().OfType<ServiceExternalDtoInfo>());
		members.AddRange(service.GetDescendants().OfType<ServiceEnumInfo>());
		members.AddRange(service.GetDescendants().OfType<ServiceExternalEnumInfo>());
		var memberRangeAtCursor = members
			.Select(member =>
			{
				var part = member.GetPart(ServicePartKind.Name);
				return part is null
					? null
					: new Range(new Position(part.Position), new Position(part.EndPosition));
			})
			.FirstOrDefault(range => range != null && request.Position >= range.Start && request.Position < range.End);

		if (memberRangeAtCursor is not null)
			return Task.FromResult<RangeOrPlaceholderRange>(memberRangeAtCursor);

		var fields = service.GetDescendants().OfType<ServiceFieldInfo>().ToList().AsReadOnly();
		var fieldRangeAtCursor = fields
			.Select(field =>
			{
				var type = service.GetFieldType(field);
				var part = field.GetPart(ServicePartKind.TypeName);

				if (type is null || part is null || !s_isRenamable.Contains(type.Kind))
					return null;

				part = FsdDefinitionUtility.GetValueTypePart(field.TypeName, part);
				return new Range(new Position(part.Position), new Position(part.EndPosition));
			})
			.FirstOrDefault(range => range != null && request.Position >= range.Start && request.Position < range.End);

		if (fieldRangeAtCursor is not null)
			return Task.FromResult<RangeOrPlaceholderRange>(fieldRangeAtCursor);

		return Task.FromResult<RangeOrPlaceholderRange>(null);
	}

	public async Task<WorkspaceEdit> Handle(RenameParams request, CancellationToken cancellationToken)
	{
		var documentUri = request.TextDocument.Uri;
		var service = GetService(documentUri);
		if (service == null)
			return null;

		var position = new Position(request.Position);

		var serviceParts = service.GetReferencedServicePartsAtPosition(position, true);

		var ranges = serviceParts
			.Select(part => new Range(new Position(part!.Position), new Position(part.EndPosition)))
			.ToList();

		var changes = new Dictionary<DocumentUri, IEnumerable<TextEdit>>();

		var textEdits = new List<TextEdit>();
		foreach (var range in ranges)
		{
			textEdits.Add(
				new TextEdit
				{
					Range = range,
					NewText = request.NewName,
				});
		}
		changes.Add(documentUri, textEdits);

		return new WorkspaceEdit
		{
			Changes = changes,
		};
	}

	private static readonly HashSet<ServiceTypeKind> s_isRenamable = new()
	{
		ServiceTypeKind.Dto,
		ServiceTypeKind.ExternalDto,
		ServiceTypeKind.Enum,
		ServiceTypeKind.ExternalEnum,
		ServiceTypeKind.Array,
		ServiceTypeKind.Nullable,
		ServiceTypeKind.Map,
		ServiceTypeKind.Result,
	};
}
