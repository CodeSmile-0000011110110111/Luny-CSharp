/*
using Luny.Attributes;
using Luny.Engine;
using System;

namespace Luny.Tests
{
	/// <summary>
	/// Mock lifecycle observer for smoke testing expected event ordering.
	/// Only instantiated in smoke test scenes.
	/// </summary>
	[LunyTestable]
	internal sealed class EngineEventOrderMock : ILunyEngineObserver
	{
		private Boolean _didRunStartup;
		private Int32 _fixedStepRunCount;
		private Int32 _updateRunCount;
		private Int32 _lateUpdateRunCount;
		private Boolean _didRunShutdown;
		private Int32 _shutdownAfterThisManyUpdates = 3;

		public EngineEventOrderMock() => LunyLogger.LogInfo($"{nameof(EngineEventOrderMock)} ctor", this);

		public void OnEngineStartup()
		{
			LunyLogger.LogInfo(nameof(OnEngineStartup), this);
			LunyAssert.IsFalse(_didRunStartup, $"{nameof(OnEngineStartup)} called more than once");
			LunyAssert.IsZero(_fixedStepRunCount, $"{nameof(OnEngineFixedStep)} already ran before {nameof(OnEngineStartup)}");
			LunyAssert.IsZero(_updateRunCount, $"{nameof(OnEngineUpdate)} already ran before {nameof(OnEngineStartup)}");
			LunyAssert.IsZero(_lateUpdateRunCount, $"{nameof(OnEngineLateUpdate)} already ran before {nameof(OnEngineStartup)}");
			LunyAssert.IsFalse(_didRunShutdown, $"{nameof(OnEngineShutdown)} already ran before {nameof(OnEngineStartup)}");
			_didRunStartup = true;
		}

		public void OnEngineFixedStep(Double fixedDeltaTime)
		{
			LunyLogger.LogInfo(nameof(OnEngineFixedStep), this);
			LunyAssert.IsTrue(_didRunStartup, $"{nameof(OnEngineFixedStep)} ran before {nameof(OnEngineStartup)}");
			_fixedStepRunCount++;
		}

		public void OnEngineUpdate(Double deltaTime)
		{
			LunyLogger.LogInfo(nameof(OnEngineUpdate), this);
			LunyAssert.IsTrue(_didRunStartup, $"{nameof(OnEngineUpdate)} ran before {nameof(OnEngineStartup)}");
			_updateRunCount++;
		}

		public void OnEngineLateUpdate(Double deltaTime)
		{
			LunyLogger.LogInfo(nameof(OnEngineLateUpdate), this);
			LunyAssert.IsTrue(_didRunStartup, $"{nameof(OnEngineLateUpdate)} ran before {nameof(OnEngineStartup)}");

			_lateUpdateRunCount++;
			LunyAssert.AreEqual(_updateRunCount, _lateUpdateRunCount,
				$"{nameof(OnEngineLateUpdate)} and {nameof(OnEngineUpdate)} did not run same number of times");

			_shutdownAfterThisManyUpdates--;
			if (_shutdownAfterThisManyUpdates <= 0)
			{
				// Force shutdown
				LunyLogger.LogInfo("Lifecycle mock calls Quit to end lifecycle testing ...");
				LunyEngine.Instance.Application.Quit();
			}
		}

		public void OnEngineShutdown()
		{
			LunyLogger.LogInfo(nameof(OnEngineShutdown), this);
			LunyAssert.IsTrue(_didRunStartup, $"{nameof(OnEngineShutdown)} ran before {nameof(OnEngineStartup)}");
			LunyAssert.IsFalse(_didRunShutdown, $"{nameof(OnEngineShutdown)} called more than once");

			_didRunShutdown = true;
		}

		public void PostShutdownVerification()
		{
			LunyAssert.IsTrue(_didRunStartup, $"{nameof(OnEngineShutdown)} ran before {nameof(OnEngineStartup)}");
			LunyAssert.IsTrue(_didRunShutdown, $"{nameof(OnEngineShutdown)} did not run");
			LunyAssert.IsTrue(_fixedStepRunCount > 0, $"{nameof(OnEngineFixedStep)} did not run");
			LunyAssert.IsTrue(_updateRunCount > 0, $"{nameof(OnEngineUpdate)} did not run");
			LunyAssert.IsTrue(_lateUpdateRunCount > 0, $"{nameof(OnEngineLateUpdate)} did not run");
			LunyAssert.AreEqual(_updateRunCount, _lateUpdateRunCount,
				$"{nameof(OnEngineLateUpdate)} and {nameof(OnEngineUpdate)} did not run same number of times");
		}
	}
}
*/


