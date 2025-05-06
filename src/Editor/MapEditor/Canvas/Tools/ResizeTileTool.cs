using Editor;
using Shared;

namespace MapEditor;

public class ResizeTileTool : ResizeTool
{
    List<TileInstance> tileChanges = [];
    public ResizeTileTool(CanvasTool prevTool, double origMouseX, double origMouseY, ResizeDir resizeDir) :
        base(prevTool, origMouseX, origMouseY, resizeDir)
    {
    }

    public override void OnMouseMove(BaseCanvas baseCanvas)
    {
        base.OnMouseMove(baseCanvas);

        baseCanvas.InvalidateImage();
    }

    public override void OnRedraw(BaseCanvas baseCanvas, Drawer drawer)
    {
        base.OnRedraw(baseCanvas, drawer);
        SectionsSC sectionsSC = (baseCanvas as MapCanvas)!.sectionsSC;

        if (sectionsSC.selectedTileCoords.Count == 0) return;

        MapSectionLayer? selectedLayer = sectionsSC.GetSelectedLayer();
        if (selectedLayer == null)
        {
            Prompt.ShowError("Multiple layers selected. Select one layer only.");
            return;
        }

        tileChanges.Clear();

        int TS = sectionsSC.TS;
        (int minI, int maxI, int minJ, int maxJ) = sectionsSC.GetMinMaxSelectionIJs();
        int selRows = maxI - minI + 1;
        int selCols = maxJ - minJ + 1;

        int xSign = resizeDir.xDir;
        double deltaX = (baseCanvas.mouseXInt - origMouseX) * xSign;
        if (deltaX < 0)
        {
            xSign *= -1;
            deltaX *= -1;
            deltaX = MyMath.ClampMin0(deltaX - (selCols * TS));
        }

        int ySign = resizeDir.yDir;
        double deltaY = (baseCanvas.mouseYInt - origMouseY) * ySign;
        if (deltaY < 0)
        {
            ySign *= -1;
            deltaY *= -1;
            deltaY = MyMath.ClampMin0(deltaY - (selRows * TS));
        }

        int deltaTilesX = resizeDir.xDir == 0 ? selCols : MyMath.Ceil(deltaX / TS);
        int deltaTilesY = resizeDir.yDir == 0 ? selRows : MyMath.Ceil(deltaY / TS);

        for (int m = -1; m < deltaTilesY / selRows; m++)
        {
            for (int n = -1; n < deltaTilesX / selCols; n++)
            {
                for (int i = minI; i <= maxI; i++)
                {
                    for (int j = minJ; j <= maxJ; j++)
                    {
                        GridCoords src = new GridCoords(i, j);
                        GridCoords dest = new GridCoords(i + (ySign * selRows * (m + 1)), j + (xSign * selCols * (n + 1)));
                        Tile tile = sectionsSC.tileset.GetTileById(selectedLayer.tileGrid[i, j]);
                        Drawer tileToDraw = sectionsSC.tileset.GetDrawerFromTileHash(tile.hash);
                        drawer.DrawImage(tileToDraw, dest.j * TS, dest.i * TS, alpha: 0.75f);
                        tileChanges.Add(new TileInstance(dest.i, dest.j, tile.id));
                    }
                }
            }
        }
    }

    public override void OnLeftMouseUp(BaseCanvas baseCanvas)
    {
        base.OnLeftMouseUp(baseCanvas);
        SectionsSC sectionsSC = (baseCanvas as MapCanvas)!.sectionsSC;

        sectionsSC.ResizeMouseUpCommit(tileChanges);
    }
}
