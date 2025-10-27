#if GRIFFIN
using System.Collections.Generic;
using UnityEngine;
using Rand = System.Random;
using Type = System.Type;

namespace Pinwheel.Griffin.PaintTool
{
    /// <summary>
    /// A component that let you paint prefab instances on terrains. This works both in the editor and runtime.
    /// The painter UI is provided for editor only. At runtime you have to build your own UI, since each game will display its painter differently.
    /// A basic UI should have buttons for selecting paint mode, density slider, prefab selector, etc.
    /// You don't have to display everything, just the settings that matter.
    /// </summary>
    [System.Serializable]
    [ExecuteInEditMode]
    public class GObjectPainter : MonoBehaviour
    {
        private static readonly List<string> BUILTIN_PAINTER_NAME = new List<string>(new string[]
        {
            "GObjectSpawner",
            "GObjectScaler"
        });

        private static List<Type> customPainterTypes;
        [ExcludeFromDoc]
        protected static List<Type> CustomPainterTypes
        {
            get
            {
                if (customPainterTypes == null)
                    customPainterTypes = new List<Type>();
                return customPainterTypes;
            }
            private set
            {
                customPainterTypes = value;
            }
        }

        [ExcludeFromDoc]
        public static string ObjectPainterInterfaceName
        {
            get
            {
                return typeof(IGObjectPainter).Name;
            }
        }

        [ExcludeFromDoc]
        static GObjectPainter()
        {
            RefreshCustomPainterTypes();
        }

        /// <summary>
        /// Iterate all runtime types and refresh the list of user create painter types (not the builtin ones such as Object Spawner)
        /// This function was call at startup, you can call it at anytime but beware that it's slow.<br/>
        /// To create a custom painter, see <see cref="IGObjectPainter"/>
        /// </summary>
        public static void RefreshCustomPainterTypes()
        {
            List<Type> loadedTypes = GCommon.GetAllLoadedTypes();
            CustomPainterTypes = loadedTypes.FindAll(
                t => t.GetInterface(ObjectPainterInterfaceName) != null &&
                !BUILTIN_PAINTER_NAME.Contains(t.Name));
        }

        /// <summary>
        /// A list of custom painter types that created by user, by implementing IGObjectPainter interface.
        /// This list doesn't contain builtin painter types such as Object Spawner.<br/> 
        /// To create a custom painter, see <see cref="IGObjectPainter"/>
        /// </summary>
        /// <returns>The list of custom painter types.</returns>
        public static List<Type> GetCustomPainterTypes()
        {
            return CustomPainterTypes;
        }

        [SerializeField]
        private int groupId;
        /// <summary>
        /// <inheritdoc cref="GFoliagePainter.GroupId"/>
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
        private bool enableTerrainMask;
        /// <summary>
        /// <inheritdoc cref="GFoliagePainter.EnableTerrainMask"/>
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
        private GObjectPaintingMode mode;
        /// <summary>
        /// <inheritdoc cref="GFoliagePainter.Mode"/>
        /// </summary>
        public GObjectPaintingMode Mode
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
        private int customPainterIndex;
        /// <summary>
        /// Index of the custom painter type in the CustomPainterTypes list.
        /// To paint with a custom painter, set Mode=<see cref="GObjectPaintingMode.Custom"/> first, then set this index value.
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
        private string customPainterArgs;
        /// <summary>
        /// <inheritdoc cref="GFoliagePainter.CustomPainterArgs"/>
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

        protected GObjectSpawner objectSpawner = new GObjectSpawner();
        protected GObjectScaler objectScaler = new GObjectScaler();

        /// <summary>
        /// <inheritdoc cref="GFoliagePainter.ActivePainter"/>
        /// </summary>
        public IGObjectPainter ActivePainter
        {
            get
            {
                if (Mode == GObjectPaintingMode.Spawn)
                {
                    return objectSpawner;
                }
                else if (Mode == GObjectPaintingMode.Scale)
                {
                    return objectScaler;
                }
                else if (mode == GObjectPaintingMode.Custom)
                {
                    if (CustomPainterIndex >= 0 && CustomPainterIndex < CustomPainterTypes.Count)
                        return System.Activator.CreateInstance(CustomPainterTypes[CustomPainterIndex]) as IGObjectPainter;
                }
                return null;
            }
        }

