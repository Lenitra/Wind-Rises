#if GRIFFIN && !GRIFFIN_EXCLUDE_HIGHLAND
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace Pinwheel.Griffin.SplineTool
{
    [ExcludeFromDoc]
    [InitializeOnLoad]
    public static class GSplineToolLayerInitializer
    {
        [InitializeOnLoadMethod]
        private static void Init()
        {
            GEditorSettings.SetupSplineLayerCallback += SetupSplineLayer;
        }

        public static void SetupSplineLayer()
        {
            int index = GEditorSettings.Instance.layers.splineLayerIndex;
            string layer = GSplineCreator.SPLINE_LAYER;
            if (GEditorSettings.Instance.layers.SetupLayer(index, layer))
            {
                Debug.Log($"POLARIS: Set layer {index} to {layer}. This layer is reserved for Spline tool to work! You can change this in ProjectSettings/ Polaris/ Editor/ Layers");
            }
        }
    }
}
#endif