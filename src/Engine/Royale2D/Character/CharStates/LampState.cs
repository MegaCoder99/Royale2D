namespace Royale2D
{
    public class LampState : CharState
    {
        public LampState(Character character) : base(character)
        {
            baseSpriteName = "char_idle";
            idleOnAnimEnd = true;
            magicCost = 2;
        }

        public override void Update()
        {
            base.Update();
            
            if (!once)
            {
                once = true;
                Point poiOffset = Point.Zero;
                if (character.dir == Direction.Left) poiOffset = new Point(-15, 0);
                else if (character.dir == Direction.Right) poiOffset = new Point(15, 0);
                else if (character.dir == Direction.Up) poiOffset = new Point(0, -15);
                else if (character.dir == Direction.Down) poiOffset = new Point(0, 15);
                Projectile proj = Projectiles.CreateLampProj(character, character.pos + poiOffset.ToFdPoint());
                character.PlaySound("fire");
            }
            if (time > 15)
            {
                character.ChangeState(new IdleState(character));
            }
        }

        public override bool CanChangeFrom(CharState oldState)
        {
            return oldState is IdleState;
        }
    }
}
