using Editor;
using Shared;
using System.Windows.Input;

namespace SpriteEditor;

public class SpriteCanvas : BaseCanvas
{
    State state;
    public SpriteCanvas(CanvasControl canvasControl, int canvasWidth, int canvasHeight, State state) 
        : base(canvasControl, canvasWidth, canvasHeight, isFixed: true, zoom: 5, mouseWheelScroll: false, tileSize: 8)
    {
        this.state = state;
        AnchorScroll();
    }

    public override MyPoint GetAnchorPoint()
    {
        return new MyPoint(totalWidth / 2, totalHeight / 2);
    }

    public override void OnKeyDown(Key key)
    {
        state.SpriteCanvasKeyDown(key);
    }

    public override void OnLeftMouseDown()
    {
        state.SpriteCanvasLeftMouseDown(mouseX, mouseY);
    }

    public override void DrawToCanvas(Drawer canvasDrawer)
    {
        state.SpriteCanvasRepaint(canvasDrawer);
    }
}
