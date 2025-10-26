#if GRIFFIN
using UnityEditor;
using UnityEngine;

namespace Pinwheel.Griffin
{
    [CustomEditor(typeof(GAssetExplorer))]
    public class GAssetExplorerInspector : Editor
    {
        private GAssetExplorer instance;

        public GUIStyle titleStyle;
        public GUIStyle TitleStyle
        {
            get
            {
                if (titleStyle == null)
                {
                    titleStyle = new GUIStyle(EditorStyles.label);
                    titleStyle.fontStyle = FontStyle.Bold;
                    titleStyle.fontSize = 13;
                    titleStyle.alignment = TextAnchor.UpperLeft;
                }
                return titleStyle;
            }
        }

        public void OnEnable()
        {
            instance = target as GAssetExplorer;
        }

        public override void OnInspectorGUI()
        {
            DrawInstructionGUI();
            DrawFeaturedAssetsGUI();
            DrawCollectionsGUI();
        }

        public override bool RequiresConstantRepaint()
        {
            return true;
        }

        private void DrawInstructionGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("More than a terrain tool", GStyles.H1);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(
                "It's an ecosystem specialized for Low Poly scene creation! " +
                "Below are some asset suggestions which we found helpful to enhance your scene.",
                GStyles.P1);
            EditorGUILayout.Space();
        }

        private void DrawFeaturedAssetsGUI()
        {
            EditorGUILayout.LabelField("From Pinwheel Studio", GStyles.H2);

            DrawFeatureAssetEntry(
                "AssetIcons/PolarisIcon",
                "Polaris",
                "Low poly terrain modeling, texturing and planting.",
                GAssetLink.POLARIS_3);

            GEditorCommon.SpacePixel(0);

            DrawFeatureAssetEntry(
                "AssetIcons/VistaIcon",
                "Vista",
                "Procedural terrain generator, node and graph workflow, multi-biomes.",
                GAssetLink.VISTA_PRO);

            GEditorCommon.SpacePixel(0);

            DrawFeatureAssetEntry(
                "AssetIcons/PoseidonIcon",
                "Poseidon",
                "Low poly water with high fidelity and performance.",
                GAssetLink.POSEIDON);

            GEditorCommon.SpacePixel(0);

            DrawFeatureAssetEntry(
                "AssetIcons/JupiterIcon",
                "Jupiter",
                "Single-pass procedural sky with day night cycle.",
                GAssetLink.JUPITER);

            GEditorCommon.SpacePixel(0);

            DrawFeatureAssetEntry(
               "AssetIcons/ContourIcon",
               "Contour",
               "Edge detection and outline effect for URP.",
               GAssetLink.CONTOUR);

            GEditorCommon.SpacePixel(0);

            DrawFeatureAssetEntry(
               "AssetIcons/BeamIcon",
               "Beam",
               "Volumetric light and fog for URP.",
               GAssetLink.BEAM);

            GEditorCommon.SpacePixel(0);
        }

        private void DrawFeatureAssetEntry(
            string iconResourcePath,
            string title,
            string description,
            string link)
        {
            Rect r = EditorGUILayout.BeginHorizontal();

            Rect iconRect = EditorGUILayout.GetControlRect(GUILayout.Width(64), GUILayout.Height(64));
            Texture2D icon = Resources.Load<Texture2D>(iconResourcePath);
            GUI.DrawTexture(iconRect, icon ?? Texture2D.blackTexture);

            EditorGUILayout.BeginVertical();
            Rect titleRect = EditorGUILayout.GetControlRect(GUILayout.Height(17));
            EditorGUI.LabelField(titleRect, title, GStyles.H3);
            EditorGUILayout.LabelField(description, GStyles.P1);
            EditorGUIUtility.AddCursorRect(r, MouseCursor.Link);
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            if (r.Contains(Event.current.mousePosition))
            {
                RectOffset offset = new RectOffset(2, 2, 2, 2);
                GEditorCommon.DrawOutlineBox(offset.Add(r), Color.gray);

                if (Event.current.type == EventType.MouseDown)
                {
                    Application.OpenURL(GNetUtils.ModURL(link, "", GAssetLink.UTM_SOURCE, $"asset-explorer-{title.Replace(" ", "-")}"));
                }
            }
        }

        private void DrawCollectionsGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Collections", GStyles.H2);
            DrawFeatureAssetEntry(
              "AssetIcons/CollectionIcon",
              "Ground Textures",
              "Tilable textures for terrain material.",
              GAssetLink.GROUND_TEXTURES_LIST);

            GEditorCommon.SpacePixel(0);

            DrawFeatureAssetEntry(
              "AssetIcons/CollectionIcon",
              "Vegetation",
              "Tree and grass asset for using with Polaris tools.",
              GAssetLink.VEGETATION_LIST);

            GEditorCommon.SpacePixel(0);

            DrawFeatureAssetEntry(
              "AssetIcons/CollectionIcon",
              "Rock & Props",
              "Rocks and props asset for using with Polaris tools.",
              GAssetLink.ROCK_PROPS_LIST);

            GEditorCommon.SpacePixel(0);

            DrawFeatureAssetEntry(
              "AssetIcons/CollectionIcon",
              "Characters",
              "Low poly characters and animation.",
              GAssetLink.CHARACTER_LIST);

            GEditorCommon.SpacePixel(0);
        }
    }
}
#endif
