using System;

namespace Luny.Engine.Bridge
{
	/// <summary>
	/// Converts between engine-native and engine-agnostic (Luny) paths.
	/// </summary>
	public interface ILunyPathConverter
	{
		/// <summary>
		/// Converts a native path to a Luny relative path.
		/// </summary>
		String ToLuny(String nativePath, LunyPathType type);

		/// <summary>
		/// Converts a Luny relative path to a native engine path.
		/// </summary>
		String ToNative(String agnosticPath, LunyPathType type);
	}
}
