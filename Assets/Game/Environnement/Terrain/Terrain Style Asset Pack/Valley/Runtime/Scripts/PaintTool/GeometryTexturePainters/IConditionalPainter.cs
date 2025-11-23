#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pinwheel.Griffin.PaintTool
{
    /// <summary>
    /// Custom painter should implement this interface to support conditional painting (paint with height, slope and noise rule). Inherit from this and conditional painting happens automatically.
    /// </summary>
    /// <seealso cref="IGTexturePainter"/>
    public interface IConditionalPainter
    {
    }
}
#endif
