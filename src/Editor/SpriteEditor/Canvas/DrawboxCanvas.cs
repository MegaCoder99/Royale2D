using Editor;
using Shared;
using System.Drawing;

namespace SpriteEditor;

public class DrawboxCanvas : BaseCanvas
{
    Spritesheet spritesheet;
    public MyRect? rect;
    int startZoom;

    public DrawboxCanvas(CanvasControl canvasControl, Spritesheet spritesheet, int startZoom)
        : base(
            canvasControl, 
            Math.Min(spritesheet.drawer.width * startZoom, 1024),
            Math.Min(spritesheet.drawer.height * startZoom, 1024), 
            isFixed: false, 
            zoom: startZoom, 
            mouseWheelScroll: true, 
            tileSize: 8)
    {
        this.spritesheet = spritesheet;
        this.startZoom = startZoom;
    }

    public void ChangeSpritesheet(Spritesheet newSpritesheet)
    {
        spritesheet = newSpritesheet;
        rect = null;
        InvalidateImage();

        // If image size changed, resize the canvas and reset scroll and zoom
        if (ResizeTotal(newSpritesheet.drawer.width * startZoom, newSpritesheet.drawer.height * startZoom))
        {
            ChangeScrollPos(0, 0);
            ChangeZoom(2);
        }
    }

    public override MyPoint GetAnchorPoint()
    {
        return new MyPoint(mouseXInt, mouseYInt);
    }

    public override void OnLeftMouseDown()
    {
        MyRect? rect = spritesheet.GetPixelClumpRect((float)mouseX, (float)mouseY);
        if (rect != null)
        {
            this.rect = rect;
        }
    }

    public override void OnLeftMouseUp()
    {
        int topLeftX = (int)Math.Min(lastClickedMouseX, mouseX);
        int topLeftY = (int)Math.Min(lastClickedMouseY, mouseY);
        int botRightX = (int)Math.Max(lastClickedMouseX, mouseX);
        int botRightY = (int)Math.Max(lastClickedMouseY, mouseY);

        for (int i = topLeftY; i <= botRightY; i++)
        {
            for (int j = topLeftX; j <= botRightX; j++)
            {
                if (spritesheet.imgPixelGrid.InRange(i, j) && spritesheet.imgPixelGrid[i, j].rgb.A != 0)
                {
                    MyRect? rect = spritesheet.GetPixelClumpRect(j, i);
                    if (rect == null) continue;
                    this.rect = rect;
                }
            }
        }
    }

    public override void DrawToCanvas(Drawer drawer)
    {
        drawer.DrawRect(new MyRect(0, 0, spritesheet.drawer.width, spritesheet.drawer.height), Color.White, null, 0);
        drawer.DrawImage(spritesheet.drawer, 0, 0);

        if (rect != null)
        {
            drawer.DrawRect(rect.Value, null, Color.Orange, 3.0f / zoom);
        }
    }
}
