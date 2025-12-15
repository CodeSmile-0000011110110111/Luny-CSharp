using System;

namespace Luny
{
    public sealed class Throw
    {
        public static void LifecycleAdapterSingletonDuplicationException(String adapterTypeName, String existingObjectName,
            Int64 existingInstanceId, String duplicateObjectName, Int64 duplicateInstanceId) => throw new InvalidOperationException(
            $"Duplicate {adapterTypeName} singleton detected! " +
            $"Existing: Name='{existingObjectName}' InstanceID={existingInstanceId}, " +
            $"Duplicate: Name='{duplicateObjectName}' InstanceID={duplicateInstanceId}");

        public static void LifecycleAdapterPrematurelyRemovedException(String godotLifecycleAdapterName) =>
            throw new InvalidOperationException(
                $"{godotLifecycleAdapterName} unexpectedly removed from SceneTree! It must remain in scene at all times.");
    }
}
