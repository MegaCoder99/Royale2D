namespace Royale2D
{
    public class CollectableComponent : Component
    {
        public int healthGain;
        public int magicGain;
        public int rupeeGain;
        public int arrowGain;

        public int hideTime;
        public bool shouldFade;
        public int delay;
        public bool collected = false;

        public int time;
        
        public CollectableComponent(Actor actor, bool shouldFade, int delay = 0, int healthGain = 0, int magicGain = 0, int rupeeGain = 0, int arrowGain = 0)
            : base(actor)
        {
            this.shouldFade = shouldFade;
            this.healthGain = healthGain;
            this.magicGain = magicGain;
            this.rupeeGain = rupeeGain;
            this.arrowGain = arrowGain;
            this.delay = delay;
        }

        public override void Update()
        {
            base.Update();

            time++;

            if (shouldFade)
            {
                if (time > 10 * 60)
                {
                    hideTime++;
                    if (hideTime > 1)
                    {
                        actor.visible = !actor.visible;
                        hideTime = 0;
                    }
                }
                if (time > 13 * 60)
                {
                    actor.DestroySelf();
                }
            }
        }

        public void Collect(Character character)
        {
            if (time < delay) return;
            if (collected) return;
            character.Collect(this);
            actor.DestroySelf();
        }
    }
}
