using Luny.Engine;
using System;

namespace Luny.Exceptions
{
	internal sealed class LunyThrow
	{
#if GODOT
		[StackTraceHidden] // not supported by Unity 6
#endif
		public static void SingletonDuplicationException(String typeName) =>
			throw new LunyLifecycleException($"Duplicate {typeName} singleton detected!");

#if GODOT
		[StackTraceHidden] // not supported by Unity 6
#endif
		public static void ServiceMustImplementSpecificInterfaceException(String serviceName) => throw new LunyServiceException(
			$"Service {serviceName} must implement a specific interface derived from {nameof(IEngineService)}, not {nameof(IEngineService)} directly.");

#if GODOT
		[StackTraceHidden] // not supported by Unity 6
#endif
		public static void ServiceImplementsMultipleInterfacesException(String serviceName, String interfacesFound) =>
			throw new LunyServiceException(
				$"Service {serviceName} implements multiple {nameof(IEngineService)}-derived interfaces: {interfacesFound}. Only one is allowed per type.");
	}
}
