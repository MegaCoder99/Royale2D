using Editor;
using Shared;

namespace MapEditor;

public class TileAnimation : StateComponentWithModel<TileAnimationModel>
{
    #region model section
    public int id { get => TrGetD(model.id); set => TrSet(value); }
    public TrackableList<int> tileIds { get => TrListGet(model.tileIds); init => TrListSet(value); }
    public string duration { get => TrGetD(model.duration?.ToString() ?? ""); set => TrSetV(value, ValidateDuration); }

    public TileAnimationModel ToModel()
    {
        return new TileAnimationModel(id, tileIds.SelectList(id => id), duration.IsSet() ? int.Parse(duration) : null);
    }

    #endregion

    public string displayName => $"Animation {id}";

    public string tileIdsDisplay => string.Join(", ", tileIds);

    public TileAnimation(EditorContext context, TileAnimationModel model) : base(context, model)
    {
    }

    public TileAnimation(EditorContext context, int id, List<int> tileIds) : this(context, new(id, tileIds, 0))
    {
    }

    public bool ValidateDuration(string value)
    {
        int parsedVal;
        if (!value.IsSet()) return true;
        if (!int.TryParse(value, out parsedVal))
        {
            Prompt.ShowError("duration must be integer.");
            return false;
        }
        if (parsedVal < 1)
        {
            Prompt.ShowError("duration must be >= 1.");
            return false;
        }
        return true;
    }
}
