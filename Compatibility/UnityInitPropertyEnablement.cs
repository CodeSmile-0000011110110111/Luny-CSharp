using System.ComponentModel;

// Only compile this if we are below .NET 5.0
// Unity 6.3+ uses either .NET Standard 2.1 or .NET Framework 4.8 so it will be active in pre-CoreCLR versions of Unity
#if !NET_5_0_OR_GREATER

// ReSharper disable CheckNamespace
namespace System.Runtime.CompilerServices
{
	/// <summary>
	/// Required for the 'init' property keyword to compile in Unity.
	/// Without this class, the following code snippet would fail to compile in Unity 6:
	/// `public String Name { get; init; }`
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static class IsExternalInit {}
}

#endif
