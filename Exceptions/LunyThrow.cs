using System;

namespace Luny.Exceptions
{
	public sealed class LunyThrow
	{
		// [StackTraceHidden] not supported by Unity 6
		public static void LifecycleAdapterSingletonDuplicationException(String adapterTypeName, String existingObjectName,
			Int64 existingInstanceId, String duplicateObjectName, Int64 duplicateInstanceId) => throw new LunyLifecycleException(
			$"Duplicate {adapterTypeName} singleton detected! " +
			$"Existing: Name='{existingObjectName}' InstanceID={existingInstanceId}, " +
			$"Duplicate: Name='{duplicateObjectName}' InstanceID={duplicateInstanceId}");

		// [StackTraceHidden] not supported by Unity 6
		public static void SingletonDuplicationException(String typeName) =>
			throw new LunyLifecycleException($"Duplicate {typeName} singleton detected!");

		// [StackTraceHidden] not supported by Unity 6
		public static void LifecycleAdapterPrematurelyRemovedException(String adapterName) => throw new LunyLifecycleException(
			$"{adapterName} unexpectedly removed from SceneTree! It must remain in scene at all times.");

		// [StackTraceHidden] not supported by Unity 6
		public static void ServiceNotFoundException(String serviceName) =>
			throw new LunyServiceException($"Service {serviceName} not found in registry.");

		// [StackTraceHidden] not supported by Unity 6
		public static void ServiceMustImplementSpecificInterfaceException(String serviceName) => throw new LunyServiceException(
			$"Service {serviceName} must implement a specific interface derived from IEngineServiceProvider, not IEngineServiceProvider directly.");

		// [StackTraceHidden] not supported by Unity 6
		public static void ServiceImplementsMultipleInterfacesException(String serviceName, String interfacesFound) =>
			throw new LunyServiceException(
				$"Service {serviceName} implements multiple IEngineServiceProvider-derived interfaces: {interfacesFound}. Only one is allowed.");
	}
}
