#if GRIFFIN
using UnityEngine;
using System.Collections.Generic;
using Type = System.Type;
using Rand = System.Random;
#if UNITY_EDITOR
using Pinwheel.Griffin.BackupTool;
#endif

namespace Pinwheel.Griffin.PaintTool
{
    /// <summary>
    /// A paint tool component that let you paint on terrain textures. This works both in the editor and runtime.
    /// The painter UI is provided for editor only. At runtime you have to build your own UI, since each game will display its painter differently.
    /// A basic UI should have buttons for selecting paint mode, opacity slider, color selector, texture selector, etc.
    /// You don't have to display everything, just the settings that matter.
    /// </summary>
    [System.Serializable]
    [ExecuteInEditMode]
    public class GTerrainTexturePainter : MonoBehaviour
    {
        /// <summary>
        /// A power constant to tone down the intensity of geometry painters.
        /// </summary>
        public const float GEOMETRY_OPACITY_EXPONENT = 3;

        protected static readonly List<string> BUILTIN_PAINTER_NAME = new List<string>(new string[]
        {
            "GElevationPainter",
            "GHeightSamplingPainter",
            "GTerracePainter",
            "GRemapPainter",
            "GNoisePainter",
            "GSubDivPainter",
            "GVisibilityPainter",
            "GAlbedoPainter",
            "GMetallicPainter",
            "GSmoothnessPainter",
            "GSplatPainter",
            "GMaskPainter"
        });

        protected static List<Type> customPainterTypes;
        [ExcludeFromDoc]
        protected static List<Type> CustomPainterTypes
        {
            get
            {
                if (customPainterTypes == null)
                    customPainterTypes = new List<Type>();
                return customPainterTypes;
            }
            set
            {
                customPainterTypes = value;
            }
        }

        [ExcludeFromDoc]
        public static string TexturePainterInterfaceName
        {
            get
            {
                return typeof(IGTexturePainter).Name;
            }
        }

        [ExcludeFromDoc]
        static GTerrainTexturePainter()
        {
            RefreshCustomPainterTypes();
        }

        /// <summary>
        /// Iterate all runtime types and refresh the list of user create painter types (not the builtin ones such as Elevation Painter)
        /// This function was call at startup, you can call it at anytime but beware that it's slow.<br/>
        /// To create a custom painter, see <see cref="IGTexturePainter"/>
        /// </summary>
        public static void RefreshCustomPainterTypes()
        {
            List<Type> loadedTypes = GCommon.GetAllLoadedTypes();
            CustomPainterTypes = loadedTypes.FindAll(
                t => t.GetInterface(TexturePainterInterfaceName) != null &&
                !t.IsAbstract &&
                !BUILTIN_PAINTER_NAME.Contains(t.Name));
        }

        /// <summary>
        /// A list of custom painter types that created by user, by implementing IGTexturePainter interface.
        /// This list doesn't contain builtin painter types such as Elevation Painter.<br/> 
        /// To create a custom painter, see <see cref="IGTexturePainter"/>
        /// </summary>
        /// <returns>The list of custom painter types.</returns>
        public static List<Type> GetCustomPainterTypes()
        {
            return CustomPainterTypes;
        }

        [SerializeField]
        protected int groupId;
        /// <summary>
        /// The terrain group this painter component will work on. Use -1 to paint on all terrains.
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
        protected GTexturePaintingMode mode;
        /// <summary>
        /// The paint mode. 
        /// If you want to paint with a custom painter, set this to Custom and set the CustomPainterIndex value.
        /// </summary>
        public GTexturePaintingMode Mode
        {
            get
            {
                return mode;
            }
            set
            {
                mode = value;
            }
        }

        [SerializeField]
        protected int customPainterIndex;
        /// <summary>
        /// Index of the custom painter type in the CustomPainterTypes list.
        /// To paint with a custom painter, set Mode=<see cref="GTexturePaintingMode.Custom"/> first, then set this index value.
        /// </summary>
        public int CustomPainterIndex
        {
            get
            {
                return customPainterIndex;
            }
            set
            {
                customPainterIndex = value;
            }
        }

        [SerializeField]
        protected string customPainterArgs;
        /// <summary>
        /// Additional argument packed in a string (use format such as hex color, json, etc.) that will be pass to custom painter on painting, your custom painter should parse this string to get its arguments.
        /// </summary>
        public string CustomPainterArgs
        {
            get
            {
                return customPainterArgs;
            }
            set
            {
                customPainterArgs = value;
            }
        }

