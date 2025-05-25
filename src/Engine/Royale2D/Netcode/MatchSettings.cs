using ProtoBuf;

namespace Royale2D
{
    [ProtoContract]
    public class MatchSettings
    {
        [ProtoMember(1)] public string mapName;
        [ProtoMember(2)] public int cpuCount;
        [ProtoMember(3)] public bool isTeams;

        // Online settings

        // Name of the match as it appears in matchmaking, lobby, scoreboard, etc.
        [ProtoMember(4)] public string matchName;

        // TODO this is used for NAT Punchthru for peers to connect directly without a relay server,
        // but it's not implemented yet due to increased complexity. Can reduce input lag by 50% if done right.
        [ProtoMember(5)] public bool isP2P;

        // TODO this field should have more uses than just a facade of hostRelayServerLocally
        [ProtoMember(6)] public bool isLAN;

        // Host the relay server locally on the same machine/process
        [ProtoMember(8)] public bool hostRelayServerLocally;

        // Technically not a setting, or at least a visible one, but it's here for convenience
        [ProtoMember(10)] public int rngSeed;

        [ProtoMember(11)] public int delayFrames;

        [ProtoMember(12)] public int maxDelayFrames;

        public MatchSettings()
        {
            mapName = "";
            matchName = "";
        }

        public MatchSettings(string mapName, string matchName)
        {
            this.mapName = mapName;
            this.matchName = matchName;
            rngSeed = Helpers.RandomRange(1, 1000000000);
            isP2P = true;
            isLAN = true;
            hostRelayServerLocally = true;
            if (Debug.main != null)
            {
                cpuCount = Debug.main.cpuCount;
                delayFrames = Debug.main.delayFrames;
                maxDelayFrames = Debug.main.maxDelayFrames;
            }
            else
            {
                cpuCount = 0;
                delayFrames = 3;
                maxDelayFrames = 30;
            }
        }
    }
}
