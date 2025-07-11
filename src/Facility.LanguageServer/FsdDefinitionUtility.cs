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
				let name = type.GetValueType().ToString()
				where name != null
				select service.FindMember(name)).FirstOrDefault();
		}

		public static IEnumerable<ServicePart> GetReferencedServicePartsAtPosition(this ServiceInfo service, Position requestPosition, bool includeDeclaration)
		{
			var members = service.GetDescendants().OfType<ServiceMemberInfo>().ToList().AsReadOnly();
			var memberParts = members
				.Select(member =>
				{
					var part = member.GetPart(ServicePartKind.Name);
					var name = member.Name;

					return (part, name);
				}).ToList().AsReadOnly();

			// will be null if the cursor is not on a member name.
			var memberNameAtCursor = memberParts
				.Where(x => x.part != null && requestPosition >= x.part.Position && requestPosition < x.part.EndPosition)
				.Select(x => x.name)
				.FirstOrDefault();

			var fields = service.GetDescendants().OfType<ServiceFieldInfo>().ToList().AsReadOnly();
			var fieldValueTypeParts = fields
				.Select(field =>
				{
					var part = field.GetPart(ServicePartKind.TypeName);
					var valueTypePart = GetValueTypePart(field.TypeName, part);

					var type = service.GetFieldType(field);
					var valueTypeName = type?.GetValueType().ToString();

					return (valueTypePart, valueTypeName);
				}).ToList().AsReadOnly();

			// will be null if the cursor is not on a field type name.
			var fieldValueTypeNameAtCursor = fieldValueTypeParts
				.Where(x => x.valueTypePart != null && requestPosition >= x.valueTypePart.Position && requestPosition < x.valueTypePart.EndPosition)
				.Select(x => x.valueTypeName)
				.FirstOrDefault();

			var referencedFields = fieldValueTypeParts
				.Where(x => x.valueTypePart != null && ((memberNameAtCursor != null && x.valueTypeName == memberNameAtCursor) || (fieldValueTypeNameAtCursor != null && x.valueTypeName == fieldValueTypeNameAtCursor)))
				.Select(x => x.valueTypePart);

			var referencedMembers = memberParts
				.Where(x => x.part != null && ((memberNameAtCursor != null && x.name == memberNameAtCursor) || (fieldValueTypeNameAtCursor != null && x.name == fieldValueTypeNameAtCursor)))
				.Select(x => x.part);

			return includeDeclaration
				? referencedFields.Concat(referencedMembers)
				: referencedFields;
		}

		public static ServicePart GetValueTypePart(string text, ServicePart part)
		{
			foreach (var (prefix, suffix) in s_templateTypePrefixSuffixes)
			{
				var valueType = TryPrefixSuffix(text, prefix, suffix);
				if (valueType is not null)
					return GetValueTypePart(valueType, TruncatePart(ServicePartKind.TypeName, part, prefix.Length, suffix.Length));
			}

			return part;
		}

		public static ServiceTypeInfo GetValueType(this ServiceTypeInfo type)
		{
			var valueType = type;
			while (valueType?.ValueType is not null)
				valueType = valueType.ValueType;
			return valueType;
		}

		private static ServicePart TruncatePart(ServicePartKind newKind, ServicePart part, int truncateLeft, int truncateRight) =>
			new ServicePart(
				newKind,
				new ServiceDefinitionPosition(part.Position.Name, part.Position.LineNumber, part.Position.ColumnNumber + truncateLeft),
				new ServiceDefinitionPosition(part.EndPosition.Name, part.EndPosition.LineNumber, part.EndPosition.ColumnNumber - truncateRight));

		private static string TryPrefixSuffix(string text, string prefix, string suffix)
		{
			return text.StartsWith(prefix, StringComparison.Ordinal) && text.EndsWith(suffix, StringComparison.Ordinal) ?
				text.Substring(prefix.Length, text.Length - prefix.Length - suffix.Length) : null;
		}

		private static readonly List<(string, string)> s_templateTypePrefixSuffixes = new() { ("", "[]"), ("nullable<", ">"), ("map<", ">"), ("result<", ">") };
	}
}
