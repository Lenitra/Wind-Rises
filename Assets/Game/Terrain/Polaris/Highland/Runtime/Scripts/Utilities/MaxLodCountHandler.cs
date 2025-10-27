#if GRIFFIN && !GRIFFIN_EXCLUDE_HIGHLAND
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;

namespace Pinwheel.Griffin
{
    [InitializeOnLoad]
    public static class MaxLodCountHandler
    {
        [InitializeOnLoadMethod]
        private static void Init()
        {
            GCommon.getMaxLodCountCallback += OnGetMaxLodCount;
        }

        private static void OnGetMaxLodCount(ref int lodCount)
        {
            lodCount = Mathf.Max(lodCount, 4);
        }
    }
}
#endif