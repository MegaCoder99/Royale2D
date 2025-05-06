using Editor;
using Shared;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace MapEditor;

// Contains state component code related to the "Edit Tiles" editor mode, focused on hotkeys that manipulate tile/tileset-related data
public partial class SectionsSC
{
    public void AddEditTileDataModeHotkeys(HotkeyManager hotkeyManager)
    {
        hotkeyManager.AddHotkeys([
            new HotkeyConfig(Key.S, () => ChangeSelectedTileHitboxCommit(TileHitboxMode.FullTile)),
            new HotkeyConfig(Key.D, () => ChangeSelectedTileHitboxCommit(TileHitboxMode.None)),
            new HotkeyConfig(Key.A, () => ChangeSelectedTileAboveIdIsSameCommit(true, null)),
            new HotkeyConfig(Key.Z, () => ChangeSelectedTileAboveIdIsSameCommit(false, null)),
            new HotkeyConfig(Key.OemPlus, () => AddTileVariationCommit()),
            new HotkeyConfig(Key.N, () => state.tileAnimationSC.AddTileAnimationCommit()),
            new HotkeyConfig(Key.T, () => state.tileClumpSC.AddTileClumpCommit()),

            new HotkeyConfig(Key.NumPad5, () => ChangeSelectedTileHitboxCommit(TileHitboxMode.FullTile)),
            new HotkeyConfig(Key.Decimal, () => ChangeSelectedTileHitboxCommit(TileHitboxMode.None)),
            new HotkeyConfig(Key.NumPad0, () => ChangeSelectedTileHitboxCommit(TileHitboxMode.None)),
            new HotkeyConfig(Key.NumPad7, () => ChangeSelectedTileHitboxCommit(TileHitboxMode.DiagTopLeft)),
            new HotkeyConfig(Key.NumPad9, () => ChangeSelectedTileHitboxCommit(TileHitboxMode.DiagTopRight)),
            new HotkeyConfig(Key.NumPad1, () => ChangeSelectedTileHitboxCommit(TileHitboxMode.DiagBotLeft)),
            new HotkeyConfig(Key.NumPad3, () => ChangeSelectedTileHitboxCommit(TileHitboxMode.DiagBotRight)),
        ], MapEditorMode.EditTileData);

        // IMPROVE extended hitbox feature
        /*
        hotkeyManager.AddHotkeys([
            new HotkeyConfig(Key.NumPad8, () => ChangeSelectedTileHitboxCommit(TileHitboxMode.BoxTop)),
            new HotkeyConfig(Key.NumPad4, () => ChangeSelectedTileHitboxCommit(TileHitboxMode.BoxLeft)),
            new HotkeyConfig(Key.NumPad6, () => ChangeSelectedTileHitboxCommit(TileHitboxMode.BoxRight)),
            new HotkeyConfig(Key.NumPad2, () => ChangeSelectedTileHitboxCommit(TileHitboxMode.BoxBot)),

            // IMPROVE have toggle for each of these numpad diagonal sets
            new HotkeyConfig(Key.NumPad7, () => ChangeSelectedTileHitboxCommit(TileHitboxMode.BoxTopLeft)),
            new HotkeyConfig(Key.NumPad9, () => ChangeSelectedTileHitboxCommit(TileHitboxMode.BoxTopRight)),
            new HotkeyConfig(Key.NumPad1, () => ChangeSelectedTileHitboxCommit(TileHitboxMode.BoxBotLeft)),
            new HotkeyConfig(Key.NumPad3, () => ChangeSelectedTileHitboxCommit(TileHitboxMode.BoxBotRight)),

            new HotkeyConfig(Key.NumPad7, () => ChangeSelectedTileHitboxCommit(TileHitboxMode.SmallDiagTopLeft)),
            new HotkeyConfig(Key.NumPad9, () => ChangeSelectedTileHitboxCommit(TileHitboxMode.SmallDiagTopRight)),
            new HotkeyConfig(Key.NumPad1, () => ChangeSelectedTileHitboxCommit(TileHitboxMode.SmallDiagBotLeft)),
            new HotkeyConfig(Key.NumPad3, () => ChangeSelectedTileHitboxCommit(TileHitboxMode.SmallDiagBotRight)),

            new HotkeyConfig(Key.NumPad7, () => ChangeSelectedTileHitboxCommit(TileHitboxMode.LargeDiagTopLeft)),
            new HotkeyConfig(Key.NumPad9, () => ChangeSelectedTileHitboxCommit(TileHitboxMode.LargeDiagTopRight)),
            new HotkeyConfig(Key.NumPad1, () => ChangeSelectedTileHitboxCommit(TileHitboxMode.LargeDiagBotLeft)),
            new HotkeyConfig(Key.NumPad3, () => ChangeSelectedTileHitboxCommit(TileHitboxMode.LargeDiagBotRight))
            
        ], MapEditorMode.EditTileData);
        */

    }

