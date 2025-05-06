using Editor;
using Shared;
using System.Windows.Input;

namespace SpriteEditor;

public class SpritesheetCanvas : BaseCanvas
{
    State state;
    public SpritesheetCanvas(CanvasControl canvasControl, int canvasWidth, int canvasHeight, State state)
        : base(canvasControl, canvasWidth, canvasHeight, isFixed: false, zoom: 2, mouseWheelScroll: true, tileSize: 8)
    {
        this.state = state;
    }

    public override MyPoint GetAnchorPoint()
    {
        return new MyPoint(mouseXInt, mouseYInt);
    }

    public override void OnKeyDown(Key key)
    {
        state.SpritesheetCanvasKeyDown(key);
    }

    public override void OnLeftMouseDown()
    {
        state.SpritesheetCanvasLeftMouseDown(mouseX, mouseY);
    }

    public override void OnLeftMouseUp()
    {
        state.SpritesheetCanvasLeftMouseUp(mouseXInt, mouseYInt);
    }

    public override void DrawToCanvas(Drawer canvasDrawer)
    {
        state.SpritesheetCanvasRepaint(canvasDrawer);
    }
}
