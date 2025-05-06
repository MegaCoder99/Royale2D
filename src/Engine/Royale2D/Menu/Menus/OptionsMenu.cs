namespace Royale2D
{
    public class OptionsMenu : Menu
    {
        public OptionsMenu(Menu prevMenu) : base(prevMenu)
        {
            title = "OPTIONS";
            footer = "Left/Right: Change, Z: Save+Back";

            menuOptions.Add(new BoolMenuOption("Full Screen: ", () => Options.main.fullScreen, 
                (value) =>
                {
                    Options.main.fullScreen = value;
                    Game.OnFullScreenChange();
                }));
            menuOptions.Add(new ListMenuOption("Window Scale: ", Options.WindowScaleOptions, () => Options.main.windowScale, 
                (index) =>
                {
                    Options.main.windowScale = index;
                    Game.OnWindowScaleChange();
                }));
            menuOptions.Add(new SliderMenuOption("Sound Volume: {0}", Options.main.soundVolume, 0, 100, (sliderValue) => { Options.main.soundVolume = sliderValue; }));
            menuOptions.Add(new SliderMenuOption("Music Volume: {0}", Options.main.musicVolume, 0, 100, (sliderValue) => { Options.main.musicVolume = sliderValue; }));

            devPositions = new List<MenuPos> { titlePos, startPos };
        }
    }
}