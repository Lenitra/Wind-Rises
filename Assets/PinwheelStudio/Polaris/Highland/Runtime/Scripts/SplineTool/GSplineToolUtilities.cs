#if GRIFFIN && !GRIFFIN_EXCLUDE_HIGHLAND
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityGraphics = UnityEngine.Graphics;

namespace Pinwheel.Griffin.SplineTool
{
    /// <summary>
    /// Contains functions for extracting spline mask texture.
    /// </summary>
    public static class GSplineToolUtilities
    {
        private static string SHADER_NAME = "Hidden/Griffin/SplineExtract";
        private static readonly int VERTICES = Shader.PropertyToID("_Vertices");
        private static readonly int ALPHAS = Shader.PropertyToID("_Alphas");
        private static readonly int WORLD_BOUNDS = Shader.PropertyToID("_WorldBounds");
        private static readonly int TEXTURE_SIZE = Shader.PropertyToID("_TextureSize");
        private static readonly int DEPTH_BUFFER = Shader.PropertyToID("_DepthBuffer");
        private static readonly int MAX_HEIGHT = Shader.PropertyToID("_MaxHeight");

        private static readonly int PASS_DEPTH = 0;
        private static readonly int PASS_MASK_ALPHA = 1;
        private static readonly int PASS_MASK_BOOL = 2;
        //private static readonly int PASS_HEIGHT_MASK = 3;
        private static readonly int PASS_HEIGHT = 4;

        /// <summary>
        /// Render the spline alpha mask on a texture.
        /// </summary>
        /// <param name="targetRt">The texture to render on.</param>
        /// <param name="worldTrianglesBuffer">The buffer containing all vertices in world space.</param>
        /// <param name="alphasBuffer">The buffer containing all alpha values.</param>
        /// <param name="vertexCount">Number of vertex.</param>
        /// <param name="worldBounds">The world region to extract from, usually the terrain's bounds.</param>
        /// 
        /// <seealso cref="GSplineCreator.GenerateWorldVerticesAndAlphas(Vector3[], float[], int, float, float)"/>
        /// <seealso cref="GSplineCreator.GetVerticesCount(int, float, float)"/>
        public static void RenderAlphaMask(RenderTexture targetRt, ComputeBuffer worldTrianglesBuffer, ComputeBuffer alphasBuffer, int vertexCount, Vector4 worldBounds)
        {
            ComputeBuffer depthBuffer = new ComputeBuffer(targetRt.width * targetRt.height, sizeof(int));
            
            Material material = new Material(Shader.Find(SHADER_NAME));
            material.SetBuffer(VERTICES, worldTrianglesBuffer);
            material.SetBuffer(ALPHAS, alphasBuffer);
            material.SetVector(WORLD_BOUNDS, worldBounds);
            material.SetVector(TEXTURE_SIZE, new Vector2(targetRt.width, targetRt.height));
            material.SetBuffer(DEPTH_BUFFER, depthBuffer);

            UnityGraphics.SetRandomWriteTarget(1, depthBuffer);
            RenderTexture.active = targetRt;
            GL.Clear(true, true, Color.black);
            GL.PushMatrix();
            GL.LoadOrtho();
            material.SetPass(PASS_DEPTH);
            UnityGraphics.DrawProceduralNow(MeshTopology.Triangles, vertexCount);
            material.SetPass(PASS_MASK_ALPHA);
            UnityGraphics.DrawProceduralNow(MeshTopology.Triangles, vertexCount);
            GL.PopMatrix();
            RenderTexture.active = null;
            UnityGraphics.ClearRandomWriteTargets();

            depthBuffer.Release();
            Object.DestroyImmediate(material);
        }

        /// <summary>
        /// Render the spline bool mask on a texture. The mask contains 1 where the spline pass through, otherwise 0.
        /// </summary>
        /// <param name="targetRt">The texture to render on.</param>
        /// <param name="worldTrianglesBuffer">The buffer containing all vertices in world space.</param>
        /// <param name="vertexCount">Number of vertex.</param>
        /// <param name="worldBounds">The world region to extract from, usually the terrain's bounds.</param>
        /// 
        /// <seealso cref="GSplineCreator.GenerateWorldVerticesAndAlphas(Vector3[], float[], int, float, float)"/>
        /// <seealso cref="GSplineCreator.GetVerticesCount(int, float, float)"/>
        public static void RenderBoolMask(RenderTexture targetRt, ComputeBuffer worldTrianglesBuffer, int vertexCount, Vector4 worldBounds)
        {
            Material material = new Material(Shader.Find(SHADER_NAME));
            material.SetBuffer(VERTICES, worldTrianglesBuffer);
            material.SetVector(WORLD_BOUNDS, worldBounds);
            material.SetVector(TEXTURE_SIZE, new Vector2(targetRt.width, targetRt.height));

            RenderTexture.active = targetRt;
            GL.PushMatrix();
            GL.LoadOrtho();
            material.SetPass(PASS_MASK_BOOL);
            UnityGraphics.DrawProceduralNow(MeshTopology.Triangles, vertexCount);
            GL.PopMatrix();
            RenderTexture.active = null;
            Object.DestroyImmediate(material);
        }

        /// <summary>
        /// Render the spline height map on a texture. It contains value in [0,1] relative to [0,maxHeight].
        /// </summary>
        /// <param name="targetRt">The texture to render on.</param>
        /// <param name="worldTrianglesBuffer">The buffer containing all vertices in world space.</param>
        /// <param name="alphasBuffer">The buffer containing all alpha values.</param>
        /// <param name="vertexCount">Number of vertex.</param>
        /// <param name="worldBounds">The world region to extract from, usually the terrain's bounds.</param>
        /// <param name="maxHeight">Maximum height level. Usually the terrain's max height.</param>
        /// 
        /// <seealso cref="GSplineCreator.GenerateWorldVerticesAndAlphas(Vector3[], float[], int, float, float)"/>
        /// <seealso cref="GSplineCreator.GetVerticesCount(int, float, float)"/>
        public static void RenderHeightMap(RenderTexture targetRt, ComputeBuffer worldTrianglesBuffer, ComputeBuffer alphasBuffer, int vertexCount, Vector4 worldBounds, float maxHeight)
        {
            ComputeBuffer depthBuffer = new ComputeBuffer(targetRt.width * targetRt.height, sizeof(int));

            Material material = new Material(Shader.Find(SHADER_NAME));
            material.SetBuffer(VERTICES, worldTrianglesBuffer);
            material.SetBuffer(ALPHAS, alphasBuffer);
            material.SetVector(WORLD_BOUNDS, worldBounds);
            material.SetFloat(MAX_HEIGHT, maxHeight);
            material.SetVector(TEXTURE_SIZE, new Vector2(targetRt.width, targetRt.height));
            material.SetBuffer(DEPTH_BUFFER, depthBuffer);

            UnityGraphics.SetRandomWriteTarget(1, depthBuffer);
            RenderTexture.active = targetRt;
            GL.Clear(true, true, Color.black);
            GL.PushMatrix();
            GL.LoadOrtho();
            //material.SetPass(PASS_HEIGHT_MASK);
            //UnityGraphics.DrawProceduralNow(MeshTopology.Triangles, vertexCount);
            material.SetPass(PASS_HEIGHT);
            UnityGraphics.DrawProceduralNow(MeshTopology.Triangles, vertexCount);
            GL.PopMatrix();
            RenderTexture.active = null;
            UnityGraphics.ClearRandomWriteTargets();

            depthBuffer.Release();
            Object.DestroyImmediate(material);
        }
    }
}
#endif
