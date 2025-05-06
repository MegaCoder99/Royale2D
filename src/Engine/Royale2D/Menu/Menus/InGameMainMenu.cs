namespace Royale2D
{
    public class InGameMainMenu : Menu
    {
        public InGameMainMenu(World world) : base(null)
        {
            inGame = true;
            backgroundTextureName = "in_game_menu";
            title = "MATCH OPTIONS";
            footer = "Up/Down: Choose, X: Select";

            //menuOptions.Add(new SliderMenuOption("Toss Rupees: {0}", 0, 0, (int)world.mainCharacter.rupees.value, (sliderValue) => { }));
            //menuOptions.Add(new SliderMenuOption("Toss Arrows: {0}", 0, 0, (int)world.mainCharacter.arrows.value, (sliderValue) => { }));
            menuOptions.Add(new MenuOption("Pause Game", () => { }));
            menuOptions.Add(new MenuOption("Spectate", () => { }));
            menuOptions.Add(new MenuOption("Leave Match", () => { Match.current?.Leave(); }));
        }

        public override void Update()
        {
            base.Update();
        }

        public override void Render()
        {
            base.Render();
        }
    }
}