using Shared;

namespace Royale2D
{
    public class DieState : CharState
    {
        Actor enemyExplosion;
        public int state;

        public DieState(Character character) : base(character)
        {
            baseSpriteName = "char_hurt";
            enterSound = "enemy dies";
            enemyExplosion = new Anim(character, character.GetRenderPos(), "enemy_explosion");
            intangible = true;
            canEnterAsBunny = true;
        }

        public override void Update()
        {
            base.Update();

            if (state == 0)
            {
                if (enemyExplosion.frameIndex >= 6)
                {
                    enemyExplosion.DestroySelf();
                    // character.DestroySelf();
                    visible = false;
                    foreach (Component component in character.components)
                    {
                        component.disabled = true;
                    }
                    state = 1;
                }
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();

            foreach (InventoryItem? item in character.inventory.items)
            {
                if (item != null)
                {
                    int angle = NetcodeSafeRng.RandomRange(0, 360);
                    Point point = new Point(MyMath.CosD(angle), MyMath.SinD(angle));
                    FieldItem fieldItem = new FieldItem(character, character.GetRenderPos(), item, point.ToFdPoint(), true);
                }
            }

            if (character.rupees.value > 0)
            {
                int angle = NetcodeSafeRng.RandomRange(0, 360);
                FdPoint point = new Point(MyMath.CosD(angle), MyMath.SinD(angle)).ToFdPoint();
                Collectables.CreateRupeeLoot(character.section, character.GetRenderPos(), point, (int)character.rupees.value);
            }

            if (character.arrows.value > 0)
            {
                int angle = NetcodeSafeRng.RandomRange(0, 360);
                FdPoint point = new Point(MyMath.CosD(angle), MyMath.SinD(angle)).ToFdPoint();
                Collectables.CreateArrowLoot(character.section, character.GetRenderPos(), point, (int)character.arrows.value);
            }
        }
    }
}
