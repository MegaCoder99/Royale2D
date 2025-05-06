namespace Royale2D
{
    public class BombExplosion : Actor
    {
        public Character owner;

        public ColliderComponent colliderComponent;
        public DamagerComponent damagerComponent;

        public BombExplosion(Actor creator, Character owner, FdPoint pos) : base(creator, pos, "bomb_explosion")
        {
            colliderComponent = AddComponent(new ColliderComponent(this, isDamager: true));
            damagerComponent = AddComponent(new DamagerComponent(this, DamagerType.bomb, owner));
            this.owner = owner;
            PlaySound("bomb explode");
        }

        public override void Update()
        {
            base.Update();

            List<TileInstance> tilesTouching = colliderComponent.GetTilesTouching();
            
            foreach (TileInstance tileInstance in tilesTouching)
            {
                TileClumpInstance? tileClumpInstance = tileInstance.GetTileClumpInstanceFromTag(TileClumpTags.CrackedWall);
                if (tileClumpInstance != null)
                {
                    layer.TransformTileClump(tileClumpInstance.Value);
                    PlaySound("secret");
                }
            }

            if (IsAnimOver())
            {
                DestroySelf();
                return;
            }
        }
    }
}
