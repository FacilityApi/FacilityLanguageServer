using Facility.Definition;

namespace Facility.LanguageServer
{
	internal static class FsdDefinitionUtility
	{
		public static ServiceMemberInfo GetMemberReferencedAtPosition(this ServiceInfo service, Position requestPosition)
		{
			return (
				from field in service.GetDescendants().OfType<ServiceFieldInfo>()
				let part = field.GetPart(ServicePartKind.TypeName)
				where part != null && requestPosition >= part.Position && requestPosition < part.EndPosition
				let type = service.GetFieldType(field)
				where type != null
				let name = type.GetMemberTypeName()
				where name != null
				select service.FindMember(name)).FirstOrDefault();
		}

		public static IEnumerable<ServicePart> GetReferencedServicePartsAtPosition(this ServiceInfo service, Position requestPosition, bool includeDeclaration)
		{
			var members = service.GetDescendants().OfType<ServiceMemberInfo>().ToList().AsReadOnly();

			// memberAtCursor will be null if the cursor is not on a member name.
			var memberAtCursor = members
				.Select(member =>
				{
					var part = member.GetPart(ServicePartKind.Name);
					var name = member.Name;

					return (part, name);
				})
				.Where(x => x.part != null && requestPosition >= x.part.Position && requestPosition < x.part.EndPosition)
				.Select(x => (x.name, x.part))
				.FirstOrDefault();

			var fields = service.GetDescendants().OfType<ServiceFieldInfo>().ToList().AsReadOnly();

			// fieldTypeNameAtCursor will be null if the cursor is not on a field type name.
			var fieldTypeNameAtCursor = fields
				.Select(field =>
				{
					var part = field.GetPart(ServicePartKind.TypeName);
					var typeName = service.GetFieldTypeName(field);

					return (part, typeName);
				})
				.Where(x => x.part != null && requestPosition >= x.part.Position && requestPosition < x.part.EndPosition)
				.Select(x => x.typeName)
				.FirstOrDefault();

			var referencedFields = fields
				.Select(field =>
				{
					var part = field.GetPart(ServicePartKind.TypeName);
					var typeName = service.GetFieldTypeName(field);

					var type = service.GetFieldType(field);
					var memberTypeName = type?.GetMemberTypeName();

					return (part, memberTypeName, typeName);
				})
				.Where(x => x.part != null && ((memberAtCursor.name != null && x.memberTypeName == memberAtCursor.name) || x.typeName == fieldTypeNameAtCursor))
				.Select(x => x.part);

			var referencedMembers = members
				.Select(member =>
				{
					var part = member.GetPart(ServicePartKind.Name);
					var name = member.Name;

					return (part, name);
				})
				.Where(x => x.part != null && (x.name == fieldTypeNameAtCursor || x.name == memberAtCursor.name))
				.Select(x => x.part);

			return includeDeclaration
				? referencedFields.Concat(referencedMembers)
				: referencedFields;
		}

		private static string GetMemberTypeName(this ServiceTypeInfo type)
		{
			switch (type.Kind)
			{
				case ServiceTypeKind.Dto:
					return type.Dto!.Name;
				case ServiceTypeKind.ExternalDto:
					return type.ExternalDto!.Name;
				case ServiceTypeKind.Enum:
					return type.Enum!.Name;
				case ServiceTypeKind.ExternalEnum:
					return type.ExternalEnum!.Name;
				case ServiceTypeKind.Array:
				case ServiceTypeKind.Result:
				case ServiceTypeKind.Map:
					return type.ValueType.GetMemberTypeName();
			}
			return null;
		}

		private static string GetFieldTypeName(this ServiceInfo service, ServiceFieldInfo field)
		{
			var type = service.GetFieldType(field);

			while (type?.ValueType is { } valueType)
				type = valueType;

			return type?.ToString() ?? field.TypeName;
		}
	}
}
