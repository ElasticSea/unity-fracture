using UnityEngine;

namespace Project.Scripts.Utils
{
    public class MeshUtils
    {
         public static Mesh GetCubeMesh(Vector3 size)
        {
            var p0 = new Vector3(-size.x * .5f, -size.y * .5f, size.z * .5f);
            var p1 = new Vector3(size.x * .5f, -size.y * .5f, size.z * .5f);
            var p2 = new Vector3(size.x * .5f, -size.y * .5f, -size.z * .5f);
            var p3 = new Vector3(-size.x * .5f, -size.y * .5f, -size.z * .5f);

            var p4 = new Vector3(-size.x * .5f, size.y * .5f, size.z * .5f);
            var p5 = new Vector3(size.x * .5f, size.y * .5f, size.z * .5f);
            var p6 = new Vector3(size.x * .5f, size.y * .5f, -size.z * .5f);
            var p7 = new Vector3(-size.x * .5f, size.y * .5f, -size.z * .5f);

            Vector3[] vertices =
            {
                // Bottom
                p0, p1, p2, p3,

                // Left
                p7, p4, p0, p3,

                // Front
                p4, p5, p1, p0,

                // Back
                p6, p7, p3, p2,

                // Right
                p5, p6, p2, p1,

                // Top
                p7, p6, p5, p4
            };

            Vector3[] normales =
            {
                Vector3.down, Vector3.down, Vector3.down, Vector3.down,
                Vector3.left, Vector3.left, Vector3.left, Vector3.left,
                Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward,
                Vector3.back, Vector3.back, Vector3.back, Vector3.back,
                Vector3.right, Vector3.right, Vector3.right, Vector3.right,
                Vector3.up, Vector3.up, Vector3.up, Vector3.up
            };

            Vector2[] uvs =
            {
                new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(0f, 0f), new Vector2(1f, 0f),
            };

            int[] triangles =
            {
                // Bottom
                3, 1, 0,
                3, 2, 1,

                // Left
                3 + 4 * 1, 1 + 4 * 1, 0 + 4 * 1,
                3 + 4 * 1, 2 + 4 * 1, 1 + 4 * 1,

                // Front
                3 + 4 * 2, 1 + 4 * 2, 0 + 4 * 2,
                3 + 4 * 2, 2 + 4 * 2, 1 + 4 * 2,

                // Back
                3 + 4 * 3, 1 + 4 * 3, 0 + 4 * 3,
                3 + 4 * 3, 2 + 4 * 3, 1 + 4 * 3,

                // Right
                3 + 4 * 4, 1 + 4 * 4, 0 + 4 * 4,
                3 + 4 * 4, 2 + 4 * 4, 1 + 4 * 4,

                // Top
                3 + 4 * 5, 1 + 4 * 5, 0 + 4 * 5,
                3 + 4 * 5, 2 + 4 * 5, 1 + 4 * 5,
            };

            var mesh = new Mesh
            {
                vertices = vertices,
                triangles = triangles,
                normals = normales,
                uv = uvs
            };
            return mesh;
        }
    }
}