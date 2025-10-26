#if GRIFFIN
namespace Pinwheel.Griffin.PaintTool
{
    /// <summary>
    /// Custom painter should implement this interface to display additional arguments in the editor.
    /// </summary>
    public interface IGTexturePainterWithCustomParams
    {
        void Editor_DrawCustomParamsGUI();
    }
}
#endif
