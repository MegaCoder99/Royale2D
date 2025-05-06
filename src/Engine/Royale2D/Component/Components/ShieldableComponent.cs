namespace Royale2D
{
    public class ShieldableComponent : Component
    {
        // Energy based projectiles are unblockable by blue shield, blocked by red shield and reflected by mirror shields
        // Non-energy based projectiles are blocked by all shields
        public bool energyBased;

        public VelComponent velComponent;
        public Character? owner;
        public bool wasReflected;

        public ShieldableComponent(Actor actor, Character? owner, bool energyBased, VelComponent velComponent) : base(actor)
        {
            this.owner = owner;
            this.energyBased = energyBased;
            this.velComponent = velComponent;
        }

        public override void OnActorCollision(ActorCollision collision)
        {
            base.OnActorCollision(collision);

            int shieldBlockType = ShieldBlockType(collision);
            if (shieldBlockType == 1)
            {
                actor.PlaySound("ding");
                actor.OnShieldBlock();
            }
            else if (shieldBlockType == 2)
            {
                if (!wasReflected)
                {
                    actor.PlaySound("ding");
                    wasReflected = true;
                    velComponent.vel *= -1;
                    if (GetComponent<DamagerComponent>() is DamagerComponent dc)
                    {
                        dc.attacker = collision.other.actor as Character;
                    }
                }
            }
            else if (ShouldFrozenEnemyBlock(collision))
            {
                actor.PlaySound("tink");
                actor.OnShieldBlock();
            }
        }

        // 0 = no block, 1 = block, 2 = reflect
        public int ShieldBlockType(ActorCollision collision)
        {
            if (!collision.mine.collider.isDamager) return 0;

            if (collision.other.actor is Character chr && chr != owner)
            {
                string tags = collision.other.collider.tags;

                bool isSidewaysShieldState = (chr.charState is SwordSwingState || chr.charState is PokeState || chr.charState is SpinAttackChargeState);
                bool isLeftBlockDir = isSidewaysShieldState ? chr.dir == Direction.Down : chr.dir == Direction.Left;
                bool isRightBlockDir = isSidewaysShieldState ? chr.dir == Direction.Up : chr.dir == Direction.Right;

                bool canBlock =
                    (chr.dir == Direction.Down && velComponent.vel.y < 0 && velComponent.vel.x.abs <= velComponent.vel.y.abs) ||
                    (chr.dir == Direction.Up && velComponent.vel.y > 0 && velComponent.vel.x.abs <= velComponent.vel.y.abs) ||
                    (isLeftBlockDir && velComponent.vel.x > 0 && velComponent.vel.y.abs <= velComponent.vel.x.abs) ||
                    (isRightBlockDir && velComponent.vel.x < 0 && velComponent.vel.y.abs <= velComponent.vel.x.abs);

                if (canBlock)
                {
                    // Blue shield can only block non energy-based
                    if (tags.Contains("shield1") && !energyBased) return 1;
                    // Mirror shield will reflect energy-based
                    if (tags.Contains("shield3") && energyBased) return 2;
                    // Both red and mirror shield will block non energy-based and energy-based
                    if (tags.Contains("shield2") || tags.Contains("shield3")) return 1;
                }
            }
            return 0;
        }

        public bool ShouldFrozenEnemyBlock(ActorCollision collision)
        {
            if (!collision.mine.collider.isDamager || !collision.other.collider.isDamagable) return false;
            if (collision.other.actor is Character chr && chr != owner && chr.charState is FrozenState)
            {
                if (collision.mine.actor.GetComponent<DamagerComponent>() is DamagerComponent dc && dc.damager.hitFrozen)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            return false;
        }
    }
}
