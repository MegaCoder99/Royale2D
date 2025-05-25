using Shared;

namespace Royale2D
{
    // The word "Host" not to be confused with concept of match host. Term is used to indicate this is a container/wrapper for World that initializes on match start
    // Ideally, World itself should NOT know anything about netcode, for separate of concerns
    public abstract class WorldHost
    {
        public Match match;
        public Map map;
        public EntranceSystem entranceSystem;
        public TextureManager textureManager = new TextureManager();
        public World world;

        public WorldHost(Match match)
        {
            this.match = match;
            map = Assets.maps[match.settings.mapName];
            map.Init();
            entranceSystem = new EntranceSystem(map);
            world = new World(this);
        }

        public virtual void Update()
        {
        }

        public virtual IInputReader GetPlayerInputReader(int playerId, int frameNum)
        {
            throw new NotImplementedException();
        }
    }

    public class OfflineWorldHost : WorldHost
    {
        public int frameNum;    // In online matches, frameNum is instead controlled by the InputSyncer
        public Dictionary<int, PlayerInputState> playerInputStates;
        public PlayerInputState localPlayerInputState;

        public OfflineWorldHost(OfflineMatch match) : base(match)
        {
            playerInputStates = new Dictionary<int, PlayerInputState>();
            foreach (SyncedPlayerData player in match.players)
            {
                playerInputStates[player.id] = new PlayerInputState();
                if (player.id == match.mainPlayer.id)
                {
                    localPlayerInputState = playerInputStates[player.id];
                }
            }
        }

        public override void Update()
        {
            if (Debug.main != null)
            {
                if (Debug.main.paused && !Debug.main.frameAdvance) return;
            }
            localPlayerInputState.Update();
            world.UpdateNonSyncedInputs();
            world.Update(frameNum);
            frameNum++;
        }

        public override PlayerInputState GetPlayerInputReader(int playerId, int frameNum)
        {
            return playerInputStates[playerId];
        }
    }

    public class OnlineWorldHost : WorldHost
    {
        public InputSyncer inputSyncer;
        public PlayerInputState localInputState;
        public OnlineMatch onlineMatch;

        public OnlineWorldHost(OnlineMatch match) : base(match)
        {
            onlineMatch = match;
            localInputState = new PlayerInputState();
            inputSyncer = new InputSyncer(
                match.mainPlayer,
                match.players,
                this,
                localInputState.GetInput,
                match.settings.delayFrames,
                match.settings.maxDelayFrames
            );
        }

        public override void Update()
        {
            AdjustDelayFromPing();
            localInputState.Update();
            world.UpdateNonSyncedInputs();
            inputSyncer.Update();

            foreach (Character chr in world.characters)
            {
                if (!chr.IsMainChar() && chr.NoMoreInputs())
                {
                    RemotePlayerSyncedInputs remotePlayer = inputSyncer.remotePlayers.First(rp => rp.id == chr.playerId);
                    remotePlayer.noMoreInputs = true;
                }
            }
        }

        // FYI this assumes that one way time = round trip time / 2, which may not always be the case
        public void AdjustDelayFromPing()
        {
            int highestPing = 0;
            foreach (SyncedPlayerData player in match.players)
            {
                if (player.id == match.mainPlayer.id) continue;
                int? ping = match.mainPlayer.ping + player.ping;
                if (ping != null && ping.Value > highestPing)
                {
                    highestPing = ping.Value;
                }
            }

            int inputDelayFrames = MyMath.Round(highestPing / 16.66f);
            inputDelayFrames = MyMath.Clamp(inputDelayFrames, 3, match.settings.maxDelayFrames);

            inputSyncer.delayFrames = inputDelayFrames;
        }

        public override IInputReader GetPlayerInputReader(int playerId, int frameNum)
        {
            if (playerId == inputSyncer.localPlayer.id)
            {
                return inputSyncer.localPlayer.GetInputReader(frameNum);
            }
            else
            {
                RemotePlayerSyncedInputs remotePlayer = inputSyncer.remotePlayers.First(rp => rp.id == playerId);
                return remotePlayer.GetInputReader(frameNum);
            }
        }
    }
}
