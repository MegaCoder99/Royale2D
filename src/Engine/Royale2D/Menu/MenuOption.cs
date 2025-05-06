namespace Royale2D
{
    public class MenuOption
    {
        public string text;
        public Action selectAction;

        // Only used by main menu right now, think of an ECS and/or builder pattern here...
        public List<string> options = new List<string>();
        public int selectedOptionIndex;
        public int time;
        public bool active;

        public MenuOption AddOptions(List<string> options)
        {
            this.options = options;
            return this;
        }

        public MenuOption(string text, List<string>? options = null)
        {
            this.text = text;
            selectAction = () => { };
            this.options = options ?? new List<string>();
        }

        public MenuOption(string text, Action updateAction)
        {
            this.text = text;
            this.selectAction = updateAction;
        }

        public virtual void Render(Drawer drawer, int x, int y)
        {
            drawer.DrawText(text, x, y);
            if (options.Count > 0 && active)
            {
                int xPos = 180;
                string optionText = options[selectedOptionIndex];
                drawer.DrawText(optionText, xPos, y, alignX: AlignX.Center);
                if (time % 60 < 30)
                {
                    int longestOptionLength = options.Max(option => option.Length);
                    string spaces = new string(' ', longestOptionLength);
                    drawer.DrawText("<" + spaces + ">", xPos, y, alignX: AlignX.Center);
                }
            }
        }

        public virtual void Update()
        {
            if (Game.input.IsPressed(Control.MenuSelectPrimary))
            {
                selectAction.Invoke();
            }

            if (options.Count > 0)
            {
                if (Game.input.IsPressed(Control.MenuLeft))
                {
                    if (selectedOptionIndex > 0)
                    {
                        selectedOptionIndex--;
                    }
                    else
                    {
                        selectedOptionIndex = options.Count - 1;
                    }
                }
                else if (Game.input.IsPressed(Control.MenuRight))
                {
                    if (selectedOptionIndex < options.Count - 1)
                    {
                        selectedOptionIndex++;
                    }
                    else
                    {
                        selectedOptionIndex = 0;
                    }
                }
            }

            time++;
        }
    }
}
