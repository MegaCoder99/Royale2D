using Editor;
using Shared;

namespace SpriteEditor;

public partial class State : StateComponent, IEditorState
{
    public SpriteCanvas spriteCanvas;
    public SpritesheetCanvas spritesheetCanvas;
    public Action<Frame> scrollToFrame;
    public SpriteWorkspace workspace;
    public ScriptManager scriptManager;

    public State(EditorContext context, SpriteWorkspace workspace, CanvasControl spriteCanvasControl, CanvasControl spritesheetCanvasControl, Action<Frame> scrollToFrame) : base(context)
    {
        this.workspace = workspace;

        spriteCanvas = new SpriteCanvas(spriteCanvasControl, UISizes.main.spriteCanvasWidth, UISizes.main.spriteCanvasHeight, this);
        spritesheetCanvas = new SpritesheetCanvas(spritesheetCanvasControl, UISizes.main.spritesheetCanvasWidth, UISizes.main.spritesheetCanvasHeight, this);

        this.scrollToFrame = scrollToFrame;

        spritesheets = new(workspace.spritesheets.Select(s => new Spritesheet(s.name, s.filePath)));
        spritesheetNames = new(spritesheets.Select(s => s.name));
        sprites = new(workspace.sprites.Select(s => new Sprite(context, s)));

        if (sprites.Count == 0)
        {
            // Create a default sprite if none exist (to prevent errors in the editor)
            SpriteModel initialSprite = SpriteModel.New("first_sprite", spritesheets[0].name);
            workspace.SaveSprite(initialSprite);
            sprites.Add(new Sprite(context, initialSprite));
        }

        scriptManager = new(this);

        // CONFIG BOOKMARK (LOAD)

        spriteFilterText = WorkspaceConfig.main.lastSpriteFilter;
        boxTagFilter = WorkspaceConfig.main.lastBoxTagFilter;

        Sprite? lastSelectedSprite = sprites.FirstOrDefault(s => s.name == WorkspaceConfig.main.lastSpriteName);

        selectedSprite = lastSelectedSprite ?? sprites.First();
    }

    public void RunScript(string script)
    {
        scriptManager.RunScript(script);
    }

    public void Redraw(RedrawData redrawData)
    {
        if (redrawData == RedrawData.Sprite || redrawData == RedrawData.All)
        {
            spriteCanvas.InvalidateImage();
        }

        if (redrawData == RedrawData.Spritesheet || redrawData == RedrawData.All)
        {
            // NOTE: we can't have this code in SpriteCanvasRepaint because at that point in the execution order, it's too late and would require another spritesheetCanvas.Redraw() to refresh
            if (selectedSpritesheet != null)
            {
                // If image size changed, resize the canvas and reset scroll and zoom
                if (spritesheetCanvas.ResizeTotal(selectedSpritesheet.drawer.width, selectedSpritesheet.drawer.height))
                {
                    spritesheetCanvas.ChangeScrollPos(0, 0);
                    spritesheetCanvas.ChangeZoom(2);
                }
            }

            spritesheetCanvas.InvalidateImage();
        }
    }

    public void SetDirty(DirtyFlag dirtyFlag)
    {
        selectedSprite.isDirty = true;
    }

    public void RedrawCommit(Action commitAction)
    {
        context.ApplyCodeCommit(RedrawData.All, [], commitAction);
    }

    public void RedrawDirtyCommit(Action commitAction)
    {
        context.ApplyCodeCommit(RedrawData.All, DirtyFlag.Default, commitAction);
    }

    public void EditorEventHandler(EditorEvent editorEvent, StateComponent firer)
    {
        if (editorEvent == EditorEvent.SESelectedFrameChange)
        {
            OnPropertyChanged(nameof(canRecomputeSelectedFrame));
        }
    }