        [SerializeField]
        protected bool enableTerrainMask = true;
        /// <summary>
        /// Enable this to use the terrain mask texture (R) to lock a region from being edited.
        /// </summary>
        public bool EnableTerrainMask
        {
            get
            {
                return enableTerrainMask;
            }
            set
            {
                enableTerrainMask = value;
            }
        }

        [SerializeField]
        protected bool forceUpdateGeometry;
        /// <summary>
        /// Update terrain geometry no matter what painter is being used.
        /// Color/texture painters usually don't update geometry because they don't paint on the height map.
        /// Some features such as Albedo To Vertex Color need to regenerate surface meshes when albedo map changed, that case you need to enable this option.
        /// </summary>
        public bool ForceUpdateGeometry
        {
            get
            {
                return forceUpdateGeometry;
            }
            set
            {
                forceUpdateGeometry = value;
            }
        }

        protected GElevationPainter elevationPainter = new GElevationPainter();
        protected GHeightSamplingPainter heightSamplingPainter = new GHeightSamplingPainter();
        protected GTerracePainter terracePainter = new GTerracePainter();
        protected GRemapPainter remapPainter = new GRemapPainter();
        protected GNoisePainter noisePainter = new GNoisePainter();
        protected GSubDivPainter subdivPainter = new GSubDivPainter();
        protected GVisibilityPainter visibilityPainter = new GVisibilityPainter();
        protected GAlbedoPainter albedoPainter = new GAlbedoPainter();
        protected GMetallicPainter metallicPainter = new GMetallicPainter();
        protected GSmoothnessPainter smoothnessPainter = new GSmoothnessPainter();
        protected GSplatPainter splatPainter = new GSplatPainter();
        protected GMaskPainter maskPainter = new GMaskPainter();

        /// <summary>
        /// Get the active painter object that will be used.<br/>
        /// You can do type check and cast with 'is' and 'as' operator.<br/>
        /// Check if the painter support live preview with '<c>painter is IGTexturePainterWithLivePreview</c>'<br/>
        /// Check if the painter has custom param (provided outside of this component, such as in asset ScriptableObject, in files, etc.) with '<c>painter is IGTexturePainterWithCustomParams</c>'<br/>
        /// Check if the painter support conditional painting (height, slope, noise rules, etc.) with '<c>painter is IConditionalPainter</c>'<br/>
        /// </summary>
        /// 
        /// <remarks>
        /// For custom painter: Each call to this property will create a new object. This doesn't work so well with serialization, so if you need some persistent arguments between frame, store them somewhere else (static, scriptable objects, files, etc.)<br/>
        /// Implement the IGTexturePainterWithCustomParams to draw your settings in the painter's inspector.
        /// </remarks>
        public IGTexturePainter ActivePainter
        {
            get
            {
                if (Mode == GTexturePaintingMode.Elevation)
                {
                    return elevationPainter;
                }
                else if (Mode == GTexturePaintingMode.HeightSampling)
                {
                    return heightSamplingPainter;
                }
                else if (Mode == GTexturePaintingMode.Terrace)
                {
                    return terracePainter;
                }
                else if (Mode == GTexturePaintingMode.Remap)
                {
                    return remapPainter;
                }
                else if (Mode == GTexturePaintingMode.Noise)
                {
                    return noisePainter;
                }
                else if (Mode == GTexturePaintingMode.SubDivision)
                {
                    return subdivPainter;
                }
                else if (Mode == GTexturePaintingMode.Visibility)
                {
                    return visibilityPainter;
                }
                else if (Mode == GTexturePaintingMode.Albedo)
                {
                    return albedoPainter;
                }
                else if (Mode == GTexturePaintingMode.Metallic)
                {
                    return metallicPainter;
                }
                else if (Mode == GTexturePaintingMode.Smoothness)
                {
                    return smoothnessPainter;
                }
                else if (Mode == GTexturePaintingMode.Splat)
                {
                    return splatPainter;
                }
                else if (Mode == GTexturePaintingMode.Mask)
                {
                    return maskPainter;
                }
                else if (mode == GTexturePaintingMode.Custom)
                {
                    if (CustomPainterIndex >= 0 && CustomPainterIndex < CustomPainterTypes.Count)
                        return System.Activator.CreateInstance(CustomPainterTypes[CustomPainterIndex]) as IGTexturePainter;
                }
                return null;
            }
        }

