namespace Royale2D
{
    public class MagicPowderState : CharState
    {
        public MagicPowderState(Character character) : base(character)
        {
            baseSpriteName = "char_powder";
            enterSound = "magic powder 1";
            idleOnAnimEnd = true;
            magicCost = 2;
        }

        public override void Update()
        {
            base.Update();
        }

        public override Actor? GetProj(FdPoint pos, Direction dir)
        {
            return Projectiles.CreatePowderProj(character); 
        }

        public override bool CanChangeFrom(CharState oldState)
        {
            return oldState is IdleState;
        }
    }
}
