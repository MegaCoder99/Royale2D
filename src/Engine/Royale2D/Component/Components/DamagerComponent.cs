namespace Royale2D
{
    public class DamagerComponent : Component
    {
        public DamagerType damagerType;
        public Character? attacker; // This is not always the same as the component's actor, which has the actual hitboxes. It is the character that initiated the damage and will get kill credit, if any

        public Damager damager => Damagers.damagers[damagerType];

        public DamagerComponent(Actor actor, DamagerType damagerType, Character? attacker) : base(actor)
        {
            this.damagerType = damagerType;
            this.attacker = attacker;
        }

        public override void OnActorCollision(ActorCollision collision)
        {
            base.OnActorCollision(collision);

            if (disabled) return;
            if (!collision.mine.collider.isDamager || !collision.other.collider.isDamagable) return;

            // By convention, the attacker is always the one initiating the collision check
            ApplyDamageFromActorCollision(collision.mine, collision.other);
        }

        private void ApplyDamageFromActorCollision(ActorColliderInstance attackerAci, ActorColliderInstance victimAci)
        {
            if (victimAci.actor.GetComponent<DamagableComponent>() is DamagableComponent dc)
            {
                bool appliedDamage = dc.ApplyDamage(damager, attacker, attackerAci);
                if (appliedDamage)
                {
                    actor.OnDamageDealt();
                }
            }
        }
    }
}