        [SerializeField]
        protected float brushRadius;
        /// <summary>
        /// Radius of the brush in world space.
        /// In range (0, +inf]
        /// </summary>
        public float BrushRadius
        {
            get
            {
                return brushRadius;
            }
            set
            {
                brushRadius = Mathf.Max(0.01f, value);
            }
        }

        [SerializeField]
        protected float brushRadiusJitter;
        /// <summary>
        /// Add variation to brush radius by making it slightly bigger or smaller, randomly.
        /// In range [0,1]
        /// </summary>
        public float BrushRadiusJitter
        {
            get
            {
                return brushRadiusJitter;
            }
            set
            {
                brushRadiusJitter = Mathf.Clamp01(value);
            }
        }

        [SerializeField]
        protected float brushRotation;
        /// <summary>
        /// Rotation of brush in world space. 
        /// In degrees unit [-360,360].
        /// </summary>
        public float BrushRotation
        {
            get
            {
                return brushRotation;
            }
            set
            {
                brushRotation = Mathf.Clamp(value, -360, 360);
            }
        }

        [SerializeField]
        protected float brushRotationJitter;
        /// <summary>
        /// Adding variation to brush rotation with slightly different angle.
        /// In range [0,1]
        /// </summary>
        public float BrushRotationJitter
        {
            get
            {
                return brushRotationJitter;
            }
            set
            {
                brushRotationJitter = Mathf.Clamp01(value);
            }
        }

        [SerializeField]
        protected float brushOpacity;
        /// <summary>
        /// Opacity of the brush. Note that painter strength also affected by TargetStrength, which is an additional multiplier.
        /// In range [0,1]
        /// </summary>
        public float BrushOpacity
        {
            get
            {
                return brushOpacity;
            }
            set
            {
                brushOpacity = Mathf.Clamp01(value);
            }
        }

        [SerializeField]
        protected float brushOpacityJitter;
        /// <summary>
        /// Adding variantion to brush opacity with slightly higher or lower value.
        /// In range [0,1]
        /// </summary>
        public float BrushOpacityJitter
        {
            get
            {
                return brushOpacityJitter;
            }
            set
            {
                brushOpacityJitter = Mathf.Clamp01(value);
            }
        }

        [SerializeField]
        protected float brushTargetStrength = 1;
        /// <summary>
        /// An additional multiplier to brush Opacity, to tone down the effect intensity of painters.
        /// </summary>
        public float BrushTargetStrength
        {
            get
            {
                return brushTargetStrength;
            }
            set
            {
                brushTargetStrength = Mathf.Clamp01(value);
            }
        }

        [SerializeField]
        protected float brushScatter;
        /// <summary>
        /// Randomly offset the brush position around mouse position. This value defines the relative offset from 0 to Radius.
        /// </summary>
        public float BrushScatter
        {
            get
            {
                return brushScatter;
            }
            set
            {
                brushScatter = Mathf.Clamp01(value);
            }
        }

        [SerializeField]
        protected float brushScatterJitter;
        /// <summary>
        /// Similar to BrushScatter, this value add variantion to relative offset to make the brush move more or less away from mouse position.
        /// </summary>
        public float BrushScatterJitter
        {
            get
            {
                return brushScatterJitter;
            }
            set
            {
                brushScatterJitter = Mathf.Clamp01(value);
            }
        }

        [SerializeField]
        protected Color brushColor;
        /// <summary>
        /// The color used for painting.<br/>
        /// Albedo: RGBA<br/>
        /// Metallic: R<br/>
        /// Smoothness: A<br/>
        /// </summary>
        public Color BrushColor
        {
            get
            {
                return brushColor;
            }
            set
            {
                brushColor = value;
            }
        }

        [SerializeField]
        protected List<Texture2D> brushMasks;
        /// <summary>
        /// A textures collection to define the brush shape.
        /// You can add to this list directly, or save your textures in a runtime Resources/PolarisBrushes/ folder, they will be loaded on startup for both editor and runtime use.
        /// Builtin brush masks won't be packed in runtime. 
        /// </summary>
        public List<Texture2D> BrushMasks
        {
            get
            {
                if (brushMasks == null)
                    brushMasks = new List<Texture2D>();
                return brushMasks;
            }
            set
            {
                brushMasks = value;
            }
        }

