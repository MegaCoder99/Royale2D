using SFML.Window;

namespace Royale2D
{
    public abstract class Menu
    {
        public int cursorY;
        public int ySpacing = 20;
        public List<MenuOption> menuOptions = new List<MenuOption>();
        public bool areOptionsReadOnly;
        public string title = "";
        public string footer = "";
        public Menu? prevMenu;
        public string backgroundTextureName = "";
        public bool inGame;
        public SpriteInstance fairyCursor;

        public Drawer drawer => Game.menuDrawer;

        public int cursorXOff = 14;
        public MenuPos startPos = new MenuPos(50, 50);
        public MenuPos titlePos = new MenuPos(Game.HalfScreenW, 19);
        public MenuPos footerPos = new MenuPos(Game.HalfScreenW, 220);

        public Gui? gui;

        public List<MenuPos> devPositions = new List<MenuPos>();
        public int devPosIndex;

        public static Menu? current;
        public int time;

        public Menu(Menu? prevMenu = null)
        {
            this.prevMenu = prevMenu;
            devPositions = new List<MenuPos> { footerPos, titlePos, startPos };
            backgroundTextureName = "secondary_menu";
            fairyCursor = new SpriteInstance("cursor");
        }

        public virtual void Update()
        {
            fairyCursor.Update();

            if (!areOptionsReadOnly)
            {
                if (Game.input.IsPressed(Control.MenuDown))
                {
                    if (cursorY < menuOptions.Count - 1)
                    {
                        cursorY++;
                        Game.PlaySound("cursor");
                    }
                }
                else if (Game.input.IsPressed(Control.MenuUp))
                {
                    if (cursorY > 0)
                    {
                        cursorY--;
                        Game.PlaySound("cursor");
                    }
                }
            }

            if (Game.input.IsPressed(Control.MenuBack) && prevMenu != null)
            {
                OnBack();
                ChangeMenu(prevMenu);
            }

            if (menuOptions.Count > 0)
            {
                for (int i = 0; i < menuOptions.Count; i++)
                {
                    if (i == cursorY)
                    {
                        menuOptions[i].Update();
                        menuOptions[i].active = true;
                    }
                    else
                    {
                        menuOptions[i].time = 0;
                        menuOptions[i].active = false;
                    }
                }
            }

            if (Debug.main?.menuDev == true)
            {
                DevControls();
            }

            time++;
        }

        public virtual void Render()
        {
            if (backgroundTextureName != "")
            {
                drawer.DrawTexture(backgroundTextureName, 0, 0);
            }

            int i = 0;
            foreach (var menuOption in menuOptions)
            {
                menuOption.Render(drawer, startPos.x, startPos.y + (i * ySpacing));
                i++;
            }

            if (title != "") drawer.DrawText(title, titlePos.x, titlePos.y, alignX: AlignX.Center, alignY: AlignY.Middle);
            if (footer != "") drawer.DrawText(footer, footerPos.x, footerPos.y, alignX: AlignX.Center, alignY: AlignY.Middle, fontType: FontType.Small);

            if (menuOptions.Count > 0 && !areOptionsReadOnly)
            {
                fairyCursor.Render(drawer, startPos.x - cursorXOff, startPos.y + (cursorY * ySpacing) + 6, ZIndex.UIGlobal);
            }

            if (Debug.main?.menuDev == true)
            {
                MenuPos devPosition = devPositions[devPosIndex];
                drawer.DrawText(devPosition.x + "," + devPosition.y, Game.ScreenW, 0, AlignX.Right);
                drawer.DrawCircle(devPosition.x, devPosition.y, 2, true, SFML.Graphics.Color.Red, 1);
            }
        }

        public static void ChangeMenu(Menu? newMenu)
        {
            current = newMenu;
            if (newMenu is MainMenu)
            {
                MusicManager.main.ChangeMusic("fairy_fountain");
            }
        }

        // REFACTOR this callback is oddly specific
        public virtual void OnBack()
        {
        }

        private void DevControls()
        {
            if (Game.input.IsKeyPressed(Keyboard.Key.PageUp))
            {
                devPosIndex++;
                if (devPosIndex >= devPositions.Count) devPosIndex = 0;
            }
            if (Game.input.IsKeyPressed(Keyboard.Key.Insert))
            {
                devPosIndex--;
                if (devPosIndex < 0) devPosIndex = devPositions.Count - 1;
            }

            MenuPos devPosition = devPositions[devPosIndex];

            int xInc = 0;
            int yInc = 0;
            
            if (Game.input.IsKeyHeld(Keyboard.Key.LShift))
            {
                if (Game.input.IsKeyPressed(Keyboard.Key.PageDown)) xInc = 10;
                if (Game.input.IsKeyPressed(Keyboard.Key.Delete)) xInc = -10;
                if (Game.input.IsKeyPressed(Keyboard.Key.Home)) yInc = -10;
                if (Game.input.IsKeyPressed(Keyboard.Key.End)) yInc = 10;
            }
            else
            {
                if (Game.input.IsKeyPressed(Keyboard.Key.PageDown)) xInc = 1;
                if (Game.input.IsKeyPressed(Keyboard.Key.Delete)) xInc = -1;
                if (Game.input.IsKeyPressed(Keyboard.Key.Home)) yInc = -1;
                if (Game.input.IsKeyPressed(Keyboard.Key.End)) yInc = 1;
            }

            devPosition.x += xInc;
            devPosition.y += yInc;
        }
    }

    public class MenuPos
    {
        public int x;
        public int y;

        public MenuPos(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
