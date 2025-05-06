namespace Royale2D
{
    public class Bee : Actor
    {
        bool isDead;
        Anim? enemyExplosion;
        ColliderComponent colliderComponent;

        public Bee(WorldSection section, FdPoint pos, Character? owner) : base(section, pos, "bee")
        {
            colliderComponent = AddComponent(new ColliderComponent(this, true, true, true));
            AddComponent(new ShadowComponent(this, ShadowType.Small));
            AddComponent(new ZComponent(this, 6));
            AddComponent(new VelComponent(this));
            AddComponent(new DamagerComponent(this, DamagerType.bee, owner));
            AddComponent(new DamagableComponent(this));
        }

        public override void Update()
        {
            base.Update();

            if (isDead)
            {
                frameSpeed = 0;
                colliderComponent.disabled = true;
                if (enemyExplosion == null)
                {
                    enemyExplosion = new Anim(this, GetRenderPos(), "enemy_explosion", new AnimOptions { soundName = "enemy dies" });
                    enemyExplosion.zLayerOffset = ZIndex.LayerOffsetActorAbove;
                }
                else if (enemyExplosion.frameIndex > 4 || enemyExplosion.isDestroyed)
                {
                    DestroySelf();
                }
            }
            else
            {
                PlaySound("bee", dontPlayIfExists: true);
            }
        }

        public override void OnDamageReceived(Damager damager, Character? attacker, ActorColliderInstance? attackerAci)
        {
            isDead = true;
        }

        public static void OnUseBottledBee(Character character)
        {
            FdPoint pos = character.GetCenterPos() + character.directionComponent.ForwardFdVec(12);
            new Bee(character.section, pos, character);
        }
    }
}
