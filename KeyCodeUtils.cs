using System.Linq;
using System.Text;
using UnityEngine;

namespace Damntry.UtilsUnity {

    public class KeyCodeUtils {

        private static StringBuilder stringBuilder;

        /// <summary>
        /// Returns a single string with the combination of key and modifiers.
        /// </summary>
        /// <param name="useLiterals">
        /// True to use the same enum name used in KeyCode, false to use a more user oriented readable text.
        /// </param>
        /// <param name="key">The key.</param>
        /// <param name="separator">The separator between each KeyCode.</param>
        /// <param name="modifiers">The optional list of modifiers</param>
        public static string KeyCombinationToString(bool useLiterals, KeyCode key, 
                string separator = "+", params KeyCode[] modifiers) {

            stringBuilder ??= new StringBuilder();
            stringBuilder.Clear();

            if (modifiers != null && modifiers.Length > 0) {
                foreach (KeyCode mod in modifiers) {
                    if (mod == KeyCode.None) {
                        continue;
                    }
                    stringBuilder.Append(useLiterals ? mod : KeyCode2UserString(mod))
                        .Append(" ")
                        .Append(separator)
                        .Append(" ");
                }
            }
            
            stringBuilder.Append(useLiterals ? key : KeyCode2UserString(key));

            return stringBuilder.ToString();
        }

        private static string[] KeyCode2UserString(KeyCode[] keyList) =>
            keyList.Select(k => KeyCode2UserString(k)).ToArray();

        public static string KeyCode2UserString(KeyCode keyCode) =>
            keyCode switch {
                KeyCode.Alpha0 => "0",
                KeyCode.Alpha1 => "1",
                KeyCode.Alpha2 => "2",
                KeyCode.Alpha3 => "3",
                KeyCode.Alpha4 => "4",
                KeyCode.Alpha5 => "5",
                KeyCode.Alpha6 => "6",
                KeyCode.Alpha7 => "7",
                KeyCode.Alpha8 => "8",
                KeyCode.Alpha9 => "9",
                KeyCode.Ampersand => "&",
                KeyCode.Asterisk => "*",
                KeyCode.Dollar => "$",
                KeyCode.Hash => "#",
                //Modifiers
                KeyCode.LeftControl => "Left Ctrl",
                KeyCode.RightControl => "Right Ctrl",
                KeyCode.LeftShift => "Left Shift",
                KeyCode.RightShift => "Right Shift",
                KeyCode.LeftAlt => "Left Alt",
                KeyCode.RightAlt => "Right Alt",
                KeyCode.AltGr => "Alt Gr",
                KeyCode.LeftWindows => "Left Win",
                KeyCode.RightWindows => "Right Win",
                _ => keyCode.ToString()
            };

    }
}
