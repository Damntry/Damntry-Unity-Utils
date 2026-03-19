using UnityEngine;

namespace Damntry.UtilsUnity.Rendering {
    public class TextureUtils {

        public static Texture2D GenerateRectangle(Color color) {
            int width = 1;
            int height = 1;

            //Create a 1x1 opaque texture
            Texture2D rect = new (width, height, TextureFormat.RGBA32, mipChain: false);

            Color32[] pixels = new Color32[width * height];
            for (int i = 0; i < pixels.Length; i++) {
                pixels[i] = color;
            }

            rect.SetPixels32(pixels);
            rect.Apply();

            return rect;
        }

        public static Texture2D GenerateCircle(int size) {
            Texture2D tex = new (size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            float radius = size / 2f;

            for (int y = 0; y < size; y++) {
                for (int x = 0; x < size; x++) {
                    float dx = x - radius;
                    float dy = y - radius;

                    float dist = Mathf.Sqrt(dx * dx + dy * dy) / radius;

                    float alpha = Mathf.Clamp01(1f - dist);

                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            tex.Apply();
            return tex;
        }

        public static Texture2D GenerateRectRoundCorners(int width, int height, float radius, float edgeSoftness, Color fillColor) {

            Texture2D tex = new(width, height, TextureFormat.RGBA32, false) {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            Vector2 halfSize = new Vector2(width, height) * 0.5f;
            Vector2 rectSize = halfSize - new Vector2(radius, radius);

            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    Vector2 p = new Vector2(x + 0.5f, y + 0.5f) - halfSize;

                    float distance = SignedDistanceRoundedRect(p, rectSize, radius);

                    float alpha = 1f - Mathf.SmoothStep(-edgeSoftness, edgeSoftness, distance);

                    Color c = fillColor;
                    c.a *= alpha;

                    tex.SetPixel(x, y, c);
                }
            }

            tex.Apply();
            return tex;
        }

        private static float SignedDistanceRoundedRect(Vector2 p, Vector2 rect, float radius) {
            Vector2 q = new Vector2(Mathf.Abs(p.x), Mathf.Abs(p.y)) - rect;

            Vector2 outside = new Vector2(Mathf.Max(q.x, 0), Mathf.Max(q.y, 0));

            float outsideDistance = outside.magnitude;
            float insideDistance = Mathf.Min(Mathf.Max(q.x, q.y), 0);

            return outsideDistance + insideDistance - radius;
        }

    }
}