        [SerializeField]
        protected int selectedBrushMaskIndex;
        /// <summary>
        /// Index of the selected brush mask.
        /// </summary>
        public int SelectedBrushMaskIndex
        {
            get
            {
                return selectedBrushMaskIndex;
            }
            set
            {
                if (BrushMasks.Count > 0)
                    selectedBrushMaskIndex = Mathf.Clamp(value, 0, BrushMasks.Count);
                else
                    selectedBrushMaskIndex = -1;
            }
        }

        /// <summary>
        /// Index of the first selected splat texture.
        /// Note that you don't add splat prototype to the painter. You need to create a Splat Prototype Group asset and assign to your terrains instead.
        /// </summary>
        public int SelectedSplatIndex
        {
            get
            {
                if (SelectedSplatIndices.Count == 0)
                {
                    SelectedSplatIndices.Add(0);
                }
                return SelectedSplatIndices[0];
            }
            set
            {
                SelectedSplatIndices.Clear();
                SelectedSplatIndices.Add(value);
            }
        }

        [SerializeField]
        protected List<int> selectedSplatIndices;
        /// <summary>
        /// Indices of the selected splat textures, splat painter will pick textures randomly using this indices list.
        /// Note that you don't add splat prototype to the painter. You need to create a Splat Prototype Group asset and assign to your terrains instead.
        /// </summary>
        public List<int> SelectedSplatIndices
        {
            get
            {
                if (selectedSplatIndices == null)
                {
                    selectedSplatIndices = new List<int>();
                }
                return selectedSplatIndices;
            }
            set
            {
                selectedSplatIndices = value;
            }
        }

        [SerializeField]
        protected Vector3 samplePoint;
        /// <summary>
        /// The world space sample point used for Height Sampling mode.
        /// </summary>
        public Vector3 SamplePoint
        {
            get
            {
                return samplePoint;
            }
            set
            {
                samplePoint = value;
            }
        }

        [SerializeField]
        protected GConditionalPaintingConfigs conditionalPaintingConfigs;
        /// <summary>
        /// Settings for conditional painting (paint with height, slope and noise rule).
        /// To add support for conditional painting in custom painter, implement the IConditionalPainter.
        /// </summary>
        public GConditionalPaintingConfigs ConditionalPaintingConfigs
        {
            get
            {
                return conditionalPaintingConfigs;
            }
            set
            {
                conditionalPaintingConfigs = value;
            }
        }

        internal static Dictionary<string, RenderTexture> internal_RenderTextures;

        [ExcludeFromDoc]
        protected void OnEnable()
        {
            ReloadBrushMasks();

            if (conditionalPaintingConfigs != null)
            {
                conditionalPaintingConfigs.UpdateCurveTextures();
            }
        }

        [ExcludeFromDoc]
        protected void OnDisable()
        {
            CleanUp();
        }
        [ExcludeFromDoc]

        protected void Reset()
        {
            GroupId = 0;
            Mode = GTexturePaintingMode.Elevation;
            BrushRadius = 50;
            BrushRadiusJitter = 0;
            BrushOpacity = 0.5f;
            BrushOpacityJitter = 0;
            BrushTargetStrength = 1;
            BrushRotation = 0;
            BrushRotationJitter = 0;
            BrushColor = Color.white;

            if (conditionalPaintingConfigs != null)
            {
                conditionalPaintingConfigs.CleanUp();
            }
            conditionalPaintingConfigs = new GConditionalPaintingConfigs();
            conditionalPaintingConfigs.UpdateCurveTextures();
        }

        /// <summary>
        /// Load brush mask textures from all Resources/PolarisBrushes folders.
        /// Builtin brush masks won't be loaded at runtime.
        /// </summary>
        public void ReloadBrushMasks()
        {
            BrushMasks = new List<Texture2D>(Resources.LoadAll<Texture2D>(GCommon.BRUSH_MASK_RESOURCES_PATH));
        }

