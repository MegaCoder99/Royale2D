using Editor;
using Shared;

namespace SpriteEditor;

public class Frame : StateComponentWithModel<FrameModel>
{
    #region model
    public RectSC rect { get => TrGetD(new RectSC(context, model.rect)); set => TrSet(value); }
    public int duration { get => TrGetD(model.duration); set => TrSet(value); }
    public PointSC offset { get => TrGetD(new PointSC(context, model.offset)); set => TrSet(value); }
    public TrackableList<Hitbox> hitboxes { get => TrListGet(model.hitboxes.Select(h => new Hitbox(context, h))); init => TrListSet(value); }
    public TrackableList<Drawbox> drawboxes { get => TrListGet(model.drawboxes?.Select(d => new Drawbox(context, d))); init => TrListSet(value); }
    public TrackableList<POI> POIs { get => TrListGet(model.POIs.Select(p => new POI(context, p))); init => TrListSet(value); }
    public string tags { get => TrGetD(model.tags); set => TrSet(value); }

    public FrameModel ToModel()
    { 
        return new FrameModel(
            rect,
            duration,
            offset,
            hitboxes.SelectList(h => h.ToModel()),
            drawboxes.SelectList(d => d.ToModel()),
            POIs.SelectList(p => p.ToModel()),
            tags,
            spritesheetName: ""
        );
    }
    #endregion

    public bool selected { get => TrGet<bool>(); set => TrSet(value, [nameof(border)]); }
    public string border => selected ? "LightGreen" : "Transparent";

    public Frame(EditorContext context, FrameModel model) : base(context, model)
    {
    }

    public Frame(EditorContext context, MyRect rect, int duration, MyPoint offset) : 
        this(context, FrameModel.New(rect, duration, offset))
    {
    }

    public void Change(int newX, int newY)
    {
        Move(newX - offset.x, newY - offset.y);
    }

    public void Move(int moveX, int moveY)
    {
        offset.x += moveX;
        offset.y += moveY;

        foreach (Hitbox hitbox in hitboxes)
        {
            hitbox.Move(moveX, moveY);
        }
        foreach (Drawbox drawbox in drawboxes)
        {
            drawbox.Move(moveX, moveY);
        }
        foreach (POI poi in POIs)
        {
            poi.Move(moveX, moveY);
        }
    }
}
