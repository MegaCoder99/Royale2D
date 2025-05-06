namespace Royale2D
{
    public class BowState : CharState
    {
        public BowState(Character character) : base(character)
        {
            baseSpriteName = "char_bow";
            idleOnAnimEnd = true;
        }

        public override void Update()
        {
            base.Update();
            FdPoint? poi = character.GetFirstPOIOrNull();
            if (poi != null && !once)
            {
                once = true;
                Projectiles.CreateArrowProj(character);
            }
        }

        public override bool CanChangeFrom(CharState oldState)
        {
            return oldState is IdleState;
        }

        public override void OnEnter()
        {
            base.OnEnter();
            character.arrows.value--;
        }

        public override string GetChangeToError()
        {
            if (character.arrows.value <= 0)
            {
                return "No arrows";
            }
            return "";
        }
    }
}
