#if GRIFFIN
using UnityEngine;

namespace Pinwheel.Griffin.TextureTool
{
    [ExcludeFromDoc]
    public interface IGTextureFilter
    {
        void Apply(RenderTexture targetRt, GTextureFilterParams param);
    }
}
#endif
