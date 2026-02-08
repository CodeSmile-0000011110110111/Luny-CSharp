using Luny.Engine.Bridge;
using Luny.Engine.Bridge.Enums;
using System;

namespace Luny.Engine.Services
{
	/// <summary>
	/// Provides engine-agnostic methods for creating objects and primitives.
	/// </summary>
	public interface ILunyObjectService : ILunyEngineService
	{
		ILunyObject CreateEmpty(String name);
		ILunyObject CreatePrimitive(String name, LunyPrimitiveType type);

		/// <summary>
		/// Creates a new object from a prefab bridge.
		/// </summary>
		ILunyObject CreateFromPrefab(ILunyPrefab prefab);
	}

	public abstract class LunyObjectServiceBase : LunyEngineServiceBase, ILunyObjectService
	{
		public abstract ILunyObject CreateEmpty(String name);
		public abstract ILunyObject CreatePrimitive(String name, LunyPrimitiveType type);
		public abstract ILunyObject CreateFromPrefab(ILunyPrefab prefab);
	}
}
