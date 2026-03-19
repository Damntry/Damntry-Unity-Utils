using Damntry.Utils.Logging;
using System.IO;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Damntry.UtilsUnity.Rendering {

    public static class AssetLoading {

        public static Sprite GetSpriteFromFile(string imagePath, Vector2 pivot,
                GraphicsFormat graphicFormat = GraphicsFormat.R8G8B8A8_SRGB, bool nonReadable = true) {

            return GetSpriteFromTexture(
                GetTextureFromFile(imagePath, graphicFormat, nonReadable), 
                pivot
            );
        }

        public static Sprite GetSpriteFromTexture(Texture2D texture, Vector2 pivot) {
            if (!texture) {
                TimeLogger.Logger.LogError($"Texture object is not initialized", LogCategories.Visuals);
                return null;
            }

            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                pivot, pixelsPerUnit: 100, extrude: 0, SpriteMeshType.Tight);
        }

        private static Texture2D GetTextureFromFile(string imagePath, 
                GraphicsFormat graphicFormat = GraphicsFormat.R8G8B8A8_SRGB, 
                bool nonReadable = true) {

            var textureCreationFlags =
                TextureCreationFlags.DontUploadUponCreate |
                TextureCreationFlags.DontInitializePixels |
                TextureCreationFlags.MipChain;

            Texture2D texture = new(width: 4, height: 4,
                    graphicFormat, textureCreationFlags) {

                name = Path.GetFileName(imagePath),
                anisoLevel = 1,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            byte[] data = File.ReadAllBytes(imagePath);
            texture.LoadImage(data, nonReadable);

            return texture;
        }

    }
}
