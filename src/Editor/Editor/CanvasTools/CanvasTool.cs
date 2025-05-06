using System.Windows.Input;

namespace Editor;

public abstract class CanvasTool
{
    public bool redrawUsesZoomScroll;
    public Cursor? cursor;

    public CanvasTool(bool redrawUsesZoomScroll)
    {
        this.redrawUsesZoomScroll = redrawUsesZoomScroll;
    }

    public virtual void OnRedraw(BaseCanvas baseCanvas, Drawer drawer) { }
    public virtual void OnMouseMove(BaseCanvas baseCanvas) { }
    public virtual void OnMouseGridCoordsChange(BaseCanvas baseCanvas) { }
    public virtual void OnLeftMouseDown(BaseCanvas baseCanvas) { }
    public virtual void OnLeftMouseUp(BaseCanvas baseCanvas) { }
    public virtual void OnRightMouseUp(BaseCanvas baseCanvas) { }
    public virtual void OnMiddleMouseUp(BaseCanvas baseCanvas) { }
    public virtual void OnMouseLeave(BaseCanvas baseCanvas) { }
    public virtual void OnDestroy(BaseCanvas baseCanvas) { }
}
