namespace Royale2D
{
    public class CreateMatchMenu : Menu
    {
        bool isOffline;
        bool inGame;
        List<string> mapList = Assets.maps.Keys.ToList();
        int mapIndex;

        MatchSettings matchSettings => new MatchSettings(mapList[mapIndex], Debug.quickStartMatchName);

        public CreateMatchMenu(Menu? prevMenu, bool isOffline, bool inGame) : base(prevMenu)
        {
            this.isOffline = isOffline;
            this.inGame = inGame;

            title = isOffline ? "CREATE OFFLINE MATCH" : "CREATE ONLINE MATCH";
            footer = "Left/Right: Change, Z: Back, X: Create";

            menuOptions.Add(new ListMenuOption("Map: ", mapList, () => mapIndex, (index) => mapIndex = index));
            /*
            menuOptions.Add(new NumericMenuOption("CPU Count: ", () => Options.main.fullScreen,
                (value) =>
                {
                    Options.main.fullScreen = value;
                    Game.OnFullScreenChange();
                }));
            */

            devPositions = new List<MenuPos> { titlePos, startPos };
        }

        public override void Update()
        {
            base.Update();
            if (Game.input.IsPressed(Control.MenuSelectPrimary))
            {
                if (isOffline)
                {
                    Game.PlaySound("sword shine 1");
                    MusicManager.main.ChangeMusic("");
                    Match.current = OfflineMatch.Create(matchSettings);
                    Match.current.Start();
                }
                else
                {
                    try
                    {
                        var onlineMatch = OnlineMatch.Create(matchSettings, Options.main.relayServerIp);
                        Match.current = onlineMatch;
                        ChangeMenu(new MatchLobbyMenu(this, onlineMatch));
                    }
                    catch (Exception ex)
                    {
                        ChangeMenu(MessageMenu.CreateErrorMenu(this, title, ex.Message));
                    }
                }
            }   
        }
    }
}