namespace Royale2D
{
    public class BoolMenuOption : MenuOption
    {
        private Func<bool> getValue;
        private Action<bool> setValue;
        public BoolMenuOption(string text, Func<bool> getValue, Action<bool> setValue) : base(text)
        {
            this.getValue = getValue;
            this.setValue = setValue;
        }

        public override void Update()
        {
            base.Update();
            if (Game.input.IsPressed(Control.MenuLeft) || Game.input.IsPressed(Control.MenuRight))
            {
                setValue(!getValue());
            }
        }

        public override void Render(Drawer drawer, int x, int y)
        {
            drawer.DrawText(text + (getValue() ? "yes" : "no"), x, y);
        }
    }
}
