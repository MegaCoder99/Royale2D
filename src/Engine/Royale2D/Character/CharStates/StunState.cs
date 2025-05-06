namespace Royale2D
{
    public class StunState : CharState
    {
        public FdPoint recoilVel;
        public Fd distTravelled;

        public StunState(Character character, FdPoint recoilUnitVel) : base(character)
        {
            baseSpriteName = "char_idle";
            enterSound = "enemy hit";
            recoilVel = recoilUnitVel * 2;
            canEnterAsBunny = true;
        }

        public override void Update()
        {
            base.Update();

            distTravelled += recoilVel.Magnitude();
            if (distTravelled > 30)
            {
                recoilVel = FdPoint.Zero;
            }

            character.IncPos(recoilVel);

            if (time > 60)
            {
                character.ChangeState(new IdleState(character));
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();
            character.shakeComponent.disabled = false;
        }

        public override void OnExit()
        {
            base.OnExit();
            character.shakeComponent.disabled = true;
        }

        /*
        public override bool CanChangeFrom(CharState oldState)
        {
            return oldState is IdleState;
        }
        */
    }
}
