using System;
using System.Diagnostics;

namespace Luny.Exceptions
{
	internal sealed class LunyThrow
	{
#if GODOT
		[StackTraceHidden] // not supported by Unity 6
#endif
		public static void EngineAdapterSingletonDuplicationException(Type instanceType, Object existingInstance, Object duplicateInstance) =>
			throw new LunyLifecycleException($"Duplicate {instanceType?.Name} singleton detected! " +
			                                 $"Existing: {existingInstance}, Duplicate: {duplicateInstance}");

#if GODOT
		[StackTraceHidden] // not supported by Unity 6
#endif
		public static void SingletonDuplicationException(String typeName) =>
			throw new LunyLifecycleException($"Duplicate {typeName} singleton detected!");

#if GODOT
		[StackTraceHidden] // not supported by Unity 6
#endif
		public static void EngineAdapterPrematurelyRemovedException(String adapterName) => throw new LunyLifecycleException(
			$"{adapterName} unexpectedly removed from SceneTree! It must remain in scene at all times.");

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
