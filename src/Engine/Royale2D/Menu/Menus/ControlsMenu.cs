namespace Royale2D
{
    public class ControlsMenu : Menu
    {
        public ControlsMenu(Menu prevMenu) : base(prevMenu)
        {
            title = "CONTROLS";
            footer = "Z: Back";
            ySpacing = 15;

            // TODO make dynamic and changable via Bindings.main.keyboardBinding
            menuOptions.Add(new ControlMenuOption("Up: Up Arrow"));
            menuOptions.Add(new ControlMenuOption("Down: Down Arrow"));
            menuOptions.Add(new ControlMenuOption("Left: Left Arrow"));
            menuOptions.Add(new ControlMenuOption("Right: Right Arrow"));
            menuOptions.Add(new ControlMenuOption("Sword: C"));
            menuOptions.Add(new ControlMenuOption("Action: X"));
            menuOptions.Add(new ControlMenuOption("Item: Z"));
            menuOptions.Add(new ControlMenuOption("Toss: D"));
            menuOptions.Add(new ControlMenuOption("Item Left: A"));
            menuOptions.Add(new ControlMenuOption("Item Right: S"));

            devPositions = new List<MenuPos> { titlePos, startPos };
        }
    }

    public class ControlMenuOption : MenuOption
    {
        public ControlMenuOption(string text) : base(text, () => { })
        {
        }
    }
}