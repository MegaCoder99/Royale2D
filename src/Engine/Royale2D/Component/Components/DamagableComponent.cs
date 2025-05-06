namespace Royale2D
{
    public class DamagableComponent : Component
    {
        public Character? lastAttacker;
        public Dictionary<string, int> damageCooldowns = new Dictionary<string, int>();
        public int hurtInvulnTime;

        // TODO when shaders are ready, use it to flash, but disable and fall back to blink if shaders are disabled
        public bool flashOnHit;

        public DamagableComponent(Actor actor) : base(actor)
        {
        }

        public override void Update()
        {
            base.Update();
            if (hurtInvulnTime > 0)
            {
                hurtInvulnTime--;
            }
        }

        public bool ApplyDamage(Damager damager, Character? attacker, ActorColliderInstance? attackerAci)
        {
            if (!damager.selfDamage && attacker == actor) return false;
            if (hurtInvulnTime > 0) return false;
            if (damager.damageCooldown > 0)
            {
                if (damageCooldowns.ContainsKey(damager.name) && damageCooldowns[damager.name] > 0)
                {
                    damageCooldowns[damager.name]--;
                    return false;
                }
                else
                {
                    damageCooldowns[damager.name] = damager.damageCooldown;
                }
            }
            if (actor is Character chr)
            {
                if (chr.charState.intangible) return false;
                if (chr.charState is FrozenState && !damager.hitFrozen) return false;
            }

            lastAttacker = attacker ?? lastAttacker;
            actor.OnDamageReceived(damager, attacker, attackerAci);
            return true;
        }

        public override bool IsVisible()
        {
            if (!flashOnHit && hurtInvulnTime > 0) return hurtInvulnTime % 2 == 0;
            return true;
        }
    }
}
