namespace Royale2D
{
    public class PoofComponent : Component
    {
        public SpriteInstance poofSpriteInstance;
        private bool showPoof;

        public PoofComponent(Actor actor) : base(actor)
        {
            poofSpriteInstance = new SpriteInstance("cape_poof");
        }

        public override void Update()
        {
            base.Update();
            if (showPoof)
            {
                poofSpriteInstance.Update();
                if (poofSpriteInstance.IsAnimOver())
                {
                    poofSpriteInstance.Reset();
                    showPoof = false;
                }
            }
        }

        public override void Render(Drawer drawer)
        {
            base.Render(drawer);
            if (showPoof)
            {
                Point renderPos = actor.GetRenderFloatPos();
                poofSpriteInstance.Render(drawer, renderPos.x, renderPos.y, actor.GetRenderZIndex(ZIndex.DrawboxOffsetPoof));
            }
        }

        public void Poof()
        {
            if (showPoof) return;
            showPoof = true;
            actor.PlaySound("cape on");
        }

        public void Unpoof()
        {
            if (showPoof) return;
            showPoof = true;
            actor.PlaySound("cape off");
        }
    }
}
