namespace Royale2D
{
    // Generic actor for things like rocks, bushes, signs, etc after they are picked up and can be thrown
    public class LiftedTile : Actor
    {
        public Character owner;
        public ZComponent zComponent;
        public VelComponent velComponent;
        public DamagerComponent damagerComponent;
        public ColliderComponent colliderComponent;
        public LiftableComponent liftableComponent;

        public LiftedTile(Character owner, FdPoint pos, string spriteName, string fadeSpriteName, string fadeSoundName) : base(owner, pos, spriteName)
        {
            colliderComponent = AddComponent(new ColliderComponent(this, true, true), true);
            bool isHeavy = spriteName == "rock_big_gray";
            zComponent = AddComponent(new ZComponent(this));
            velComponent = AddComponent(new VelComponent(this));
            AddComponent(new ShadowComponent(this, ShadowType.Large, disabled: true));
            damagerComponent = AddComponent(new DamagerComponent(this, GetDamagerType(spriteName), owner), true);
            liftableComponent = AddComponent(new LiftableComponent(this, zComponent, velComponent, colliderComponent, 3, 0, true, isHeavy, fadeSpriteName, fadeSoundName));
            AddComponent(new ShieldableComponent(this, owner, false, velComponent));
            this.owner = owner;
        }

        public DamagerType GetDamagerType(string spriteName)
        {
            if (spriteName.Contains("bush")) return DamagerType.bush;
            if (spriteName.Contains("sign")) return DamagerType.sign;
            if (spriteName == "rock_small_gray") return DamagerType.rockSmallGray;
            if (spriteName == "rock_big_gray") return DamagerType.rockBigGray;
            if (spriteName == "rock_small_black") return DamagerType.rockSmallBlack;
            if (spriteName == "rock_big_black") return DamagerType.rockBigBlack;
            return DamagerType.sword1;
        }

        public override void OnShieldBlock()
        {
            liftableComponent.CheckDestroy();
        }
    }
}
