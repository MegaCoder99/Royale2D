using Editor;

namespace MapEditor;

public class EraseTileTool : CanvasTool
{
    int heldDownCount;
    public EraseTileTool() : base(true)
    {
        cursor = Resources.EraserCursor;
    }

    public override void OnLeftMouseDown(BaseCanvas baseCanvas)
    {
        base.OnLeftMouseDown(baseCanvas);
        SectionsSC sectionsSC = (baseCanvas as MapCanvas)!.sectionsSC;

        sectionsSC.EraseTilesCommit(baseCanvas.mouseI, baseCanvas.mouseJ);
    }

    public override void OnLeftMouseUp(BaseCanvas baseCanvas)
    {
        base.OnLeftMouseUp(baseCanvas);
        SectionsSC sectionsSC = (baseCanvas as MapCanvas)!.sectionsSC;

        if (heldDownCount > 0)
        {
            sectionsSC.undoManager?.MergeLastNUndoGroups(heldDownCount + 1);
            heldDownCount = 0;
        }
    }

    public override void OnMouseGridCoordsChange(BaseCanvas baseCanvas)
    {
        base.OnMouseGridCoordsChange(baseCanvas);

        if (baseCanvas.isLeftMouseDown)
        {
            OnLeftMouseDown(baseCanvas);
            heldDownCount++;
        }
    }

    public override void OnMouseLeave(BaseCanvas baseCanvas)
    {
        base.OnMouseLeave(baseCanvas);
        OnLeftMouseUp(baseCanvas);
    }
}
