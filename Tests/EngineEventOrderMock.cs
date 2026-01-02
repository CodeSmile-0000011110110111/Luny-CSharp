using Luny.Attributes;
using Luny.Diagnostics;
using System;

namespace Luny.Tests
{
	/// <summary>
	/// Mock lifecycle observer for smoke testing expected event ordering.
	/// Only instantiated in smoke test scenes.
	/// </summary>
	[LunyTestable]
	internal sealed class EngineEventOrderMock : IEngineObserver
	{
		private Boolean _didRunStartup;
		private Int32 _fixedStepRunCount;
		private Int32 _updateRunCount;
		private Int32 _lateUpdateRunCount;
		private Boolean _didRunShutdown;
		private Int32 _shutdownAfterThisManyUpdates = 3;

		public EngineEventOrderMock() => LunyLogger.LogInfo($"{nameof(EngineEventOrderMock)} ctor", this);

		public void OnStartup()
		{
			LunyLogger.LogInfo(nameof(OnStartup), this);
			LunyAssert.IsFalse(_didRunStartup, $"{nameof(OnStartup)} called more than once");
			LunyAssert.IsZero(_fixedStepRunCount, $"{nameof(OnFixedStep)} already ran before {nameof(OnStartup)}");
			LunyAssert.IsZero(_updateRunCount, $"{nameof(OnUpdate)} already ran before {nameof(OnStartup)}");
			LunyAssert.IsZero(_lateUpdateRunCount, $"{nameof(OnLateUpdate)} already ran before {nameof(OnStartup)}");
			LunyAssert.IsFalse(_didRunShutdown, $"{nameof(OnShutdown)} already ran before {nameof(OnStartup)}");
			_didRunStartup = true;
		}

		public void OnFixedStep(Double fixedDeltaTime)
		{
			LunyLogger.LogInfo(nameof(OnFixedStep), this);
			LunyAssert.IsTrue(_didRunStartup, $"{nameof(OnFixedStep)} ran before {nameof(OnStartup)}");
			_fixedStepRunCount++;
		}

		public void OnUpdate(Double deltaTime)
		{
			LunyLogger.LogInfo(nameof(OnUpdate), this);
			LunyAssert.IsTrue(_didRunStartup, $"{nameof(OnUpdate)} ran before {nameof(OnStartup)}");
			_updateRunCount++;
		}

		public void OnLateUpdate(Double deltaTime)
		{
			LunyLogger.LogInfo(nameof(OnLateUpdate), this);
			LunyAssert.IsTrue(_didRunStartup, $"{nameof(OnLateUpdate)} ran before {nameof(OnStartup)}");

			_lateUpdateRunCount++;
			LunyAssert.AreEqual(_updateRunCount, _lateUpdateRunCount,
				$"{nameof(OnLateUpdate)} and {nameof(OnUpdate)} did not run same number of times");

			_shutdownAfterThisManyUpdates--;
			if (_shutdownAfterThisManyUpdates <= 0)
			{
				// Force shutdown
				LunyLogger.LogInfo("Lifecycle mock calls Quit to end lifecycle testing ...");
				LunyEngine.Instance.Application.Quit();
			}
		}

		public void OnShutdown()
		{
			LunyLogger.LogInfo(nameof(OnShutdown), this);
			LunyAssert.IsTrue(_didRunStartup, $"{nameof(OnShutdown)} ran before {nameof(OnStartup)}");
			LunyAssert.IsFalse(_didRunShutdown, $"{nameof(OnShutdown)} called more than once");

			_didRunShutdown = true;
		}

		public void PostShutdownVerification()
		{
			LunyAssert.IsTrue(_didRunStartup, $"{nameof(OnShutdown)} ran before {nameof(OnStartup)}");
			LunyAssert.IsTrue(_didRunShutdown, $"{nameof(OnShutdown)} did not run");
			LunyAssert.IsTrue(_fixedStepRunCount > 0, $"{nameof(OnFixedStep)} did not run");
			LunyAssert.IsTrue(_updateRunCount > 0, $"{nameof(OnUpdate)} did not run");
			LunyAssert.IsTrue(_lateUpdateRunCount > 0, $"{nameof(OnLateUpdate)} did not run");
			LunyAssert.AreEqual(_updateRunCount, _lateUpdateRunCount,
				$"{nameof(OnLateUpdate)} and {nameof(OnUpdate)} did not run same number of times");
		}
	}
}
