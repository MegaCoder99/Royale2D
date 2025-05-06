using Lidgren.Network;
using ProtoBuf;

namespace Royale2D
{
    [ProtoContract]
    public class ServerToClientMatchSyncUM : IUdpMessage
    {
        [ProtoMember(1)] public List<SyncedPlayerData> playerDatas;
        [ProtoMember(2)] public bool matchStarted;
        [ProtoMember(3)] public bool desyncDetected;

        public byte id => UdpMessageId.MatchSync;
        public NetDeliveryMethod netDeliveryMethod => NetDeliveryMethod.Unreliable;

        public ServerToClientMatchSyncUM()
        {
            playerDatas = new List<SyncedPlayerData>();
        }

        public ServerToClientMatchSyncUM(List<SyncedPlayerData> playerDatas, bool matchStarted, bool desyncDetected)
        {
            this.playerDatas = playerDatas;
            this.matchStarted = matchStarted;
            this.desyncDetected = desyncDetected;
        }
    }
}
