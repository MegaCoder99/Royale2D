using Editor;

namespace MapEditor;

public class FillTileTool : CanvasTool
{
    int tileIdToFillWith;
    public FillTileTool(int tileIdToFillWith) : base(true)
    {
        this.tileIdToFillWith = tileIdToFillWith;
        cursor = Resources.BucketCursor;
    }

    public override void OnLeftMouseDown(BaseCanvas baseCanvas)
    {
        base.OnLeftMouseDown(baseCanvas);
        SectionsSC sectionsSC = (baseCanvas as MapCanvas)!.sectionsSC;

        sectionsSC.FillTilesCommit(baseCanvas.mouseI, baseCanvas.mouseJ, tileIdToFillWith);
    }
}