        /// <summary>
        /// Perform painting on the terrains.
        /// This function also record terrain backup in the editor.
        /// </summary>
        /// <param name="args">Struct containing all neccessary arguments for painting. Most arguments were passed from this component, while some come from the editor or your game mechanic.</param>
        public void Paint(GTexturePainterArgs args)
        {
            IGTexturePainter p = ActivePainter;
            if (p == null)
                return;

            FillArgs(ref args);

            List<GStylizedTerrain> overlappedTerrain = GUtilities.ExtractTerrainsFromOverlapTest(GPaintToolUtilities.OverlapTest(GroupId, args.HitPoint, args.Radius, args.Rotation));
#if UNITY_EDITOR
            if (args.MouseEventType == GPainterMouseEventType.Down ||
                args.MouseEventType == GPainterMouseEventType.Drag)
            {
                Editor_CreateInitialHistoryEntry(args, overlappedTerrain);
            }
#endif
            foreach (GStylizedTerrain t in overlappedTerrain)
            {
                p.BeginPainting(t, args);
            }

            foreach (GStylizedTerrain t in overlappedTerrain)
            {
                p.EndPainting(t, args);
            }

#if UNITY_EDITOR
            EditedTerrains.UnionWith(overlappedTerrain);
            if (args.MouseEventType == GPainterMouseEventType.Up)
            {
                Editor_CreateHistory(args);
                currentInitialBackupName = null;
                InitialRecordedTerrains.Clear();
                EditedTerrains.Clear();
            }
#endif
        }

#if UNITY_EDITOR
        protected HashSet<GStylizedTerrain> initialRecordedTerrains;
        [ExcludeFromDoc]
        protected HashSet<GStylizedTerrain> InitialRecordedTerrains
        {
            get
            {
                if (initialRecordedTerrains == null)
                {
                    initialRecordedTerrains = new HashSet<GStylizedTerrain>();
                }
                return initialRecordedTerrains;
            }
        }

        protected HashSet<GStylizedTerrain> editedTerrains;
        [ExcludeFromDoc]
        protected HashSet<GStylizedTerrain> EditedTerrains
        {
            get
            {
                if (editedTerrains == null)
                {
                    editedTerrains = new HashSet<GStylizedTerrain>();
                }
                return editedTerrains;
            }
        }

        protected string currentInitialBackupName;
        [ExcludeFromDoc]
        protected void Editor_CreateInitialHistoryEntry(GTexturePainterArgs args, List<GStylizedTerrain> overlappedTerrains)
        {
            if (!GEditorSettings.Instance.paintTools.enableHistory)
                return;
            if (overlappedTerrains.Count == 0)
                return;

            List<GTerrainResourceFlag> flags = new List<GTerrainResourceFlag>();
            flags.AddRange(ActivePainter.GetResourceFlagForHistory(args));

            if (InitialRecordedTerrains.Count == 0)
            {
                currentInitialBackupName = GBackup.TryCreateInitialBackup(ActivePainter.HistoryPrefix, overlappedTerrains[0], flags, false);
                if (!string.IsNullOrEmpty(currentInitialBackupName))
                {
                    InitialRecordedTerrains.Add(overlappedTerrains[0]);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(currentInitialBackupName))
                {
                    for (int i = 0; i < overlappedTerrains.Count; ++i)
                    {
                        if (InitialRecordedTerrains.Contains(overlappedTerrains[i]))
                            continue;
                        GBackup.BackupTerrain(overlappedTerrains[i], currentInitialBackupName, flags);
                        InitialRecordedTerrains.Add(overlappedTerrains[i]);
                    }
                }
            }
        }

        [ExcludeFromDoc]
        protected void Editor_CreateHistory(GTexturePainterArgs args)
        {
            if (!GEditorSettings.Instance.paintTools.enableHistory)
                return;
            if (EditedTerrains.Count == 0)
                return;

            List<GTerrainResourceFlag> flags = new List<GTerrainResourceFlag>();
            flags.AddRange(ActivePainter.GetResourceFlagForHistory(args));

            List<GStylizedTerrain> terrainList = new List<GStylizedTerrain>(EditedTerrains);
            string backupName = GBackup.TryCreateBackup(ActivePainter.HistoryPrefix, terrainList[0], flags, false);
            if (!string.IsNullOrEmpty(backupName))
            {
                for (int i = 1; i < terrainList.Count; ++i)
                {
                    GBackup.BackupTerrain(terrainList[i], backupName, flags);
                }
            }
        }
#endif

