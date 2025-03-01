namespace Damntry.UtilsUnity.Tasks {

	/*
	[Obsolete("Incomplete and abandoned at its early stages. Use UniTask.Delay instead.", true)]
	public class UnityDelay {

		public static Task Delay(int millisecondsDelay) {
			return Delay(millisecondsDelay, default(CancellationToken));
		}


		public async static Task Delay(int millisecondsDelay, CancellationToken cancellationToken) {
			if (millisecondsDelay < -1) {
				throw new ArgumentOutOfRangeException("millisecondsDelay", "The argumento millisecondsDelay must be greater or equals than -1.");
			}

			if (millisecondsDelay == 0) {
				return;
			}
			if (millisecondsDelay == -1 && !cancellationToken.CanBeCanceled) {

			}

			if (cancellationToken.CanBeCanceled) {
				if (cancellationToken.IsCancellationRequested) {
					return;
				}
			} else if (millisecondsDelay == -1) {
				
				
			}

			

			float time = 0;
			double timeDouble = 0;
			Performance.Start("Time.time", true);
			for (int i = 0; i < 100000; i++) {
				time = Time.time;
			}
			Performance.StopLogAndReset("Time.time");
			Performance.Start("Time.timeAsDouble", true);
			for (int i = 0; i < 100000; i++) {
				timeDouble = Time.timeAsDouble;
			}
			Performance.StopLogAndReset("Time.timeAsDouble");
			Performance.Start("Time.deltaTime", true);
			for (int i = 0; i < 100000; i++) {
				time = Time.deltaTime;
			}
			Performance.StopLogAndReset("Time.deltaTime");


			//I should probably just use a coroutine so its checked every frame so it works 
			//	more like Unity does in general. I ll need to test the performance of each though.

			//I need to add the case where millisecondsDelay != -1, where I can avoid using unityTimeStopwatch... but 
			//		then whats the point of using this class? I guess I should give it support anyway by calling a normal Task.Delay?

			if (cancellationToken.CanBeCanceled) {
				UnityTimeStopwatch unityTimeStopwatch = UnityTimeStopwatch.StartNew();

				while (unityTimeStopwatch.ElapsedMillisecondsPrecise < millisecondsDelay && !cancellationToken.IsCancellationRequested) {
					await Task.Delay(1);    //On windows this can vary up to its 15ms resolution.
				}
			} else {
				UnityTimeStopwatch unityTimeStopwatch = UnityTimeStopwatch.StartNew();

				while (unityTimeStopwatch.ElapsedMillisecondsPrecise < millisecondsDelay) {
					await Task.Delay(1);    //On windows this can vary up to its 15ms resolution.
				}
			}
			
		}

	}
	*/
}
