namespace Royale2D
{
    public class Cucco : Actor
    {
        bool isPanic;
        public ColliderComponent colliderComponent;
        public ShadowComponent shadowComponent;
        public VelComponent velComponent;
        public DamagerComponent damagerComponent;
        public DamagableComponent damagableComponent;
        public LiftableComponent liftableComponent;
        public ShieldableComponent shieldableComponent;
        public WanderComponent wanderComponent;

        public Cucco(WorldSection section, FdPoint pos) : base(section, pos, "cucco_idle")
        {
            colliderComponent = AddComponent(new ColliderComponent(this, true, true, true));
            shadowComponent = AddComponent(new ShadowComponent(this, ShadowType.Large), true);
            var zComponent = AddComponent(new ZComponent(this, customGravity: -Fd.New(0, 7)));
            velComponent = AddComponent(new VelComponent(this));
            damagerComponent = AddComponent(new DamagerComponent(this, DamagerType.cuccoThrow, null));
            damagableComponent = AddComponent(new DamagableComponent(this));
            liftableComponent = AddComponent(new LiftableComponent(this, zComponent, velComponent, colliderComponent, Fd.New(3, 50), Fd.New(0, 50), false, false));
            shieldableComponent = AddComponent(new ShieldableComponent(this, null, false, velComponent));
            wanderComponent = AddComponent(new WanderComponent(this, colliderComponent, speed: Fd.New(0, 75), timeToPause: 60, maxStrayDist: 20));
        }

        public override void PreUpdate()
        {
            base.PreUpdate();

            damagerComponent.disabled = true;
            damagableComponent.disabled = true;
            shadowComponent.disabled = true;
            wanderComponent.disabled = true;

            if (liftableComponent.isLifted)
            {
                baseSpriteName = "cucco_panic_lifted";
                PlaySound("cucco", true);
            }
            else if (liftableComponent.isThrown)
            {
                shadowComponent.disabled = false;
                damagerComponent.disabled = false;
                damagerComponent.attacker = liftableComponent.lastThrower;
                shieldableComponent.owner = liftableComponent.lastThrower;
                baseSpriteName = "cucco_fly";
            }
            else if (isPanic)
            {
                damagableComponent.disabled = false;
                baseSpriteName = "cucco_panic";
            }
            else if (wanderComponent.isPaused)
            {
                damagableComponent.disabled = false;
                baseSpriteName = "cucco_idle";
                wanderComponent.disabled = false;
            }
            else
            {
                damagableComponent.disabled = false;
                baseSpriteName = "cucco_move";
                wanderComponent.disabled = false;
            }
        }

        public override void OnDamageDealt()
        {
            base.OnDamageDealt();
            PlaySound("cucco");
            colliderComponent.projMoveStrategy?.BounceBack();
        }

        public override void OnDamageReceived(Damager damager, Character? attacker, ActorColliderInstance? attackerAci)
        {
            damagableComponent.hurtInvulnTime = 60;
            isPanic = true;
            PlaySound("cucco");
            PlaySound("enemy hit small");
        }

        public override void OnShieldBlock()
        {
            colliderComponent.projMoveStrategy?.BounceBack();
        }
    }
}
