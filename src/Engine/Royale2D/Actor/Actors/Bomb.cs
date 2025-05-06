namespace Royale2D
{
    public class Bomb : Actor
    {
        public Character owner;
        public ZComponent zComponent;
        public VelComponent velComponent;
        public ColliderComponent colliderComponent;

        public Bomb(Character owner, FdPoint pos) : base(owner, pos, "bomb")
        {
            colliderComponent = AddComponent(new ColliderComponent(this));
            AddComponent(new ShadowComponent(this, ShadowType.Large));
            AddComponent(new WadeComponent(this));
            zComponent = AddComponent(new ZComponent(this, bounce: true, playLandSound: true));
            velComponent = AddComponent(new VelComponent(this));
            AddComponent(new LiftableComponent(this, zComponent, velComponent, colliderComponent, Fd.New(1, 50), 1, false, false));
            this.owner = owner;
        }

        public override void Update()
        {
            base.Update();
            if (spriteName == "bomb" && elapsedFrames > 90)
            {
                baseSpriteName = "bomb_flash";
            }
            else if (spriteName == "bomb_flash" && IsAnimOver())
            {
                DestroySelf(() =>
                {
                    new BombExplosion(this, owner, GetRenderPos());
                });
            }
        }

        public static void OnUseBomb(Character character)
        {
            FdPoint pos = character.GetCenterPos() + character.directionComponent.ForwardFdVec(12);
            new Bomb(character, pos);
            character.PlaySound("bomb place");
        }
    }
}