    public void SaveAll()
    {
        foreach (Sprite sprite in sprites)
        {
            if (sprite.isDirty)
            {
                workspace.SaveSprite(sprite.ToModel());
                sprite.isDirty = false;
            }
        }
        OnPropertyChanged(nameof(canSave));
        OnPropertyChanged(nameof(canSaveAll));
    }

    public void ForceSaveAll()
    {
        foreach (Sprite sprite in sprites)
        {
            workspace.SaveSprite(sprite.ToModel());
            sprite.isDirty = false;
        }
        OnPropertyChanged(nameof(canSave));
        OnPropertyChanged(nameof(canSaveAll));
        undoManager?.Nuke();
    }

    public void ReloadSpritesheet(Spritesheet spritesheet)
    {
        spritesheet.Reload();
        Redraw(RedrawData.All);
    }

    public void AddSpriteCommit(string spriteName, string spritesheetName)
    {
        // This code is intentionally not part of the RedrawCommit. Any operation that would result in a new file being created on disk when saved, will not be added to undo stack.
        // Instead it is just saved immediately to disk when added. See readme patterns for more info
        Sprite newSprite = new(context, spriteName, spritesheetName);
        sprites.Add(newSprite);
        workspace.SaveSprite(newSprite.ToModel());

        // The only things we add to undo stack are selection and filter changes that support switching to the new sprite created
        RedrawCommit(() =>
        {
            selectedSprite = newSprite;
            // We want to select the sprite that was created, and have the list box snap/scroll to it. To ensure this, we must clear out the filter if it's set.
            if (spriteFilterText.IsSet())
            {
                spriteFilterText = "";
            }
        });
    }

    public void AddGhostCommit()
    {
        if (selectedSprite.selectedFrame != null)
        {
            RedrawCommit(() =>
            {
                ghost = new Ghost(selectedSprite, selectedSprite.selectedFrame);
            });
        }
    }

    public void RemoveGhostCommit()
    {
        if (ghost == null) return;
        RedrawCommit(() =>
        {
            ghost = null;
        });
    }

    public void AddPOICommit(int x, int y)
    {
        if (selectedFrame == null) return;
        RedrawDirtyCommit(() =>
        {
            POI poi = new POI(context, "", x, y);
            selectedFrame.POIs.Add(poi);
            selection = poi;
        });
    }

    public List<Hitbox> GetVisibleHitboxes()
    {
        List<Hitbox> hitboxes = new List<Hitbox>();
        if (selectedSprite != null)
        {
            hitboxes.AddRange(selectedSprite.hitboxes);
        }
        if (selectedFrame != null)
        {
            hitboxes.AddRange(selectedFrame.hitboxes);
        }
        return hitboxes;
    }

    public void AddGlobalHitboxCommit()
    {
        RedrawDirtyCommit(() =>
        {
            Hitbox hitbox = new(context);
            selectedSprite.hitboxes.Add(hitbox);
        });
    }

    public void SelectGlobalHitboxCommit(Hitbox hitbox)
    {
        RedrawDirtyCommit(() =>
        {
            selection = hitbox;
        });
    }

    public void RemoveGlobalHitboxCommit(Hitbox hitbox)
    {
        RedrawDirtyCommit(() =>
        {
            selectedSprite.hitboxes.Remove(hitbox);
        });
    }

    public void AddFrameHitboxCommit()
    {
        if (selectedFrame == null) return;
        RedrawDirtyCommit(() =>
        {
            Hitbox hitbox = new(context);
            selectedFrame.hitboxes.Add(hitbox);
        });
    }

    public void SelectFrameHitboxCommit(Hitbox hitbox)
    {
        RedrawDirtyCommit(() =>
        {
            selection = hitbox;
        });
    }

    public void RemoveFrameHitboxCommit(Hitbox hitbox)
    {
        if (selectedFrame == null) return;
        RedrawDirtyCommit(() =>
        {
            selectedFrame.hitboxes.Remove(hitbox);
        });
    }

