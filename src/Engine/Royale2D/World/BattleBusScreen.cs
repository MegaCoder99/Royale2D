namespace Royale2D
{
    // Every character has one of these
    public class BattleBusData
    {
        public float dropIFloat;
        public float dropJFloat;
        public int dropI => (int)dropIFloat;
        public int dropJ => (int)dropJFloat;
        public int droppingTime;
        public bool dropped;
        public SpriteInstance fallSpriteInstance = new SpriteInstance("char_fall");
    }

    public class BattleBusScreen : BigMinimapScreen
    {
        public int remainingTime = (Debug.main?.battleBusMaxTime ?? 30) * 60;
        public bool mainPlayerDropped;
        public int immediateDropGuard;

        public BattleBusScreen(World world, Storm storm) : base(world, storm, world.map.minimapSpriteData, "battle_bus_screen")
        {
            foreach (Character character in characters)
            {
                character.battleBusData.dropIFloat = gridHeight / 2;
                character.battleBusData.dropJFloat = gridWidth / 2;

                if (Debug.main?.skipBattleBus == true)
                {
                    character.battleBusData.dropped = true;
                }
            }
        }

        public void Update()
        {
            remainingTime--;
            if (remainingTime < 0) remainingTime = 0;

            for (int i = 0; i < characters.Count; i++)
            {
                Character character = characters[i];
                BattleBusData battleBusData = character.battleBusData;

                if (battleBusData.dropped)
                {
                    continue;
                }
                else if (battleBusData.droppingTime > 0)
                {
                    battleBusData.fallSpriteInstance.Update();
                    battleBusData.droppingTime++;
                    if (battleBusData.droppingTime > 90)
                    {
                        battleBusData.dropped = true;
                        int dropI = battleBusData.dropI;
                        int dropJ = battleBusData.dropJ;
                        WorldSection section = GetSectionFromGridCoords(dropI, dropJ);
                        if (section.mapSection.mainSectionChildPos != null)
                        {
                            dropI -= section.mapSection.mainSectionChildPos.Value.i;
                            dropJ -= section.mapSection.mainSectionChildPos.Value.j;
                        }
                        Point landPos = new Point(8 + (dropJ * 8), 8 + (dropI * 8));
                        character.pos = landPos.ToFdPoint();
                        character.ChangeState(new LandState(character));
                    }
                    continue;
                }

                if (remainingTime <= 0)
                {
                    if (!CanLand(battleBusData.dropI, battleBusData.dropJ))
                    {
                        MoveToNearestDroppableTile(battleBusData);
                    }
                    battleBusData.droppingTime = 1;
                    continue;
                }

                float cursorSpeed = world.mainSection.mapSection.pixelWidth / 2048f;

                if (character.input.IsHeld(Control.Left))
                {
                    battleBusData.dropJFloat -= cursorSpeed;
                    if (battleBusData.dropJFloat < 0) battleBusData.dropJFloat = 0;
                }
                else if (character.input.IsHeld(Control.Right))
                {
                    battleBusData.dropJFloat += cursorSpeed;
                    if (battleBusData.dropJFloat >= gridWidth) battleBusData.dropJFloat = gridWidth - 1;
                }

                if (character.input.IsHeld(Control.Up))
                {
                    battleBusData.dropIFloat -= cursorSpeed;
                    if (battleBusData.dropIFloat < 0) battleBusData.dropIFloat = 0;
                }
                else if (character.input.IsHeld(Control.Down))
                {
                    battleBusData.dropIFloat += cursorSpeed;
                    if (battleBusData.dropIFloat >= gridHeight) battleBusData.dropIFloat = gridHeight - 1;
                }

                if (immediateDropGuard > 5 && character.input.IsPressed(Control.Action) && CanLand(battleBusData.dropI, battleBusData.dropJ))
                {
                    battleBusData.droppingTime = 1;
                }
            }

            // Stops IsPressed and drop from being detected immediately from previous menu press if match loads quickly
            immediateDropGuard++;
        }

        public void Render(Drawer drawer)
        {
            world.map.RenderMinimap(drawer);

            minimap.Render(drawer, 0, 0);

            bool drawControls = false;

            for (int i = 0; i < characters.Count; i++)
            {
                Character character = characters[i];
                BattleBusData battleBusData = character.battleBusData;

                Point minimapPos = minimap.GetMinimapPos(new Point(battleBusData.dropJFloat * 8, battleBusData.dropIFloat * 8));
                float x = minimapPos.x;
                float y = minimapPos.y;

                if (character == world.specCharacter && battleBusData.droppingTime == 0 && !battleBusData.dropped)
                {
                    drawControls = true;
                    Assets.GetSprite("char_map_dungeon").Render(drawer, x, y, 0);
                    if (CanLand(battleBusData.dropI, battleBusData.dropJ))
                    {
                        Assets.GetSprite("map_good").Render(drawer, x + 1, y + 1, 0);
                    }
                    else
                    {
                        Assets.GetSprite("map_bad").Render(drawer, x + 1, y + 1, 0);
                    }
                }
                else if (battleBusData.droppingTime > 0 && !battleBusData.dropped)
                {
                    battleBusData.fallSpriteInstance.Render(drawer, x, y, default);
                }
            }

            if (drawControls)
            {
                drawer.DrawText("Select Landing Zone", 128, 0, AlignX.Center);
                drawer.DrawText("Seconds before auto-drop: " + (remainingTime / 60).ToString(), 128, Game.ScreenH - 8, AlignX.Center, AlignY.Bottom, fontType: FontType.Small);
                drawer.DrawText("Arrow Keys: Move, X: Select", 128, Game.ScreenH, AlignX.Center, AlignY.Bottom, fontType: FontType.Small);
            }
        }
    }
}