    public List<Tile> GetSelectedClonedTiles()
    {
        List<Tile> selectedTiles = [];
        foreach (GridCoords selectedTileCoord in selectedTileCoords)
        {
            int selectedTileId = GetTopTileId(selectedTileCoord.i, selectedTileCoord.j);
            if (selectedTileId == Tile.TransparentTileId) continue;
            Tile selectedTile = state.tileset.GetClonedTileById(selectedTileId);
            selectedTiles.Add(selectedTile);
        }

        return selectedTiles;
    }

    public void ChangeSelectedTileHitboxCommit(TileHitboxMode newHitbox)
    {
        List<Tile> selectedClonedTiles = GetSelectedClonedTiles();
        if (selectedClonedTiles.Count == 0)
        {
            Prompt.ShowError("No tiles selected.");
            return;
        }
        context.ApplyCodeCommit(RedrawData.ToolingAll, DirtyFlag.Tile, () =>
        {
            List<Tile> newTile = [];
            foreach (Tile selectedTile in selectedClonedTiles)
            {
                if (selectedTile.hitboxMode != newHitbox)
                {
                    selectedTile.hitboxMode = newHitbox;
                    newTile.Add(selectedTile);
                }
            }
            if (state.tilesetSC.ChangeTile(newTile))
            {
                state.showTileHitboxes = true;
            }
        });
    }

    public void ChangeSelectedTileTagsCommit(string newTag, Action additionalAction)
    {
        List<Tile> selectedClonedTiles = GetSelectedClonedTiles();
        if (selectedClonedTiles.Count == 0)
        {
            Prompt.ShowError("No tiles selected.");
            return;
        }
        context.ApplyCodeCommit(RedrawData.ToolingAll, DirtyFlag.Tile, () =>
        {
            List<Tile> newTiles = [];
            foreach (Tile selectedTile in selectedClonedTiles)
            {
                if (selectedTile.tags != newTag)
                {
                    selectedTile.tags = newTag;
                    newTiles.Add(selectedTile);
                }
            }
            if (state.tilesetSC.ChangeTile(newTiles))
            {
                additionalAction.Invoke();
            }
        });
    }

    public void ChangeColorMaskCommit(string colorMask, Action additionalAction)
    {
        List<Tile> selectedClonedTiles = GetSelectedClonedTiles();
        if (selectedClonedTiles.Count == 0)
        {
            Prompt.ShowError("No tiles selected.");
            return;
        }
        else if (colorMask.IsSet() && !Regex.IsMatch(colorMask, "^#?[A-Fa-f0-9]{6}$"))
        {
            Prompt.ShowError("Must be a valid hex color.", "Validation Error");
            return;
        }

        context.ApplyCodeCommit(RedrawData.ToolingAll, DirtyFlag.Tile, () =>
        {
            List<Tile> newTiles = [];
            foreach (Tile selectedTile in selectedClonedTiles)
            {
                if (selectedTile.zIndexMaskColor != colorMask)
                {
                    selectedTile.zIndexMaskColor = colorMask;
                    if (colorMask.IsSet())
                    {
                        selectedTile.tileAboveId = null;
                        selectedTile.tileAboveIdIsSame = false;
                    }
                    newTiles.Add(selectedTile);
                }
            }
            if (state.tilesetSC.ChangeTile(newTiles))
            {
                additionalAction.Invoke();
                state.showTileZIndices = true;
            }
        });
    }

    public void ChangeSelectedTileAboveIdCommit(int? tileAboveId, Action additionalAction)
    {
        List<Tile> selectedClonedTiles = GetSelectedClonedTiles();
        if (selectedClonedTiles.Count == 0)
        {
            Prompt.ShowError("No tiles selected.");
            return;
        }
        else if (tileAboveId != null && !tileset.idToTile.ContainsKey(tileAboveId.Value))
        {
            Prompt.ShowError($"Tile ID \"{tileAboveId}\" does not exist.", "Validation error");
            return;
        }
        else if (tileAboveId == Tile.TransparentTileId)
        {
            Prompt.ShowError("Cannot set tile above to the empty (transparent) tile.", "Validation error");
            return;
        }

        context.ApplyCodeCommit(RedrawData.ToolingAll, DirtyFlag.Tile, () =>
        {
            List<Tile> newTiles = [];
            foreach (Tile selectedTile in selectedClonedTiles)
            {
                if (selectedTile.tileAboveId != tileAboveId)
                {
                    selectedTile.tileAboveId = tileAboveId;
                    if (tileAboveId != null)
                    {
                        selectedTile.zIndexMaskColor = null;
                        selectedTile.tileAboveIdIsSame = false;
                    }
                    newTiles.Add(selectedTile);
                }
            }
            if (state.tilesetSC.ChangeTile(newTiles))
            {
                additionalAction.Invoke();
                state.showTileZIndices = true;
            }
        });
    }

