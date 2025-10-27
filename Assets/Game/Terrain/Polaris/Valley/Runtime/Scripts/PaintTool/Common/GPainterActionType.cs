#if GRIFFIN
namespace Pinwheel.Griffin.PaintTool
{
    /// <summary>
    /// The action type a painter will perform.
    /// </summary>
    public enum GPainterActionType
    {
        /// <summary>
        /// Normal painting, e.g: raise height
        /// </summary>
        Normal, 
        /// <summary>
        /// Negattive painting, e.g: lower height
        /// </summary>
        Negative, 
        /// <summary>
        /// Alternative painting, e.g: smooth height
        /// </summary>
        Alternative
    }
}
#endif