        [SerializeField]
        private float brushRadius;
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
        private float brushRadiusJitter;
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
        private float brushRotation;
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
        private float brushRotationJitter;
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
        private int brushDensity;
        /// <summary>
        /// <inheritdoc cref="GFoliagePainter.BrushDensity"/>
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
        private float brushDensityJitter;
        /// <summary>
        /// <inheritdoc cref="GFoliagePainter.BrushDensityJitter"/>
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
        private float brushScatter;
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
        private float brushScatterJitter;
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
        private List<Texture2D> brushMasks;
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
        private int selectedBrushMaskIndex;
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

        [SerializeField]
        private List<GameObject> prototypes;
        /// <summary>
        /// Collection of prefabs for spawning, selected prefabs were determined by <see cref="SelectedPrototypeIndices"/>
        /// </summary>
        public List<GameObject> Prototypes
        {
            get
            {
                if (prototypes == null)
                {
                    prototypes = new List<GameObject>();
                }
                return prototypes;
            }
            set
            {
                prototypes = value;
            }
        }

        [SerializeField]
        private List<int> selectedPrototypeIndices;
        /// <summary>
        /// Index of the selected tree prototypes, painters will pick prefabs randomly using this indices list.
        /// </summary>
        public List<int> SelectedPrototypeIndices
        {
            get
            {
                if (selectedPrototypeIndices == null)
                {
                    selectedPrototypeIndices = new List<int>();
                }
                return selectedPrototypeIndices;
            }
            set
            {
                selectedPrototypeIndices = value;
            }
        }

        [SerializeField]
        private float eraseRatio;
        /// <summary>
        /// <inheritdoc cref="GFoliagePainter.EraseRatio"/>
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
        private float scaleStrength;
        /// <summary>
        /// <inheritdoc cref="GFoliagePainter.ScaleStrength"/>
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

        private void OnEnable()
        {
            ReloadBrushMasks();
        }

        private void Reset()
        {
            GroupId = 0;
            Mode = GObjectPaintingMode.Spawn;
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
        /// </summary>
        /// <param name="args">Struct containing all neccessary arguments for painting. Most arguments were passed from this component, while some come from the editor or your game mechanic.</param>
        public void Paint(GObjectPainterArgs args)
        {
            IGObjectPainter p = ActivePainter;
            if (p == null)
                return;

            args.Radius = BrushRadius;
            args.Rotation = BrushRotation;
            args.Density = BrushDensity;
            args.EraseRatio = EraseRatio;
            args.ScaleStrength = ScaleStrength;
            args.CustomArgs = CustomPainterArgs;
            args.Filters = GetComponents<GSpawnFilter>();
            if (SelectedBrushMaskIndex >= 0 && SelectedBrushMaskIndex < BrushMasks.Count)
            {
                args.Mask = BrushMasks[SelectedBrushMaskIndex];
            }
            if (SelectedPrototypeIndices.Count == 0)
            {
                return;
            }
            args.Prototypes = Prototypes;
            args.PrototypeIndices = SelectedPrototypeIndices;
            args.EnableTerrainMask = EnableTerrainMask;

            ProcessBrushDynamic(ref args);
            Vector3[] corners = GCommon.GetBrushQuadCorners(args.HitPoint, args.Radius, args.Rotation);
            args.WorldPointCorners = corners;

            List<GStylizedTerrain> terrains = GUtilities.ExtractTerrainsFromOverlapTest(GPaintToolUtilities.OverlapTest(groupId, args.HitPoint, args.Radius, args.Rotation));
            foreach (GStylizedTerrain t in terrains)
            {
                p.Paint(t, args);
            }
        }

        private Rand GetRandomGenerator()
        {
            return new Rand(Time.frameCount);
        }

        private void ProcessBrushDynamic(ref GObjectPainterArgs args)
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
