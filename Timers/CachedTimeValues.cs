using System;
using UnityEngine;

namespace Damntry.UtilsUnity.Timers {

	//TODO Global 5 - Change this weird thing into a MonoBehaviour that updates values at the start of each FixedUpdate

	/// <summary>
	/// Keeps Unity Time values in between FixedUpdate cycles.
	/// </summary>
	public class CachedTimeValues {

		private static UnityTimeStopwatch unitySW;

		private static float fixedDeltaTime;

		/// <summary>How many calls are made to FixedUpdate in a second.</summary>
		private static int fixedUpdateCycleMax;

		public static (float fixedDeltaTime, int fixedUpdateCycleMax) GetFixedTimeCachedValues() {
			if (unitySW == null || unitySW.ElapsedSecondsPrecise > 1) {
				unitySW ??= UnityTimeStopwatch.StartNew(UnityTimeStopwatch.TimeType.FixedTime);

				fixedDeltaTime = Time.fixedDeltaTime;
				fixedUpdateCycleMax = (int)Math.Round(1 / fixedDeltaTime);
			}

			return (fixedDeltaTime, fixedUpdateCycleMax);
		}

	}
}
