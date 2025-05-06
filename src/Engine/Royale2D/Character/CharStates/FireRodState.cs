namespace Royale2D
{
    public class FireRodState : CharState
    {
        public FireRodState(Character character) : base(character)
        {
            baseSpriteName = "char_firerod";
            idleOnAnimEnd = true;
            enterSound = "fire rod";
            magicCost = 4;
        }

        public override void Update()
        {
            base.Update();
        }

        public override Actor? GetProj(FdPoint pos, Direction dir)
        {
            return Projectiles.CreateFireRodProj(character);
        }

        public override bool CanChangeFrom(CharState oldState)
        {
            return oldState is IdleState;
        }
    }
}
