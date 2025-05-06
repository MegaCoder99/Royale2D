using Editor;
using Shared;

namespace MapEditor;

// State component for the display section where it shows selected tile data
public class SelectedTileDisplaySC : StateComponent
{
    private State state;
    private SectionsSC selectedSectionsSC => state.lastSelectedSectionsSC;
    private TrackableList<GridCoords> selectedTileCoords => selectedSectionsSC.selectedTileCoords;

    public SelectedTileDisplaySC(EditorContext context, State state) : base(context)
    {
        this.state = state;
    }

    public void EditorEventHandler(EditorEvent editorEvent)
    {
        if (editorEvent == EditorEvent.SelectedTileChange || 
            editorEvent == EditorEvent.SectionChange || 
            editorEvent == EditorEvent.LayerChange)
        {
            QueueOnPropertyChanged(nameof(tileCoordsDisplayText));
            QueueOnPropertyChanged(nameof(tileGridCoordsDisplayText));
            QueueOnPropertyChanged(nameof(showSelectedTileData));

            RefreshSelectedTileDisplay();
        }
        
        if (editorEvent == EditorEvent.TileDataChange ||
            editorEvent == EditorEvent.LayerTileGridChange)
        {
            RefreshSelectedTileDisplay();
        }
    }

    public void RefreshSelectedTileDisplay()
    {
        cachedSelectedTopTiles = null;
        //var stopwatch = new Stopwatch(); stopwatch.Start();
        QueueOnPropertyChanged(nameof(selectedTileId));
        QueueOnPropertyChanged(nameof(selectedTileLayer));
        QueueOnPropertyChanged(nameof(selectedTileHitbox));
        QueueOnPropertyChanged(nameof(selectedTileAnimationId));
        QueueOnPropertyChanged(nameof(selectedTileZMaskColor));
        QueueOnPropertyChanged(nameof(selectedTileAboveId));
        QueueOnPropertyChanged(nameof(selectedTileAboveIdIsSame));
        QueueOnPropertyChanged(nameof(selectedTileTags));
        //stopwatch.Stop(); Console.WriteLine($"RefreshSelectedTileDisplay took {stopwatch.ElapsedMilliseconds}ms");
    }

    public bool showSelectedTileData => selectedTileCoords.Count >= 1;

    // Selected tile data
    public string tileCoordsDisplayText
    {
        get
        {
            int TS = state.tileset.tileSize;
            if (selectedTileCoords.Count > 1) return "-";
            else if (selectedTileCoords.Count == 1) return $"x:{selectedTileCoords[0].j * TS},y:{selectedTileCoords[0].i * TS}";
            else return "";
        }
    }

    public string tileGridCoordsDisplayText
    {
        get
        {
            if (selectedTileCoords.Count > 1) return "-";
            else if (selectedTileCoords.Count == 1) return $"i:{selectedTileCoords[0].i},j:{selectedTileCoords[0].j}";
            else return "";
        }
    }

    // Generally, try to limit use of LINQ in these helpers and in general in this class. With large selections with 10-100k tiles, LINQ can be noticably slow.
    // Removing LINQ in many places reduced time from 200ms to 50ms for large tile selections, a noticable improvement in UI snappiness, and we can probably improve it even more
    public string UniqueTileValHelper(Func<Tile, string?> tileToStringVal)
    {
        IEnumerable<Tile> uniqueSelTiles = GetCachedSelectedTopTiles();
        List<string?> stringVals = new List<string?>();
        foreach (Tile tile in uniqueSelTiles)
        {
            stringVals.Add(tileToStringVal(tile));
        }
        return UniqueStringValHelper(stringVals);
    }

    public string UniqueStringValHelper(IEnumerable<string?> stringVals)
    {
        HashSet<string?> uniqueStringVals = new HashSet<string?>();
        foreach (string? val in stringVals)
        {
            if (!uniqueStringVals.Contains(val))
            {
                uniqueStringVals.Add(val);
                if (uniqueStringVals.Count > 1) return "-";
            }
        }
        if (uniqueStringVals.Count == 0)
        {
            return "";
        }
        else
        {
            return uniqueStringVals.First() ?? "";
        }
    }

    private IEnumerable<Tile>? cachedSelectedTopTiles = null;
    public IEnumerable<Tile> GetCachedSelectedTopTiles()
    {
        if (cachedSelectedTopTiles == null)
        {
            HashSet<Tile> distinctTiles = new HashSet<Tile>();
            foreach (var c in selectedTileCoords)
            {
                Tile tile = selectedSectionsSC.GetTopTile(c.i, c.j);
                distinctTiles.Add(tile);
            }
            cachedSelectedTopTiles = distinctTiles;
        }
        return cachedSelectedTopTiles;
    }

    public string selectedTileId => UniqueTileValHelper(tile => tile.id.ToString());
    public string selectedTileLayer => UniqueStringValHelper(selectedTileCoords.Select(c => selectedSectionsSC.GetTopTileLayer(c.i, c.j).ToString()));
    public string selectedTileHitbox => UniqueTileValHelper(tile => tile.hitboxMode.ToString());
    public string selectedTileAnimationId => UniqueTileValHelper(tile => tile.GetAnimationId(state)?.ToString() ?? "");

    public string selectedTileZMaskColor
    {
        get => UniqueTileValHelper(tile => tile.zIndexMaskColor);
        set
        {
            selectedSectionsSC.ChangeColorMaskCommit(value, () => QueueOnPropertyChanged(nameof(selectedTileZMaskColor)));
        }
    }

    public string selectedTileTags
    {
        get => UniqueTileValHelper(tile => tile.tags);
        set
        {
            selectedSectionsSC.ChangeSelectedTileTagsCommit(value, () => QueueOnPropertyChanged(nameof(selectedTileTags)));
        }
    }

    public string selectedTileAboveId
    {
        get => UniqueTileValHelper(tile => tile.tileAboveId?.ToString());
        set
        {
            int? intValue;
            int intValueNonNull;
            if (!value.IsSet())
            {
                intValue = null;
            }
            else if (!int.TryParse(value, out intValueNonNull))
            {
                Prompt.ShowError("Invalid tile above id. Please enter a valid integer.");
                return;
            }
            else if (!state.tileset.idToTile.ContainsKey(intValueNonNull))
            {
                Prompt.ShowError("Tile id entered does not exist.");
                return;
            }
            else
            {
                intValue = intValueNonNull;
            }
            selectedSectionsSC.ChangeSelectedTileAboveIdCommit(intValue, () => QueueOnPropertyChanged(nameof(selectedTileAboveId)));
        }
    }

    public bool selectedTileAboveIdIsSame
    {
        get => GetCachedSelectedTopTiles().All(tile => tile.tileAboveIdIsSame == true);
        set
        {
            selectedSectionsSC.ChangeSelectedTileAboveIdIsSameCommit(value, () => QueueOnPropertyChanged(nameof(selectedTileAboveIdIsSame)));
        }
    }
}
