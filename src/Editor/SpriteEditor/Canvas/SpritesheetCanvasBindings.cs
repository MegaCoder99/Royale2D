using Editor;
using Shared;
using System.Drawing;
using System.Windows.Input;

namespace SpriteEditor;

// Right now the methods being bound to the canvas are in a partial class instead of being in SpritesheetCanvas so we avoid a zillion "state.<foo>" references
public partial class State
{
    public void SpritesheetCanvasRepaint(Drawer drawer)
    {
        if (selectedSpritesheet == null) return;

        drawer.DrawRect(new MyRect(0, 0, selectedSpritesheet.drawer.width, selectedSpritesheet.drawer.height), Color.White, null, 0);
        drawer.DrawImage(selectedSpritesheet.drawer, 0, 0);

        if (selectedSprite != null)
        {
            var i = 0;
            foreach (Frame frame in selectedSprite.frames)
            {
                drawer.DrawRect(frame.rect, null, Color.Blue, 4.0f / spriteCanvas.zoom);
                drawer.DrawText(i.ToString(), frame.rect.x1, frame.rect.y1, Color.Red, Color.Black, 12);
                i++;
            }
        }

        if (selectedFrame != null)
        {
            drawer.DrawRect(selectedFrame.rect.ToModel().AddSize(1), null, Color.Green, 4.0f / spriteCanvas.zoom);
        }

        if (pendingFrame != null)
        {
            drawer.DrawRect(pendingFrame.rect, null, Color.Orange, 4.0f / spriteCanvas.zoom);
        }
    }

    public void SpritesheetCanvasKeyDown(Key key)
    {
        if (pendingFrame != null && key == Key.F)
        {
            AddPendingFrameCommit();
        }

        if (selectedFrame != null)
        {
            if (key == Key.R)
            {
                ReplaceWithPendingFrameCommit();
            }
            if (key == Key.P)
            {
                // RecomputeSelectedFrameCommit();
            }
        }
    }

    public void SpritesheetCanvasLeftMouseDown(double mouseX, double mouseY)
    {
    }

    public void SpritesheetCanvasLeftMouseUp(int mouseX, int mouseY)
    {
        if (selectedSprite == null || selectedSpritesheet == null) return;

        RedrawCommit(() =>
        {
            int topLeftX = (int)Math.Min(spritesheetCanvas.lastClickedMouseX, mouseX);
            int topLeftY = (int)Math.Min(spritesheetCanvas.lastClickedMouseY, mouseY);
            int botRightX = (int)Math.Max(spritesheetCanvas.lastClickedMouseX, mouseX);
            int botRightY = (int)Math.Max(spritesheetCanvas.lastClickedMouseY, mouseY);

            // This block selects a single contiguous blob if all pixels in selected drag rect are non-empty
            for (int i = topLeftY; i <= botRightY; i++)
            {
                for (int j = topLeftX; j <= botRightX; j++)
                {
                    if (selectedSpritesheet.imgPixelGrid.InRange(i, j) && selectedSpritesheet.imgPixelGrid[i, j].rgb.A != 0)
                    {
                        MyRect? rect = selectedSpritesheet.GetPixelClumpRect(mouseX, mouseY);
                        if (rect != null)
                        {
                            pendingFrame = new Frame(context, rect.Value, 10, new MyPoint(0, 0));
                            return;
                        }
                    }
                }
            }

            // This block selects minimum surrounding rect if you drag select multiple separate "islands"
            MyRect? rect2 = selectedSpritesheet.GetSelectedPixelRect(topLeftX, topLeftY, botRightX, botRightY);
            if (rect2 != null)
            {
                pendingFrame = new Frame(context, rect2.Value, 10, new MyPoint(0, 0));
            }
        });
    }
}
