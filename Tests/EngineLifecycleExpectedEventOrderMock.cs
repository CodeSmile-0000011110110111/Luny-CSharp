using Luny.Attributes;
using Luny.Providers;
using System;

namespace Luny.Tests
{
	/// <summary>
	/// Mock lifecycle observer for smoke testing expected event ordering.
	/// Only instantiated in smoke test scenes.
	/// </summary>
	[LunyTestable]
	public sealed class EngineLifecycleExpectedEventOrderMock : IEngineLifecycleObserver
	{
		private Boolean _didRunStartup;
		private Int32 _fixedStepRunCount;
		private Int32 _updateRunCount;
		private Int32 _lateUpdateRunCount;
		private Boolean _didRunShutdown;
		private Int32 _shutdownAfterThisManyUpdates = 3;

		public EngineLifecycleExpectedEventOrderMock() => LunyLogger.LogInfo($"{nameof(EngineLifecycleExpectedEventOrderMock)} ctor", this);

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
			_fixedStepRunCount++;
		}

		public void OnUpdate(Double deltaTime)
		{
			LunyLogger.LogInfo(nameof(OnUpdate), this);
			LunyAssert.IsGreaterThan(_fixedStepRunCount, 0, $"{nameof(OnUpdate)} ran before {nameof(OnFixedStep)}");
			_updateRunCount++;
		}

		public void OnLateUpdate(Double deltaTime)
		{
			LunyLogger.LogInfo(nameof(OnLateUpdate), this);
			LunyAssert.IsGreaterThanOrEqualTo(_updateRunCount, 1, $"{nameof(OnLateUpdate)} ran before {nameof(OnUpdate)}");

			_lateUpdateRunCount++;
			LunyAssert.AreEqual(_updateRunCount, _lateUpdateRunCount,
				$"{nameof(OnLateUpdate)} and {nameof(OnUpdate)} did not run same number of times");

			_shutdownAfterThisManyUpdates--;
			if (_shutdownAfterThisManyUpdates <= 0)
			{
				// Force shutdown
				LunyLogger.LogInfo("Lifecycle mock calls Quit to end lifecycle testing ...");
				var applicationProvider = LunyEngine.Instance.GetService<IApplicationServiceProvider>();
				applicationProvider.Quit();
			}
		}

		public void OnShutdown()
		{
			LunyLogger.LogInfo(nameof(OnShutdown), this);

			LunyAssert.IsTrue(_didRunStartup, $"{nameof(OnStartup)} did not run");
			LunyAssert.IsGreaterThan(_fixedStepRunCount, 0, $"{nameof(OnFixedStep)} did not run");
			LunyAssert.IsGreaterThan(_updateRunCount, 0, $"{nameof(OnUpdate)} did not run");
			LunyAssert.IsGreaterThan(_lateUpdateRunCount, 0, $"{nameof(OnLateUpdate)} did not run");
			LunyAssert.AreEqual(_updateRunCount, _lateUpdateRunCount,
				$"{nameof(OnLateUpdate)} and {nameof(OnUpdate)} did not run same number of times");
			LunyAssert.IsFalse(_didRunShutdown, $"{nameof(OnShutdown)} called more than once");

			_didRunShutdown = true;
		}
	}
}
