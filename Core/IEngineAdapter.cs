using Luny.Exceptions;
using System;

namespace Luny
{
	/// <summary>
	/// Marker interface for the native engine adapter.
	/// </summary>
	public interface IEngineAdapter
	{
		static IEngineAdapter GetValidatedSingletonInstance(IEngineAdapter existingInstance, Object current)
		{
			if (existingInstance != null)
			{
				throw new LunyLifecycleException($"Duplicate {nameof(IEngineAdapter)} singleton detected! " +
				                                 $"Existing: {existingInstance}, Duplicate: {current}");
			}

			if (current is not IEngineAdapter adapter)
				throw new LunyLifecycleException($"New {nameof(IEngineAdapter)} instance is null or incorrect type: {current}");

			return adapter;
		}

		static void AssertEngineAdapterNotNull(IEngineAdapter adapter)
		{
			if (adapter == null)
				throw new LunyLifecycleException($"{nameof(IEngineAdapter)} is null");
		}

		static void AssertLunyEngineNotNull(ILunyEngine lunyEngine)
		{
			if (lunyEngine == null)
				throw new LunyLifecycleException($"{nameof(ILunyEngine)} is null");
		}
	}
}