        [ExcludeFromDoc]
        internal static RenderTexture Internal_GetRenderTexture(GStylizedTerrain t, int resolution, int id = 0)
        {
            if (internal_RenderTextures == null)
            {
                internal_RenderTextures = new Dictionary<string, RenderTexture>();
            }

            string key = string.Format("{0}_{1}", t != null ? t.GetInstanceID() : 0, id);
            if (!internal_RenderTextures.ContainsKey(key) ||
                internal_RenderTextures[key] == null)
            {
                RenderTexture rt = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                internal_RenderTextures[key] = rt;
            }
            else if (internal_RenderTextures[key].width != resolution ||
                internal_RenderTextures[key].height != resolution ||
                internal_RenderTextures[key].format != GGeometry.HeightMapRTFormat)
            {
                internal_RenderTextures[key].Release();
                Object.DestroyImmediate(internal_RenderTextures[key]);
                RenderTexture rt = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                internal_RenderTextures[key] = rt;
            }

            internal_RenderTextures[key].wrapMode = TextureWrapMode.Clamp;

            return internal_RenderTextures[key];
        }

        [ExcludeFromDoc]
        public static void Internal_ReleaseRenderTextures()
        {
            if (internal_RenderTextures != null)
            {
                foreach (string k in internal_RenderTextures.Keys)
                {
                    RenderTexture rt = internal_RenderTextures[k];
                    if (rt == null)
                        continue;
                    rt.Release();
                    Object.DestroyImmediate(rt);
                }
            }
        }

        [ExcludeFromDoc]
        protected Rand GetRandomGenerator()
        {
            return new Rand(System.DateTime.Now.Millisecond);
        }

        [ExcludeFromDoc]
        protected void ProcessBrushDynamic(ref GTexturePainterArgs args)
        {
            Rand rand = GetRandomGenerator();
            args.Radius -= BrushRadius * BrushRadiusJitter * (float)rand.NextDouble();
            args.Rotation += Mathf.Sign((float)rand.NextDouble() - 0.5f) * BrushRotation * BrushRotationJitter * (float)rand.NextDouble();
            args.Opacity -= BrushOpacity * BrushOpacityJitter * (float)rand.NextDouble();

            Vector3 scatterDir = new Vector3((float)(rand.NextDouble() * 2 - 1), 0, (float)(rand.NextDouble() * 2 - 1)).normalized;
            float scatterLengthMultiplier = BrushScatter - (float)rand.NextDouble() * BrushScatterJitter;
            float scatterLength = args.Radius * scatterLengthMultiplier;

            args.HitPoint += scatterDir * scatterLength;
        }

        /// <summary>
        /// Fill the paint arguments with this component properties. You don't need to call this function before painting because the Pain() function already did it.
        /// This function can be used to make runtime live preview.
        /// </summary>
        /// <param name="args">The paint arguments object</param>
        /// <param name="useBrushDynamic">Set to <see langword="true"/>to add variation to brush strokes.</param>
        public void FillArgs(ref GTexturePainterArgs args, bool useBrushDynamic = true)
        {
            args.Radius = BrushRadius;
            args.Rotation = BrushRotation;
            args.Opacity = BrushOpacity * BrushTargetStrength;
            args.Color = BrushColor;
            if (SelectedSplatIndices.Count > 0)
            {
                args.SplatIndex = SelectedSplatIndices[Random.Range(0, SelectedSplatIndices.Count)];
            }
            else
            {
                args.SplatIndex = -1;
            }
            args.SamplePoint = SamplePoint;
            args.CustomArgs = CustomPainterArgs;
            args.ForceUpdateGeometry = ForceUpdateGeometry;
            args.EnableTerrainMask = EnableTerrainMask;
            if (SelectedBrushMaskIndex >= 0 && SelectedBrushMaskIndex < BrushMasks.Count)
            {
                args.BrushMask = BrushMasks[SelectedBrushMaskIndex];
            }

            if (args.ActionType == GPainterActionType.Alternative &&
                args.MouseEventType == GPainterMouseEventType.Down)
            {
                SamplePoint = args.HitPoint;
                args.SamplePoint = args.HitPoint;
            }

            if (useBrushDynamic)
            {
                ProcessBrushDynamic(ref args);
            }

            Vector3[] corners = GCommon.GetBrushQuadCorners(args.HitPoint, args.Radius, args.Rotation);
            args.WorldPointCorners = corners;

            args.ConditionalPaintingConfigs = this.ConditionalPaintingConfigs;
        }

        [ExcludeFromDoc]
        public void CleanUp()
        {
            Internal_ReleaseRenderTextures();
            if (conditionalPaintingConfigs != null)
            {
                conditionalPaintingConfigs.CleanUp();
            }
        }
    }
}
#endif
