namespace Royale2D
{
    public class BigFairy : Actor
    {
        public HealSparkles? healSparkles;
        public bool fade;
        public int fadeTime;

        public BigFairy(WorldSection section, FdPoint pos) :
            base(section, pos, "big_fairy")
        {
            AddComponent(new ZComponent(this, 15));
            AddComponent(new ColliderComponent(this));
        }

        public override void Update()
        {
            base.Update();

            if (healSparkles == null)
            {
                healSparkles = new HealSparkles(this, pos.AddXY(0, -15));
            }

            if (fadeTime > 0)
            {
                fadeTime++;
                if (fadeTime > 120)
                {
                    DestroySelf();
                }
            }
        }

        public override void RenderUpdate()
        {
            base.RenderUpdate();
            if (fadeTime > 60)
            {
                float fadeFactor = (fadeTime - 60) / 60f;
                alpha = 1 - fadeFactor;
            }
        }

        public void Heal(Character character)
        {
            if (fade) return;
            if (healSparkles != null) healSparkles.targetChar = character;
            fade = true;
            fadeTime = 1;
            PlaySound("fairy4x");
        }
    }

    public class HealSparkles : Actor
    {
        public int stateTime;
        public Character? targetChar;

        public HealSparkles(Actor creator, FdPoint pos) : base(creator, pos, "empty")
        {
        }

        public override void Update()
        {
            base.Update();

            if (targetChar != null)
            {
                if (MoveToPos(targetChar.pos, Fd.New(1, 50)) && stateTime == 0)
                {
                    targetChar.health.FillMax();
                    targetChar.magic.FillMax();
                    stateTime = 1;
                }
            }

            if (stateTime > 0)
            {
                stateTime++;
                if (stateTime > 120)
                {
                    DestroySelf();
                    return;
                }
            }
        }

        public override void RenderUpdate()
        {
            base.RenderUpdate();
            Point randPos = new Point(Helpers.RandomRange(-15, 15), Helpers.RandomRange(-15, 15));
            new Anim(this, pos + randPos.ToFdPoint(), "sword_sparkle", new AnimOptions { isParticle = true });
        }
    }
}
