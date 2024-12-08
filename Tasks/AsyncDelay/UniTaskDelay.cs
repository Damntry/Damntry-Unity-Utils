using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Damntry.Utils.Tasks.AsyncDelay;

namespace Damntry.UtilsUnity.Tasks.AsyncDelay {

	public class UniTaskDelay : AsyncDelayBase<UniTaskDelay> {

		public override Task Delay(int millisecondsDelay) {
			return UniTask.Delay(millisecondsDelay).AsTask();
		}

		public override Task Delay(int millisecondsDelay, CancellationToken cancellationToken) {
			return UniTask.Delay(millisecondsDelay, cancellationToken: cancellationToken).AsTask();
		}

	}

}
