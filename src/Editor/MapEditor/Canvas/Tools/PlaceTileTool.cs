using Editor;
namespace MapEditor;

public class PlaceTileTool : PlaceTool
{
    SelectedTileIdGrids tileIdsToPlace;
    int heldDownCount;
    public PlaceTileTool(Drawer mouseImageDrawer, SelectedTileIdGrids tileIdsToPlace) : base(mouseImageDrawer, true)
    {
        this.tileIdsToPlace = tileIdsToPlace;
    }

    public override void OnLeftMouseDown(BaseCanvas baseCanvas)
    {
        base.OnLeftMouseDown(baseCanvas);
        SectionsSC sectionsSC = (baseCanvas as MapCanvas)!.sectionsSC;

        sectionsSC.PlacePreviewTilesCommit(tileIdsToPlace);
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
