namespace Royale2D
{
    // Every character has one of these
    public class FluteScreenData
    {
        public int dropI;
        public int dropJ;
        public bool isInFluteScreen;
        public int fluteBirdXDir = 1;
        public SpriteInstance fluteBirdSpriteInstance = new SpriteInstance("flute_bird") { frameSpeed = Fd.Point5 };
    }

    public class FluteScreen : BigMinimapScreen
    {
        public int remainingTime = 30 * 60;

        public FluteScreen(World world, Storm storm) : base(world, storm, world.map.minimapSpriteData, "flute_screen")
        {
        }

        public void Update()
        {
            remainingTime--;
            if (remainingTime < 0) remainingTime = 0;

            for (int i = 0; i < characters.Count; i++)
            {
                Character character = characters[i];
                FluteScreenData fluteScreenData = character.fluteScreenData;

                if (!fluteScreenData.isInFluteScreen)
                {
                    continue;
                }
                else
                {
                    
                }

                fluteScreenData.fluteBirdSpriteInstance.Update();

                if (remainingTime <= 0)
                {
                    /*
                    if (!CanLand(battleBusData.dropI, battleBusData.dropJ))
                    {
                        MoveToNearestDroppableTile(battleBusData);
                    }
                    battleBusData.droppingTime = 1;
                    continue;
                    */
                }

                if (character.input.IsHeld(Control.Left))
                {
                    fluteScreenData.dropJ -= 2;
                    if (fluteScreenData.dropJ < 0) fluteScreenData.dropJ = 0;
                    fluteScreenData.fluteBirdXDir = -1;
                }
                else if (character.input.IsHeld(Control.Right))
                {
                    fluteScreenData.dropJ += 2;
                    if (fluteScreenData.dropJ >= gridWidth) fluteScreenData.dropJ = gridWidth - 1;
                    fluteScreenData.fluteBirdXDir = 1;
                }

                if (character.input.IsHeld(Control.Up))
                {
                    fluteScreenData.dropI -= 2;
                    if (fluteScreenData.dropI < 0) fluteScreenData.dropI = 0;
                }
                else if (character.input.IsHeld(Control.Down))
                {
                    fluteScreenData.dropI += 2;
                    if (fluteScreenData.dropI >= gridHeight) fluteScreenData.dropI = gridHeight - 1;
                }

                if (character.input.IsPressed(Control.Action) && CanLand(fluteScreenData.dropI, fluteScreenData.dropJ))
                {
                    fluteScreenData.isInFluteScreen = false;
                }
            }
        }

        public void Render(Drawer drawer)
        {
            world.map.RenderMinimap(drawer);

            minimap.Render(drawer, 0, 0);

            bool drawControls = false;

            for (int i = 0; i < characters.Count; i++)
            {
                Character character = characters[i];
                FluteScreenData fluteScreenData = character.fluteScreenData;

                Point minimapPos = minimap.GetMinimapPos(new Point(fluteScreenData.dropJ * 8, fluteScreenData.dropI * 8));
                float x = minimapPos.x;
                float y = minimapPos.y;

                if (character.IsSpecChar())
                {
                    drawControls = true;
                    fluteScreenData.fluteBirdSpriteInstance.Render(drawer, x, y, default, xDir: fluteScreenData.fluteBirdXDir);

                    if (CanLand(fluteScreenData.dropI, fluteScreenData.dropJ))
                    {
                        Assets.GetSprite("map_good").Render(drawer, x + 1, y + 1, 0);
                    }
                    else
                    {
                        Assets.GetSprite("map_bad").Render(drawer, x + 1, y + 1, 0);
                    }
                }
            }

            if (drawControls)
            {
                drawer.DrawText("Select Landing Spot", 128, 0, AlignX.Center);
                drawer.DrawText("Seconds before auto-land: " + ((int)remainingTime).ToString(), 128, Game.ScreenH - 8, AlignX.Center, AlignY.Bottom, fontType: FontType.Small);
                drawer.DrawText("Arrow Keys: Move, X: Select", 128, Game.ScreenH, AlignX.Center, AlignY.Bottom, fontType: FontType.Small);
            }
        }
    }
}
