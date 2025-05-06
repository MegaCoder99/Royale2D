namespace Royale2D
{
    public class WadeComponent : Component
    {
        string wadeSpriteName = "";
        int frameIndex;
        int framesElapsed;
        bool checkedOnce;

        Sprite? wadeSprite => wadeSpriteName == "" ? null :  Assets.GetSprite(wadeSpriteName);

        public WadeComponent(Actor actor) : base(actor)
        {
        }

        public override void Update()
        {
            bool moved = actor.deltaPos.IsNonZero();
            if (moved || !checkedOnce)
            {
                wadeSpriteName = GetWadeSpriteName();
                if (wadeSpriteName == "wade_water")
                {
                    framesElapsed++;
                    if (framesElapsed > 4)
                    {
                        framesElapsed = 0;
                        frameIndex++;
                    }
                    if (frameIndex > 3)
                    {
                        frameIndex = 0;
                    }
                }
                else if (wadeSpriteName == "wade_grass" && moved)
                {
                    framesElapsed++;
                    if (framesElapsed > 6)
                    {
                        framesElapsed = 0;
                        frameIndex++;
                    }
                    if (frameIndex > 1)
                    {
                        frameIndex = 0;
                    }
                }
            }
        }

        public override void Render(Drawer drawer)
        {
            if (wadeSprite != null)
            {
                float yOff = 0;
                if (actor is Character chr) yOff = -4;
                Point renderPos = actor.GetRenderFloatPos().AddXY(0, (actor.sprite.frames[0].rect.h / 2.0f) + yOff);
                wadeSprite.Render(drawer, renderPos, frameIndex, actor.GetRenderZIndex(ZIndex.ChildOffsetWade));
            }
        }
        
        public string GetWadeSpriteName()
        {
            var zComponent = GetComponent<ZComponent>();
            if (zComponent != null && !zComponent.HasLanded())
            {
                return "";
            }

            var colliderComponent = GetComponent<ColliderComponent>();
            if (colliderComponent == null)
            {
                return "";
            }
            else if (colliderComponent.IsInTileWithTag(TileTag.ShallowWater))
            {
                return "wade_water";
                
            }
            else if (colliderComponent.IsInTileWithTag(TileTag.TallGrass))
            {
                return "wade_grass";
            }
            else
            {
                return "";
            }
        }
    }
}
