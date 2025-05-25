namespace Royale2D
{
    public class QuantityComponent : Component
    {
        public int quantity;

        public QuantityComponent(Actor actor, int quantity) : base(actor)
        {
            this.quantity = quantity;
        }

        public override void Render(Drawer drawer)
        {
            base.Render(drawer);
            Point renderPos = actor.GetRenderFloatPos();
            drawer.DrawText(quantity.ToString(), renderPos.x + 1, renderPos.y + 3, fontType: FontType.NumberWorld, zIndex: actor.GetRenderZIndex(ZIndex.DrawboxOffsetUI));
        }
    }
}
