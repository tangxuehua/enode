using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NetSerializer
{
	static class Helpers
	{
		public static bool GetPrimitives(Type containerType, Type type, out MethodInfo writer, out MethodInfo reader)
		{
			if (type.IsEnum)
				type = Enum.GetUnderlyingType(type);

			if (type.IsGenericType == false)
			{
				writer = containerType.GetMethod("WritePrimitive", BindingFlags.Static | BindingFlags.Public | BindingFlags.ExactBinding, null,
					new Type[] { typeof(Stream), type }, null);

				reader = containerType.GetMethod("ReadPrimitive", BindingFlags.Static | BindingFlags.Public | BindingFlags.ExactBinding, null,
					new Type[] { typeof(Stream), type.MakeByRefType() }, null);
			}
			else
			{
				var genType = type.GetGenericTypeDefinition();

				writer = GetGenWriter(containerType, genType);
				reader = GetGenReader(containerType, genType);
			}

			if (writer == null && reader == null)
				return false;
			else if (writer != null && reader != null)
				return true;
			else
				throw new InvalidOperationException(String.Format("Missing a {0}Primitive() for {1}",
					reader == null ? "Read" : "Write", type.FullName));
		}

		static MethodInfo GetGenWriter(Type containerType, Type genType)
		{
			var mis = containerType.GetMethods(BindingFlags.Static | BindingFlags.Public)
				.Where(mi => mi.IsGenericMethod && mi.Name == "WritePrimitive");

			foreach (var mi in mis)
			{
				var p = mi.GetParameters();

				if (p.Length != 2)
					continue;

				if (p[0].ParameterType != typeof(Stream))
					continue;

				var paramType = p[1].ParameterType;

				if (paramType.IsGenericType == false)
					continue;

				var genParamType = paramType.GetGenericTypeDefinition();

				if (genType == genParamType)
					return mi;
			}

			return null;
		}

		static MethodInfo GetGenReader(Type containerType, Type genType)
		{
			var mis = containerType.GetMethods(BindingFlags.Static | BindingFlags.Public)
				.Where(mi => mi.IsGenericMethod && mi.Name == "ReadPrimitive");

			foreach (var mi in mis)
			{
				var p = mi.GetParameters();

				if (p.Length != 2)
					continue;

				if (p[0].ParameterType != typeof(Stream))
					continue;

				var paramType = p[1].ParameterType;

				if (paramType.IsByRef == false)
					continue;

				paramType = paramType.GetElementType();

				if (paramType.IsGenericType == false)
					continue;

				var genParamType = paramType.GetGenericTypeDefinition();

				if (genType == genParamType)
					return mi;
			}

			return null;
		}

		public static IEnumerable<FieldInfo> GetFieldInfos(Type type)
		{
            //Debug.Assert(type.IsSerializable);  //断言失败？

			var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
				.Where(fi => (fi.Attributes & FieldAttributes.NotSerialized) == 0)
				.OrderBy(f => f.Name, StringComparer.Ordinal);

			if (type.BaseType == null)
			{
				return fields;
			}
			else
			{
				var baseFields = GetFieldInfos(type.BaseType);
				return baseFields.Concat(fields);
			}
		}
	}
}
