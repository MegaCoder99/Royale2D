using Editor;
using Shared;
using System.Drawing;
using System.Windows.Input;

namespace MapEditor;

// High coupling with SectionsSC. If this poses tangible problems in the future, look into refactor
public class MapCanvas : BaseCanvas
{
    public SectionsSC sectionsSC;

    // "Tool mirroring": both canvases should share the same tool reference for good usability
    // CanvasTool does not take a reference on BaseCanvas for this reason, to make it easy to share/reuse the same tool reference
    private static CanvasTool? sharedTool;
    public override CanvasTool tool 
    {
        get => sharedTool ??= GetDefaultTool();
        set
        {
            sharedTool?.OnDestroy(this);
            sharedTool = value;
            OnToolChange(value);
            sectionsSC.otherSectionsSC!.canvas.OnToolChange(value);
        }
    }

    public MapCanvas(CanvasControl canvasControl, int canvasWidth, int canvasHeight, int tileSize, SectionsSC sectionsSC) :
        base(canvasControl, canvasWidth, canvasHeight, isFixed: false, zoom: 1, mouseWheelScroll: false, tileSize)
    {
        this.sectionsSC = sectionsSC;
    }

    public override CanvasTool GetDefaultTool()
    {
        return new SelectTool();
    }

    public override Drawer? GetEditorImageDrawer()
    {
        return sectionsSC.layerRenderer.layerContainerDrawer;
    }

    public override void DrawToCanvas(Drawer canvasDrawer)
    {
        sectionsSC.DrawToCanvas(canvasDrawer);
    }

    public override void OnKeyDown(Key key)
    {
        sectionsSC.OnKeyDown(key);
    }

    public void DrawGrid(Drawer drawer)
    {
        Color color = Color.FromArgb(164, 255, 0, 0);
        float thickness = 1.0f / zoom;
        if (thickness == 1) thickness = 0.5f;
        for (int j = MyMath.Snap(scrollX, TS); j <= MyMath.Snap(scrollX + canvasWidth, TS); j += TS)
        {
            drawer.DrawLine(j, scrollY, j, scrollY + canvasHeight, color, thickness);
        }
        for (int i = MyMath.Snap(scrollY, TS); i <= MyMath.Snap(scrollY + canvasHeight, TS); i += TS)
        {
            drawer.DrawLine(scrollX, i, scrollX + canvasWidth, i, color, thickness);
        }
    }
}
