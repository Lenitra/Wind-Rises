#if GRIFFIN
#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.Rendering;

namespace Pinwheel.Griffin
{
    [ExcludeFromDoc]
    public static class GLivePreviewDrawer
    {
        internal delegate void GeometryLPHandler1(GStylizedTerrain t, Camera cam, Texture newHeightMap, Rect dirtyRect);
        internal static GeometryLPHandler1 DrawGeometryLivePreviewCallback1;
        public static void DrawGeometryLivePreview(GStylizedTerrain t, Camera cam, Texture newHeightMap, Rect dirtyRect)
        {
            DrawGeometryLivePreviewCallback1?.Invoke(t, cam, newHeightMap, dirtyRect);
        }

        internal delegate void GeometryLPHandler2(GStylizedTerrain t, Camera cam, Texture newHeightMap, bool[] chunkCulling);
        internal static GeometryLPHandler2 DrawGeometryLivePreviewCallback2;
        public static void DrawGeometryLivePreview(GStylizedTerrain t, Camera cam, Texture newHeightMap, bool[] chunkCulling)
        {
            DrawGeometryLivePreviewCallback2?.Invoke(t, cam, newHeightMap, chunkCulling);
        }

        internal delegate void SubdivLPHandler(GStylizedTerrain t, Camera cam, Texture newHeightMap, Rect dirtyRect, Texture mask, Matrix4x4 worldPointToMaskMatrix);
        internal static SubdivLPHandler DrawSubdivLivePreviewCallback;
        public static void DrawSubdivLivePreview(GStylizedTerrain t, Camera cam, Texture newHeightMap, Rect dirtyRect, Texture mask, Matrix4x4 worldPointToMaskMatrix)
        {
            DrawSubdivLivePreviewCallback?.Invoke(t, cam, newHeightMap, dirtyRect, mask, worldPointToMaskMatrix);
        }

        internal delegate void VisibilityLPHandler1(GStylizedTerrain t, Camera cam, Texture newHeightMap, Rect dirtyRect, Texture mask, Matrix4x4 worldPointToMaskMatrix);
        internal static VisibilityLPHandler1 DrawVisibilityLivePreviewCallback1;
        public static void DrawVisibilityLivePreview(GStylizedTerrain t, Camera cam, Texture newHeightMap, Rect dirtyRect, Texture mask, Matrix4x4 worldPointToMaskMatrix)
        {
            DrawVisibilityLivePreviewCallback1?.Invoke(t, cam, newHeightMap, dirtyRect, mask, worldPointToMaskMatrix);
        }

        internal delegate void VisibilityLPHandler2(GStylizedTerrain t, Camera cam, Texture newHeightMap, bool[] chunkCulling, Texture mask, Matrix4x4 worldPointToMaskMatrix);
        internal static VisibilityLPHandler2 DrawVisibilityLivePreviewCallback2;
        public static void DrawVisibilityLivePreview(GStylizedTerrain t, Camera cam, Texture newHeightMap, bool[] chunkCulling, Texture mask, Matrix4x4 worldPointToMaskMatrix)
        {
            DrawVisibilityLivePreviewCallback2?.Invoke(t, cam, newHeightMap, chunkCulling, mask, worldPointToMaskMatrix);
        }

        internal delegate void AlbedoLPHandler(GStylizedTerrain t, Camera cam, Texture newAlbedo, Rect dirtyRect);
        internal static AlbedoLPHandler DrawAlbedoLivePreviewCallback;
        public static void DrawAlbedoLivePreview(GStylizedTerrain t, Camera cam, Texture newAlbedo, Rect dirtyRect)
        {
            DrawAlbedoLivePreviewCallback?.Invoke(t, cam, newAlbedo, dirtyRect);
        }

        internal delegate void MetallicSmoothnessLPHandler(GStylizedTerrain t, Camera cam, Texture newMetallicMap, Rect dirtyRect);
        internal static MetallicSmoothnessLPHandler DrawMetallicSmoothnessLivePreviewCallback;
        public static void DrawMetallicSmoothnessLivePreview(GStylizedTerrain t, Camera cam, Texture newMetallicMap, Rect dirtyRect)
        {
            DrawMetallicSmoothnessLivePreviewCallback?.Invoke(t, cam, newMetallicMap, dirtyRect);
        }

