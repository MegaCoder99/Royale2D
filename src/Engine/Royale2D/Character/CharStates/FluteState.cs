namespace Royale2D
{
    public class FluteState : CharState
    {
        public FluteState(Character character) : base(character)
        {
            baseSpriteName = "char_prayer";
            enterSound = "flute 1";
        }

        public override void Update()
        {
            base.Update();
            // REFACTOR use sound and stop it instead on exit
            if (time > 180)
            {
                FluteBird.New(character, false);
                ChangeState(new IdleState(character));
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            character.world.soundManager.StopSound("flute 1", character);
        }
    }
}
