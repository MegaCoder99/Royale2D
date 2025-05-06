namespace Royale2D
{
    public class IceRodState : CharState
    {
        public IceRodState(Character character) : base(character)
        {
            baseSpriteName = "char_icerod";
            idleOnAnimEnd = true;
            enterSound = "ice rod";
            magicCost = 4;
        }

        public override void Update()
        {
            base.Update();
        }

        public override Actor? GetProj(FdPoint pos, Direction dir)
        {
            return Projectiles.CreateIceRodProj(character);
        }
    }
}