        internal delegate void DrawASMLPHandler1(GStylizedTerrain t, Camera cam, Texture newAlbedo, Texture newMetallicMap, Rect dirtyRect);
        internal static DrawASMLPHandler1 DrawAMSLivePreviewCallback1;
        public static void DrawAMSLivePreview(GStylizedTerrain t, Camera cam, Texture newAlbedo, Texture newMetallicMap, Rect dirtyRect)
        {
            DrawAMSLivePreviewCallback1?.Invoke(t, cam, newAlbedo, newMetallicMap, dirtyRect);
        }

        internal delegate void DrawASMLPHandler2(GStylizedTerrain t, Camera cam, Texture newAlbedo, Texture newMetallicMap, bool[] chunkCulling);
        internal static DrawASMLPHandler2 DrawAMSLivePreviewCallback2;
        public static void DrawAMSLivePreview(GStylizedTerrain t, Camera cam, Texture newAlbedo, Texture newMetallicMap, bool[] chunkCulling)
        {
            DrawAMSLivePreviewCallback2?.Invoke(t, cam, newAlbedo, newMetallicMap, chunkCulling);
        }

        internal delegate void DrawSplatLPHandler1(GStylizedTerrain t, Camera cam, Texture[] newControlMaps, Rect dirtyRect);
        internal static DrawSplatLPHandler1 DrawSplatLivePreviewCallback1;
        public static void DrawSplatLivePreview(GStylizedTerrain t, Camera cam, Texture[] newControlMaps, Rect dirtyRect)
        {
            DrawSplatLivePreviewCallback1?.Invoke(t, cam, newControlMaps, dirtyRect);
        }

        internal delegate void DrawSplatLPHandler2(GStylizedTerrain t, Camera cam, Texture[] newControlMaps, bool[] chunkCulling);
        internal static DrawSplatLPHandler2 DrawSplatLivePreviewCallback2;
        public static void DrawSplatLivePreview(GStylizedTerrain t, Camera cam, Texture[] newControlMaps, bool[] chunkCulling)
        {
            DrawSplatLivePreviewCallback2?.Invoke(t, cam, newControlMaps, chunkCulling);
        }

        internal delegate void DrawMasksLPHandler1(GStylizedTerrain t, Camera cam, Texture[] masks, Color[] colors, Rect dirtyRect, int channel = 0);
        internal static DrawMasksLPHandler1 DrawMaskLivePreviewCallback1;
        public static void DrawMasksLivePreview(GStylizedTerrain t, Camera cam, Texture[] masks, Color[] colors, Rect dirtyRect, int channel = 0)
        {
            DrawMaskLivePreviewCallback1?.Invoke(t, cam, masks, colors, dirtyRect, channel);
        }

        internal delegate void DrawMasksLPHandler2(GStylizedTerrain t, Camera cam, Texture[] masks, Color[] colors, bool[] chunkCulling, int channel = 0);
        internal static DrawMasksLPHandler2 DrawMaskLivePreviewCallback2;
        public static void DrawMasksLivePreview(GStylizedTerrain t, Camera cam, Texture[] masks, Color[] colors, bool[] chunkCulling, int channel = 0)
        {
            DrawMaskLivePreviewCallback2?.Invoke(t, cam, masks, colors, chunkCulling, channel);
        }

        //Display a red mask on terrain, available in all editions
        public static void DrawTerrainMask(GStylizedTerrain t, Camera cam)
        {
            if (t.TerrainData == null)
                return;
            GLivePreviewDrawer.DrawMasksLivePreview(
                t, cam,
                new Texture[] { t.TerrainData.Mask.MaskMapOrDefault },
                new Color[] { Color.red },
                GCommon.UnitRect);
        }

        internal delegate void DrawMask4ChannelsLPHandler(GStylizedTerrain t, Camera cam, Texture masks, Rect dirtyRect);
        internal static DrawMask4ChannelsLPHandler DrawMask4ChannelsLivePreviewCallback;
        public static void DrawMask4ChannelsLivePreview(GStylizedTerrain t, Camera cam, Texture masks, Rect dirtyRect)
        {
            DrawMask4ChannelsLivePreviewCallback?.Invoke(t, cam, masks, dirtyRect);
        }
    }
}
#endif
#endif