    public void ChangeSelectedTileAboveIdIsSameCommit(bool tileAboveIdIsSame, Action? additionalAction)
    {
        List<Tile> selectedClonedTiles = GetSelectedClonedTiles();
        if (selectedClonedTiles.Count == 0)
        {
            Prompt.ShowError("No tiles selected.");
            return;
        }

        context.ApplyCodeCommit(RedrawData.ToolingAll, DirtyFlag.Tile, () =>
        {
            List<Tile> newTiles = [];
            foreach (Tile selectedTile in selectedClonedTiles)
            {
                if (selectedTile.tileAboveIdIsSame != tileAboveIdIsSame)
                {
                    selectedTile.tileAboveIdIsSame = tileAboveIdIsSame;
                    if (tileAboveIdIsSame)
                    {
                        selectedTile.zIndexMaskColor = null;
                        selectedTile.tileAboveId = null;
                    }
                    newTiles.Add(selectedTile);
                }
            }
            if (state.tilesetSC.ChangeTile(newTiles))
            {
                additionalAction?.Invoke();
                state.showTileZIndices = true;
            }
        });
    }

    public void AddTileVariationCommit()
    {
        if (selectedTileCoords.Count < 1)
        {
            Prompt.ShowError("Select 1 or more tiles");
            return;
        }
        MapSectionLayer? selectedLayer = GetSelectedLayer();
        if (selectedLayer == null)
        {
            Prompt.ShowError("Multiple layers selected. Select one layer only.");
            return;
        }

        foreach (GridCoords tc in selectedTileCoords)
        {
            Tile tile = tileset.GetTileById(selectedLayer.tileGrid[tc.i, tc.j]);
            if (tile.id == Tile.TransparentTileId)
            {
                Prompt.ShowError("Please select non-transparent tiles");
                return;
            }
        }

        context.ApplyCodeCommit(new(RedrawFlag.Diffs, RedrawTarget.All), [DirtyFlag.Map, DirtyFlag.Scratch, DirtyFlag.Tile], () =>
        {
            HashSet<string> usedHashes = [];
            HashSet<Tile> newTiles = [];
            List<TileInstance> newTileInstances = [];
            foreach (GridCoords tc in selectedTileCoords)
            {
                Tile tile = tileset.GetTileById(selectedLayer.tileGrid[tc.i, tc.j]);
                
                // Only clone each unique selected hash once, because the use case of this is the user just quickly
                // grabbing a clump that could have dups but they don't want each one to be a new cloned tile
                if (usedHashes.Contains(tile.hash)) continue;
                usedHashes.Add(tile.hash);

                Tile newTile = tilesetSC.AddNewTile(tile.hash);
                newTiles.Add(newTile);
                newTileInstances.Add(new TileInstance(tc.i, tc.j, newTile.id));
            }

            selectedLayer.ChangeTileGrid(newTileInstances, true);

            Prompt.ShowMessage("Added " + newTiles.Count + " tile variations");
        });
    }

