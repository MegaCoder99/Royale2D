using Color = SFML.Graphics.Color;

namespace Royale2D
{
    public class SkinsMenu : Menu
    {
        public int cursorX;
        public new int cursorY;
        public List<List<List<string>>> skinGridPages = new List<List<List<string>>>();
        public int skinGridPageIndex;
        public List<List<string>> skinGrid => skinGridPages[skinGridPageIndex];
        public string selectedSkin => skinGridPages[skinGridPageIndex][cursorY][cursorX];

        public const int gridW = 20;
        public const int gridH = 24;
        public int gridRowCount;
        public int gridColCount;

        public MenuPos skinNamePos = new MenuPos(Game.HalfScreenW, 45);
        public MenuPos leftArrowPos = new MenuPos(9, 118);
        public MenuPos rightArrowPos = new MenuPos(Game.ScreenW - 9, 118);
        public int blinkFrames;

        public SkinsMenu(Menu prevMenu) : base(prevMenu)
        {
            title = "SKINS";
            footer = "X: Select, A/S: Prev/Next Page, Z: Back";

            gridRowCount = 6;
            gridColCount = 10;

            PopulateSkinGridPages(Assets.skins);

            startPos = new MenuPos(29, 64);

            devPositions = new List<MenuPos> { leftArrowPos, rightArrowPos };
        }

        public void PopulateSkinGridPages(List<string> skins)
        {
            int totalPages = (int)Math.Ceiling((double)skins.Count / (gridRowCount * gridColCount));
            skinGridPages = new List<List<List<string>>>(totalPages);

            for (int pageIndex = 0; pageIndex < totalPages; pageIndex++)
            {
                List<List<string>> gridPage = new List<List<string>>(gridRowCount);
                for (int row = 0; row < gridRowCount; row++)
                {
                    int startIndex = pageIndex * gridRowCount * gridColCount + row * gridColCount;
                    List<string> gridRow = new List<string>(gridColCount);
                    for (int col = 0; col < gridColCount; col++)
                    {
                        int currentIndex = startIndex + col;
                        if (currentIndex < skins.Count)
                        {
                            gridRow.Add(skins[currentIndex]);
                        }
                        else
                        {
                            gridRow.Add("");
                        }
                    }

                    gridPage.Add(gridRow);
                }

                skinGridPages.Add(gridPage);
            }
        }

        public override void Update()
        {
            base.Update();

            blinkFrames++;
            if (blinkFrames > 60)
            {
                blinkFrames = 0;
            }

            if (Game.input.IsPressed(Control.MenuLeft))
            {
                if (cursorX > 0)
                {
                    cursorX--;
                }
                else if (skinGridPageIndex > 0)
                {
                    skinGridPageIndex--;
                    cursorX = skinGrid[cursorY].Count - 1;
                }
            }
            else if (Game.input.IsPressed(Control.MenuRight))
            {
                if (cursorX < skinGrid[cursorY].Count - 1)
                {
                    cursorX++;
                    MoveOutOfEmptySlot(Direction.Left);
                }
                else if (skinGridPageIndex < skinGridPages.Count - 1)
                {
                    skinGridPageIndex++;
                    cursorX = 0;
                    MoveOutOfEmptySlot(Direction.Up);
                }
            }
            else if (Game.input.IsPressed(Control.MenuUp))
            {
                if (cursorY > 0)
                {
                    cursorY--;
                }
            }
            else if (Game.input.IsPressed(Control.MenuDown))
            {
                if (cursorY < skinGrid.Count - 1)
                {
                    cursorY++;
                    MoveOutOfEmptySlot(Direction.Left);
                }
            }
            else if (Game.input.IsPressed(Control.ItemLeft))
            {
                if (skinGridPageIndex > 0)
                {
                    skinGridPageIndex--;
                    cursorX = skinGrid[cursorY].Count - 1;
                }
                else
                {
                    cursorX = 0;
                }
            }
            else if (Game.input.IsPressed(Control.ItemRight))
            {
                if (skinGridPageIndex < skinGridPages.Count - 1)
                {
                    skinGridPageIndex++;
                    cursorX = 0;
                    MoveOutOfEmptySlot(Direction.Up);
                }
                else
                {
                    cursorX = skinGrid[cursorY].Count - 1;
                    MoveOutOfEmptySlot(Direction.Left);
                }
            }
            else if (Game.input.IsPressed(Control.MenuSelectPrimary))
            {
                Options.main.skin = selectedSkin;
                Options.main.SaveToFile();
                ChangeMenu(prevMenu);
            }

        }

        public void MoveOutOfEmptySlot(Direction moveDir)
        {
            while (skinGridPages[skinGridPageIndex][cursorY][cursorX] == "")
            {
                if (moveDir == Direction.Left)
                {
                    if (cursorX > 0)
                    {
                        cursorX--;
                    }
                    else
                    {
                        cursorY--;
                        cursorX = skinGrid[cursorY].Count - 1;
                    }
                }
                else if (moveDir == Direction.Up)
                {
                    if (cursorY > 0)
                    {
                        cursorY--;
                    }
                }
            }
        }

        public override void Render()
        {
            base.Render();
            for (int y = 0; y < gridRowCount; y++)
            {
                for (int x = 0; x < gridColCount; x++)
                {
                    if (skinGridPages[skinGridPageIndex][y][x] == "") continue;
                }
            }
            drawer.DrawText(selectedSkin, skinNamePos.x, skinNamePos.y, AlignX.Center);
            drawer.DrawRectWH(startPos.x + cursorX * gridW, startPos.y + cursorY * gridH, gridW, gridH, false, Color.Green, 1);
            if (blinkFrames < 30)
            {
                if (skinGridPageIndex > 0)
                    drawer.DrawText("<", leftArrowPos.x, leftArrowPos.y, AlignX.Center);
                if (skinGridPageIndex < skinGridPages.Count - 1) 
                    drawer.DrawText(">", rightArrowPos.x, rightArrowPos.y, AlignX.Center);
            }
        }
    }
}