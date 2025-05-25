namespace Royale2D
{
    public class MessageMenu : Menu
    {
        public MenuPos captionPos = new MenuPos(Game.HalfScreenW, 73);
        public MenuPos messagePos = new MenuPos(Game.HalfScreenW, 116);
        public string caption;
        public string message;
        public Menu nextMenu;

        public MessageMenu(Menu nextMenu, string title, string caption, string message) : base(null)
        {
            this.nextMenu = nextMenu;
            this.title = title;
            this.caption = caption;
            this.message = Helpers.InsertNewlines(message, 40);
            //menuOptions.Add(new MenuOption("OK", () => { }));
            footer = "X: Continue";

            devPositions = new List<MenuPos> { messagePos, captionPos };
        }

        public static MessageMenu CreateErrorMenu(Menu nextMenu, string title, string errorMessage)
        {
            return new MessageMenu(nextMenu, title, "( ERROR )", errorMessage);
        }

        public override void Update()
        {
            base.Update();
            if (Game.input.IsPressed(Control.MenuSelectPrimary))
            {
                ChangeMenu(nextMenu);
            }
        }

        public override void Render()
        {
            base.Render();
            drawer.DrawText(caption, captionPos.x, captionPos.y, alignX: AlignX.Center);
            drawer.DrawText(message, messagePos.x, messagePos.y, alignX: AlignX.Center);
        }
    }

    public class QuickJoinMenu : Menu
    {
        public MenuPos captionPos = new MenuPos(Game.HalfScreenW, 73);
        public MenuPos messagePos = new MenuPos(Game.HalfScreenW, 116);
        public string message;
        public Action confirmAction;

        public QuickJoinMenu(Action confirmAction) : base(null)
        {
            this.title = "QUICK JOIN";
            this.message = "Press X to quick join\nonce host initialized.";
            this.confirmAction = confirmAction;
            footer = "X: Continue";
        }

        public override void Update()
        {
            base.Update();
            if (Game.input.IsPressed(Control.MenuSelectPrimary))
            {
                confirmAction.Invoke();
            }
        }

        public override void Render()
        {
            base.Render();
            drawer.DrawText(message, messagePos.x, messagePos.y, alignX: AlignX.Center);
        }
    }
}
