#if GRIFFIN
using UnityEngine;

namespace Pinwheel.Griffin.TextureTool
{
    [ExcludeFromDoc]
    public interface IGTextureGenerator
    {
        void Generate(RenderTexture targetRt);
    }
}
#endif
