using Shared;

namespace Royale2D
{
    public class SpinAttackState : CharState
    {
        int particleTime;
        bool spinParticleEnd;

        public SpinAttackState(Character character) : base(character)
        {
            baseSpriteName = "char_spin";
            disableFlipX = true;
            idleOnAnimEnd = true;
            damagerType = character.inventory.GetSwordDamagerType();
        }

        public override void Update()
        {
            base.Update();
        }

        public override void RenderUpdate()
        {
            float startAngle = 0;
            if (character.directionComponent.direction == Direction.Up)
            {
                startAngle = 90;
            }
            else if (character.directionComponent.direction == Direction.Down)
            {
                startAngle = 270;
            }
            else if (character.directionComponent.direction == Direction.Left)
            {
                startAngle = 0;
            }
            else if (character.directionComponent.direction == Direction.Right)
            {
                startAngle = 180;
            }
            Point offset;
            float animTime = MyMath.ClampMin((character.totalFrameTime - 6) / 60f, 0);
            float totalAnimTime = (character.sprite.GetAnimDuration() - 6) / 60f;
            offset.x = 20 * MyMath.CosD(startAngle + 420 * (animTime / totalAnimTime));
            offset.y = 20 * MyMath.SinD(startAngle + 420 * (animTime / totalAnimTime));

            particleTime++;
            if (particleTime > 3)
            {
                Point origin = character.pos.ToFloatPoint();
                FdPoint animPos = (origin + offset).ToFdPoint();
                particleTime = 0;
                new Anim(character, animPos, "spin_particle", new AnimOptions { isParticle = true });
                if (!once)
                {
                    once = true;
                    new Anim(character, animPos, "spin_particle_start");
                    character.PlaySound("spin attack");
                }
                if (!spinParticleEnd && animTime / totalAnimTime >= 1)
                {
                    spinParticleEnd = true;
                    new Anim(character, animPos, "spin_particle_end");
                }
            }
        }

        public override bool CanChangeFrom(CharState oldState)
        {
            return oldState is SpinAttackChargeState;
        }
    }
}
