using System;

namespace Royale2D
{
    public class MainMenu : Menu
    {
        //public MenuPos skinPos = new MenuPos(Game.HalfScreenW, 84);
        //public MenuPos skinTextOffY = new MenuPos(0, 8);

        public MenuPos skinPos = new MenuPos(175, 141);
        public MenuPos skinTextOffY = new MenuPos(0, 11);
        public SpriteInstance skinSprite;

        public MainMenu()
        {
            gui = Assets.guis["main_menu"];

            //menuOptions.Add(new TransitionMenuOption("Join Match", () => new JoinMatchMenu(this)));
            //menuOptions.Add(new TransitionMenuOption("Host Match", () => new CreateMatchMenu(this, false, false)));
            menuOptions.Add(new TransitionMenuOption("Battle Royale!", () => new CreateMatchMenu(this, true, false)).AddOptions(["Offline", "LAN", "Online"]));
            menuOptions.Add(new TransitionMenuOption("Skins", () => new SkinsMenu(this)));
            menuOptions.Add(new TransitionMenuOption("Options", () => new OptionsMenu(this)));
            menuOptions.Add(new MenuOption("Controls", Debug.CreateAndStartOfflineMatch).AddOptions(["Keyboard", "Controller"]));
            menuOptions.Add(new MenuOption("Quit", () => Game.window.Close()));

            Div menuOptionsDiv = gui.GetNodeById("menu-options") as Div;
            Point menuOptionsPos = menuOptionsDiv.GetPos();

            //startPos.x = (int)menuOptionsPos.x;
            startPos.y = (int)menuOptionsPos.y;

            footer = "Up/Down: Choose, X: Select";

            backgroundTextureName = "";

            skinSprite = new SpriteInstance("char_idle_down");
        }

        public override void Update()
        {
            base.Update();
        }

        public override void Render()
        {
            drawer.DrawTexture("main_menu", 0, 0, hasMediumQuality: true);
            base.Render();

            ImageNode skinNode = gui.GetNodeById("skin") as ImageNode;
            Point skinPos = skinNode.GetPos();
            TextNode textNode = gui.GetNodeById("player-name") as TextNode;
            Point textPos = textNode.GetPos();
            ImageNode menuOptionImage = gui.GetNodeById("menu-option-image") as ImageNode;
            Point menuOptionImagePos = menuOptionImage.GetPos();

            skinSprite.Render(drawer, skinPos.x, skinPos.y, ZIndex.UIGlobal, childFrameTagsToHide: ["shield1", "shield2", "shield3"], overrideTexture: Options.main.skin);

            drawer.DrawText(Options.main.playerName, textPos.x, textPos.y, alignX: textNode.hAlign, alignY: textNode.vAlign, fontType: FontType.Small);
        }
    }
}
