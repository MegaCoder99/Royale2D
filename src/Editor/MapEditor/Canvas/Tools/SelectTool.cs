using Editor;
using Shared;
using System.Windows.Input;

namespace MapEditor;

public class SelectTool : Editor.SelectTool
{
    public SelectTool() : base()
    {
    }

    public override void OnLeftMouseDown(BaseCanvas baseCanvas)
    {
        base.OnLeftMouseDown(baseCanvas);
        SectionsSC sectionsSC = (baseCanvas as MapCanvas)!.sectionsSC;
        double mouseX = baseCanvas.mouseX;
        double mouseY = baseCanvas.mouseY;

        ResizeDir? resizeDir = GetResizeDir(baseCanvas);
        if (resizeDir != null)
        {
            baseCanvas.tool = new ResizeTileTool(this, mouseX, mouseY, resizeDir);
            return;
        }

        if (Helpers.ShiftHeld())
        {
            sectionsSC.ShiftSelectCommit(baseCanvas.mouseI, baseCanvas.mouseJ);
        }
        else if (Helpers.AltHeld())
        {
            sectionsSC.AltSelectCommit(sectionsSC.selectedTileCoords.LastOrDefault(), baseCanvas.mouseI, baseCanvas.mouseJ);
        }
        else
        {
            sectionsSC.SelectTilesCommit(baseCanvas.mouseI, baseCanvas.mouseJ);
        }
    }

    public override void OnLeftMouseUp(BaseCanvas baseCanvas)
    {
        base.OnLeftMouseUp(baseCanvas);
        SectionsSC sectionsSC = (baseCanvas as MapCanvas)!.sectionsSC;
        int TS = sectionsSC.TS;
        double mouseX = baseCanvas.mouseX;
        double mouseY = baseCanvas.mouseY;

        sectionsSC.SetSelectionText((int)mouseX, (int)mouseY);
        
        int mouseI = (int)mouseY / TS;
        int mouseJ = (int)mouseX / TS;
        int lastClickedMouseI = (int)baseCanvas.lastClickedMouseY / TS;
        int lastClickedMouseJ = (int)baseCanvas.lastClickedMouseX / TS;

        if (mouseI != lastClickedMouseI || mouseJ != lastClickedMouseJ)
        {
            sectionsSC.DragSelectCommit(
                Math.Min(mouseI, lastClickedMouseI),
                Math.Min(mouseJ, lastClickedMouseJ),
                Math.Max(mouseI, lastClickedMouseI),
                Math.Max(mouseJ, lastClickedMouseJ)
            );
        }
    }

    public override void OnRightMouseUp(BaseCanvas baseCanvas)
    {
        base.OnRightMouseUp(baseCanvas);
        SectionsSC sectionsSC = (baseCanvas as MapCanvas)!.sectionsSC;

        sectionsSC.SelectInstanceOrZoneCommit();
    }

    public override void OnMiddleMouseUp(BaseCanvas baseCanvas)
    {
        base.OnMiddleMouseUp(baseCanvas);
        SectionsSC sectionsSC = (baseCanvas as MapCanvas)!.sectionsSC;

        sectionsSC.FillSelectCommit(baseCanvas.mouseI, baseCanvas.mouseJ);
    }

    public override void OnMouseMove(BaseCanvas baseCanvas)
    {
        base.OnMouseMove(baseCanvas);
        if (baseCanvas.isLeftMouseDown)
        {
            Mouse.OverrideCursor = null;
            return;
        }

        ResizeDir? resizeDir = GetResizeDir(baseCanvas);
        if (resizeDir != null)
        {
            Mouse.OverrideCursor = resizeDir.cursor;
        }
        else
        {
            Mouse.OverrideCursor = null;
        }
    }

    public ResizeDir? GetResizeDir(BaseCanvas baseCanvas)
    {
        SectionsSC sectionsSC = (baseCanvas as MapCanvas)!.sectionsSC;
        if (sectionsSC.selectedTileCoords.Count == 0) return null;
        if (sectionsSC.state.selectedMode != MapEditorMode.PaintTile) return null;
        if (!sectionsSC.state.toggleResizeTiles) return null;

        (int minI, int maxI, int minJ, int maxJ) = sectionsSC.GetMinMaxSelectionIJs();

        int?[,]? selectedTileIdGrid = sectionsSC.GetSelectedTileIdGrid(false);
        if (selectedTileIdGrid == null || selectedTileIdGrid.Any(tileId => tileId == null)) return null;

        int TS = sectionsSC.TS;
        MyRect selectedTileCoordsRect = new(minJ * TS, minI * TS, (maxJ + 1) * TS, (maxI + 1) * TS);
        return GetResizeDirFromRect(baseCanvas, selectedTileCoordsRect);
    }
}
