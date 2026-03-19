using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Damntry.UtilsUnity.ExtensionMethods
{
	public static class UnityObjectExtensions {

		/// <summary>
		/// Gets the GameObject associated to this component, ensuring that no 
		/// NullReferenceException is thrown no matter the state of the component.
		/// </summary>
		/// <param name="comp">The Unity Object.</param>
		/// <returns>The GameObject, or null if the component is null or non alive.</returns>
		public static GameObject GameObject(this Component comp) =>
            //Needed for Unity and the nice idea of allowing non null, but not "alive" objects,
            //	that throw a null exception when trying to access its GameObject.
            //	UnityEngine.Object overrides the boolean comparison and the operators ==, and !=,
            //	so those do the null-like check correctly, but not with other operators like ?? or ??=
            //	(not yet at least), so they fail to check if the object is alive or not.
            comp ? comp.gameObject : null;

        /// <summary>
        /// Gets the alive object instance, or null if the object has been disposed.
        /// </summary>
        public static T NullableObject<T>(this T obj) where T : Object =>
            obj ? obj : null;

        /// <summary>
        /// Returns the object if alive, otherwise assigns to it the instance passed by parameter, and returns itself.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="instance">The instance to assign if the object is not alive.</param>
        public static T AssignIfDeadReference<T>(ref T obj, T instance) where T : Object {
            if (!obj) {
                obj = instance;
            }

            return obj;
        }

        public static void RemoveDeadKeyReferences<T1, T2>(this Dictionary<T1, T2> dict)
                where T1 : Component {

            foreach (var item in dict.ToArray()) {
                if (item.Key != true) {
                    dict.Remove(item.Key);
                }
            }
        }

        public static void RemoveDeadValueReferences<T1, T2>(this Dictionary<T1, T2> dict)
                where T2 : Component {

            foreach (var item in dict.ToArray()) {
                if (item.Value != true) {
                    dict.Remove(item.Key);
                }
            }
        }

    }
}
