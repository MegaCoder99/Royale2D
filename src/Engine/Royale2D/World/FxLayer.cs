namespace Royale2D
{
    // REFACTOR swap i and j in this file
    public abstract class FxLayer
    {
        public bool isForeground;

        public FxLayer(bool isForeground)
        {
            this.isForeground = isForeground;
        }

        public virtual void Update()
        {
        }

        public virtual void Render(Drawer drawer)
        {
        }
    }

    public class WoodsFogFxLayer : FxLayer
    {
        public Point woodsFogOffset;
        public Point woodsBackdropOffset;
        public float alpha = 1;
        public bool fadeOut;

        public WoodsFogFxLayer() : base(true)
        {
        }

        public override void Update()
        {
            woodsFogOffset.x += Game.spfConst * 6;
            woodsFogOffset.y += Game.spfConst * 6;
            if (woodsFogOffset.x > 256)
            {
                woodsFogOffset.x = 0;
                woodsFogOffset.y = 0;
            }
            if (fadeOut)
            {
                alpha -= Game.spfConst * 4;
                if (alpha < 0) alpha = 0;
            }
        }

        // PERF optimize by not rendering textures that are off-screen
        // REFACTOR reconsider accessing drawer.pos directly. Pass camera or something?
        public override void Render(Drawer drawer)
        {
            for (int i = -1; i < 4; i++)
            {
                for (int j = -1; j < 4; j++)
                {
                    drawer.DrawTexture(Assets.textures["woods_fog"], woodsFogOffset.x + (i * 512), woodsFogOffset.y + (j * 448), ZIndex.UIGlobal, alpha: alpha);
                }
            }
        }
    }

    public class WoodsShadowFxLayer : FxLayer
    {
        float alpha;

        public WoodsShadowFxLayer() : base(true)
        {
        }

        public override void Update()
        {
            if (alpha < 1)
            {
                alpha += Game.spfConst * 4;
                if (alpha > 1) alpha = 1;
            }
        }

        // PERF optimize by not rendering textures that are off-screen
        public override void Render(Drawer drawer)
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    float x = (drawer.pos.x - Game.ScreenW) * 0.5f;
                    float y = (drawer.pos.y - Game.ScreenH) * 0.5f;
                    drawer.DrawTexture(Assets.textures["woods"], x + (i * 512), y + (j * 448), ZIndex.UIGlobal, alpha: alpha);
                }
            }
        }
    }

    public class MountainFxLayer : FxLayer
    {
        int time;
        int frame;

        public MountainFxLayer() : base(false)
        {
        }

        public override void Update()
        {
            time++;
            if (time > 8)
            {
                time = 0;
                frame++;
                if (frame > 2)
                {
                    frame = 0;
                }
            }
        }

        // PERF optimize by not rendering textures that are off-screen
        public override void Render(Drawer drawer)
        {
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    float x = (drawer.pos.x - Game.ScreenW) * 0.5f;
                    float y = (drawer.pos.y - Game.ScreenH) * 0.5f;

                    drawer.DrawTexture(Assets.textures[$"mountain_ground{frame + 1}"], x + (i * 256), y + (j * 256), ZIndex.FxGlobalBelow);
                }
            }
        }
    }
}
