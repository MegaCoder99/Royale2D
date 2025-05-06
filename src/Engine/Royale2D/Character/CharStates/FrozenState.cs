namespace Royale2D
{
    public class FrozenState : CharState
    {
        public bool shatter;

        public FrozenState(Character character) : base(character)
        {
            baseSpriteName = "char_idle";
            canEnterAsBunny = true;
        }

        public override void Update()
        {
            base.Update();
            
            if (time > 300)
            {
                character.ChangeState(new IdleState(character));
            }
        }

        public override void RenderUpdate()
        {
            base.RenderUpdate();
            if (time % 120 == 0)
            {
                Point randOffset = new Point(Helpers.RandomRange(-8, 8), Helpers.RandomRange(-8, 8));
                new Anim(character, character.pos + randOffset.ToFdPoint(), "particle_frozen_sparkle");
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();
            character.zComponent = character.ResetComponent(new ZComponent(character, startZ: Fd.New(0, 1), zVel: 1, useGravity: true));
            character.PlaySound("ice rod");
        }

        public override void OnExit()
        {
            base.OnExit();
            if (shatter)
            {
                new Anim(character, character.pos, "particle_ice_break", new AnimOptions { soundName = "rock break", host = character });
            }
            else
            {
                new Anim(character, character.pos.AddXY(0, character.sprite.frames[0].rect.h / 2), "particle_melt");
            }
        }

        /*
        public override bool CanChangeFrom(CharState oldState)
        {
            return oldState is IdleState;
        }
        */
    }
}
