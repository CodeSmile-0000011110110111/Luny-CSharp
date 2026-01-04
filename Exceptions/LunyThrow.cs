using Luny.Engine.Registries;
using System;

namespace Luny.Exceptions
{
	internal sealed class LunyThrow
	{
		//[StackTraceHidden] // not supported by Unity 6
		public static void SingletonDuplicationException(String typeName) =>
			throw new LunyLifecycleException($"Duplicate {typeName} singleton detected!");

		//[StackTraceHidden] // not supported by Unity 6
		public static void ServiceMustImplementSpecificInterfaceException(String serviceName) => throw new LunyServiceException(
			$"Service {serviceName} must implement a specific interface derived from {nameof(ILunyEngineService)}, not {nameof(ILunyEngineService)} directly.");

		//[StackTraceHidden] // not supported by Unity 6
		public static void ServiceImplementsMultipleInterfacesException(String serviceName, String interfacesFound) =>
			throw new LunyServiceException(
				$"Service {serviceName} implements multiple {nameof(ILunyEngineService)}-derived interfaces: {interfacesFound}. Only one is allowed per type.");
	}
}
