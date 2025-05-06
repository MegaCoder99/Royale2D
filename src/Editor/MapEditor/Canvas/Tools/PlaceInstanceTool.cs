using Editor;
using Shared;

namespace MapEditor;

public class PlaceInstanceTool : PlaceTool
{
    public PlaceInstanceTool(Drawer mouseImageDrawer) : base(mouseImageDrawer, true)
    {
    }

    public override MyPoint GetDrawPos(BaseCanvas baseCanvas)
    {
        return base.GetDrawPos(baseCanvas).AddXY(-mouseImageDrawer.width / 2, -mouseImageDrawer.height / 2);
    }

    public override void OnLeftMouseDown(BaseCanvas baseCanvas)
    {
        base.OnLeftMouseDown(baseCanvas);
        SectionsSC sectionsSC = (baseCanvas as MapCanvas)!.sectionsSC;

        MyPoint placePos = GetPlacePos(baseCanvas);
        sectionsSC.PlaceInstanceCommit(placePos.x, placePos.y);
    }
}
