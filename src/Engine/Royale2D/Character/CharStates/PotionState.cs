namespace Royale2D
{
    public class PotionState : CharState
    {
        bool healMagic;
        bool healLife;

        public PotionState(Character character, bool healMagic, bool healLife) : base(character)
        {
            baseSpriteName = "char_prayer";
            this.healMagic = healMagic;
            this.healLife = healLife;
        }

        public override void Update()
        {
            base.Update();
            if (healMagic && character.magic.IsChanging())
            {
                return;
            }
            if (healLife && character.health.IsChanging())
            {
                return;
            }
            ChangeState(new IdleState(character));
        }

        public override void OnEnter()
        {
            base.OnEnter();
            if (healMagic)
            {
                character.magic.FillMax();
            }
            if (healLife)
            {
                character.health.FillMax();
            }
        }
    }
}
