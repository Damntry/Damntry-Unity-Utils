using Damntry.Utils.Collections;
using Damntry.UtilsUnity.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SuperQoLity {

	/* Ended up unused in favor of sucking it up and doing it in a more sane 
	 *	way with the Unity editor and exporting an AssetBundle for later use.
	 *	Some good came out from this code though, and it is being used around.
	 *	
	 *	This does work, its just that even if it where fully finished, it
	 *	would have been only good for making relatively simple panels and I
	 *	ended up expanding my UI plans, so this fell out of place. Specially
	 *	considering the amount of work that there was left to do.
	

	public class UIPanelBuilder : UIGameObjectElement {
	

		private readonly int UI_Layer;

		internal static GameObject MainPanelContainer { get; private set; }

		//Add a new abstract class or something that has the functionality to 
		//		save its object into some cache dictionary with the name as key and object as value.
		//		It will be used to save commonly accessed elements like text labels, images, etc.
		//		There will be a parameter or something where you can specify if the element is static
		//		and will never change, to skip adding it innecessarily to the cache.

		//I just noticed I can create a panel, but now I need to have methods to
		//		access its contents in a easy way, from the cache I have yet to create.
		//		I should be able to do it both by getting a single element by name, and
		//		by manually transversing the hierarchy.


		/// <param name="UILayer">The Unity layer used in the project for UI elements.
		/// Default Unity UI is 5.</param>
		public UIPanelBuilder(int UILayer = 5) {
			this.UI_Layer = UILayer;
		}

		
		public InContainer<UIPanelBuilder> CreateMainPanelContainer(string containerName) {
			MainPanelContainer = CreateBasicPanelContainer(containerName);
			MainPanelContainer.AddComponent<CanvasRenderer>();
			MainPanelContainer.layer = UI_Layer;

			return new InContainer<UIPanelBuilder>(MainPanelContainer, this, UI_Layer);
		}

		public class InContainer<P> : UIGameObjectElement<GameObject, P>
			where P : UIGameObjectElement {

			internal InContainer(GameObject panel, P parent, int UILayer) : 
				base(panel, parent, UILayer) {}

			public InContainer<InContainer<P>> CreateSubContainer(string containerName) {
				return new InContainer<InContainer<P>>(CreateBasicPanelContainer(containerName), this, UILayer);
			}

			/// <summary>
			/// Sets the size of the rect. Ignoring or passing -1 in a 
			/// parameter, will keep the related size value as is.
			/// </summary>
			public InContainer<P> SetSize(float width = -1, float height = -1) {
				RectTransform rect = unityObject.GetComponent<RectTransform>();
				if (width != -1) {
					rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
				}
				if (height != -1) {
					rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
				}
				return this;
			}

			public InContainer<P> SetPosition(float x, float y) {
				unityObject.transform.position = new Vector3(x, y);
				return this;
			}

			public InImage<InContainer<P>> AddImage() {
				Image image = this.unityObject.AddComponent<Image>();
				return new InImage<InContainer<P>>(image, this);
			}

			public InText<InContainer<P>> AddText() {
				TextMeshProUGUI tmPro = this.unityObject.AddComponent<TextMeshProUGUI>();
				return new InText<InContainer<P>>(tmPro, this);
			}

		}

		public class InImage<P> : UIComponentElement<Image, P> 
			where P : UIComponentElement {

			internal InImage(Image image, P parent) : 
				base(image, parent) { }

			//Image stuff

		}

		public class InText<P> : UIComponentElement<TextMeshProUGUI, P>
			where P : UIComponentElement {

			internal InText(TextMeshProUGUI tmPro, P parent) : 
				base(tmPro, parent) { }
						
			public InText<P> SetText(string text) {
				unityObject.text = text;
				return this;
			}

			//Add configurable text member

		}

		private static GameObject CreateBasicPanelContainer(string containerName) {
			GameObject panelContainer = new GameObject(containerName);
			
			panelContainer.AddComponent<RectTransform>();

			return panelContainer;
		}

	}


	public abstract class UIComponentElement {
		/// <summary>Gets the generated panel.</summary>
		public GameObject GetMainPanelContainer() {
			return UIPanelBuilder.MainPanelContainer;
		}
		/// <summary>Adds the generated panel into the Canvas transform passed by parameter.</summary>
		public GameObject AttachToCanvas(Transform canvasTransform) {
			GameObject mainPanel = GetMainPanelContainer();
			CanvasMethods.AttachPanelToCanvas(mainPanel, canvasTransform);
			return mainPanel;
		}

	}

	public abstract class UIGameObjectElement : UIComponentElement {	}


	public abstract class UIComponentElement<O, P> : UIGameObjectElement
		where O : UnityEngine.Object
		where P : UIComponentElement {

		protected O unityObject;

		public P Parent { get; private set; }

		internal UIComponentElement(O unityObject, P parent) {
			this.Parent = parent;
			this.unityObject = unityObject;
		}

		private UIComponentElement() {	}

	}

	public abstract class UIGameObjectElement<O, P> : UIComponentElement<O, P>
	where O : UnityEngine.Object
	where P : UIGameObjectElement {

		protected int UILayer;

		internal UIGameObjectElement(O unityObject, P parent, int UILayer) :
				base(unityObject, parent) {
			this.UILayer = UILayer;
		}

	}

	public abstract class CachedHierarchy() {

		private TreeNode<UIComponentElement> hierarchy;

	}
	*/
}