    public void AddFrameDrawboxCommit(string spritesheetName, MyRect rect)
    {
        if (selectedFrame == null) return;
        RedrawDirtyCommit(() =>
        {
            Drawbox drawbox = new(context, spritesheetName, rect);
            selectedFrame.drawboxes.Add(drawbox);
            selection = drawbox;
        });
    }

    public void EditFrameDrawboxCommit(Drawbox drawbox, string spritesheetName, MyRect rect)
    {
        RedrawDirtyCommit(() =>
        {
            drawbox.spritesheetName = spritesheetName;
            drawbox.rect = new RectSC(context, rect);
        });
    }

    public void SelectFrameDrawboxCommit(Drawbox drawbox)
    {
        if (selection == drawbox) return;
        RedrawDirtyCommit(() =>
        {
            selection = drawbox;
        });
    }

    public void CopyToAllFramesDrawboxCommit(Drawbox drawbox)
    {
        RedrawDirtyCommit(() =>
        {
            bool found = false;
            foreach (Frame frame in selectedSprite.frames)
            {
                if (!frame.drawboxes.Any(d => d.spritesheetName == drawbox.spritesheetName && d.rect.EqualTo(drawbox.rect)))
                {
                    found = true;
                    frame.drawboxes.Add(new Drawbox(context, drawbox.ToModel()));
                }
            }
            if (!found)
            {
                Prompt.ShowError("Drawbox already exists in all frames");
            }
        });
    }

    public void RemoveFrameDrawboxCommit(Drawbox drawbox)
    {
        if (selectedFrame == null) return;
        RedrawDirtyCommit(() =>
        {
            selectedFrame.drawboxes.Remove(drawbox);
        });
    }

    public void SelectPOICommit(POI poi)
    {
        RedrawCommit(() =>
        {
            selection = poi;
        });
    }

    public void RemovePOICommit(POI poi)
    {
        if (selectedFrame == null) return;
        RedrawDirtyCommit(() =>
        {
            selectedFrame.POIs.Remove(poi);
        });
    }

    public bool IsSelectedFrameAdded()
    {
        if (selectedFrame == null) return false;
        return selectedSprite.frames.Contains(selectedFrame);
    }

    public void AddPendingFrameCommit()
    {
        if (pendingFrame == null) return;

        RedrawDirtyCommit(() =>
        {
            if (selectedSprite.frames.Count > 0) pendingFrame.duration = selectedSprite.frames.Last().duration;
            selectedSprite.selectedFrame = new Frame(context, pendingFrame.ToModel());
            selectedSprite.frames.Add(selectedSprite.selectedFrame);
        });
    }

    public void ReplaceWithPendingFrameCommit()
    {
        if (selectedFrame == null || pendingFrame == null) return;

        RedrawDirtyCommit(() =>
        {
            pendingFrame.duration = selectedFrame.duration;
            int selectedFrameIndex = selectedSprite.frames.IndexOf(selectedFrame);
            selectedSprite.frames[selectedFrameIndex] = pendingFrame;
            selectedSprite.selectedFrame = pendingFrame;
        });
    }

    public void RecomputeSelectedFrameCommit()
    {
        if (selectedFrame == null) return;
        RedrawDirtyCommit(() =>
        {
            MyPoint center = selectedFrame.rect.ToModel().Center();
            MyRect? newRect = selectedSpritesheet.GetPixelClumpRect(center.x, center.y);
            if (newRect == null) return;
            selectedFrame.rect = new RectSC(context, newRect.Value);
        });
    }

    public void SelectFrameCommit(Frame frame)
    {
        if (selectedFrame != frame && selectedFrame != null)
        {
            RedrawCommit(() =>
            {
                selectedSprite.selectedFrame = frame;
                // If you wish to uncomment this, make sure it doesn't happen if the screen's width is reduced and there is a horizontal scrollbar.
                // Otherwise, it will snap focus to the list which is undesirable as we want to maintain the top-level horizontal scroll position
                //scrollToFrame.Invoke(selectedFrame);
            });
        }
    }

