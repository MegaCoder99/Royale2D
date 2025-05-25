namespace Royale2D
{
    // Basic "transition to the next menu" option on press
    public class TransitionMenuOption : MenuOption
    {
        public TransitionMenuOption(string text, Func<Menu> newMenuFunc) : base(text)
        {
            selectAction = () => Menu.ChangeMenu(newMenuFunc.Invoke());
        }

        public TransitionMenuOption(string text, Func<string, Menu> newMenuFunc) : base(text)
        {
            selectActionWithOption = (string option) => Menu.ChangeMenu(newMenuFunc.Invoke(option));
        }
    }
}
