using static SFML.Window.Keyboard;

namespace Royale2D
{
    public class EnterTextMenu : Menu
    {
        public string prompt;
        public MenuPos promptPos = new MenuPos(Game.HalfScreenW, 90);
        public MenuPos cursorPos = new MenuPos(89, 112);
        public new Menu prevMenu;

        public string text = "";
        public int blinkFrames = 0;
        public Action<string>? submitAction;
        public int maxLength;
        public bool allowEmpty;

        public EnterTextMenu(Menu prevMenu, string title, string prompt, int maxLength, Action<string>? submitAction) 
            : base(null)
        {
            this.title = title;
            this.prompt = prompt;
            this.maxLength = maxLength;
            this.submitAction = submitAction;
            this.prevMenu = prevMenu;

            footer = "Enter: confirm, Esc: Back";
            backgroundTextureName = "secondary_menu";

            devPositions = new List<MenuPos> { promptPos, cursorPos };
        }

        public override void Update()
        {
            base.Update();

            blinkFrames++;
            if (blinkFrames >= 60) blinkFrames = 0;

            text = GetTypedString(text, maxLength);

            if (Game.input.IsKeyPressed(Key.Enter) && (allowEmpty || !string.IsNullOrWhiteSpace(text.Trim())))
            {
                submitAction?.Invoke(text);
            }
            else if (Game.input.IsKeyPressed(Key.Escape) && prevMenu != null)
            {
                OnBack();
                ChangeMenu(prevMenu);
            }
        }

        // REFACTOR menu Render() should take in a Drawer object
        public override void Render()
        {
            base.Render();
            drawer.DrawText(prompt, promptPos.x, promptPos.y, alignX: AlignX.Center);
            drawer.DrawText(text, cursorPos.x, cursorPos.y);
            if (blinkFrames >= 30)
            {
                drawer.DrawText(text + "<", cursorPos.x, cursorPos.y);
            }
            else
            {
                drawer.DrawText(text, cursorPos.x, cursorPos.y);
            }
        }

        public string GetTypedString(string str, int maxLength)
        {
            var pressedChar = Game.input.GetKeyCharPressed();
            if (pressedChar != null)
            {
                if (pressedChar == KeyMaps.BackspaceChar)
                {
                    if (str.Length > 0)
                    {
                        str = str.Substring(0, str.Length - 1);
                    }
                }
                else if (str.Length < maxLength)
                {
                    str += pressedChar;
                }
            }

            return str;
        }

    }
}
