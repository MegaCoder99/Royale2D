namespace Royale2D
{
    public class BurnableComponent : Component
    {
        public SpriteInstance burnSpriteInstance;
        public int burnTime;
        public Character? arsonist;
        public DamagerType? damagerType;

        public BurnableComponent(Actor actor) : base(actor)
        {
            burnSpriteInstance = new SpriteInstance("flame_burn");
        }

        public override void Update()
        {
            base.Update();

            if (burnTime > 0)
            {
                burnSpriteInstance.Update();
                burnTime--;
                
                if (damagerType != null && actor.GetComponent<DamagableComponent>() is DamagableComponent dc)
                {
                    Damager damager = Damagers.damagers[damagerType.Value];

                    // We want to re-use the same base damager as the original projectile, but we don't want it to re-burn again every time it applies burn damage
                    damager.burnTime = 0;
                    damager.flinch = false;
                    damager.damageCooldown = 60;

                    dc.ApplyDamage(damager, arsonist, null);
                }
            }
        }

        public override void Render(Drawer drawer)
        {
            base.Render(drawer);
            if (burnTime > 0)
            {
                Point renderPos = actor.GetRenderFloatPos();
                burnSpriteInstance.Render(drawer, renderPos.x, renderPos.y, actor.GetRenderZIndex(ZIndex.DrawboxOffsetBurn));
            }
        }

        public void Burn(int burnTime, DamagerType damagerType, Character? arsonist)
        {
            this.burnTime = burnTime;
            this.damagerType = damagerType;
            this.arsonist = arsonist;
        }
    }
}
