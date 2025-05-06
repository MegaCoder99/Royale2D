namespace Royale2D
{
    public class HammerState : CharState
    {
        public HammerState(Character character) : base(character)
        {
            baseSpriteName = "char_hammer";
            idleOnAnimEnd = true;
            damagerType = DamagerType.hammer;
        }

        public override void Update()
        {
            base.Update();

            FdPoint? poi = character.GetFirstPOIOrNull();

            if (poi != null && !once)
            {
                once = true;

                List<TileInstance> tilesTouching = character.colliderComponent.GetTilesTouching("hammer");
                
                if (tilesTouching.Any(t => t.tileData.HasAnyTag(["swater", "water"])))
                {
                    character.PlaySound("item in water");
                    new Anim(character, character.GetFirstPOI(), "splash_object");
                }
                else
                {
                    character.PlaySound("hammer");
                    new Anim(character, character.GetFirstPOI(), "hammer_hit");
                }

                foreach (TileInstance tileInstance in tilesTouching)
                {
                    TileClumpInstance? tileClumpInstance = tileInstance.GetTileClumpInstanceFromTag(TileClumpTags.Stake);
                    if (tileClumpInstance != null)
                    {
                        character.layer.TransformTileClump(tileClumpInstance.Value);
                    }
                }
            }
        }

        public override bool CanChangeFrom(CharState oldState)
        {
            return oldState is IdleState;
        }
    }
}
