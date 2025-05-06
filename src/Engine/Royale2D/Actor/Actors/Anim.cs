namespace Royale2D
{
    public class AnimOptions
    {
        public string soundName = "";
        public int? framesToLive;
        public Direction? direction;
        public Actor? host;
        public bool isParticle;
        public float? fadeTime;
        public FdPoint? vel;
    }

    public class Anim : Actor
    {
        public int? framesToLive;
        public float? fadeTime;
        Actor? host;
        public FdPoint? vel;
        public bool isParticle;

        private void ConstructorCode(WorldSection section, AnimOptions animParams)
        {
            framesToLive = animParams.framesToLive;
            host = animParams.host;
            isParticle = animParams.isParticle;
            fadeTime = animParams.fadeTime;
            vel = animParams.vel;

            if (animParams.soundName != "")
            {
                PlaySound(animParams.soundName);
            }
            if (animParams.direction != null)
            {
                AddComponent(new DirectionComponent(this, animParams.direction.Value));
            }
            if (isParticle)
            {
                section.particleEffects.Add(this);
            }
        }

        public Anim(Actor creator, FdPoint pos, string spriteName, AnimOptions? animParams = null) :
            base(creator, pos, spriteName, addToSection: animParams?.isParticle != true)
        {
            ConstructorCode(creator.section, animParams ?? new AnimOptions());
        }

        public Anim(WorldSection section, FdPoint pos, string spriteName, AnimOptions? animParams = null) :
            base(section, pos, spriteName, addToSection: animParams?.isParticle != true)
        {
            ConstructorCode(section, animParams ?? new AnimOptions());
        }

        public override void Update()
        {
            base.Update();

            if (host != null)
            {
                pos += host.deltaPos;
            }

            if (vel != null)
            {
                pos += vel.Value;
            }

            if (fadeTime != null)
            {
                alpha -= (1 / (fadeTime.Value * 60));
            }

            if (framesToLive != null)
            {
                framesToLive--;
                if (framesToLive <= 0)
                {
                    DestroyHelper();
                }
            }
            else if (spriteInstance.IsAnimOver())
            {
                DestroyHelper();
            }
        }

        public void DestroyHelper()
        {
            if (!isParticle)
            {
                DestroySelf();
            }
            else
            {
                section.particleEffects.Remove(this);
            }
        }
    }
}
