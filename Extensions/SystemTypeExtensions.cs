using System;
using System.Reflection;

namespace Luny.Extensions
{
	public static class SystemTypeExtensions
	{
		public static Boolean HasAttribute<T>(this Type type) where T : Attribute => type.GetCustomAttribute<T>() != null;
	}
}
