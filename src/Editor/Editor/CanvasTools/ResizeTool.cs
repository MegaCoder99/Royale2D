using System.Windows.Input;

namespace Editor;

public abstract class ResizeTool : CanvasTool
{
    public CanvasTool prevTool;
    public double origMouseX;
    public double origMouseY;
    public ResizeDir resizeDir;

    public ResizeTool(CanvasTool prevTool, double origMouseX, double origMouseY, ResizeDir resizeDir) : base(true)
    {
        this.prevTool = prevTool;
        this.origMouseX = origMouseX;
        this.origMouseY = origMouseY;
        this.resizeDir = resizeDir;
        Mouse.OverrideCursor = resizeDir.cursor;
    }

    public override void OnLeftMouseUp(BaseCanvas baseCanvas)
    {
        baseCanvas.tool = prevTool;
        baseCanvas.InvalidateImage();
    }

    public override void OnMouseLeave(BaseCanvas baseCanvas)
    {
        OnLeftMouseUp(baseCanvas);
    }
}