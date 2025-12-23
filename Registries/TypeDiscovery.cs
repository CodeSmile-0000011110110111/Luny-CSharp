using Luny.Proxies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Luny.Registries
{
	/// <summary>
	/// Utilities for discovering types via reflection across all loaded assemblies.
	/// Provides consistent error handling and performance characteristics.
	/// </summary>
	public static class TypeDiscovery
	{
		/// <summary>
		/// Discovers all concrete (non-abstract, non-interface) types assignable to T.
		/// </summary>
		public static IEnumerable<Type> FindAll<T>() => AppDomain.CurrentDomain.GetAssemblies()
			.SelectMany(GetTypesFromAssembly)
			.Where(t => typeof(T).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface);

		/// <summary>
		/// Safe wrapper for Assembly.GetTypes() with consistent error handling.
		/// </summary>
		private static IEnumerable<Type> GetTypesFromAssembly(Assembly assembly)
		{
			try
			{
				return assembly.GetTypes();
			}
			catch (ReflectionTypeLoadException ex)
			{
				// Return successfully loaded types, log warning
				LunyLogger.LogWarning(
					$"Partial type load from {assembly.GetName().Name}: {ex.Message}",
					typeof(TypeDiscovery));

				return ex.Types.Where(t => t != null);
			}
			catch (Exception ex)
			{
				// Total failure - log and return empty
				LunyLogger.LogWarning(
					$"Failed to load types from {assembly.GetName().Name}: {ex.Message}",
					typeof(TypeDiscovery));

				return Array.Empty<Type>();
			}
		}
	}
}
