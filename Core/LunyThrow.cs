using System;
using System.Diagnostics;

namespace Luny
{
    public sealed class LunyThrow
    {
        // [StackTraceHidden] not supported by Unity 6
        public static void LifecycleAdapterSingletonDuplicationException(String adapterTypeName, String existingObjectName,
            Int64 existingInstanceId, String duplicateObjectName, Int64 duplicateInstanceId) => throw new InvalidOperationException(
            $"Duplicate {adapterTypeName} singleton detected! " +
            $"Existing: Name='{existingObjectName}' InstanceID={existingInstanceId}, " +
            $"Duplicate: Name='{duplicateObjectName}' InstanceID={duplicateInstanceId}");

        // [StackTraceHidden] not supported by Unity 6
        public static void SingletonDuplicationException(String typeName) =>
            throw new InvalidOperationException($"Duplicate {typeName} singleton detected!");

        // [StackTraceHidden] not supported by Unity 6
        public static void ServiceNotFoundException(String serviceName) =>
            throw new InvalidOperationException($"Service {serviceName} not found in registry.");

        // [StackTraceHidden] not supported by Unity 6
        public static void LifecycleAdapterPrematurelyRemovedException(String godotLifecycleAdapterName) => throw new InvalidOperationException(
            $"{godotLifecycleAdapterName} unexpectedly removed from SceneTree! It must remain in scene at all times.");

        // [StackTraceHidden] not supported by Unity 6
        public static void ServiceMustImplementSpecificInterfaceException(String serviceTypeName) =>
            throw new InvalidOperationException(
                $"Service {serviceTypeName} must implement a specific interface derived from IEngineServiceProvider, not IEngineServiceProvider directly.");

        // [StackTraceHidden] not supported by Unity 6
        public static void ServiceImplementsMultipleInterfacesException(String serviceTypeName, String interfacesFound) =>
            throw new InvalidOperationException(
                $"Service {serviceTypeName} implements multiple IEngineServiceProvider-derived interfaces: {interfacesFound}. Only one is allowed.");
    }
}
