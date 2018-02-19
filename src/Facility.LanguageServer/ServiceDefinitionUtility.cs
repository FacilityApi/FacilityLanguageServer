using System;
using System.Collections.Generic;
using System.Linq;
using Facility.Definition;

namespace Facility.LanguageServer
{
	static class ServiceDefinitionUtility
	{
		public static IServiceNamedInfo Find(this IServiceNamedInfo info, Position position)
		{
			var seen = new HashSet<IServiceNamedInfo>();
			IServiceNamedInfo last = null;
			foreach (var cur in GetAll(info).OrderBy(x => new Position(x.Position)))
			{
				Position? curPos = cur.Position != null ? new Position(cur.Position) : default(Position?);
				if (position >= last?.Position && position < curPos)
					return last;
				last = cur;
			}
			return null;
		}

		public static string GetMemberTypeName(this ServiceTypeInfo type)
		{
			switch (type.Kind)
			{
				case ServiceTypeKind.Dto:
					return type.Dto.Name;
				case ServiceTypeKind.Enum:
					return type.Enum.Name;
				case ServiceTypeKind.Array:
				case ServiceTypeKind.Result:
				case ServiceTypeKind.Map:
					return GetMemberTypeName(type.ValueType);
			}
			return null;
		}

		public static IServiceMemberInfo GetMemberReferencedAtPosition(this ServiceInfo service, Position requestPosition)
		{
			var infoAtPosition = service.Find(requestPosition);
			if (infoAtPosition is ServiceFieldInfo field)
			{
				if (requestPosition >= field.TypeNamePosition)
				{
					var fieldType = service.GetFieldType(field);
					var typeName = fieldType.GetMemberTypeName();
					if (typeName != null)
						return service.FindMember(typeName);
				}
			}
			return null;
		}

		static IEnumerable<IServiceNamedInfo> GetAll(IServiceNamedInfo info)
		{
			return new[] { info }.Concat(GetChildren(info).SelectMany(x => GetAll(x)));
		}

		static IEnumerable<IServiceNamedInfo> GetChildren(IServiceNamedInfo info)
		{
			switch (info)
			{
				case ServiceAttributeInfo x:
					return GetChildren(x);
				case ServiceAttributeParameterInfo x:
					return GetChildren(x);
				case ServiceDtoInfo x:
					return GetChildren(x);
				case ServiceEnumInfo x:
					return GetChildren(x);
				case ServiceEnumValueInfo x:
					return GetChildren(x);
				case ServiceErrorInfo x:
					return GetChildren(x);
				case ServiceErrorSetInfo x:
					return GetChildren(x);
				case ServiceFieldInfo x:
					return GetChildren(x);
				case ServiceInfo x:
					return GetChildren(x);
				case ServiceMethodInfo x:
					return GetChildren(x);
			}
			throw new ArgumentException();
		}

		static IEnumerable<IServiceNamedInfo> GetChildren(ServiceAttributeInfo info)
		{
			return info.Parameters;
		}

		static IEnumerable<IServiceNamedInfo> GetChildren(ServiceAttributeParameterInfo info)
		{
			return Enumerable.Empty<IServiceNamedInfo>();
		}

		static IEnumerable<IServiceNamedInfo> GetChildren(ServiceDtoInfo info)
		{
			return info.Attributes.Cast<IServiceNamedInfo>().Concat(info.Fields);
		}

		static IEnumerable<IServiceNamedInfo> GetChildren(ServiceEnumInfo info)
		{
			return info.Attributes.Cast<IServiceNamedInfo>().Concat(info.Values);
		}

		static IEnumerable<IServiceNamedInfo> GetChildren(ServiceEnumValueInfo info)
		{
			return info.Attributes;
		}

		static IEnumerable<IServiceNamedInfo> GetChildren(ServiceErrorInfo info)
		{
			return info.Attributes;
		}

		static IEnumerable<IServiceNamedInfo> GetChildren(ServiceErrorSetInfo info)
		{
			return info.Attributes.Cast<IServiceNamedInfo>().Concat(info.Errors);
		}

		static IEnumerable<IServiceNamedInfo> GetChildren(ServiceFieldInfo info)
		{
			return info.Attributes;
		}

		static IEnumerable<IServiceNamedInfo> GetChildren(ServiceInfo info)
		{
			return info.Attributes.Cast<IServiceNamedInfo>().Concat(info.Members);
		}

		static IEnumerable<IServiceNamedInfo> GetChildren(ServiceMethodInfo info)
		{
			return info.Attributes.Cast<IServiceNamedInfo>().Concat(info.RequestFields).Concat(info.ResponseFields);
		}
	}
}
