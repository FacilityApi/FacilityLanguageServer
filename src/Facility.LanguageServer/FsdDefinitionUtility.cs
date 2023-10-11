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

		public static IEnumerable<ServicePart> GetReferencedServicePartsAtPosition(this ServiceInfo service, Position requestPosition)
		{
			var members = service.GetDescendants().OfType<ServiceMemberInfo>().ToList().AsReadOnly();

			// memberNameAtPosition will be null if the cursor is not on a member name.
			var memberNameAtCursor = members
				.Select(member =>
				{
					var part = member.GetPart(ServicePartKind.Name);
					var name = member.Name;

					return new { part, name };
				})
				.Where(x => x.part != null && requestPosition >= x.part.Position && requestPosition < x.part.EndPosition)
				.Select(x => x.name)
				.FirstOrDefault();

			var fields = service.GetDescendants().OfType<ServiceFieldInfo>().ToList().AsReadOnly();

			// fieldTypeNameAtPosition will be null if the cursor is not on a field type name.
			var fieldTypeNameAtCursor = fields
				.Select(field =>
				{
					var part = field.GetPart(ServicePartKind.TypeName);
					var typeName = field.TypeName;

					return new { part, typeName };
				})
				.Where(x => x.part != null && requestPosition >= x.part.Position && requestPosition < x.part.EndPosition)
				.Select(x => x.typeName)
				.FirstOrDefault();

			return fields
				.Select(field =>
				{
					var part = field.GetPart(ServicePartKind.TypeName);
					var typeName = field.TypeName;

					var type = service.GetFieldType(field);
					var memberTypeName = type?.GetMemberTypeName();

					return new { part, memberTypeName, typeName };
				})
				.Where(x => x.part != null && ((memberNameAtCursor != null && x.memberTypeName == memberNameAtCursor) || x.typeName == fieldTypeNameAtCursor))
				.Select(x => x.part);
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
	}
}
