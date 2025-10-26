#if GRIFFIN
using System.Collections.Generic;
using UnityEngine;

namespace Pinwheel.Griffin.BillboardTool
{
    /// <summary>
    /// Utility class for creating billboard asset and texture for tree.
    /// </summary>
    public static class GBillboardCreator
    {
        private static MaterialPropertyBlock materialProperties;
        private static MaterialPropertyBlock MaterialProperties
        {
            get
            {
                if (materialProperties == null)
                    materialProperties = new MaterialPropertyBlock();
                return materialProperties;
            }
        }

        /// <summary>
        /// Make sure the provided render texture is in correct format and dimension before rendering the billboard texture
        /// </summary>
        /// <param name="rt">The render texture to render on.</param>
        /// <param name="args">Billboard creation arguments.</param>
        public static void PrepareRenderTexture(ref RenderTexture rt, GBillboardCreatorArgs args)
        {
            int width = args.Column * args.CellSize;
            int height = args.Row * args.CellSize;
            if (args.Mode == GBillboardRenderMode.Flipbook)
            {
                width = args.CellSize;
                height = args.CellSize;
            }

            int depth = 16;


            RenderTextureFormat format = RenderTextureFormat.ARGB32;
            RenderTextureReadWrite rw = RenderTextureReadWrite.Default;
            if (rt == null ||
                rt.width != width ||
                rt.height != height ||
                rt.depth != depth ||
                rt.format != format)
            {
                if (rt != null)
                {
                    rt.Release();
                }
                rt = new RenderTexture(width, height, depth, format, rw);
            }
        }

        /// <summary>
        /// Draw billboards on a render texture.
        /// </summary>
        /// <param name="rt">The render texture to draw on. Call PrepareRenderTexture to create the RT first.</param>
        /// <param name="args">Billboard creation arguments.</param>
        public static void RenderPreview(RenderTexture rt, GBillboardCreatorArgs args)
        {
            if (args.Mode == GBillboardRenderMode.Atlas)
                RenderPreviewAtlas(rt, args);
            else if (args.Mode == GBillboardRenderMode.Normal)
                RenderPreviewNormal(rt, args);
            else if (args.Mode == GBillboardRenderMode.Flipbook)
                RenderPreviewFlipbook(rt, args);
        }