    public void CopyFrameCommit(Frame frame, int dir)
    {
        var index = selectedSprite.frames.IndexOf(frame);
        if (dir == -1) dir = 0;
        selectedSprite.frames.Insert(index + dir, new Frame(context, frame.ToModel()));
    }

    public void MoveFrameCommit(Frame frame, int dir)
    {
        var index = selectedSprite.frames.IndexOf(frame);
        if (index + dir < 0 || index + dir >= selectedSprite.frames.Count) return;
        var temp = selectedSprite.frames[index];
        selectedSprite.frames[index] = selectedSprite.frames[index + dir];
        selectedSprite.frames[index + dir] = temp;
    }

    public void RemoveFrameCommit(Frame frame)
    {
        RedrawDirtyCommit(() =>
        {
            selectedSprite.frames.Remove(frame);
            selectedSprite.selectedFrame = selectedSprite.frames[0];
        });
    }

    public void SelectNextFrameCommit()
    {
        if (selectedFrame == null) return;
        selection = null;
        var frameIndex = selectedSprite.frames.IndexOf(selectedFrame);
        var selFrame = frameIndex < selectedSprite.frames.Count - 1 ? selectedSprite.frames[frameIndex + 1] : null;
        if (selFrame == null) selFrame = selectedSprite.frames[0];
        SelectFrameCommit(selFrame);
    }

    public void SelectPrevFrameCommit()
    {
        if (selectedFrame == null) return;
        selection = null;
        var frameIndex = selectedSprite.frames.IndexOf(selectedFrame);
        var selFrame = frameIndex > 0 ? selectedSprite.frames[frameIndex - 1] : null;
        if (selFrame == null) selFrame = selectedSprite.frames[selectedSprite.frames.Count - 1];
        SelectFrameCommit(selFrame);
    }

    public void ReverseFrames()
    {
        RedrawDirtyCommit(() =>
        {
            selectedSprite.frames.Reverse();
        });
    }

    public void ApplyBulkDurationCommit()
    {
        RedrawDirtyCommit(() =>
        {
            foreach (Frame frame in selectedSprite.frames)
            {
                frame.duration = bulkDuration;
                frame.OnPropertyChanged(nameof(frame.duration));
            }
        });
    }

    public void MoveFrameUpCommit(Frame frame)
    {
        RedrawDirtyCommit(() =>
        {
            selectedSprite.selectedFrame = frame;
            int index = selectedSprite.frames.IndexOf(frame);
            if (index > 0)
            {
                Frame temp = selectedSprite.frames[index];
                selectedSprite.frames[index] = selectedSprite.frames[index - 1];
                selectedSprite.frames[index - 1] = temp;
            }
        });
    }

    public void MoveFrameDownCommit(Frame frame)
    {
        RedrawDirtyCommit(() =>
        {
            selectedSprite.selectedFrame = frame;
            int index = selectedSprite.frames.IndexOf(frame);
            if (index < selectedSprite.frames.Count - 1)
            {
                Frame temp = selectedSprite.frames[index];
                selectedSprite.frames[index] = selectedSprite.frames[index + 1];
                selectedSprite.frames[index + 1] = temp;
            }
        });
    }

    public void DeleteFrameCommit(Frame frame)
    {
        RedrawDirtyCommit(() =>
        {
            int? frameIndexToChangeTo = null;
            if (selectedSprite.selectedFrame == frame)
            {
                frameIndexToChangeTo = selectedSprite.frames.IndexOf(selectedSprite.selectedFrame);
            }
            selectedSprite.frames.Remove(frame);
            if (frameIndexToChangeTo != null)
            {
                if (frameIndexToChangeTo >= selectedSprite.frames.Count) frameIndexToChangeTo = selectedSprite.frames.Count - 1;
                selectedSprite.selectedFrame = selectedSprite.frames.SafeGet(frameIndexToChangeTo.Value);
            }
        });
    }
}
