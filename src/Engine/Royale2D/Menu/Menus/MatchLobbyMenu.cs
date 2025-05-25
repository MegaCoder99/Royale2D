namespace Royale2D
{
    public class MatchLobbyMenu : Menu
    {
        public MenuPos matchNamePos;
        public MenuPos endIpX;

        public MenuPos topLeftHeaderPos;
        public MenuPos header1X;
        public MenuPos middleHeaderX;
        public MenuPos endHeaderX;

        public OnlineMatch match;
        public bool isHost => match.isHost;

        public MatchLobbyMenu(Menu prevMenu, OnlineMatch match) : base(prevMenu)
        {
            this.match = match;

            title = "MATCH LOBBY";
            footer = isHost ? $"Left/Right: Change Spec, Z: Leave" : "Z: Leave";
            areOptionsReadOnly = !isHost;

            UpdateMatchOptions();

            // Default is (40,50)
            startPos = new MenuPos(35, 73);

            matchNamePos = new MenuPos(24, 43);
            endIpX = new MenuPos(234, 0);

            topLeftHeaderPos = new MenuPos(28, 57);
            header1X = new MenuPos(130, 0);
            middleHeaderX = new MenuPos(165, 0);
            endHeaderX = new MenuPos(Game.ScreenW - 50, 0);

            devPositions = new List<MenuPos> { header1X, middleHeaderX, startPos };
        }

        public override void Update()
        {
            base.Update();
            UpdateMatchOptions();

            if (match.isHost && !match.started)
            {
                if (Game.input.IsPressed(Control.MenuSelectPrimary))
                {
                    try
                    {
                        Console.WriteLine("Initiating request to relay server to start the match as host.");
                        match.matchTcpClient.SendTcpRequest<bool>(new StartMatchRequest(match.settings.matchName));
                        match.Start();
                        return;
                    }
                    catch (Exception ex)
                    {
                        ChangeMenu(MessageMenu.CreateErrorMenu(this, "Couldn't start match.", ex.Message));
                        return;
                    }
                }
            }
            
            if (!match.started)
            {
                if (Game.input.IsPressed(Control.MenuBack))
                {
                    match.Leave();
                }
            }
        }

        public override void Render()
        {
            base.Render();

            drawer.DrawText("Name: " + match.settings.matchName, matchNamePos.x, matchNamePos.y, fontType: FontType.Small);
            if (!string.IsNullOrEmpty(match.hostIp))
            {
                drawer.DrawText("Ip: " + match.hostIp, endIpX.x, matchNamePos.y, fontType: FontType.Small, alignX: AlignX.Right);
            }
            drawer.DrawText("Player", topLeftHeaderPos.x, topLeftHeaderPos.y, fontType: FontType.Small);
            drawer.DrawText("Spec?", header1X.x, topLeftHeaderPos.y, fontType: FontType.Small);
            drawer.DrawText("Host?", middleHeaderX.x, topLeftHeaderPos.y, fontType: FontType.Small);
            drawer.DrawText("Ping", endHeaderX.x, topLeftHeaderPos.y, fontType: FontType.Small);
        }

        public override void OnBack()
        {
            base.OnBack();
            // NETCODE when user quits window, invoke this too
            // MatchmakingServer.LeaveMatchServer();
        }

        public void UpdateMatchOptions()
        {
            menuOptions.Clear();
            foreach (SyncedPlayerData player in match.players)
            {
                menuOptions.Add(new LobbyPlayerMenuOption(player, this));
            }
        }
    }

    public class LobbyPlayerMenuOption : MenuOption
    {
        SyncedPlayerData player;
        MatchLobbyMenu matchLobbyMenu;
        SpriteInstance skinSprite;
        public LobbyPlayerMenuOption(SyncedPlayerData player, MatchLobbyMenu matchLobbyMenu) : 
            base("   " + player.name)
        {
            this.player = player;
            this.matchLobbyMenu = matchLobbyMenu;
            skinSprite = new SpriteInstance("char_idle_down");
        }

        public override void Render(Drawer drawer, int x, int y)
        {
            base.Render(drawer, x, y);

            skinSprite.Render(drawer, x, y + 6, ZIndex.UIGlobal, drawboxTagsToHide: ["shield1", "shield2", "shield3"], overrideTexture: Options.main.skin);

            drawer.DrawText("No", matchLobbyMenu.header1X.x, y);
            drawer.DrawText(Helpers.BoolToYesNo(player.id == 0), matchLobbyMenu.middleHeaderX.x, y);
            drawer.DrawText(player.ping.ToString(), matchLobbyMenu.endHeaderX.x, y);
        }
    }
}
