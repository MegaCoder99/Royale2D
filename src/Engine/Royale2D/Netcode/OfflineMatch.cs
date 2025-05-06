namespace Royale2D
{
    public class OfflineMatch : Match
    {
        public OfflineMatch(MatchSettings settings, SyncedPlayerData mainPlayer) : base(settings, mainPlayer)
        {
            for (int i = 0; i < settings.cpuCount; i++)
            {
                players.Add(new SyncedPlayerData("CPU " + (i + 1), (byte)(i + 1)));
            }
        }

        public static Match Create(MatchSettings settings)
        {
            var mainPlayer = new SyncedPlayerData(Options.main.playerName, 0);
            return new OfflineMatch(settings, mainPlayer);
        }

        public override void Start()
        {
            worldHost = new OfflineWorldHost(this);
            Menu.ChangeMenu(null);
        }

        public override void Update()
        {
            worldHost?.Update();
        }
    }
}
