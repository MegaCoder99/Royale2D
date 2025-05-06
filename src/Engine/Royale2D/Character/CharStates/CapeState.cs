namespace Royale2D
{
    public class CapeState : CharState
    {
        public CapeState(Character character) : base(character)
        {
            baseSpriteName = "char_idle";
            magicCost = 1;
        }

        public override void Update()
        {
            base.Update();

            if (time > 10)
            {
                ChangeState(new IdleState(character));
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();
            character.ToggleCape();
        }

        public override bool CanChangeFrom(CharState oldState)
        {
            return oldState is IdleState;
        }

        public static void OnUse(Character character)
        {
            if (character.magicCapeOn)
            {
                character.ToggleCape();
            }
            else
            {
                character.ChangeState(new CapeState(character));
            }
        }
    }
}
