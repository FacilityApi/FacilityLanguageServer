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

		private static string GetMemberTypeName(this ServiceTypeInfo type)
		{
			switch (type.Kind)
			{
				case ServiceTypeKind.Dto:
					return type.Dto!.Name;
				case ServiceTypeKind.Enum:
					return type.Enum!.Name;
				case ServiceTypeKind.Array:
				case ServiceTypeKind.Result:
				case ServiceTypeKind.Map:
					return type.ValueType.GetMemberTypeName();
			}
			return null;
		}
	}
}
