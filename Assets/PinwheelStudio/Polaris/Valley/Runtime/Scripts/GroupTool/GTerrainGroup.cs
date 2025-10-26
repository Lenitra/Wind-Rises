#if GRIFFIN
using System.Collections.Generic;
using UnityEngine;

namespace Pinwheel.Griffin.GroupTool
{
    /// <summary>
    /// A component for modifying the settings of many terrains at once.
    /// </summary>
    [System.Serializable]
    [ExecuteInEditMode]
    public class GTerrainGroup : MonoBehaviour
    {
        [SerializeField]
        private int groupId;
        /// <summary>
        /// The terrain group that will be affected by this component. Use -1 to affect all terrain group.
        /// </summary>
        public int GroupId
        {
            get
            {
                return groupId;
            }
            set
            {
                groupId = value;
            }
        }

        [SerializeField]
        private bool deferredUpdate;
        /// <summary>
        /// Editor only. Turn this on will delay the modification of terrains, you can change value for many properties and officially override them by clicking on the Update button.
        /// At runtime, this value takes no effect. Terrains are overriden when calling OverrideXXX() function.
        /// </summary>
        public bool DeferredUpdate
        {
            get
            {
                return deferredUpdate;
            }
            set
            {
                deferredUpdate = value;
            }
        }

        [SerializeField]
        private GGeometryOverride geometryOverride;
        /// <summary>
        /// Contains override states and values for Geometry settings.
        /// </summary>
        public GGeometryOverride GeometryOverride
        {
            get
            {
                return geometryOverride;
            }
            set
            {
                geometryOverride = value;
            }
        }

        [SerializeField]
        private GShadingOverride shadingOverride;
        /// <summary>
        /// Contain override states and values for Shading settings.
        /// </summary>
        public GShadingOverride ShadingOverride
        {
            get
            {
                return shadingOverride;
            }
            set
            {
                shadingOverride = value;
            }
        }

        [SerializeField]
        private GRenderingOverride renderingOverride;
        /// <summary>
        /// Contain override states and values for Rendering settings.
        /// </summary>
        public GRenderingOverride RenderingOverride
        {
            get
            {
                return renderingOverride;
            }
            set
            {
                renderingOverride = value;
            }
        }

        [SerializeField]
        private GFoliageOverride foliageOverride;
        /// <summary>
        /// Contain override states and values for Foliage settings.
        /// </summary>
        public GFoliageOverride FoliageOverride
        {
            get
            {
                return foliageOverride;
            }
            set
            {
                foliageOverride = value;
            }
        }

        [SerializeField]
        private GMaskOverride maskOverride;
        /// <summary>
        /// Contain override states and values for Mask settings.
        /// </summary>
        public GMaskOverride MaskOverride
        {
            get
            {
                return maskOverride;
            }
            set
            {
                maskOverride = value;
            }
        }

        private void Reset()
        {
            geometryOverride.Reset();
            shadingOverride.Reset();
            renderingOverride.Reset();
            foliageOverride.Reset();
            maskOverride.Reset();
        }

        /// <summary>
        /// Reset Geometry override states and values
        /// </summary>
        public void ResetGeometry()
        {
            geometryOverride.Reset();
        }

        /// <summary>
        /// Reset Shading override states and values
        /// </summary>
        public void ResetShading()
        {
            shadingOverride.Reset();
        }

        /// <summary>
        /// Reset Rendering override states and values
        /// </summary>
        public void ResetRendering()
        {
            renderingOverride.Reset();
        }

        /// <summary>
        /// Reset Foliage override states and values
        /// </summary>
        public void ResetFoliage()
        {
            foliageOverride.Reset();
        }

        /// <summary>
        /// Reset Mask override states and values
        /// </summary>
        public void ResetMask()
        {
            maskOverride.Reset();
        }

        /// <summary>
        /// Override Geometry settings for target terrain group and regenerate surface mesh
        /// </summary>
        public void OverrideGeometry()
        {
            IEnumerator<GStylizedTerrain> terrains = GStylizedTerrain.ActiveTerrains.GetEnumerator();
            while (terrains.MoveNext())
            {
                if (terrains.Current.TerrainData != null &&
                    (terrains.Current.GroupId == GroupId || GroupId < 0))
                {
                    GeometryOverride.Override(terrains.Current.TerrainData.Geometry);
                    terrains.Current.TerrainData.Geometry.SetRegionDirty(new Rect(0, 0, 1, 1));
                    terrains.Current.TerrainData.SetDirty(GTerrainData.DirtyFlags.GeometryTimeSliced);
                }
            }
            GStylizedTerrain.MatchEdges(GroupId);
        }

