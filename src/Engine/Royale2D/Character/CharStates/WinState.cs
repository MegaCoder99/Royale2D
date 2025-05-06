namespace Royale2D
{
    public class WinState : CharState
    {
        public WinState(Character character) : base(character)
        {
            baseSpriteName = "char_win";
            canEnterAsBunny = true;
            intangible = true;
        }

        public override void Update()
        {
            base.Update();
        }

        public override void OnEnter()
        {
        }

        public override void OnExit()
        {
        }

        public override bool CanChangeFrom(CharState oldState)
        {
            return oldState is IdleState;
        }
    }
}
