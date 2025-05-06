using Editor;
using Shared;

namespace MapEditor;

public class TileAnimationsSC : StateComponent
{
    public State state;
    public SectionsSC lastSelectedMapCanvas => state.lastSelectedSectionsSC;

    public TrackableList<TileAnimation> tileAnimations { get => TrListGet<TileAnimation>(changeEvent: EditorEvent.TileAnimationChange); set => TrListSet(value); }
    public Dictionary<int, int> cachedTileIdToAnimationId = new();
    public void UpdateTileAnimationCache()
    {
        cachedTileIdToAnimationId.Clear();
        foreach (TileAnimation tileAnimation in tileAnimations)
        {
            foreach (int tileId in tileAnimation.tileIds)
            {
                cachedTileIdToAnimationId[tileId] = tileAnimation.id;
            }
        }
    }
    public TileAnimation? selectedTileAnimation { get => TrGet<TileAnimation?>(); set => TrSet(value, [nameof(canRemoveTileAnimation), nameof(showSelectedTileAnimation)]); }
    public bool canAddTileAnimation => lastSelectedMapCanvas.selectedTileCoords.Count > 1;
    public bool canRemoveTileAnimation => selectedTileAnimation != null;
    public bool showSelectedTileAnimation => selectedTileAnimation != null;
    public bool isDirty { get => TrGet<bool>(); set => TrSet(value, [nameof(tileAnimationLabel)]); }
    public string tileAnimationLabel => "Tile Animations" + (isDirty ? "*" : "");
    public int currentTileAnimationId => tileAnimations.Count > 0 ? tileAnimations.Max(ta => ta.id) : 0;

    public TileAnimationsSC(EditorContext context, State state, List<TileAnimationModel> tileAnimations) : base(context)
    {
        this.state = state;
        this.tileAnimations = new(tileAnimations.Select(ta => new TileAnimation(context, ta)));
        UpdateTileAnimationCache();
    }

    public List<TileAnimationModel> ToModel()
    {
        return tileAnimations.Select(ta => ta.ToModel()).ToList();
    }

    public void Save(MapWorkspace workspace, bool forceSave)
    {
        if (isDirty || forceSave)
        {
            workspace.SaveTileAnimations(ToModel());
            isDirty = false;
        }
    }

    public void EditorEventHandler(EditorEvent editorEvent, StateComponent firer)
    {
        if (editorEvent == EditorEvent.SelectedTileChange && firer is SectionsSC sectionsSC)
        {
            // If selecting a tile that is a part of an animation, select that animation. Otherwise clear out the selected tile animation
            IEnumerable<int?> animIds = sectionsSC.selectedTileCoords.Select(c => sectionsSC.GetTopTile(c.i, c.j).GetAnimationId(state));
            bool allSame = animIds.Distinct().Count() == 1 && animIds.First() != null;
            int? allSameVal = allSame ? animIds.First() : null;
            if (allSame && allSameVal != null)
            {
                TileAnimation? tileAnim = tileAnimations.FirstOrDefault(ta => ta.id == allSameVal);
                selectedTileAnimation = tileAnim;
            }
            else if (selectedTileAnimation != null)
            {
                selectedTileAnimation = null;
            }
            QueueOnPropertyChanged(nameof(canAddTileAnimation));
        }
        if (editorEvent == EditorEvent.TileAnimationChange)
        {
            QueueGenericAction(UpdateTileAnimationCache);
        }
    }

    public int? GetTileAnimationId(Tile tile)
    {
        return cachedTileIdToAnimationId.ContainsKey(tile.id) ? cachedTileIdToAnimationId[tile.id] : null;
    }

    public void AddTileAnimationCommit()
    {
        if (lastSelectedMapCanvas.selectedTileCoords.Count < 2)
        {
            return;
        }

        // IMPROVEMENT for better usability, should we consider all empty selections in say layer 1 above, to be a single layer?
        MapSectionLayer? selectedLayer = lastSelectedMapCanvas.GetSelectedLayer();
        if (selectedLayer == null)
        {
            Prompt.ShowError("Multiple layers selected. Select one layer only.");
            return;
        }

        List<int> tileIds = lastSelectedMapCanvas.selectedTileCoords.Select(c => selectedLayer.tileGrid[c.i, c.j]).ToList();
        if (tileIds.Distinct().Count() != tileIds.Count)
        {
            Prompt.ShowError("Cannot create a tile animation with duplicate tiles");
            return;
        }
        if (tileAnimations.Any(ta => ta.tileIds.Intersect(tileIds).Any()))
        {
            Prompt.ShowError("Tile id(s) already used in existing animation");
            return;
        }

        context.ApplyCodeCommit(RedrawData.ToolingAll, DirtyFlag.TileAnimation, () =>
        {
            TileAnimation newTileAnimation = new TileAnimation(context, currentTileAnimationId + 1, tileIds);
            tileAnimations.Add(newTileAnimation);
            selectedTileAnimation = newTileAnimation;
            state.showTileAnimations = true;
        });
    }

    public void RemoveTileAnimationCommit()
    {
        if (selectedTileAnimation == null) return;

        context.ApplyCodeCommit(RedrawData.ToolingAll, DirtyFlag.TileAnimation, () =>
        {
            tileAnimations.Remove(selectedTileAnimation);
            selectedTileAnimation = null;
        });
    }
}
