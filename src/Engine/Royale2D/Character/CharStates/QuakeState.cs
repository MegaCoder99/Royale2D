namespace Royale2D
{
    public class QuakeState : CharState
    {
        Actor? quakeLightning;

        public QuakeState(Character character) : base(character)
        {
            baseSpriteName = "char_quake";
            magicCost = 8;
            enterSound = "medallion";
        }

        public override void Update()
        {
            base.Update();

            if (character.IsAnimOver())
            {
                if (quakeLightning == null)
                {
                    /*
                    foreach (var character in Global.game.characters)
                    {
                        if (character != actor && character.level == character.level && character.pos.distTo(character.pos) < 128)
                        {
                            Damager damager = new Damager(actor, Item.quake, 0);
                            damager.bunnify = true;
                            character.applyDamage(damager, Point.Zero);
                        }
                    }
                    */
                    character.PlaySound("ram");
                    character.PlaySound("quake 1");
                    quakeLightning = new Anim(character, character.pos.AddXY(-13, -6), "quake_lightning");
                }
                else
                {
                    character.cameraShakeComponent.Shake(1, 1, 3);
                }
            }
            if (quakeLightning != null && quakeLightning.IsAnimOver())
            {
                ChangeState(new IdleState(character));
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            if (quakeLightning != null) quakeLightning.DestroySelf();
        }

        public override bool CanChangeFrom(CharState oldState)
        {
            return oldState is IdleState;
        }
    }
}
