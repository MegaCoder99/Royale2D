using Editor;
using Shared;

namespace MapEditor;

// Some of these are now called by non-script, "first-class" editor code. But we leave them here because:
// - Moving them back and forth to State.cs would be combersome, if we change our minds and don't want to run on every export
// - We don't need to have a "perfectly neat" system. It's ok for stuff to be in Scripts even if they are now "first-class", especially
//   if they were recently promoted to first class editor functionality or we're still not sure.
public partial class State
{
    [Script("ru", "Remove unused tiles")]
    public void RemoveUnusedTiles(string[] args)
    {
        bool autoOnSave = false;
        if (args.Length == 1 && args[0] == "a")
        {
            autoOnSave = true;
        }

        HashSet<int> unusedTileIds = GetUnusedTileIds();

        HashSet<int> actualRemovedIds = RemoveTilesWithChecks(unusedTileIds);

        if (actualRemovedIds.Count > 0)
        {
            ForceSaveAll();
            if (!autoOnSave)
            {
                Prompt.ShowMessage("Removed " + actualRemovedIds.Count + " unused tiles. Changes were saved to disk.");
            }
            else
            {
                Prompt.ShowMessage("Pre-save cleanup: removed " + actualRemovedIds.Count + " unused tiles before saving.");
            }
        }
        else if (!autoOnSave)
        {
            Prompt.ShowMessage("No unused tiles were found or removed.");
        }
    }

    public HashSet<int> GetUnusedTileIds()
    {
        HashSet<int> unusedTileIds = new(tileset.idToTile.Keys);

        foreach (MapSection section in GetAllSections())
        {
            foreach (MapSectionLayer layer in section.layers)
            {
                for (int i = 0; i < layer.tileGrid.GetLength(0); i++)
                {
                    for (int j = 0; j < layer.tileGrid.GetLength(1); j++)
                    {
                        // Remove ones in use, so what's left remaining are the orphans
                        unusedTileIds.Remove(layer.tileGrid[i, j]);
                    }
                }
            }
        }

        return unusedTileIds;
    }

    public bool HasOrphanedTileIds()
    {
        return GetUnusedTileIds().Count > 0;
    }

    [Script("rd")]
    public void RemoveTilesWithDuplicateHashes(string[] args)
    {
        if (HasOrphanedTileIds())
        {
            Prompt.ShowMessage("There are unused tile ids. Please remove them first.");
            return;
        }

        Dictionary<Tile, int> tileToCount = GetTileToCount();

        Dictionary<string, List<Tile>> hashToTiles = tileset.GetAllTiles().GroupBy(tile => tile.hash).ToDictionary(group => group.Key, group => group.ToList());

        Dictionary<int, int> replacedTileIds = new();
        foreach (List<Tile> tileGroup in hashToTiles.Values)
        {
            if (tileGroup.Count <= 1) continue;

            Tile masterTile = tileGroup.OrderByDescending(t => tileToCount[t]).First();
            foreach (Tile tile in tileGroup)
            {
                if (tile == masterTile) continue;
                replacedTileIds[tile.id] = masterTile.id;
                tileset.RemoveTile(tile.id);
            }
        }

        ReplaceTileIds(replacedTileIds);

        ForceSaveAll();
        Prompt.ShowMessage("Done");
    }

    [Script("sh", "Shift tile ids")]
    public void ShiftTileIds(string[] args)
    {
        Dictionary<int, int> replacedTileIds = tileset.ShiftTileIds();

        ReplaceTileIds(replacedTileIds);

        ForceSaveAll();
        Prompt.ShowMessage("Done");
    }

    
    [Script("vt", "Validate tiles")]
    public void ValidateTiles(string[] args)
    {
        int invalidTileAnims = 0;
        foreach (TileAnimation tileAnim in tileAnimationSC.tileAnimations)
        {
            if (tileAnim.tileIds.Any(tileId => !tileset.idToTile.ContainsKey(tileId)))
            {
                invalidTileAnims++;
            }
        }

        int invalidTileClumps = 0;
        foreach (TileClump tileClump in tileClumpSC.tileClumps)
        {
            if (tileClump.tileIds.Any(tileId => !tileset.idToTile.ContainsKey(tileId)))
            {
                invalidTileClumps++;
            }
        }

        int invalidTiles = 0;
        foreach (Tile tile in tileset.GetAllTiles())
        {
            if (tile.tileAboveId != null && !tileset.idToTile.ContainsKey(tile.tileAboveId.Value))
            {
                invalidTiles++;
            }
        }

        List<string> validationIssues = [];
        if (invalidTileAnims > 0)
        {
            validationIssues.Add($"{invalidTileAnims} tile animation(s) reference non-existent tile ids.");
        }
        if (invalidTileClumps > 0)
        {
            validationIssues.Add($"{invalidTileClumps} tile clumps(s) reference non-existent tile ids.");
        }
        if (invalidTiles > 0)
        {
            validationIssues.Add($"{invalidTiles} tile id(s) reference non-existent tile above ids");
        }

        if (validationIssues.Count > 0)
        {
            Prompt.ShowMessage("Validation issues found:\n\n" + string.Join("\n", validationIssues));
        }
        else
        {
            Prompt.ShowMessage("No validation issues.");
        }
    }

    public void ReplaceTileIds(Dictionary<int, int> replacedTileIds)
    {
        foreach (MapSection section in GetAllSections())
        {
            foreach (MapSectionLayer layer in section.layers)
            {
                for (int i = 0; i < layer.tileGrid.GetLength(0); i++)
                {
                    for (int j = 0; j < layer.tileGrid.GetLength(1); j++)
                    {
                        int tileId = layer.tileGrid[i, j];
                        if (replacedTileIds.ContainsKey(tileId))
                        {
                            layer.ChangeTileGrid([new TileInstance(i, j, replacedTileIds[tileId])]);
                        }
                    }
                }
            }
        }
        tileset.ReplaceTileForeignKeyRefs(replacedTileIds, this);
    }

    public HashSet<int> RemoveTilesWithChecks(HashSet<int> tileIdsToRemove)
    {
        (HashSet<int> idsThatCanBeRemoved, HashSet<int> idsThatCantBeRemoved, Dictionary<int, string> idToCantRemoveError) = tileset.GetRemovableTiles(tileIdsToRemove, this);

        foreach (int tileId in idsThatCanBeRemoved)
        {
            tileset.RemoveTile(tileId);
        }

        if (idsThatCantBeRemoved.Count > 0)
        {
            Prompt.ShowMessage("Could not remove " + idsThatCantBeRemoved.Count + " tiles since they are referenced in other data models.");
        }

        return idsThatCanBeRemoved;
    }
}