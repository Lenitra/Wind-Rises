#if GRIFFIN && !GRIFFIN_EXCLUDE_HIGHLAND
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;
using Pinwheel.Griffin.SplineTool;
using Pinwheel.Griffin.StampTool;

namespace Pinwheel.Griffin.Wizard.Highland
{
    [InitializeOnLoad]
    public static class WizardCommandHandler
    {
        [InitializeOnLoadMethod]
        private static void Init()
        {
            MainPage.AddSplineToSceneCallback += OnAddSplineToScene;
            MainPage.AddStampersToSceneCallback += OnAddStampersToScene;
        }

        private static void OnAddSplineToScene()
        {
            GameObject root = GWizard.GetTerrainToolsRoot();
            if (root == null)
            {
                root = GWizard.CreateTerrainToolsRoot();
            }
            GameObject g = new GameObject("Spline");
            g.transform.parent = root.transform;
            g.transform.position = Vector3.zero;
            g.transform.rotation = Quaternion.identity;
            g.transform.localScale = Vector3.one;

            GSplineCreator component = g.AddComponent<GSplineCreator>();

            EditorGUIUtility.PingObject(component);
            Selection.activeGameObject = component.gameObject;
        }

        private static void OnAddStampersToScene()
        {
            GameObject root = GWizard.GetTerrainToolsRoot();
            if (root == null)
            {
                root = GWizard.CreateTerrainToolsRoot();
            }

            GameObject geoStamperGO = new GameObject("Geometry Stamper");
            geoStamperGO.transform.parent = root.transform;
            geoStamperGO.transform.position = Vector3.zero;
            geoStamperGO.transform.rotation = Quaternion.identity;
            geoStamperGO.transform.localScale = Vector3.one;
            GGeometryStamper geoStamper = geoStamperGO.AddComponent<GGeometryStamper>();

            GameObject texStamperGO = new GameObject("Texture Stamper");
            texStamperGO.transform.parent = root.transform;
            texStamperGO.transform.position = Vector3.zero;
            texStamperGO.transform.rotation = Quaternion.identity;
            texStamperGO.transform.localScale = Vector3.one;
            GTextureStamper texStamper = texStamperGO.AddComponent<GTextureStamper>();

            GameObject foliageStamperGO = new GameObject("Foliage Stamper");
            foliageStamperGO.transform.parent = root.transform;
            foliageStamperGO.transform.position = Vector3.zero;
            foliageStamperGO.transform.rotation = Quaternion.identity;
            foliageStamperGO.transform.localScale = Vector3.one;
            GFoliageStamper foliageStamper = foliageStamperGO.AddComponent<GFoliageStamper>();

            GameObject objectStamperGO = new GameObject("Object Stamper");
            objectStamperGO.transform.parent = root.transform;
            objectStamperGO.transform.position = Vector3.zero;
            objectStamperGO.transform.rotation = Quaternion.identity;
            objectStamperGO.transform.localScale = Vector3.one;
            GObjectStamper objectStamper = objectStamperGO.AddComponent<GObjectStamper>();

            EditorGUIUtility.PingObject(geoStamper);
            Selection.activeGameObject = geoStamper.gameObject;
        }
    }
}
#endif