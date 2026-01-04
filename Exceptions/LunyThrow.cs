using Luny.Engine;
using System;
using System.Diagnostics;

namespace Luny.Exceptions
{
	internal sealed class LunyThrow
	{
		//[StackTraceHidden] // not supported by Unity 6
		public static void SingletonDuplicationException(String typeName) =>
			throw new LunyLifecycleException($"Duplicate {typeName} singleton detected!");

		//[StackTraceHidden] // not supported by Unity 6
		public static void ServiceMustImplementSpecificInterfaceException(String serviceName) => throw new LunyServiceException(
			$"Service {serviceName} must implement a specific interface derived from {nameof(IEngineService)}, not {nameof(IEngineService)} directly.");

		//[StackTraceHidden] // not supported by Unity 6
		public static void ServiceImplementsMultipleInterfacesException(String serviceName, String interfacesFound) =>
			throw new LunyServiceException(
				$"Service {serviceName} implements multiple {nameof(IEngineService)}-derived interfaces: {interfacesFound}. Only one is allowed per type.");
	}
}
