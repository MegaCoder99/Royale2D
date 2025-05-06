namespace Royale2D
{
    public class ShadowComponent : Component
    {
        public ShadowType shadow;
        public int? yOffset;

        public ShadowComponent(Actor actor, ShadowType shadow, int? yOffset = null, bool disabled = false) : base(actor)
        {
            this.shadow = shadow;
            this.yOffset = yOffset;
            this.disabled = disabled;
        }

        public override void Render(Drawer drawer)
        {
            string shadowSpriteName = "";
            if (shadow == ShadowType.Small) shadowSpriteName = "shadow_small";
            else if (shadow == ShadowType.Large) shadowSpriteName = "shadow";
            if (shadowSpriteName != "")
            {
                Point shadowPos = GetShadowPos();
                Assets.GetSprite(shadowSpriteName).Render(drawer, shadowPos, 0, ZIndex.FromLayerIndex(actor.layerIndex, actor.zLayerOffset - 1));
            }
        }

        // Shadow is special. Unlike most places, it renders at the "real" actor pos and not the render pos
        public Point GetShadowPos()
        {
            if (yOffset != null)
            {
                // Need to find another way to offset shadow, this messes with the LandState's spinning falling animation shadows...
                /*
                int yOff = 0;
                if (actor is Character chr && (chr.dir == Direction.Left || chr.dir == Direction.Right))
                {
                    yOff = 1;
                }
                */

                return actor.pos.ToFloatPoint().AddXY(0, yOffset.Value);
            }
            return actor.pos.ToFloatPoint().AddXY(0, actor.sprite.frames[0].rect.h / 2.0f);
        }
    }
}