    public bool DrawHighlightedTiles(Drawer drawer, HashSet<int> processedCoords)
    {
        if (!state.showTileHitboxes && !state.showTileZIndices && state.showTilesWithTagsText.Unset() && !state.showTileAnimations && !state.showTileClumps && !state.showSameTiles && state.showRarelyUsedTilesCount == 0)
        {
            return false;
        }

        int?[,] selectedTileGrid = GetSelectedTileIdGrids(true).GetTop();
        int?[,] otherSelectedTileGrid = otherSectionsSC!.GetSelectedTileIdGrids(true).GetTop();

        List<MyRect> tileClumpRectsToDraw = [];
        List<MyRect> tileClumpSubsecsToDraw = [];
        List<MyRect> selTileClumpRectsToDraw = [];

        int rowCount = selectedMapSection.rowCount;
        int colCount = selectedMapSection.colCount;

        for (int i = 0; i < rowCount; i++)
        {
            int y1 = i * TS;
            int y2 = (i + 1) * TS;
            for (int j = 0; j < colCount; j++)
            {
                int x1 = j * TS;
                int x2 = (j + 1) * TS;

                if (!(x2 > scrollX && y2 > scrollY && x1 < scrollX + canvas.canvasWidth && y1 < scrollY + canvas.canvasHeight))
                {
                    continue;
                }

                if (processedCoords.Contains(GridCoords.GetHashCode(i, j)))
                {
                    continue;
                }

                processedCoords.Add(GridCoords.GetHashCode(i, j));

                MyRect rect = new MyRect(x1, y1, x2, y2);
                Tile tile = state.tileset.GetTileById(GetTopTileId(i, j));
                if (state.showTileHitboxes && tile.hitboxMode != TileHitboxMode.None)
                {
                    TileShapeToDraw? shape = tile.GetHitboxShapeToDraw(i, j, tileset);
                    if (shape?.rect != null)
                    {
                        drawer.DrawRect(shape.rect.Value, Color.Blue, null, 0, 0.5f, offX: -0.5f, offY: -0.5f);
                    }
                    else if (shape?.shape != null)
                    {
                        drawer.DrawPolygon(shape.shape, true, Color.Blue, null, alpha: 0.5f, offX: -0.5f, offY: -0.5f);
                    }
                }

                if (state.showTileZIndices)
                {
                    if (!string.IsNullOrEmpty(tile.zIndexMaskColor) || tile.tileAboveId.HasValue || tile.tileAboveIdIsSame)
                    {
                        drawer.DrawRect(rect, Color.Yellow, null, 0, 0.5f, offX: -0.5f, offY: -0.5f);
                    }
                }

                if (state.showTileAnimations)
                {
                    if (state.tileAnimationSC.GetTileAnimationId(tile) != null)
                    {
                        drawer.DrawRect(rect, Color.Pink, null, 0, 0.5f, offX: -0.5f, offY: -0.5f);
                    }
                }

                if (state.showTileClumps)
                {
                    HashSet<TileClump>? clumps = state.tileClumpSC.GetPotentialTileClumps(tile);
                    if (clumps != null)
                    {
                        foreach (TileClump clump in clumps)
                        {
                            if (clump.CheckIfClumpMatches((i, j) => GetTopTileId(i, j), selectedMapSection.rowCount, selectedMapSection.colCount, i, j))
                            {
                                MyRect clumpRect = MyRect.CreateWH(j * TS, i * TS, clump.cols * TS, clump.rows * TS);
                                if (clump == state.tileClumpSC.selectedTileClump)
                                {
                                    selTileClumpRectsToDraw.Add(clumpRect);
                                }
                                else
                                {
                                    tileClumpRectsToDraw.Add(clumpRect);
                                }

                                foreach (TileClumpSubsection subsection in clump.subsections)
                                {
                                    foreach (GridCoords cell in subsection.cells)
                                    {
                                        tileClumpSubsecsToDraw.Add(cell.AddIJ(i, j).GetRect(TS));
                                    }
                                }

                                break;
                            }
                        }
                    }
                }

                if (!string.IsNullOrEmpty(state.showTilesWithTagsText) && tile.ContainsTag(state.showTilesWithTagsText) == true)
                {
                    drawer.DrawRect(rect, Color.Red, null, 0, 0.5f, offX: -0.5f, offY: -0.5f);
                }

                if (state.showSameTiles)
                {
                    if (selectedTileGrid.Any(tileId => tileId == tile.id) || otherSelectedTileGrid.Any(tileId => tileId == tile.id))
                    {
                        drawer.DrawRect(rect, Color.GreenYellow, null, 0, 0.5f, offX: -0.5f, offY: -0.5f);
                    }
                }

                if (state.showRarelyUsedTilesCount > 0 && tile.id > 0 && state.tileToCountForSrScript[tile] <= state.showRarelyUsedTilesCount)
                {
                    drawer.DrawRect(rect, Color.Magenta, null, 0, 0.5f, offX: -0.5f, offY: -0.5f);
                    drawer.DrawText(state.tileToCountForSrScript[tile].ToString(), j * TS, (i * TS) + (TS), Color.Black, null, 12);
                }
            }
        }

        // Draw here to fix a bug where z-index isn't consistent with other highlighted tile colors
        drawer.DrawRects(tileClumpRectsToDraw, Color.Salmon, null, 0, 0.5f, offX: -0.5f, offY: -0.5f);
        drawer.DrawRects(selTileClumpRectsToDraw, Color.Lime, null, 0, 0.5f, offX: -0.5f, offY: -0.5f);
        drawer.DrawRects(tileClumpSubsecsToDraw, Color.Red, null, 0, 0.25f, offX: -0.5f, offY: -0.5f);

        return true;
    }
}
