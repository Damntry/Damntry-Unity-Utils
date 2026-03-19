using System.Xml.Linq;
using UnityEngine;

namespace Damntry.UtilsUnity.Rendering {

    public class MeshUtils {

        /// <summary>
        /// Same as GameObject.CreatePrimitive(PrimitiveType.Quad), but skipps adding the collider component.
        /// </summary>
        public static GameObject CreateQuadPrimitive(string name, float width, float height, bool shadowsOff = true) {
            GameObject obj = new (name);
            MeshFilter meshFilter = obj.AddComponent<MeshFilter>();
            meshFilter.mesh = CreateQuadMesh(width, height);
            Renderer renderer = obj.AddComponent<MeshRenderer>();
            if (shadowsOff) {
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }

            return obj;
        }

        public static GameObject CreatePlanePrimitive(string name, float width, float height, bool shadowsOff = true) {
            GameObject obj = new (name);
            MeshFilter meshFilter = obj.AddComponent<MeshFilter>();
            meshFilter.mesh = CreatePlaneMesh(width, height);
            Renderer renderer = obj.AddComponent<MeshRenderer>();
            if (shadowsOff) {
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }

            return obj;
        }

        public static GameObject CreateCubePrimitive(string name, float width, float height, float depth, bool shadowsOff = true) {
            GameObject obj = new (name);
            MeshFilter meshFilter = obj.AddComponent<MeshFilter>();
            meshFilter.mesh = CreateCubeMesh(width, height, depth);
            Renderer renderer = obj.AddComponent<MeshRenderer>();
            if (shadowsOff) {
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }

            return obj;
        }

        

        /// <summary>
        /// https://docs.unity3d.com/6000.2/Documentation/Manual/Example-CreatingaBillboardPlane.html
        /// </summary>
        public static Mesh CreateQuadMesh(float width, float height) {
            Mesh mesh = new ();

            Vector3[] vertices =
            [
                new (0, 0, 0),
                new (width, 0, 0),
                new (0, height, 0),
                new (width, height, 0)
            ];
            mesh.vertices = vertices;

            int[] tris =
            [
                // lower left triangle
                0, 2, 1,
                // upper right triangle
                2, 3, 1
            ];
            mesh.triangles = tris;

            Vector3[] normals =
            [
                -Vector3.forward,
                -Vector3.forward,
                -Vector3.forward,
                -Vector3.forward
            ];
            mesh.normals = normals;

            Vector2[] uv =
            [
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(1, 1)
            ];
            mesh.uv = uv;

            return mesh;
        }


        public static Mesh CreatePlaneMesh(float width, float height) {
            int widthSegments = Mathf.Max(1, Mathf.RoundToInt(width));
            int heightSegments = Mathf.Max(1, Mathf.RoundToInt(height));

            return CreatePlaneMesh(width, height, widthSegments, heightSegments);
        }

        public static Mesh CreatePlaneMesh(float width, float height, int widthSegments = 10, int heightSegments = 10) {
            widthSegments = Mathf.Max(1, widthSegments);
            heightSegments = Mathf.Max(1, heightSegments);

            float halfW = width * 0.5f;
            float halfH = height * 0.5f;

            int vertCount = (widthSegments + 1) * (heightSegments + 1);
            int triCount = widthSegments * heightSegments * 6;

            Vector3[] vertices = new Vector3[vertCount];
            Vector3[] normals = new Vector3[vertCount];
            Vector4[] tangents = new Vector4[vertCount];
            Vector2[] uvs = new Vector2[vertCount];
            int[] triangles = new int[triCount];

            int v = 0;
            for (int y = 0; y <= heightSegments; y++) {
                float yf = (float)y / heightSegments;
                float z = yf * height - halfH;

                for (int x = 0; x <= widthSegments; x++) {
                    float xf = (float)x / widthSegments;
                    float xPos = xf * width - halfW;

                    vertices[v] = new Vector3(xPos, 0f, z);
                    normals[v] = Vector3.up;
                    tangents[v] = new Vector4(1f, 0f, 0f, -1f);
                    uvs[v] = new Vector2(xf, yf);
                    v++;
                }
            }

            int t = 0;
            int i = 0;
            for (int y = 0; y < heightSegments; y++) {
                for (int x = 0; x < widthSegments; x++) {
                    triangles[t++] = i;
                    triangles[t++] = i + widthSegments + 1;
                    triangles[t++] = i + 1;

                    triangles[t++] = i + 1;
                    triangles[t++] = i + widthSegments + 1;
                    triangles[t++] = i + widthSegments + 2;

                    i++;
                }
                i++;
            }

            Mesh mesh = new();
            mesh.name = "Plane";

            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.tangents = tangents;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();

            return mesh;
        }

        public static Mesh CreateCubeMesh(float width, float height, float depth) {
            float hx = width * 0.5f;
            float hy = height * 0.5f;
            float hz = depth * 0.5f;

            // 24 vertices (4 per face × 6 faces)
            Vector3[] vertices = {
                // Front (+Z)
                new(-hx, -hy,  hz), new(hx, -hy,  hz), new(hx,  hy,  hz), new(-hx,  hy,  hz),
                // Back (-Z)
                new(hx, -hy, -hz), new(-hx, -hy, -hz), new(-hx,  hy, -hz), new(hx,  hy, -hz),
                // Left (-X)
                new(-hx, -hy, -hz), new(-hx, -hy,  hz), new(-hx,  hy,  hz), new(-hx,  hy, -hz),
                // Right (+X)
                new(hx, -hy,  hz), new(hx, -hy, -hz), new(hx,  hy, -hz), new(hx,  hy,  hz),
                // Top (+Y)
                new(-hx,  hy,  hz), new(hx,  hy,  hz), new(hx,  hy, -hz), new(-hx,  hy, -hz),
                // Bottom (-Y)
                new(-hx, -hy, -hz), new(hx, -hy, -hz), new(hx, -hy,  hz), new(-hx, -hy,  hz),
            };

            Vector3[] normals = {
                // Front
                Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward,
                // Back
                Vector3.back, Vector3.back, Vector3.back, Vector3.back,
                // Left
                Vector3.left, Vector3.left, Vector3.left, Vector3.left,
                // Right
                Vector3.right, Vector3.right, Vector3.right, Vector3.right,
                // Top
                Vector3.up, Vector3.up, Vector3.up, Vector3.up,
                // Bottom
                Vector3.down, Vector3.down, Vector3.down, Vector3.down,
            };

            Vector4 tangent = new(1f, 0f, 0f, -1f);
            Vector4[] tangents = new Vector4[24];
            for (int i = 0; i < tangents.Length; i++)
                tangents[i] = tangent;

            Vector2[] uvs = {
                new(0,0), new(1,0), new(1,1), new(0,1),
                new(0,0), new(1,0), new(1,1), new(0,1),
                new(0,0), new(1,0), new(1,1), new(0,1),
                new(0,0), new(1,0), new(1,1), new(0,1),
                new(0,0), new(1,0), new(1,1), new(0,1),
                new(0,0), new(1,0), new(1,1), new(0,1),
            };

            int[] triangles = {
                0,  2,  1,  0,  3,  2,      // Front
                4,  6,  5,  4,  7,  6,      // Back
                8, 10,  9,  8, 11, 10,      // Left
                12,14, 13, 12, 15, 14,      // Right
                16,18, 17, 16, 19, 18,      // Top
                20,22, 21, 20, 23, 22       // Bottom
            };

            Mesh mesh = new ();
            mesh.name = "Cube";

            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.tangents = tangents;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();

            return mesh;
        }

    }
}
