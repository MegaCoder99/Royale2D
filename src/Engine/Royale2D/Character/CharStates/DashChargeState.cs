namespace Royale2D
{
    public class DashChargeState : CharState
    {
        int particleTime;

        public DashChargeState(Character character) : base(character)
        {
            baseSpriteName = "char_dash_charge";
        }

        public override void Update()
        {
            base.Update();

            character.PlaySound("pegasus boots", true);

            if (!input.IsHeld(Control.Action))
            {
                character.ChangeState(new IdleState(character));
            }
            else if (character.IsAnimOver())
            {
                character.ChangeState(new DashState(character));
            }
        }

        public override void RenderUpdate()
        {
            base.RenderUpdate();

            particleTime++;
            if (particleTime >= 5)
            {
                particleTime = 0;
                once = !once;
                float xOnceOff = once ? 8 : 0;
                float xOff = 0;
                if (character.dir == Direction.Up) xOff = -4;
                else if (character.dir == Direction.Down) xOff = -4;
                else if (character.dir == Direction.Right) xOff = -9;
                else if (character.dir == Direction.Left) xOff = 0;
                Point origin = character.pos.ToFloatPoint().AddXY(xOnceOff + xOff, 10);
                new Anim(character, origin.ToFdPoint(), "dust", new AnimOptions { isParticle = true });
            }
        }
    }
}
