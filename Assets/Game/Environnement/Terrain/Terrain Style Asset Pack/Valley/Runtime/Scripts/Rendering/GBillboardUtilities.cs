#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pinwheel.Griffin.Rendering
{
    /// <summary>
    /// Utility class for getting the mesh corresponding to a billboard asset.
    /// </summary>
    public static class GBillboardUtilities
    {
        private static Dictionary<BillboardAsset, Mesh> billboardMeshes;
        private static Dictionary<BillboardAsset, Mesh> BillboardMeshes
        {
            get
            {
                if (billboardMeshes == null)
                {
                    billboardMeshes = new Dictionary<BillboardAsset, Mesh>();
                }
                return billboardMeshes;
            }
        }

        /// <summary>
        /// Get the billboard mesh for rendering.
        /// </summary>
        /// <param name="billboard">The billboard asset, usually created with Billboard Tool.</param>
        /// <returns>The shared mesh for rendering.</returns>
        public static Mesh GetMesh(BillboardAsset billboard)
        {
            if (!BillboardMeshes.ContainsKey(billboard))
            {
                BillboardMeshes.Add(billboard, null);
            }
            if (BillboardMeshes[billboard] == null)
            {
                Mesh m = CreateMesh(billboard);
                BillboardMeshes[billboard] = m;
            }
            return BillboardMeshes[billboard];
        }

        private static Mesh CreateMesh(BillboardAsset billboard)
        {
            Mesh m = new Mesh();
            Vector2[] uvs = billboard.GetVertices();
            Vector3[] vertices = new Vector3[billboard.vertexCount];
            for (int i = 0; i < vertices.Length; ++i)
            {
                vertices[i] = new Vector3(
                    (uvs[i].x - 0.5f) * billboard.width,
                    uvs[i].y * billboard.height + billboard.bottom,
                    0);
            }
            ushort[] tris = billboard.GetIndices();
            int[] trisInt = new int[tris.Length];
            for (int i = 0; i < trisInt.Length; ++i)
            {
                trisInt[i] = tris[i];
            }

            m.vertices = vertices;
            m.uv = uvs;
            m.triangles = trisInt;
            m.name = billboard.name;
            m.RecalculateNormals();
            m.RecalculateTangents();
            m.RecalculateBounds();
            return m;
        }

        public static void CleanUp()
        {
            foreach (Mesh m in BillboardMeshes.Values)
            {
                GUtilities.DestroyObject(m);
            }
            BillboardMeshes.Clear();
        }
    }
}
#endif
