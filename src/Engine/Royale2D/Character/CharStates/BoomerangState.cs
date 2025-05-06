namespace Royale2D
{
    public class BoomerangState : CharState
    {
        bool isMagical;

        public BoomerangState(Character character, bool isMagical) : base(character)
        {
            baseSpriteName = "char_boomerang";
            idleOnAnimEnd = true;
            this.isMagical = isMagical;
        }

        public override void Update()
        {
            base.Update();
            
            if (character.frameIndex == 2 && !once)
            {
                once = true;
                if (character.boomerang == null)
                {
                    FdPoint move = Helpers.GetInputNormFdVec(input);
                    if (move.IsZero())
                    {
                        move = character.directionComponent.ForwardFdVec(1);
                    }

                    Boomerang boomerang = new Boomerang(character, move, isMagical);
                    character.boomerang = boomerang;
                }
            }
        }

        public override bool CanChangeFrom(CharState oldState)
        {
            return oldState is IdleState;
        }
    }
}
