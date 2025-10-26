#if GRIFFIN
using System.Collections.Generic;
using UnityEngine;

namespace Pinwheel.Griffin.TextureTool
{
    [ExcludeFromDoc]
    public class GNormalMapGenerator : IGTextureGenerator
    {
        public void Generate(RenderTexture targetRt)
        {
            GNormalMapGeneratorParams param = GTextureToolParams.Instance.NormalMap;
            Color defaultColor = param.Space == GNormalMapSpace.Local ? new Color(0.5f, 1, 0.5f, 1) : new Color(0.5f, 0.5f, 1, 1);
            if (param.Terrain == null || param.Terrain.TerrainData == null)
            {
                GCommon.FillTexture(targetRt, defaultColor);
            }
            else
            {
                if (param.Mode == GNormalMapMode.Sharp)
                {
                    RenderSharpNormalMap(param, targetRt);
                }
                else if (param.Mode == GNormalMapMode.Interpolated)
                {
                    RenderInterpolatedNormalMap(param, targetRt);
                }
                else if (param.Mode == GNormalMapMode.PerPixel)
                {
                    RenderPerPixelNormalMap(param, targetRt);
                }
                else
                {
                    GCommon.FillTexture(targetRt, defaultColor);
                }
            }
        }

        public void RenderSharpNormalMap(GNormalMapGeneratorParams param, RenderTexture targetRt)
        {
            Material mat = GInternalMaterials.TerrainNormalMapRendererMaterial;
            mat.SetInt("_TangentSpace", param.Space == GNormalMapSpace.Tangent ? 1 : 0);
            mat.SetPass(0);
            RenderTexture.active = targetRt;
            GL.PushMatrix();
            GL.LoadOrtho();

            GTerrainChunk[] chunks = param.Terrain.GetChunks();
            for (int i = 0; i < chunks.Length; ++i)
            {
                Mesh m = chunks[i].MeshFilterComponent.sharedMesh;
                if (m == null)
                    continue;
                Graphics.DrawMeshNow(m, Matrix4x4.identity);
            }
            GL.PopMatrix();
            RenderTexture.active = null;
        }

        public void RenderInterpolatedNormalMap(GNormalMapGeneratorParams param, RenderTexture targetRt)
        {
            Material mat = GInternalMaterials.TerrainNormalMapRendererMaterial;
            mat.SetInt("_TangentSpace", param.Space == GNormalMapSpace.Tangent ? 1 : 0);
            mat.SetPass(0);
            RenderTexture sharpNormalRT = RenderTexture.GetTemporary(targetRt.descriptor);
            RenderTexture.active = sharpNormalRT;
            GL.PushMatrix();
            GL.LoadOrtho();

            GTerrainChunk[] chunks = param.Terrain.GetChunks();
            for (int i = 0; i < chunks.Length; ++i)
            {
                Mesh m = chunks[i].MeshFilterComponent.sharedMesh;
                if (m == null)
                    continue;
                Graphics.DrawMeshNow(m, Matrix4x4.identity);
            }

            RenderTexture.active = targetRt; //release the sharpNormalRT from active RT so it can be binded to the material

            mat.SetTexture("_SharpNormalMap", sharpNormalRT);
            mat.SetVector("_SharpNormalMap_TexelSize", sharpNormalRT.texelSize);
            mat.SetPass(1);
            for (int i = 0; i < chunks.Length; ++i)
            {
                Mesh m = chunks[i].MeshFilterComponent.sharedMesh;
                if (m == null)
                    continue;
                Graphics.DrawMeshNow(m, Matrix4x4.identity);
            }

            GL.PopMatrix();

            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(sharpNormalRT);
        }

        public void RenderPerPixelNormalMap(GNormalMapGeneratorParams param, RenderTexture targetRt)
        {
            Material mat = GInternalMaterials.TerrainPerPixelNormalMapRendererMaterial;
            mat.SetTexture("_HeightMap", param.Terrain.TerrainData.Geometry.HeightMap);
            mat.SetFloat("_Width", param.Terrain.TerrainData.Geometry.Width);
            mat.SetFloat("_Height", param.Terrain.TerrainData.Geometry.Height);
            mat.SetFloat("_Length", param.Terrain.TerrainData.Geometry.Length);
            mat.SetInt("_TangentSpace", param.Space == GNormalMapSpace.Tangent ? 1 : 0);

            GCommon.DrawQuad(targetRt, GCommon.FullRectUvPoints, mat, 0);
        }
    }
}
#endif
