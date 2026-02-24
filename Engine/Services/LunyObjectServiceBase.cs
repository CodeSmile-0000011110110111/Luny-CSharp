using Luny.Engine.Bridge;
using System;

namespace Luny.Engine.Services
{
	/// <summary>
	/// Provides engine-agnostic methods for creating objects and primitives.
	/// </summary>
	public interface ILunyObjectService : ILunyEngineService
	{
		ILunyObject CreateEmpty(String name, ILunyObject parent, LunyVector3 position, LunyQuaternion rotation, LunyVector3 scale);
		ILunyObject CreatePrimitive(String name, LunyPrimitiveType type, ILunyObject parent, LunyVector3 position, LunyQuaternion rotation, LunyVector3 scale);

		/// <summary>
		/// Creates a new object from a prefab bridge.
		/// </summary>
		ILunyObject CreateFromPrefab(ILunyPrefab prefab, ILunyObject parent, LunyVector3 position, LunyQuaternion rotation, LunyVector3 scale);
	}

	public abstract class LunyObjectServiceBase : LunyEngineServiceBase, ILunyObjectService
	{
		public abstract ILunyObject CreateEmpty(String name, ILunyObject parent, LunyVector3 position, LunyQuaternion rotation, LunyVector3 scale);
		public abstract ILunyObject CreatePrimitive(String name, LunyPrimitiveType type, ILunyObject parent, LunyVector3 position, LunyQuaternion rotation, LunyVector3 scale);
		public abstract ILunyObject CreateFromPrefab(ILunyPrefab prefab, ILunyObject parent, LunyVector3 position, LunyQuaternion rotation, LunyVector3 scale);
	}
}
