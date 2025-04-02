using UnityEngine;

namespace Damntry.UtilsUnity.ExtensionMethods
{
	public static class ComponentExtensions {

		/// <summary>
		/// Gets the GameObject associated to this component, ensuring that no 
		/// NullReferenceException is thrown no matter the state of the component.
		/// </summary>
		/// <param name="comp">The Unity Object.</param>
		/// <returns>The GameObject, or null if the component is null or non alive.</returns>
		public static GameObject GameObject(this Component comp) {
			//Needed for Unity and the nice idea of allowing non null, but not "alive" objects,
			//	that throw a null exception when trying to access its GameObject.
			//	UnityEngine.Object overrides the boolean comparison, and the operators ==, and !=,
			//	so those do the null-like check correctly, but not other operators like ?? or ??=
			//	(or not yet at least), so they fail to check if the object is alive or not.
			if (comp == null) {
				return null;
			}
			return comp ? comp.gameObject : null;
		}

	}
}