        /// <summary>
        /// Override Shading settings for target terrain group and refresh terrain material.
        /// </summary>
        public void OverrideShading()
        {
            IEnumerator<GStylizedTerrain> terrains = GStylizedTerrain.ActiveTerrains.GetEnumerator();
            while (terrains.MoveNext())
            {
                if (terrains.Current.TerrainData != null &&
                    (terrains.Current.GroupId == GroupId || GroupId < 0))
                {
                    ShadingOverride.Override(terrains.Current.TerrainData.Shading);
                    terrains.Current.TerrainData.SetDirty(GTerrainData.DirtyFlags.Shading);
                }
            }
        }

        /// <summary>
        /// Override Rendering settings for target terrain group.
        /// </summary>
        public void OverrideRendering()
        {
            IEnumerator<GStylizedTerrain> terrains = GStylizedTerrain.ActiveTerrains.GetEnumerator();
            while (terrains.MoveNext())
            {
                if (terrains.Current.TerrainData != null &&
                    (terrains.Current.GroupId == GroupId || GroupId < 0))
                {
                    RenderingOverride.Override(terrains.Current.TerrainData.Rendering);
                    terrains.Current.TerrainData.SetDirty(GTerrainData.DirtyFlags.Rendering);
                }
            }
        }

        /// <summary>
        /// Override Foliage settings for target terrain group and refresh foliage renderer.
        /// </summary>
        public void OverrideFoliage()
        {
            IEnumerator<GStylizedTerrain> terrains = GStylizedTerrain.ActiveTerrains.GetEnumerator();
            while (terrains.MoveNext())
            {
                if (terrains.Current.TerrainData != null &&
                    (terrains.Current.GroupId == GroupId || GroupId < 0))
                {
                    FoliageOverride.Override(terrains.Current.TerrainData.Foliage);
                    terrains.Current.TerrainData.SetDirty(GTerrainData.DirtyFlags.Foliage);
                }
            }
        }

        /// <summary>
        /// Override Mask settings for target terrain group.
        /// </summary>
        public void OverrideMask()
        {
            IEnumerator<GStylizedTerrain> terrains = GStylizedTerrain.ActiveTerrains.GetEnumerator();
            while (terrains.MoveNext())
            {
                if (terrains.Current.TerrainData != null &&
                    (terrains.Current.GroupId == GroupId || GroupId < 0))
                {
                    MaskOverride.Override(terrains.Current.TerrainData.Mask);
                    terrains.Current.TerrainData.SetDirty(GTerrainData.DirtyFlags.Mask);
                }
            }
        }

        /// <summary>
        /// Utility function to snap terrains into a grid.
        /// </summary>
        public void ReArrange()
        {
            IEnumerator<GStylizedTerrain> terrains = GStylizedTerrain.ActiveTerrains.GetEnumerator();
            while (terrains.MoveNext())
            {
                if (terrains.Current.TerrainData != null &&
                    (terrains.Current.GroupId == GroupId || GroupId < 0))
                {
                    GStylizedTerrain t = terrains.Current;
                    if (t.TopNeighbor != null)
                    {
                        t.transform.rotation = Quaternion.identity;
                        t.transform.localScale = Vector3.one;
                        t.TopNeighbor.transform.position = t.transform.position + Vector3.forward * t.TerrainData.Geometry.Length;
                        t.TopNeighbor.transform.rotation = Quaternion.identity;
                        t.TopNeighbor.transform.localScale = Vector3.one;
                    }
                    if (t.BottomNeighbor != null)
                    {
                        t.transform.rotation = Quaternion.identity;
                        t.transform.localScale = Vector3.one;
                        t.BottomNeighbor.transform.position = t.transform.position + Vector3.back * t.TerrainData.Geometry.Length;
                        t.BottomNeighbor.transform.rotation = Quaternion.identity;
                        t.BottomNeighbor.transform.localScale = Vector3.one;
                    }
                    if (t.LeftNeighbor != null)
                    {
                        t.transform.rotation = Quaternion.identity;
                        t.transform.localScale = Vector3.one;
                        t.LeftNeighbor.transform.position = t.transform.position + Vector3.left * t.TerrainData.Geometry.Width;
                        t.LeftNeighbor.transform.rotation = Quaternion.identity;
                        t.LeftNeighbor.transform.localScale = Vector3.one;
                    }
                    if (t.RightNeighbor != null)
                    {
                        t.transform.rotation = Quaternion.identity;
                        t.transform.localScale = Vector3.one;
                        t.RightNeighbor.transform.position = t.transform.position + Vector3.right * t.TerrainData.Geometry.Width;
                        t.RightNeighbor.transform.rotation = Quaternion.identity;
                        t.RightNeighbor.transform.localScale = Vector3.one;
                    }
                }
            }
        }
    }
}
#endif
