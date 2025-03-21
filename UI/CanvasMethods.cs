using System;
using Damntry.UtilsUnity.UI.Extensions;
using UnityEngine;

namespace Damntry.UtilsUnity.UI {

	public static class CanvasMethods {

		/// <summary>Adds the generated panel to the Canvas transform passed by parameter.</summary>
		public static void AttachPanelToCanvas(GameObject mainPanel, Transform canvasTransformParent) {
			CheckAttachArgs(mainPanel, canvasTransformParent);

			mainPanel.transform.SetParent(canvasTransformParent);
		}

		/// <summary>Adds the generated panel to the Canvas transform passed by parameter.</summary>
		public static void AttachPanelToCanvasWithAnchor(GameObject mainPanel, Transform canvasTransformParent) {
			CheckAttachArgs(mainPanel, canvasTransformParent);

			mainPanel.transform.SetParent(canvasTransformParent);

			RectTransform rect = mainPanel.GetComponent<RectTransform>();
			rect.SetAnchor(AnchorPresets.TopCenter);
			rect.sizeDelta = Vector2.zero;
			rect.offsetMin = Vector2.zero;
			rect.offsetMax = Vector2.zero;
			//rect.anchoredPosition = canvasTransformParent.position;
			rect.anchoredPosition = Vector2.zero;
		}

		private static void CheckAttachArgs(GameObject mainPanel, Transform canvasTransformParent) {
			if (mainPanel == null) {
				throw new ArgumentNullException(nameof(mainPanel));
			}
			if (canvasTransformParent == null) {
				throw new ArgumentNullException(nameof(canvasTransformParent));
			}
		}

	}
}
