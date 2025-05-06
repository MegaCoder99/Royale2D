using Shared;

namespace Editor;

public abstract class PlaceTool : CanvasTool
{
    protected Drawer mouseImageDrawer;
    public bool snapToTile;

    public PlaceTool(Drawer mouseImageDrawer, bool snapToTile) : base(true)
    {
        this.mouseImageDrawer = mouseImageDrawer;
        this.snapToTile = snapToTile;
    }

    public virtual MyPoint GetPlacePos(BaseCanvas baseCanvas)
    {
        int TS = baseCanvas.tileSize;
        int x = baseCanvas.mouseXInt;
        int y = baseCanvas.mouseYInt;
        if (snapToTile)
        {
            x = (baseCanvas.mouseXInt / TS) * TS;
            y = (baseCanvas.mouseYInt / TS) * TS;
        }
        return new MyPoint(x, y);
    }

    public virtual MyPoint GetDrawPos(BaseCanvas baseCanvas)
    {
        return GetPlacePos(baseCanvas);
    }

    public override void OnMouseMove(BaseCanvas baseCanvas)
    {
        if (!snapToTile)
        {
            baseCanvas.InvalidateImage();
        }
    }

    public override void OnMouseGridCoordsChange(BaseCanvas baseCanvas)
    {
        if (snapToTile)
        {
            baseCanvas.InvalidateImage();
        }
    }

    public override void OnRedraw(BaseCanvas baseCanvas, Drawer drawer)
    {
        if (!baseCanvas.isMouseOver) return;
        MyPoint drawPos = GetDrawPos(baseCanvas);
        drawer.DrawImage(mouseImageDrawer, drawPos.x, drawPos.y, alpha: 0.5f);
    }

    public override void OnMouseLeave(BaseCanvas baseCanvas)
    {
        base.OnMouseLeave(baseCanvas);
        baseCanvas.InvalidateImage();
    }

    public override void OnDestroy(BaseCanvas baseCanvas)
    {
        mouseImageDrawer.Dispose();
    }
}