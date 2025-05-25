namespace Royale2D
{
    public class HurtState : CharState
    {
        public FdPoint recoilVel;

        public HurtState(Character character, FdPoint recoilUnitVec) : base(character)
        {
            baseSpriteName = "char_hurt";
            recoilVel = recoilUnitVec * 2;
            intangible = true;
            canEnterAsBunny = true;
        }

        public override void OnEnter()
        {
            base.OnEnter();
        }

        public override void Update()
        {
            base.Update();
            if (recoilVel.IsNonZero())
            {
                character.IncPos(recoilVel);
            }
            if (time > 15)
            {
                character.ChangeState(new IdleState(character));
            }
        }
    }
}
