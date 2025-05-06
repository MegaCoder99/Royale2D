using ProtoBuf;

namespace Royale2D
{
    [ProtoContract]
    public class CreateMatchResponse
    {
        [ProtoMember(1)] public MatchSettings settings;
        [ProtoMember(2)] public int udpPort;
        [ProtoMember(3)] public SyncedPlayerData playerData;

        public CreateMatchResponse()
        {
        }

        public CreateMatchResponse(MatchSettings settings, int udpPort, SyncedPlayerData playerData)
        {
            this.settings = settings;
            this.udpPort = udpPort;
            this.playerData = playerData;
        }
    }
}
