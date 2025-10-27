#if GRIFFIN
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.Griffin
{
    public static class GLinkStripDrawer
    {
        private static List<string> s_linkLabels = new List<string>();
        private static Dictionary<string, string> s_linkContents = new Dictionary<string, string>()
        {
            {"Docs",  "https://docs.pinwheelstud.io/memo"},
            {"Support",  "https://discord.gg/btX4pdmZdV"},
            {"|", "" },
            {"Poseidon", "https://assetstore.unity.com/packages/vfx/shaders/low-poly-water-poseidon-153826" },
            {"Jupiter" ,"https://assetstore.unity.com/packages/2d/textures-materials/sky/procedural-sky-shader-day-night-cycle-jupiter-159992" },
            {"Beam","https://assetstore.unity.com/packages/vfx/shaders/fullscreen-camera-effects/beam-froxel-based-volumetric-lighting-fog-urp-render-graph-317850" }
        };

        private static GUIStyle s_linkStyleSmall;
        public static GUIStyle linkStyleSmall
        {
            get
            {
                if (s_linkStyleSmall == null)
                {
                    s_linkStyleSmall = new GUIStyle(EditorStyles.miniLabel);
                }
                s_linkStyleSmall.normal.textColor = new Color32(125, 170, 240, 255);
                s_linkStyleSmall.alignment = TextAnchor.MiddleLeft;
                return s_linkStyleSmall;
            }
        }

        private static GUIStyle s_linkStyleNormal;
        public static GUIStyle linkStyleNormal
        {
            get
            {
                if (s_linkStyleNormal == null)
                {
                    s_linkStyleNormal = new GUIStyle(EditorStyles.label);
                }
                s_linkStyleNormal.normal.textColor = new Color32(125, 170, 240, 255);
                //s_linkStyleNormal.alignment = TextAnchor.UpperLeft;
                return s_linkStyleNormal;
            }
        }

        public static void Draw(string utmCampaign = "", string utmSource = "", string utmMedium = "")
        {
            s_linkLabels.Clear();
            s_linkLabels.AddRange(s_linkContents.Keys);
            Rect r = EditorGUILayout.GetControlRect(false, 12);
            var rects = EditorGUIUtility.GetFlowLayoutedRects(r, linkStyleSmall, 4, 0, s_linkLabels);

            for (int i = 0; i < rects.Count; ++i)
            {
                Rect rect = rects[i];
                string label = s_linkLabels[i];
                string url = s_linkContents[label];

                if (!string.IsNullOrEmpty(url))
                {
                    EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
                    if (GUI.Button(rect, label, linkStyleSmall))
                    {
                        url = GNetUtils.ModURL(url, utmCampaign, utmSource, $"{utmMedium}-{label.Replace(" ", "")}");
                        Application.OpenURL(url);
                    }
                }
                else
                {
                    GUI.Label(rect, label, linkStyleSmall);
                }
            }
        }

        public static void DrawSingleLink(string text, string url, GUIStyle style = null, string utmCampaign = "", string utmSource = "", string utmMedium = "")
        {
            if (style == null)
            {
                style = linkStyleNormal;
            }

            GUIContent labelContent = EditorGUIUtility.TrTempContent(text);
            Vector2 rectSize = style.CalcSize(labelContent);
            Rect rect = EditorGUILayout.GetControlRect(GUILayout.Width(rectSize.x), GUILayout.Height(rectSize.y));
            if (!string.IsNullOrEmpty(url))
            {
                EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
                if (GUI.Button(rect, labelContent, style))
                {
                    url = GNetUtils.ModURL(url, utmCampaign, utmSource, $"{utmMedium}-{labelContent.text.Replace(" ", "")}");
                    Application.OpenURL(url);
                }
            }
            else
            {
                GUI.Label(rect, labelContent, linkStyleSmall);
            }
        }
    }
}
#endif