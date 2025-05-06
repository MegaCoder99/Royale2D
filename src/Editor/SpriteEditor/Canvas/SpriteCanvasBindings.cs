using Editor;
using Shared;
using System.Drawing;
using System.Windows.Input;

namespace SpriteEditor;

// Right now the methods being bound to the canvas are in a partial class instead of being in SpriteCanvas so we avoid a zillion "state.<foo>" references
public partial class State
{
    public bool AreBoxTagsFiltered(string tags)
    {
        if (boxTagFilter.Unset()) return false;
        return !boxTagFilter.Split(',').Any(tag => tags.Contains(tag));
    }

    public void SpriteCanvasRepaint(Drawer drawer)
    {
        if (selectedSprite == null) return;

        Frame? frame = null;

        if (!MainWindow.isAnimPlaying)
        {
            if (selectedFrame != null && selectedSpritesheet != null && selectedSpritesheet.drawer != null)
            {
                frame = selectedFrame;
            }
        }
        else
        {
            frame = selectedSprite.frames.SafeGet(MainWindow.animFrameIndex);
        }

        if (frame == null) return;

        int cX = spriteCanvas.canvasWidth / 2;
        int cY = spriteCanvas.canvasHeight / 2;

        List<Drawbox> sortedDrawboxes = frame.drawboxes.ToList();
        sortedDrawboxes.Sort((a, b) => a.zIndex.CompareTo(b.zIndex));

        int frameIndex = selectedSprite.frames.IndexOf(frame);
        if (frameIndex >= 0)
        {
            foreach (Drawbox drawbox in sortedDrawboxes)
            {
                if (drawbox.zIndex < 0 && !AreBoxTagsFiltered(drawbox.tags))
                {
                    drawer.DrawImage(drawbox.GetSpritesheet(this).drawer, drawbox.pos.x + cX, drawbox.pos.y + cY, drawbox.rect.x1, drawbox.rect.y1, drawbox.rect.w, drawbox.rect.h);
                }
            }

            selectedSprite.Draw(this, drawer, frameIndex, cX, cY);

            foreach (Drawbox drawbox in sortedDrawboxes)
            {
                if (drawbox.zIndex >= 0 && !AreBoxTagsFiltered(drawbox.tags))
                {
                    drawer.DrawImage(drawbox.GetSpritesheet(this).drawer, drawbox.pos.x + cX, drawbox.pos.y + cY, drawbox.rect.x1, drawbox.rect.y1, drawbox.rect.w, drawbox.rect.h);
                }
            }
        }

        if (ghost != null)
        {
            ghost.sprite.Draw(this, drawer, ghost.sprite.frames.IndexOf(ghost.frame), cX, cY, 0.5f);
        }

        if (!hideGizmos)
        {
            var len = 1000;
            drawer.DrawLine(cX, cY - len, cX, cY + len, Color.Red, 1);
            drawer.DrawLine(cX - len, cY, cX + len, cY, Color.Red, 1);
            drawer.DrawCircle(cX, cY, 1, Color.Red);

            foreach (Drawbox drawbox in sortedDrawboxes)
            {
                if (selection == drawbox)
                {
                    drawer.DrawRect(MyRect.CreateWH(drawbox.pos.x + cX, drawbox.pos.y + cY, drawbox.rect.w, drawbox.rect.h), null, Color.Green, 2.0f / spriteCanvas.zoom);
                }
            }

            foreach (Hitbox hitbox in GetVisibleHitboxes())
            {
                if (AreBoxTagsFiltered(hitbox.tags)) continue;

                MyRect hitboxRect = hitbox.rect.ToModel().AddXY(cX, cY);

                Color? strokeColor = null;
                float strokeWidth = 0;
                if (selection == hitbox)
                {
                    strokeColor = Color.Blue;
                    strokeWidth = 3;
                }

                drawer.DrawRect(hitboxRect, Color.Blue, strokeColor, strokeWidth / spriteCanvas.zoom, 0.25f);
            }

            foreach (POI poi in frame.POIs)
            {
                drawer.DrawCircle(cX + poi.x, cY + poi.y, 1, Color.Green);
                if (selection == poi)
                {
                    drawer.DrawCircle(cX + poi.x, cY + poi.y, 2f, null, Color.FromArgb(128, 0, 255, 0), 1f);
                }
            }
        }
    }

