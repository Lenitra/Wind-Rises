#if GRIFFIN && !GRIFFIN_EXCLUDE_HIGHLAND
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using Pinwheel.Griffin.SplineTool;
using Pinwheel.Griffin.StampTool;

namespace Pinwheel.Griffin
{
    public static class PolarisHighlandEditor
    {
        [MenuItem("GameObject/3D Object/Polaris/Spline Tool", false, -698)]
        public static void CreateSpline(MenuCommand menuCmd)
        {
            GameObject g = new GameObject("Spline");
            if (menuCmd != null)
                GameObjectUtility.SetParentAndAlign(g, menuCmd.context as GameObject);
            g.transform.localPosition = Vector3.zero;
            g.transform.hideFlags = HideFlags.HideInInspector;
            GSplineCreator spline = g.AddComponent<GSplineCreator>();
            spline.GroupId = -1;

            Selection.activeGameObject = g;
            Undo.RegisterCreatedObjectUndo(g, "Creating Spline Tool");
        }
    

    [MenuItem("GameObject/3D Object/Polaris/Stamp Tools/Geometry Stamper", false, -799)]
        public static void CreateGeometryStamper(MenuCommand menuCmd)
        {
            GameObject geometryStamperGO = new GameObject("Geometry Stamper");
            if (menuCmd != null)
                GameObjectUtility.SetParentAndAlign(geometryStamperGO, menuCmd.context as GameObject);
            geometryStamperGO.transform.localPosition = Vector3.zero;
            geometryStamperGO.transform.hideFlags = HideFlags.HideInInspector;
            GGeometryStamper geoStamper = geometryStamperGO.AddComponent<GGeometryStamper>();
            geoStamper.GroupId = -1;

            Selection.activeGameObject = geometryStamperGO;
            Undo.RegisterCreatedObjectUndo(geometryStamperGO, "Creating Geometry Stamper");
        }

        [MenuItem("GameObject/3D Object/Polaris/Stamp Tools/Texture Stamper", false, -798)]
        public static void CreateTextureStamper(MenuCommand menuCmd)
        {
            GameObject textureStamperGO = new GameObject("Texture Stamper");
            if (menuCmd != null)
                GameObjectUtility.SetParentAndAlign(textureStamperGO, menuCmd.context as GameObject);
            textureStamperGO.transform.localPosition = Vector3.zero;
            textureStamperGO.transform.hideFlags = HideFlags.HideInInspector;
            GTextureStamper texStamper = textureStamperGO.AddComponent<GTextureStamper>();
            texStamper.GroupId = -1;

            Selection.activeGameObject = textureStamperGO;
            Undo.RegisterCreatedObjectUndo(textureStamperGO, "Creating Texture Stamper");
        }

        [MenuItem("GameObject/3D Object/Polaris/Stamp Tools/Foliage Stamper", false, -797)]
        public static void CreateFoliageStamper(MenuCommand menuCmd)
        {
            GameObject foliageStamperGO = new GameObject("Foliage Stamper");
            if (menuCmd != null)
                GameObjectUtility.SetParentAndAlign(foliageStamperGO, menuCmd.context as GameObject);
            foliageStamperGO.transform.localPosition = Vector3.zero;
            foliageStamperGO.transform.hideFlags = HideFlags.HideInInspector;
            GFoliageStamper foliageStamper = foliageStamperGO.AddComponent<GFoliageStamper>();
            foliageStamper.GroupId = -1;

            Selection.activeGameObject = foliageStamperGO;
            Undo.RegisterCreatedObjectUndo(foliageStamperGO, "Creating Foliage Stampers");
        }

        [MenuItem("GameObject/3D Object/Polaris/Stamp Tools/Object Stamper", false, -796)]
        public static void CreateObjectStamper(MenuCommand menuCmd)
        {
            GameObject objectStamperGO = new GameObject("Object Stamper");
            if (menuCmd != null)
                GameObjectUtility.SetParentAndAlign(objectStamperGO, menuCmd.context as GameObject);
            objectStamperGO.transform.localPosition = Vector3.zero;
            objectStamperGO.transform.hideFlags = HideFlags.HideInInspector;
            GObjectStamper objectStamper = objectStamperGO.AddComponent<GObjectStamper>();
            objectStamper.GroupId = -1;

            Selection.activeGameObject = objectStamperGO;
            Undo.RegisterCreatedObjectUndo(objectStamperGO, "Creating Object Stampers");
        }
        }
}
#endif