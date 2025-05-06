using Editor;
using Shared;

namespace SpriteEditor;

public class Drawbox : StateComponentWithModel<DrawboxModel>, ISelectable
{
    #region model
    public PointSC pos { get => TrGetD(new PointSC(context, model.pos)); set => TrSet(value); }
    public string spritesheetName { get => TrGetD(model.spritesheetName); set => TrSet(value); }
    public RectSC rect { get => TrGetD(new RectSC(context, model.rect)); set => TrSet(value); }
    public string tags { get => TrGetD(model.tags); set => TrSet(value); }
    public long zIndex { get => TrGetD(model.zIndex); set => TrSet(value); }
    public DrawboxModel ToModel() => new DrawboxModel(pos, spritesheetName, rect, tags, zIndex);
    #endregion

    public bool selected { get => TrGet<bool>(); set => TrSet(value, [nameof(border)]); }
    public string border => selected ? "LightGreen" : "Transparent";
    public Spritesheet GetSpritesheet(State state) => state.spritesheets.First(s => s.name == spritesheetName);

    public Drawbox(EditorContext context, DrawboxModel model) : base(context, model)
    {
    }

    public Drawbox(EditorContext context, string spritesheetName, MyRect rect) : 
        base(context, DrawboxModel.New(spritesheetName, rect))
    {
    }

    public void Move(int moveX, int moveY)
    {
        pos.x += moveX;
        pos.y += moveY;
    }

    public MyRect GetFrameRect()
    {
        return MyRect.CreateWH(pos.x, pos.y, rect.w, rect.h);
    }

    public MyPoint GetCenterPointInFrame()
    {
        return new MyPoint(pos.x + rect.w / 2, pos.y + rect.h / 2);
    }
}
