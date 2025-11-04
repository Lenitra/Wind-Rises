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
    /// A component that let you paint tree and grass on terrains. This works both in the editor and runtime.
    /// The painter UI is provided for editor only. At runtime you have to build your own UI, since each game will display its painter differently.
    /// A basic UI should have buttons for selecting paint mode, density slider, prototype selector, etc.
    /// You don't have to display everything, just the settings that matter.
    /// </summary>
    [System.Serializable]
    [ExecuteInEditMode]
    public class GFoliagePainter : MonoBehaviour
    {
        protected static readonly List<string> BUILTIN_PAINTER_NAME = new List<string>(new string[]
        {
            "GTreePainter",
            "GTreeScaler",
            "GGrassPainter",
            "GGrassScaler"
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
        public static string FoliagePainterInterfaceName
        {
            get
            {
                return typeof(IGFoliagePainter).Name;
            }
        }

        [ExcludeFromDoc]
        static GFoliagePainter()
        {
            RefreshCustomPainterTypes();
        }

        /// <summary>
        /// Iterate all runtime types and refresh the list of user create painter types (not the builtin ones such as Tree Painter)
        /// This function was call at startup, you can call it at anytime but beware that it's slow.<br/>
        /// To create a custom painter, see <see cref="IGFoliagePainter"/>
        /// </summary>
        public static void RefreshCustomPainterTypes()
        {
            List<Type> loadedTypes = GCommon.GetAllLoadedTypes();
            CustomPainterTypes = loadedTypes.FindAll(
                t => t.GetInterface(FoliagePainterInterfaceName) != null &&
                !BUILTIN_PAINTER_NAME.Contains(t.Name));
        }

        /// <summary>
        /// A list of custom painter types that created by user, by implementing IGFoliagePainter interface.
        /// This list doesn't contain builtin painter types such as Tree Painter.<br/> 
        /// To create a custom painter, see <see cref="IGFoliagePainter"/>
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
        protected GFoliagePaintingMode mode;
        /// <summary>
        /// The paint mode. 
        /// If you want to paint with a custom painter, set this to Custom and set the CustomPainterIndex value.
        /// </summary>
        public GFoliagePaintingMode Mode
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
        /// To paint with a custom painter, set Mode=<see cref="GFoliagePaintingMode.Custom"/> first, then set this index value.
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
        protected bool enableTerrainMask;
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

        protected GTreePainter treePainter = new GTreePainter();
        protected GTreeScaler treeScaler = new GTreeScaler();
        protected GGrassPainter grassPainter = new GGrassPainter();
        protected GGrassScaler grassScaler = new GGrassScaler();

        /// <summary>
        /// Get the active painter object that will be used.<br/>
        /// You can do type check and cast with 'is' and 'as' operator.<br/>         
        /// </summary>
        /// 
        /// <remarks>
        /// For custom painter: Each call to this property will create a new object. This doesn't work so well with serialization, so if you need some persistent arguments between frame, store them somewhere else (static, scriptable objects, files, etc.).
        /// </remarks>
        public IGFoliagePainter ActivePainter
        {
            get
            {
                if (Mode == GFoliagePaintingMode.PaintTree)
                {
                    return treePainter;
                }
                else if (Mode == GFoliagePaintingMode.ScaleTree)
                {
                    return treeScaler;
                }
                else if (Mode == GFoliagePaintingMode.PaintGrass)
                {
                    return grassPainter;
                }
                else if (Mode == GFoliagePaintingMode.ScaleGrass)
                {
                    return grassScaler;
                }
                else if (mode == GFoliagePaintingMode.Custom)
                {
                    if (CustomPainterIndex >= 0 && CustomPainterIndex < CustomPainterTypes.Count)
                        return System.Activator.CreateInstance(CustomPainterTypes[CustomPainterIndex]) as IGFoliagePainter;
                }
                return null;
            }
        }

        [SerializeField]
        protected float brushRadius;
        /// <summary>
        /// <inheritdoc cref="GTerrainTexturePainter.BrushRadius"/>
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
        /// <inheritdoc cref="GTerrainTexturePainter.BrushRadiusJitter"/>
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
        /// <inheritdoc cref="GTerrainTexturePainter.BrushRotation"/>
        /// </summary>
        public float BrushRotation
        {
            get
            {
                return brushRotation;
            }
            set
            {
                brushRotation = value;
            }
        }

        [SerializeField]
        protected float brushRotationJitter;
        /// <summary>
        /// <inheritdoc cref="GTerrainTexturePainter.BrushRotationJitter"/>
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
        protected int brushDensity;
        /// <summary>
        /// Instance density for brush stroke. Higher value spawns more instances in a single stroke.
        /// </summary>
        public int BrushDensity
        {
            get
            {
                return brushDensity;
            }
            set
            {
                brushDensity = Mathf.Clamp(value, 1, 100);
            }
        }

        [SerializeField]
        protected float brushDensityJitter;
        /// <summary>
        /// Adding variantion to brush density with slightly higher or lower value.
        /// In range [0,1]
        /// </summary>
        public float BrushDensityJitter
        {
            get
            {
                return brushDensityJitter;
            }
            set
            {
                brushDensityJitter = Mathf.Clamp01(value);
            }
        }

        [SerializeField]
        protected float brushScatter;
        /// <summary>
        /// <inheritdoc cref="GTerrainTexturePainter.BrushScatter"/>
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
        /// <inheritdoc cref="GTerrainTexturePainter.BrushScatterJitter"/>
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
        protected List<Texture2D> brushMasks;
        /// <summary>
        /// <inheritdoc cref="GTerrainTexturePainter.BrushMasks"/>
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
        /// <inheritdoc cref="GTerrainTexturePainter.SelectedBrushMaskIndex"/>
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
        /// Index of the first selected tree prototype.
        /// Note that you don't add tree prototype to the painter. You need to create a Tree Prototype Group asset and assign to your terrains instead.
        /// </summary>
        public int SelectedTreeIndex
        {
            get
            {
                if (SelectedTreeIndices.Count == 0)
                {
                    SelectedTreeIndices.Add(0);
                }
                return SelectedTreeIndices[0];
            }
            set
            {
                SelectedTreeIndices.Clear();
                SelectedTreeIndices.Add(value);
            }
        }

        [SerializeField]
        protected List<int> selectedTreeIndices;
        /// <summary>
        /// Index of the selected tree prototypes, tree painter will pick prefabs randomly using this indices list.
        /// Note that you don't add tree prototype to the painter. You need to create a Tree Prototype Group asset and assign to your terrains instead.
        /// </summary>
        public List<int> SelectedTreeIndices
        {
            get
            {
                if (selectedTreeIndices == null)
                {
                    selectedTreeIndices = new List<int>();
                }
                return selectedTreeIndices;
            }
            set
            {
                selectedTreeIndices = value;
            }
        }

        /// <summary>
        /// Index of the first selected grass prototype.
        /// Note that you don't add grass prototype to the painter. You need to create a Grass Prototype Group asset and assign to your terrains instead.
        /// </summary>
        public int SelectedGrassIndex
        {
            get
            {
                if (SelectedGrassIndices.Count == 0)
                {
                    SelectedGrassIndices.Add(0);
                }
                return SelectedGrassIndices[0];
            }
            set
            {
                SelectedGrassIndices.Clear();
                SelectedGrassIndices.Add(value);
            }
        }

        [SerializeField]
        protected List<int> selectedGrassIndices;
        /// <summary>
        /// Index of the selected grass prototypes, grass painter will pick prefabs randomly using this indices list.
        /// Note that you don't add grass prototype to the painter. You need to create a Grass Prototype Group asset and assign to your terrains instead.
        /// </summary>
        public List<int> SelectedGrassIndices
        {
            get
            {
                if (selectedGrassIndices == null)
                {
                    selectedGrassIndices = new List<int>();
                }
                return selectedGrassIndices;
            }
            set
            {
                selectedGrassIndices = value;
            }
        }

        [SerializeField]
        protected float eraseRatio;
        /// <summary>
        /// The probability of a spawned instance to be removed when the painter is performing negative/erasing operation.
        /// Set to 1 to remove everything and lower value to perform some sort of 'thin out' action.
        /// </summary>
        public float EraseRatio
        {
            get
            {
                return eraseRatio;
            }
            set
            {
                eraseRatio = Mathf.Clamp01(value);
            }
        }

        [SerializeField]
        protected float scaleStrength;
        /// <summary>
        /// The intensity used by tree and grass scaler.
        /// </summary>
        public float ScaleStrength
        {
            get
            {
                return scaleStrength;
            }
            set
            {
                scaleStrength = Mathf.Max(0, value);
            }
        }

        [ExcludeFromDoc]
        protected void OnEnable()
        {
            ReloadBrushMasks();
        }

        [ExcludeFromDoc]
        protected void Reset()
        {
            GroupId = 0;
            Mode = GFoliagePaintingMode.PaintTree;
            BrushRadius = 50;
            BrushRadiusJitter = 0;
            BrushDensity = 1;
            BrushDensityJitter = 0;
            BrushRotation = 0;
            BrushRotationJitter = 0;
            EraseRatio = 1;
            ScaleStrength = 1;
        }

        /// <summary>
        /// <inheritdoc cref="GTerrainTexturePainter.ReloadBrushMasks"/>
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
        public void Paint(GFoliagePainterArgs args)
        {
            IGFoliagePainter p = ActivePainter;
            if (p == null)
                return;
            args.Radius = BrushRadius;
            args.Rotation = BrushRotation;
            args.Density = BrushDensity;
            args.EraseRatio = EraseRatio;
            args.ScaleStrength = ScaleStrength;
            args.TreeIndices = SelectedTreeIndices;
            args.GrassIndices = SelectedGrassIndices;

            args.CustomArgs = CustomPainterArgs;
            if (SelectedBrushMaskIndex >= 0 && SelectedBrushMaskIndex < BrushMasks.Count)
            {
                args.Mask = BrushMasks[SelectedBrushMaskIndex];
            }
            args.Filters = GetComponents<GSpawnFilter>();
            args.EnableTerrainMask = EnableTerrainMask;

            ProcessBrushDynamic(ref args);
            Vector3[] corners = GCommon.GetBrushQuadCorners(args.HitPoint, args.Radius, args.Rotation);
            args.WorldPointCorners = corners;

            List<GStylizedTerrain> overlappedTerrain = GUtilities.ExtractTerrainsFromOverlapTest(GPaintToolUtilities.OverlapTest(GroupId, args.HitPoint, args.Radius, args.Rotation));
#if UNITY_EDITOR
            if ((args.MouseEventType == GPainterMouseEventType.Down ||
                args.MouseEventType == GPainterMouseEventType.Drag) &&
                args.ShouldCommitNow == false)
            {
                Editor_CreateInitialHistoryEntry(args, overlappedTerrain);
            }
#endif

            foreach (GStylizedTerrain t in overlappedTerrain)
            {
                p.Paint(t, args);
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
        protected void Editor_CreateInitialHistoryEntry(GFoliagePainterArgs args, List<GStylizedTerrain> overlappedTerrains)
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
        protected void Editor_CreateHistory(GFoliagePainterArgs args)
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
        protected Rand GetRandomGenerator()
        {
            return new Rand(Time.frameCount);
        }

        [ExcludeFromDoc]
        protected void ProcessBrushDynamic(ref GFoliagePainterArgs args)
        {
            Rand rand = GetRandomGenerator();
            args.Radius -= BrushRadius * BrushRadiusJitter * (float)rand.NextDouble();
            args.Rotation += Mathf.Sign((float)rand.NextDouble() - 0.5f) * BrushRotation * BrushRotationJitter * (float)rand.NextDouble();
            args.Density -= Mathf.RoundToInt(BrushDensity * BrushDensityJitter * (float)rand.NextDouble());

            Vector3 scatterDir = new Vector3((float)(rand.NextDouble() * 2 - 1), 0, (float)(rand.NextDouble() * 2 - 1)).normalized;
            float scatterLengthMultiplier = BrushScatter - (float)rand.NextDouble() * BrushScatterJitter;
            float scatterLength = args.Radius * scatterLengthMultiplier;

            args.HitPoint += scatterDir * scatterLength;
        }
    }
}
#endif
