namespace Royale2D
{
    // Basic "transition to the next menu" option on press
    public class TransitionMenuOption : MenuOption
    {
        public TransitionMenuOption(string text, Func<Menu> newMenuFunc) : base(text)
        {
            selectAction = () => Menu.ChangeMenu(newMenuFunc.Invoke());
        }

        // REFACTOR this really feels like it should be part of ListMenuOption and that TransitionMenuOption is unnecessary abstraction that doesn't do much

        /*
        // Maps sub-option text to the action to perform when that sub-option is selected
        public Dictionary<string, Action> subOptions = new();
        public int selectedOptionIndex;

        public TransitionMenuOption AddSubOption(string key, Action action)
        {
            subOptions[key] = action;
            return this;
        }
        
        public override void Update()
        {
            var subOptionsTexts = subOptions.Keys.ToList();
            if (subOptionsTexts.Count > 0)
            {
                if (Game.input.IsPressed(Control.MenuLeft))
                {
                    if (selectedOptionIndex > 0)
                    {
                        selectedOptionIndex--;
                    }
                    else
                    {
                        selectedOptionIndex = subOptionsTexts.Count - 1;
                    }
                }
                else if (Game.input.IsPressed(Control.MenuRight))
                {
                    if (selectedOptionIndex < subOptionsTexts.Count - 1)
                    {
                        selectedOptionIndex++;
                    }
                    else
                    {
                        selectedOptionIndex = 0;
                    }
                }
            }

            if (Game.input.IsPressed(Control.MenuSelectPrimary))
            {
                if (subOptionsTexts.Count > 0)
                {
                    subOptions[subOptionsTexts[selectedOptionIndex]].Invoke();
                }
                else
                {
                    selectAction.Invoke();
                }
            }

            time++;
        }

        public override void Render(Drawer drawer, int x, int y)
        {
            base.Render(drawer, x, y);

            var subOptionsTexts = subOptions.Keys.ToList();
            if (subOptions.Count > 0 && active)
            {
                int xPos = 180;
                string optionText = subOptionsTexts[selectedOptionIndex];
                drawer.DrawText(optionText, xPos, y, alignX: AlignX.Center);
                if (time % 60 < 30)
                {
                    int longestOptionLength = subOptionsTexts.Max(option => option.Length);
                    string spaces = new string(' ', longestOptionLength);
                    drawer.DrawText("<" + spaces + ">", xPos, y, alignX: AlignX.Center);
                }
            }
        }
        */
    }
}
