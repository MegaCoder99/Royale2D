using Editor;
using Shared;

namespace SpriteEditor;

public class Hitbox : StateComponentWithModel<HitboxModel>, ISelectable
{
    #region model
    public string tags { get => TrGetD(model.tags); set => TrSet(value); }
    public RectSC rect { get => TrGetD(new RectSC(context, model.rect)); set => TrSet(value); }
    public HitboxModel ToModel() => new HitboxModel(tags, rect);
    #endregion

    public bool selected { get => TrGet<bool>(); set => TrSet(value, [nameof(border)]); }
    public string border => selected ? "LightGreen" : "Transparent";

    public Hitbox(EditorContext context, HitboxModel model) : base(context, model)
    {
    }

    public Hitbox(EditorContext context) : base(context, new("", new MyRect(-8, -8, 8, 8))) 
    {
    }

    public void Move(int moveX, int moveY)
    {
        rect.IncXY(moveX, moveY);
    }
}
