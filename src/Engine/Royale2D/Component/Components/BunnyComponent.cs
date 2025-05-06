namespace Royale2D
{
    public class BunnyComponent : Component
    {
        public int bunnyTime;
        public Character? bunnifier;
        public PoofComponent poofComponent;
        public string baseSpriteName;

        public WorldSection section => actor.layer.section;
        public bool isBunny => bunnyTime > 0;

        public BunnyComponent(Actor actor, PoofComponent poofComponent, string baseSpriteName) : base(actor)
        {
            this.poofComponent = poofComponent;
            this.baseSpriteName = baseSpriteName;
        }

        public override void Update()
        {
            base.Update();
            if (bunnyTime > 0)
            {
                bunnyTime--;
                if (bunnyTime == 0)
                {
                    bunnyTime = 0;
                    poofComponent.Unpoof();
                }
            }

            if (section.world.storm.IsPosInStorm(actor.pos, section))
            {
                Bunnify(5, null);
                if (GetComponent<DamagableComponent>() is DamagableComponent damagableComponent)
                {
                    damagableComponent.ApplyDamage(Damagers.damagers[DamagerType.storm], null, null);
                }
            }
        }

        public void Bunnify(int bunnyTime, Character? bunnifier)
        {
            if (this.bunnyTime == 0)
            {
                poofComponent.Poof();
            }
            this.bunnyTime = bunnyTime;
            this.bunnifier = bunnifier;
            if (actor is Character chr && !chr.charState.canEnterAsBunny)
            {
                chr.ChangeState(new IdleState(chr));
            }
        }
    }
}
