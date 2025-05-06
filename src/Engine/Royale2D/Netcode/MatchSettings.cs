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
        [ProtoMember(4)] public string matchName;    // Name of the match as it appears in matchmaking, lobby, scoreboard, etc.
        [ProtoMember(5)] public bool isP2P;
        [ProtoMember(6)] public bool isLAN;
        [ProtoMember(8)] public bool hostRelayServerLocally;
        [ProtoMember(9)] public int batchFrames;
        [ProtoMember(10)] public int rngSeed;   // Technically not a setting, or at least a visible one, but it's here for convenience
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
            cpuCount = Debug.cpuCount;
            delayFrames = Debug.delayFrames;
            maxDelayFrames = Debug.maxDelayFrames;
        }
    }
}
