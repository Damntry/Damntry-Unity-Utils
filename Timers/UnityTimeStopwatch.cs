using Damntry.Utils.Timers.StopwatchImpl;
using UnityEngine;

namespace Damntry.UtilsUnity.Timers {

	/// <summary>
	/// A clone of the Stopwatch class, but adapted to use Unity Time.timeAsDouble instead.
	/// </summary>
	public class UnityTimeStopwatch : IStopwatch {


		public enum TimeType {
			Time,
			FixedTime,
			UnscaledTime,
			UnscaledFixedTime
		}

		private double elapsed;

		private double startTimeStamp;

		private bool isRunning;

		private TimeType timeType;


		/// <summary>Gets a value indicating whether this UnityTimeStopwatch timer is running.</summary>
		public bool IsRunning {
			get {
				return isRunning;
			}
		}


		/// <summary>Gets the total elapsed time measured by the current instance, in milliseconds.</summary>
		public long ElapsedMilliseconds {
			get {
				return (long)GetElapsedSeconds(false) * 1000;
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
				return GetElapsedSeconds(true) * 1000;
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
			this.timeType = TimeType.Time;
			Reset();
		}

		public UnityTimeStopwatch(TimeType timeType) {
			this.timeType = timeType;
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

		public static UnityTimeStopwatch StartNew(TimeType timeType) {
			UnityTimeStopwatch unityTimeStopwatch = new UnityTimeStopwatch(timeType);
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

		private float GetTimestamp() => timeType switch {
				TimeType.Time => Time.time,
				TimeType.FixedTime => Time.fixedTime,
				TimeType.UnscaledTime => Time.unscaledTime,
				TimeType.UnscaledFixedTime => Time.fixedUnscaledTime,
				_ => throw new System.NotImplementedException()
			};

		private double GetTimestampDouble() => timeType switch {
			TimeType.Time => Time.timeAsDouble,
			TimeType.FixedTime => Time.fixedTimeAsDouble,
			TimeType.UnscaledTime => Time.unscaledTimeAsDouble,
			TimeType.UnscaledFixedTime => Time.fixedUnscaledTimeAsDouble,
			_ => throw new System.NotImplementedException()
		};

		private double GetElapsedSeconds(bool highPrecision) {
			double num = elapsed;
			if (isRunning) {

				num += (highPrecision ? GetTimestampDouble() : GetTimestamp()) - startTimeStamp;
			}

			return num;

		}

	}
}
