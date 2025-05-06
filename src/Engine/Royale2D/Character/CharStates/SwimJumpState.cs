namespace Royale2D
{
    public class SwimJumpState : CharState
    {
        FdPoint lastLandPos;
        int state;
        bool skipJump;

        public SwimJumpState(Character character, FdPoint lastLandPos, bool skipJump) : base(character)
        {
            baseSpriteName = "char_hurt";
            this.lastLandPos = lastLandPos;
            this.skipJump = skipJump;
            enterSound = skipJump ? "" : "fall";
            canEnterAsBunny = true;
        }

        public override void Update()
        {
            base.Update();

            if (state == 0)
            {
                if (skipJump || character.zComponent.HasLanded())
                {
                    new Anim(character, character.pos, "splash_swim", new AnimOptions() { soundName = "water" });
                    visible = false;
                    state = 1;
                    time = 0;
                }
            }
            else if (state == 1)
            {
                if (time > 7)
                {
                    visible = true;
                    character.pos = lastLandPos;
                    character.damagableComponent.ApplyDamage(Damagers.damagers[DamagerType.water], null, null);
                    character.ChangeState(new IdleState(character));
                }
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();
            if (!skipJump)
            {
                character.zComponent = character.ResetComponent(new ZComponent(character, zVel: Fd.New(1, 0), useGravity: true));
                character.velComponent = character.ResetComponent(new VelComponent(character, character.directionComponent.ForwardFdVec(1)));
            }
        }

        public override bool CanChangeFrom(CharState oldState)
        {
            return oldState is not SwimState && oldState is not SwimJumpState;
        }
    }
}
