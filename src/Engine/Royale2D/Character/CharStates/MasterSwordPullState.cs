using Shared;

namespace Royale2D
{
    public class MasterSwordPullState : CharState
    {
        Actor? origSword;
        Actor? spark1;
        Actor? spark2;
        Actor? sword;
        int iterCount;
        bool frameAlt;
        int state;
        int stateTime;
        MasterSwordWoods masterSwordWoods;
        float iterTime;

        public MasterSwordPullState(Character character, MasterSwordWoods masterSwordWoods) : base(character)
        {
            baseSpriteName = "char_grab";
            this.masterSwordWoods = masterSwordWoods;
            superArmor = true;
        }

        public override void Update()
        {
            base.Update();
            stateTime++;

            if (state == 0)
            {
                if (stateTime > 210)
                {
                    state = 1;
                    stateTime = 0;
                    spark1?.DestroySelf();
                    spark1 = null;
                    spark2 = new Anim(character, character.pos.AddXY(0, 15), "particle_pull_sword_2");
                }
            }
            else if (state == 1)
            {
                if (stateTime > 210)
                {
                    state = 2;
                    stateTime = 0;
                    character.frameSpeed = 1;
                    character.sprite.wrapMode = "once";
                }
            }
            else if (state == 2)
            {
                if (stateTime > 30 && sword == null && !once)
                {
                    once = true;
                    origSword?.DestroySelf();
                    origSword = null;
                    spark2?.DestroySelf();
                    spark2 = null;
                    character.PlaySound("sword shine 1");
                    sword = new Actor(character, character.pos.AddXY(5, -12), "master_sword_flash");
                    character.baseSpriteName = "char_item_get";
                    masterSwordWoods.Pull();
                    int slot = character.inventory.GetEmptySlot();
                    character.inventory.AddItem(new InventoryItem(ItemType.sword2), slot);
                    // Global.game.killFeed.Add(new KillFeedEntry(character.playerName + " claimed the Master Sword"));
                }
                if (stateTime > 60)
                {
                    state = 3;
                    stateTime = 0;
                }
            }
            else if (state == 3)
            {
                if (stateTime > 240)
                {
                    state = 4;
                    stateTime = 0;
                    ChangeState(new IdleState(character));
                }
            }
        }

        public override void Render(Drawer drawer)
        {
            base.Render(drawer);
            if (state == 1)
            {
                Point origin = character.pos.AddXY(0, 15).ToFloatPoint();
                SFML.Graphics.Color lineCol = SFML.Graphics.Color.White;
                iterTime += Game.spfConst;
                if (iterTime > 0.1)
                {
                    iterCount++;
                    if (iterCount > 2) iterCount = 0;
                    frameAlt = !frameAlt;
                    iterTime = 0;
                }

                float mag = 5;
                if (stateTime > 1) mag = 10;
                if (stateTime > 2) mag = 15;
                if (stateTime > 3) mag = 20;

                for (int i = 0; i < 8; i++)
                {
                    if (i % 2 != (frameAlt ? 1 : 0)) continue;
                    float xDist = mag * MyMath.CosD(i * 45);
                    float yDist = mag * MyMath.SinD(i * 45);
                    int iters = iterCount + (i % 2);
                    Point start = new Point(origin.x + xDist * iters, origin.y + yDist * iters);
                    Point end = new Point(origin.x + xDist * (iters + 1), origin.y + yDist * (iters + 1));

                    drawer.DrawLine(origin.x, origin.y, end.x, end.y, lineCol, 1, zIndex: ZIndex.FxGlobalAbove);
                }
            }
            else if (state == 2)
            {
                float alpha = -1;
                if (stateTime < 0.5)
                {
                    alpha = stateTime * 2;
                }
                else if (stateTime >= 0.5 && stateTime < 1)
                {
                    alpha = (1 - stateTime) * 2;
                }
                if (alpha != -1)
                {
                    if (character.IsSpecChar())
                    {
                        drawer.DrawRect(
                            drawer.pos.x - Game.HalfScreenW,
                            drawer.pos.y - Game.HalfScreenH,
                            drawer.pos.x + Game.HalfScreenW,
                            drawer.pos.y + Game.HalfScreenH,
                            true,
                            new SFML.Graphics.Color(255, 255, 255, (byte)(int)(alpha * 255)),
                            1,
                            zIndex: ZIndex.FxGlobalAbove
                        );
                    }
                }
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();
            spark1 = new Anim(character, character.pos.AddXY(0, 15), "particle_pull_sword_1");
            character.frameSpeed = 0;
            masterSwordWoods.isPulling = true;
        }

        public override void OnExit()
        {
            base.OnExit();
            spark1?.DestroySelf();
            spark2?.DestroySelf();
            sword?.DestroySelf();
            masterSwordWoods.isPulling = false;
        }

        public override bool CanChangeFrom(CharState oldState)
        {
            return oldState is IdleState;
        }
    }
}
