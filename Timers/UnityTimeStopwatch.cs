using UnityEngine;
using Damntry.Utils.Timers.StopwatchImpl;


namespace Damntry.UtilsUnity.Timers {

	/// <summary>
	/// A clone of the Stopwatch class, but adapted to use Unity Time.timeAsDouble instead.
	/// </summary>
	public class UnityTimeStopwatch : IStopwatch {

		private double elapsed;

		private double startTimeStamp;

		private bool isRunning;


		/// <summary>Gets a value indicating whether this UnityTimeStopwatch timer is running.</summary>
		public bool IsRunning {
			get {
				return isRunning;
			}
		}


		/// <summary>Gets the total elapsed time measured by the current instance, in milliseconds.</summary>
		public long ElapsedMilliseconds {
			get {
				return (long)GetElapsedSeconds(false) / 10000;
			}
		}

		/// <summary>Gets the total elapsed time measured by the current instance, in seconds.</summary>
		public long ElapsedSeconds {
			get {
				return (long)GetElapsedSeconds(false);
			}
		}

		/// <summary>Gets the total elapsed time measured by the current instance, in milliseconds.</summary>
		public double ElapsedMillisecondsPrecise {
			get {
				return GetElapsedSeconds(true) / 10000;
			}
		}

		/// <summary>Gets the total elapsed time measured by the current instance, in seconds.</summary>
		public double ElapsedSecondsPrecise {
			get {
				return GetElapsedSeconds(true);
			}
		}


		static UnityTimeStopwatch() { }

		/// <summary>Initializes a new instance of this class.</summary>
		public UnityTimeStopwatch() {
			Reset();
		}

		/// <summary>Starts, or resumes, measuring elapsed time for an interval.</summary>
		public void Start() {
			if (!isRunning) {
				startTimeStamp = GetTimestampDouble();
				isRunning = true;
			}
		}

		/// <summary>A UnityTimeStopwatch that has just begun measuring elapsed time.</summary>
		public static UnityTimeStopwatch StartNew() {
			UnityTimeStopwatch unityTimeStopwatch = new UnityTimeStopwatch();
			unityTimeStopwatch.Start();
			return unityTimeStopwatch;
		}

		/// <summary>Stops measuring elapsed time for an interval.</summary>
		public void Stop() {
			if (isRunning) {
				double num = GetTimestampDouble() - startTimeStamp;
				elapsed += num;
				isRunning = false;
				if (elapsed < 0) {
					elapsed = 0L;
				}
			}
		}

		/// <summary>Stops time interval measurement and resets the elapsed time to zero.</summary>
		public void Reset() {
			elapsed = 0L;
			isRunning = false;
			startTimeStamp = 0L;
		}

		/// <summary>Stops time interval measurement, resets the elapsed time to zero, and starts measuring elapsed time.</summary>
		public void Restart() {
			elapsed = 0L;
			startTimeStamp = GetTimestampDouble();
			isRunning = true;
		}

		private float GetTimestamp() {
			return Time.time;
		}

		private double GetTimestampDouble() {
			return Time.timeAsDouble;
		}

		private double GetElapsedSeconds(bool highPrecision) {
			double num = elapsed;
			if (isRunning) {

				num += (highPrecision ? GetTimestampDouble() : GetTimestamp()) - startTimeStamp;
			}

			return num;

		}

	}
}
