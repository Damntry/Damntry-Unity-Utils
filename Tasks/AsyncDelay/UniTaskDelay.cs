using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Damntry.Utils.Tasks.AsyncDelay;

namespace Damntry.UtilsUnity.Tasks.AsyncDelay {

	public class UniTaskDelay : AsyncDelayBase<UniTaskDelay> {

		public override Task Delay(int millisecondsDelay) {
			return UniTask.Delay(AdaptToUnitask(millisecondsDelay)).AsTask();
		}

		public override Task Delay(int millisecondsDelay, CancellationToken cancellationToken) {
			return UniTask.Delay(AdaptToUnitask(millisecondsDelay), cancellationToken: cancellationToken).AsTask();
		}

		/// <summary>
		/// Unitask does not allow negative values, so instead we pass the longest int possible.
		/// </summary>
		private int AdaptToUnitask(int millisecondsDelay) {
			return millisecondsDelay >= 0 ? millisecondsDelay : int.MaxValue;
		}

	}

}
