namespace Royale2D
{
    public class CaneBlock : Actor
    {
        public Character owner;
        public int health = 4;

        public ZComponent zComponent;
        public VelComponent velComponent;
        public ColliderComponent colliderComponent;
        public LiftableComponent liftableComponent;
        public DamagerComponent damagerComponent;
        public ShieldableComponent shieldableComponent;

        public CaneBlock(Character creator, FdPoint pos) : base(creator, pos, "somaria_block")
        {
            colliderComponent = AddComponent(new ColliderComponent(this, true, true));
            zComponent = AddComponent(new ZComponent(this, bounce: true, playLandSound: true));
            velComponent = AddComponent(new VelComponent(this));
            damagerComponent = AddComponent(new DamagerComponent(this, DamagerType.caneBlock, creator), true);
            liftableComponent = AddComponent(new LiftableComponent(this, zComponent, velComponent, colliderComponent, Fd.New(1, 50), 1, false, false));
            shieldableComponent = AddComponent(new ShieldableComponent(this, creator, false, velComponent));

            owner = creator;
        }

        public override void Update()
        {
            base.Update();
            if (health <= 0)
            {
                //onBreak();
            }

            if (!liftableComponent.isThrown)
            {
                damagerComponent.disabled = true;
            }
            else
            {
                damagerComponent.disabled = false;
            }
        }

        public override void OnDamageDealt()
        {
            base.OnDamageDealt();
            // projColliderComponent.BounceBack();
        }

        public void Split()
        {
            DestroySelf(() => {
                Anim caneBreak = new Anim(owner, pos, "somaria_spawn");
                Projectiles.CreateCaneBlockProj(owner, pos, Direction.Left);
                Projectiles.CreateCaneBlockProj(owner, pos, Direction.Right);
                Projectiles.CreateCaneBlockProj(owner, pos, Direction.Up);
                Projectiles.CreateCaneBlockProj(owner, pos, Direction.Down);
            });
        }

        public override void OnShieldBlock()
        {
            colliderComponent.projMoveStrategy?.BounceBack();
        }

        /*
        public void onBreak()
        {
            Anim caneBreak = new Anim(owner, pos, "SomariaBreak");
            owner.caneBlock = null;
            DestroySelf();
        }

        public void deductHealth(int amount)
        {
            health -= amount;
            if (health <= 0)
            {
                onBreak();
            }
        }
        */
    }
}
