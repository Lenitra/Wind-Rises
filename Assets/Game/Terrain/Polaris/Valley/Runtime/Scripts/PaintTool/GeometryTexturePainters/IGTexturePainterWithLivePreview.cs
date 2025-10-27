#if GRIFFIN
using UnityEngine;

namespace Pinwheel.Griffin.PaintTool
{
    /// <summary>
    /// Custom painter should this interface to draw live preview on terrains.
    /// </summary>
    public interface IGTexturePainterWithLivePreview
    {
        void Editor_DrawLivePreview(GStylizedTerrain terrain, GTexturePainterArgs args, Camera cam);
    }
}
#endif