    public void SpriteCanvasKeyDown(Key key)
    {
        if (selectedSprite == null || selectedFrame == null) return;

        if (key == Key.System && !addPOIMode)
        {
            addPOIMode = true;
            RedrawCommit(() => 
            {
                if (selection != null)
                {
                    selection = null;
                }
            });
            return;
        }

        // Move the selected frame or hitbox
        int moveX = 0;
        int moveY = 0;
        if (key == Key.W) moveY = -1;
        else if (key == Key.S) moveY = 1;
        if (key == Key.A) moveX = -1;
        else if (key == Key.D) moveX = 1;
        if (Helpers.ShiftHeld())
        {
            moveX *= 10;
            moveY *= 10;
        }
        if (moveX != 0 || moveY != 0)
        {
            RedrawDirtyCommit(() =>
            {
                if (selection != null)
                {
                    selection.Move(moveX, moveY);
                }
                else
                {
                    selectedFrame.Move(moveX, moveY);
                }
            });
            return;
        }

        // Resize the selected hitbox
        int resizeX = 0;
        int resizeY = 0;
        if (key == Key.Left) resizeX = -1;
        else if (key == Key.Right) resizeX = 1;
        if (key == Key.Up) resizeY = -1;
        else if (key == Key.Down) resizeY = 1;
        bool resizeBotRight = false;
        if (Helpers.ShiftHeld())
        {
            resizeBotRight = true;
        }
        if ((resizeX != 0 || resizeY != 0) && selection is Hitbox hitbox)
        {
            if (!resizeBotRight)
            {
                RedrawDirtyCommit(() =>
                {
                    hitbox.rect.x1 += resizeX;
                    hitbox.rect.y1 += resizeY;
                });
            }
            else
            {
                RedrawDirtyCommit(() =>
                {
                    hitbox.rect.x2 += resizeX;
                    hitbox.rect.y2 += resizeY;
                });
            }
            return;
        }

        if (key == Key.G)
        {
            AddGhostCommit();
            return;
        }

        if (key == Key.Escape)
        {
            if (addPOIMode)
            {
                addPOIMode = false;
                Redraw(RedrawData.Sprite);
            }
            else if (selection != null)
            {
                RedrawCommit(() =>
                {
                    selection = null;
                });
            }
            else if (ghost != null)
            {
                RemoveGhostCommit();
                return;
            }
        }
    }

    public void SpriteCanvasLeftMouseDown(double mouseX, double mouseY)
    {
        int posAtMouseX = (int)mouseX - spriteCanvas.canvasWidth / 2;
        int posAtMouseY = (int)mouseY - spriteCanvas.canvasHeight / 2;

        if (addPOIMode)
        {
            addPOIMode = false;
            AddPOICommit(posAtMouseX, posAtMouseY);
            return;
        }

        // Check if we clicked a POI
        foreach (POI poi in selectedFrame?.POIs ?? [])
        {
            if (poi.GetRect().Contains(posAtMouseX, posAtMouseY))
            {
                RedrawCommit(() =>
                {
                    if (selection != poi)
                    {
                        selection = poi;
                    }
                });
                return;
            }
        }

        RedrawCommit(() =>
        {
            // Check if we clicked a hitbox
            foreach (Hitbox hitbox in GetVisibleHitboxes())
            {
                if (AreBoxTagsFiltered(hitbox.tags)) continue;

                MyRect hitboxRect = hitbox.rect.ToModel();
                if (hitboxRect.Contains(posAtMouseX, posAtMouseY))
                {
                    if (selection != hitbox)
                    {
                        selection = hitbox;
                        return;
                    }
                }
            }

            // Check if we clicked a drawbox
            foreach (Drawbox drawbox in selectedFrame?.drawboxes ?? [])
            {
                if (AreBoxTagsFiltered(drawbox.tags)) continue;

                MyRect drawboxRect = drawbox.GetFrameRect();
                if (drawboxRect.Contains(posAtMouseX, posAtMouseY))
                {
                    if (selection != drawbox)
                    {
                        selection = drawbox;
                        return;
                    }
                }
            }

            if (selection != null)
            {
                selection = null;
            }
        });
    }
}
