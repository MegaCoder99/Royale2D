using Shared;
using System.Drawing;

namespace Editor;

public class SelectTool : CanvasTool
{
    public SelectTool() : base(false)
    {
    }

    public override void OnRedraw(BaseCanvas baseCanvas, Drawer drawer)
    {
        if (baseCanvas.isLeftMouseDown)
        {
            var (relLastClickedMouseX, relLastClickedMouseY) = baseCanvas.ConvertAbsoluteToViewport((int)baseCanvas.lastClickedMouseX, (int)baseCanvas.lastClickedMouseY);
            var (relMouseX, relMouseY) = baseCanvas.ConvertAbsoluteToViewport(baseCanvas.mouseXInt, baseCanvas.mouseYInt);
            var rect = new MyRect(
                Math.Min(relMouseX, relLastClickedMouseX),
                Math.Min(relMouseY, relLastClickedMouseY),
                Math.Max(relMouseX, relLastClickedMouseX),
                Math.Max(relMouseY, relLastClickedMouseY)
            );
            if (rect.w > 2 || rect.h > 2)
            {
                drawer.DrawRect(rect, null, Color.FromArgb(255, 0, 0, 255), 1);
            }
        }
    }

    public override void OnMouseMove(BaseCanvas baseCanvas)
    {
        if (baseCanvas.isLeftMouseDown)
        {
            baseCanvas.InvalidateImage();
        }
    }

    public override void OnLeftMouseUp(BaseCanvas baseCanvas)
    {
        baseCanvas.InvalidateImage();
    }

    public ResizeDir? GetResizeDirFromRect(BaseCanvas baseCanvas, MyRect rect)
    {
        double mouseX = baseCanvas.mouseX;
        double mouseY = baseCanvas.mouseY;
        double threshold = 1;
        if (baseCanvas.zoom > 1) threshold = 1.5 / baseCanvas.zoom;

        bool nearLeft = Math.Abs(mouseX - rect.x1) <= threshold;
        bool nearRight = Math.Abs(mouseX - rect.x2) <= threshold;
        bool nearTop = Math.Abs(mouseY - rect.y1) <= threshold;
        bool nearBottom = Math.Abs(mouseY - rect.y2) <= threshold;

        if (nearLeft && nearTop) return ResizeDirs.TopLeft;
        if (nearRight && nearTop) return ResizeDirs.TopRight;
        if (nearLeft && nearBottom) return ResizeDirs.BottomLeft;
        if (nearRight && nearBottom) return ResizeDirs.BottomRight;
        if (nearTop && mouseX >= rect.x1 - threshold && mouseX <= rect.x2 + threshold) return ResizeDirs.Top;
        if (nearBottom && mouseX >= rect.x1 - threshold && mouseX <= rect.x2 + threshold) return ResizeDirs.Bottom;
        if (nearLeft && mouseY >= rect.y1 - threshold && mouseY <= rect.y2 + threshold) return ResizeDirs.Left;
        if (nearRight && mouseY >= rect.y1 - threshold && mouseY <= rect.y2 + threshold) return ResizeDirs.Right;

        return null;
    }
}