        private static void RenderPreviewAtlas(RenderTexture atlasRT, GBillboardCreatorArgs args)
        {
            Clear(atlasRT, Color.clear);

            if (args.AtlasMaterial == null)
                return;
            args.Mode = GBillboardRenderMode.Atlas;

            Vector2 viewPortSize = new Vector2(1f / args.Column, 1f / args.Row);
            Vector2 viewPortPosition = new Vector2(0, 0);
            RenderTexture cellRT = new RenderTexture(Mathf.RoundToInt(viewPortSize.x * atlasRT.width), Mathf.RoundToInt(viewPortSize.y * atlasRT.height), 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
            if (args.AntiAliasing) 
                cellRT.antiAliasing = 4;
            Camera cam = CreatePreviewCamera(args);
            cam.targetTexture = cellRT;
            GameObject g = CreatePreviewGameObject(cam.transform, args);

            bool isFogEnabled = RenderSettings.fog;
            RenderSettings.fog = false;

            int imageCount = args.Row * args.Column;
            float angleStep = 360f / imageCount;

            for (int y = 0; y < args.Row; ++y)
            {
                for (int x = 0; x < args.Column; ++x)
                {
                    Clear(cellRT, Color.clear);
                    g.transform.rotation = Quaternion.Euler(0, GUtilities.To1DIndex(x, y, args.Column) * angleStep - 90, 0);
                    cam.Render();
                    viewPortPosition = new Vector2(x * viewPortSize.x, y * viewPortSize.y);
                    GCommon.DrawTexture(atlasRT, cellRT, new Rect(viewPortPosition, viewPortSize), GInternalMaterials.UnlitTransparentMaterial);
                }
            }

            RenderSettings.fog = isFogEnabled;

            cam.targetTexture = null;
            GUtilities.DestroyGameobject(cam.gameObject);
            GUtilities.DestroyGameobject(g);
            cellRT.Release();
            GUtilities.DestroyObject(cellRT);

            PostProcessAtlas(atlasRT, args);
        }

        private static void RenderPreviewNormal(RenderTexture rt, GBillboardCreatorArgs args)
        {
            //Clear(rt, new Color(0.5f, 0.5f, 1f, 1f));
            Clear(rt, Color.clear);

            if (args.NormalMaterial == null)
                return;
            args.Mode = GBillboardRenderMode.Normal;

            Vector2 viewPortSize = new Vector2(1f / args.Column, 1f / args.Row);
            Vector2 viewPortPosition = new Vector2(0, 0);
            RenderTexture tempRt = new RenderTexture(Mathf.RoundToInt(viewPortSize.x * rt.width), Mathf.RoundToInt(viewPortSize.y * rt.height), 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
            if (args.AntiAliasing) 
                tempRt.antiAliasing = 4;
            Camera cam = CreatePreviewCamera(args);
            cam.targetTexture = tempRt;
            GameObject g = CreatePreviewGameObject(cam.transform, args);

            bool isFogEnabled = RenderSettings.fog;
            RenderSettings.fog = false;

            int imageCount = args.Row * args.Column;
            float angleStep = 360f / imageCount;

            for (int y = 0; y < args.Row; ++y)
            {
                for (int x = 0; x < args.Column; ++x)
                {
                    g.transform.rotation = Quaternion.Euler(0, GUtilities.To1DIndex(x, y, args.Column) * angleStep - 90, 0);
                    cam.Render();

                    viewPortPosition = new Vector2(x * viewPortSize.x, y * viewPortSize.y);
                    GCommon.DrawTexture(rt, tempRt, new Rect(viewPortPosition, viewPortSize), GInternalMaterials.UnlitTransparentMaterial);
                }
            }

            RenderSettings.fog = isFogEnabled;

            cam.targetTexture = null;
            GUtilities.DestroyGameobject(cam.gameObject);
            GUtilities.DestroyGameobject(g);
            tempRt.Release();
            GUtilities.DestroyObject(tempRt);

            PostProcessAtlas(rt, args);
        }

        private static void RenderPreviewFlipbook(RenderTexture rt, GBillboardCreatorArgs args)
        {
            Clear(rt, Color.clear);

            if (args.AtlasMaterial == null)
                return;
            args.Mode = GBillboardRenderMode.Flipbook;

            Camera cam = CreatePreviewCamera(args);
            cam.targetTexture = rt;
            GameObject g = CreatePreviewGameObject(cam.transform, args);

            bool isFogEnabled = RenderSettings.fog;
            RenderSettings.fog = false;

            int imageCount = args.Row * args.Column;
            float angleStep = 360f / imageCount;

            g.transform.rotation = Quaternion.Euler(0, args.CellIndex * angleStep, 0);
            cam.rect = new Rect(0, 0, 1, 1);
            cam.Render();

            RenderSettings.fog = isFogEnabled;

            cam.targetTexture = null;
            GUtilities.DestroyGameobject(cam.gameObject);
            GUtilities.DestroyGameobject(g);
        }

        private static Camera CreatePreviewCamera(GBillboardCreatorArgs args)
        {
            GameObject previewCam = new GameObject("~BillboardEditorCam");
            //previewCam.hideFlags = HideFlags.HideAndDontSave;
            previewCam.transform.position = -Vector3.one * 10000;
            previewCam.transform.rotation = Quaternion.identity;
            previewCam.transform.localScale = Vector3.one;

            Camera cam = previewCam.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = args.CameraSize;
            cam.clearFlags = CameraClearFlags.SolidColor;
            //cam.backgroundColor = args.Mode == GBillboardRenderMode.Normal ? new Color(0.5f, 0.5f, 1f, 1f) : Color.clear;
            cam.backgroundColor = Color.clear;
            cam.depth = -1000;
            cam.aspect = 1;
            cam.farClipPlane = 2 * Mathf.Abs(args.CameraOffset.z);
            cam.enabled = false;
            return cam;
        }

        private static GameObject CreatePreviewGameObject(Transform cameraTransform, GBillboardCreatorArgs args)
        {
            if (args.Target == null)
            {
                return new GameObject("~EmptyBillboardCreatorTarget");
            }
            GameObject g = GameObject.Instantiate(args.Target) as GameObject;
            g.name = "~BillboardCreatorTarget";
            //g.hideFlags = HideFlags.HideAndDontSave;
            g.transform.position = cameraTransform.transform.TransformPoint(args.CameraOffset);
            g.transform.rotation = cameraTransform.rotation;
            g.transform.localScale = Vector3.one;

            Material baseMaterial = args.Mode == GBillboardRenderMode.Normal ? args.NormalMaterial : args.AtlasMaterial;
            MeshRenderer[] renderers = g.GetComponentsInChildren<MeshRenderer>();
            for (int i = 0; i < renderers.Length; ++i)
            {
                Material[] sharedMaterials = renderers[i].sharedMaterials;
                for (int j = 0; j < sharedMaterials.Length; ++j)
                {
                    //Material mat = Object.Instantiate<Material>(baseMaterial);
                    //Material mat = new Material(baseMaterial.shader);
                    MaterialPropertyBlock mat = new MaterialPropertyBlock();
                    mat.SetColor(args.DesColorProps, Color.white);
                    mat.SetTexture(args.DesTextureProps, Texture2D.whiteTexture);
                    //mat.CopyPropertiesFromMaterial(sharedMaterials[j]);
                    try
                    {
                        if (sharedMaterials[j].HasProperty(args.SrcColorProps))
                        {
                            Color color = sharedMaterials[j].GetColor(args.SrcColorProps);
                            //color.a = 1;
                            if (mat.HasProperty(args.DesColorProps))
                            {
                                mat.SetColor(args.DesColorProps, color);
                            }
                        }
                        else
                        {
                            if (mat.HasProperty(args.DesColorProps))
                            {
                                mat.SetColor(args.DesColorProps, Color.white);
                            }
                        }
                    }
                    catch { }

                    try
                    {
                        if (sharedMaterials[j].HasProperty("_MainTex")) //Builtin & standard shaders
                        {
                            Texture tex = sharedMaterials[j].GetTexture("_MainTex");
                            if (mat.HasProperty(args.DesTextureProps))
                            {
                                mat.SetTexture(args.DesTextureProps, tex ?? Texture2D.whiteTexture);
                            }
                        }
                        else if (sharedMaterials[j].HasProperty("_MainTexture")) //Some popular 3d assets use this property
                        {
                            Texture tex = sharedMaterials[j].GetTexture("_MainTexture");
                            if (mat.HasProperty(args.DesTextureProps))
                            {
                                mat.SetTexture(args.DesTextureProps, tex ?? Texture2D.whiteTexture);
                            }
                        }
                        else if (sharedMaterials[j].HasProperty(args.SrcTextureProps)) //Custom property name
                        {
                            Texture tex = sharedMaterials[j].GetTexture(args.SrcTextureProps);
                            if (mat.HasProperty(args.DesTextureProps))
                            {
                                mat.SetTexture(args.DesTextureProps, tex ?? Texture2D.whiteTexture);
                            }
                        }
                        else
                        {
                            if (mat.HasProperty(args.DesTextureProps))
                            {
                                mat.SetTexture(args.DesTextureProps, Texture2D.whiteTexture);
                            }
                        }
                    }
                    catch { }
                    renderers[i].SetPropertyBlock(mat, j);
                    sharedMaterials[j] = baseMaterial;
                }
                renderers[i].sharedMaterials = sharedMaterials;
            }

            return g;
        }

        private static void Clear(RenderTexture rt, Color backgroundColor)
        {
            RenderTexture.active = rt;
            GL.Clear(true, true, backgroundColor);
            RenderTexture.active = null;
        }

        [ExcludeFromDoc]
        public static ushort[] Triangulate(Vector2[] vertices)
        {
            List<ushort> tris = new List<ushort>();
            for (ushort x = 0; x < vertices.Length; ++x)
            {
                for (ushort y = 0; y < vertices.Length; ++y)
                {
                    for (ushort z = 0; z < vertices.Length; ++z)
                    {
                        Vector2 v0 = vertices[x];
                        Vector2 v1 = vertices[y];
                        Vector2 v2 = vertices[z];

                        Vector3 cross = Vector3.Cross(v1 - v0, v2 - v0);
                        if (cross.z < 0 && !IsTriangleAdded(tris, new ushort[] { x, y, z }))
                        {
                            tris.Add(x);
                            tris.Add(y);
                            tris.Add(z);
                        }
                    }
                }
            }

            List<ushort> result = new List<ushort>();
            int trisCount = tris.Count / 3;
            for (ushort i = 0; i < trisCount; ++i)
            {
                ushort t0 = tris[i * 3 + 0];
                ushort t1 = tris[i * 3 + 1];
                ushort t2 = tris[i * 3 + 2];

                bool isValidTriangle = true;
                for (ushort vIndex = 0; vIndex < vertices.Length; ++vIndex)
                {
                    if (vIndex == t0 || vIndex == t1 || vIndex == t2)
                        continue;
                    if (GUtilities.IsPointInCircumcircle(vertices[t0], vertices[t1], vertices[t2], vertices[vIndex]))
                    {
                        isValidTriangle = false;
                        break;
                    }
                }

                if (isValidTriangle)
                {
                    result.Add(t0);
                    result.Add(t1);
                    result.Add(t2);
                }
            }

            return result.ToArray();
        }

        private static bool IsTriangleAdded(List<ushort> tris, ushort[] newTris)
        {
            int trisCount = tris.Count / 3;
            for (ushort i = 0; i < trisCount; ++i)
            {
                ushort i0 = tris[i * 3 + 0];
                ushort i1 = tris[i * 3 + 1];
                ushort i2 = tris[i * 3 + 2];
                if (GUtilities.AreSetEqual(new ushort[] { i0, i1, i2 }, newTris))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Fit the camera view to the target prefab.
        /// </summary>
        /// <param name="args">Original billboard creation arguments.</param>
        /// <returns>The modified arguments where camera view was fit.</returns>
        public static GBillboardCreatorArgs FitCameraToTarget(GBillboardCreatorArgs args)
        {
            if (args.Target == null)
                return args;
            Renderer[] renderers = args.Target.GetComponentsInChildren<Renderer>();

            if (renderers.Length > 0)
            {
                Bounds b = new Bounds();
                b.SetMinMax(
                    renderers[0].localBounds.min,
                    renderers[0].localBounds.max);
                for (int i = 1; i < renderers.Length; ++i)
                {
                    Bounds bi = renderers[i].localBounds;
                    b.Encapsulate(bi.min);
                    b.Encapsulate(bi.max);
                }

                Vector3 center = Vector3.zero;
                float r0 = Vector3.Distance(center, new Vector3(b.min.x, 0, b.min.z));
                float r1 = Vector3.Distance(center, new Vector3(b.min.x, 0, b.max.z));
                float r2 = Vector3.Distance(center, new Vector3(b.max.x, 0, b.max.z));
                float r3 = Vector3.Distance(center, new Vector3(b.max.x, 0, b.min.z));
                float spinRadius = Mathf.Max(Mathf.Max(r0, r1), Mathf.Max(r2, r3));

                args.CameraSize = Mathf.Max(spinRadius, b.size.y * 0.5f);
                args.CameraOffset = new Vector3(0, -args.CameraSize, spinRadius * 2);

                args.Bottom = b.min.y;
                args.Width = args.CameraSize * 2;
                args.Height = args.CameraSize * 2;
            }
            return args;
        }

        /// <summary>
        /// Create a new billboard asset.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static BillboardAsset CreateBillboardAsset(GBillboardCreatorArgs args)
        {
            BillboardAsset billboard = new BillboardAsset();
            billboard.SetVertices(args.Vertices);
            billboard.SetIndices(Triangulate(args.Vertices));
            billboard.width = args.Width;
            billboard.height = args.Height;
            billboard.bottom = args.Bottom;

            Vector4[] texcoords = new Vector4[args.Row * args.Column];
            Vector2 imageSize = new Vector2(1f / args.Column, 1f / args.Row);
            Vector2 imageTopLeft = new Vector2(0, 0);

            for (int y = 0; y < args.Row; ++y)
            {
                for (int x = 0; x < args.Column; ++x)
                {
                    imageTopLeft = new Vector2(x * imageSize.x, y * imageSize.y);
                    texcoords[GUtilities.To1DIndex(x, y, args.Column)] = new Vector4(imageTopLeft.x, imageTopLeft.y, imageSize.x, imageSize.y);
                }
            }
            billboard.SetImageTexCoords(texcoords);
            billboard.name = args.Target.name + "_Billboard";
            return billboard;
        }

        /// <summary>
        /// Render a color atlas to a texture 2D
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static Texture2D RenderAtlas(GBillboardCreatorArgs args)
        {
            args.Mode = GBillboardRenderMode.Atlas;
            RenderTexture rt = null;
            PrepareRenderTexture(ref rt, args);
            RenderPreviewAtlas(rt, args);
            Texture2D atlas = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, true, false);
            GCommon.CopyFromRT(atlas, rt);
            rt.Release();
            GUtilities.DestroyObject(rt);
            atlas.name = args.Target.name + "_Atlas";
            return atlas;
        }

        private static void PostProcessAtlas(RenderTexture atlas, GBillboardCreatorArgs args)
        {
            Material mat = args.AtlasPostProcessMaterial;
            if (mat == null)
                return;

            RenderTexture tempRT = RenderTexture.GetTemporary(atlas.descriptor);

            for (int i = 0; i < args.CellSize / 4; ++i)
            {
                GCommon.DrawTexture(tempRT, atlas, GCommon.UnitRect, mat, 0);
                GCommon.DrawTexture(atlas, tempRT, GCommon.UnitRect, mat, 0);
            }

            RenderTexture.ReleaseTemporary(tempRT);
        }

        /// <summary>
        /// Render a normal map atlas to a texture 2D
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static Texture2D RenderNormal(GBillboardCreatorArgs args)
        {
            args.Mode = GBillboardRenderMode.Normal;
            RenderTexture rt = null;
            PrepareRenderTexture(ref rt, args);
            RenderPreviewNormal(rt, args);
            Texture2D atlas = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, true, true);
            GCommon.CopyFromRT(atlas, rt);
            rt.Release();
            GUtilities.DestroyObject(rt);
            atlas.name = args.Target.name + "_Normal";
            return atlas;
        }
    }
}
#endif
