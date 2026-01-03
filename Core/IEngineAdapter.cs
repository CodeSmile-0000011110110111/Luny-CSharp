using Luny.Exceptions;
using System;

namespace Luny
{
	/// <summary>
	/// Marker interface for the native engine adapter.
	/// </summary>
	public interface IEngineAdapter
	{
		static IEngineAdapter EnsureSingleInstance(IEngineAdapter existingInstance, Object current)
		{
			if (existingInstance != null)
				LunyThrow.EngineAdapterSingletonDuplicationException(typeof(IEngineAdapter), existingInstance, current);
			return (IEngineAdapter)current;
		}
	}
}
