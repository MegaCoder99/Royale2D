
namespace Royale2D
{
    // Match is the high-level wrapper encapsulating a match (whether online or offline) and all its data
    public abstract class Match
    {
        public static Match? current;
        public WorldHost? worldHost;
        public World? world => worldHost?.world;

        public bool started => worldHost != null;
        public MatchSettings settings;
        public int mainPlayerId;
        public List<SyncedPlayerData> players = new List<SyncedPlayerData>();
        public SyncedPlayerData mainPlayer => players.First(p => p.id == mainPlayerId);

        public Match(MatchSettings settings, SyncedPlayerData mainPlayer)
        {
            this.settings = settings;
            mainPlayerId = mainPlayer.id;
            players.Add(mainPlayer);
        }

        // NETCODE once started is set to true, tcp server must flag any new joiners as spectators, so others don't udp connect to them
        public virtual void Start()
        {
        }

        public virtual void Update()
        {
        }

        public void Render()
        {
            world?.Render();
        }

        public virtual void Leave(string forceLeaveMessage = "")
        {
            current = null;

            Console.WriteLine("Leaving match. Reason: " + forceLeaveMessage);

            if (string.IsNullOrEmpty(forceLeaveMessage))
            {
                Menu.ChangeMenu(new MainMenu());
            }
            else
            {
                Menu.ChangeMenu(new MessageMenu(new MainMenu(), "ATTENTION", "", forceLeaveMessage));
            }
        }
    }
}
