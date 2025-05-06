namespace Royale2D
{
    public class ListMenuOption : MenuOption
    {
        private Func<int> getIndex;
        private Action<int> setIndex;
        private List<string> options;
        public ListMenuOption(string text, List<string> options, Func<int> getIndex, Action<int> setIndex) : base(text)
        {
            this.getIndex = getIndex;
            this.setIndex = setIndex;
            this.options = options;
        }

        public override void Update()
        {
            base.Update();
            int currentIndex = getIndex();
            if (Game.input.IsPressed(Control.MenuLeft) && currentIndex > 0)
            {
                setIndex(currentIndex - 1);
            }
            else if (Game.input.IsPressed(Control.MenuRight) && currentIndex < options.Count - 1)
            {
                setIndex(currentIndex + 1);
            }
        }

        public override void Render(Drawer drawer, int x, int y)
        {
            drawer.DrawText(text + options[getIndex()], x, y);
        }
    }

}
