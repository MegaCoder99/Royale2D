using Lidgren.Network;
using ProtoBuf;

namespace Royale2D
{
    [ProtoContract]
    public class ClientToServerDesyncDetectUM : IUdpMessage
    {
        [ProtoMember(1)] public int senderId;
        [ProtoMember(2)] public int lastCompleteFrameNum;
        [ProtoMember(3)] public string gameStateHash;

        public NetDeliveryMethod netDeliveryMethod => NetDeliveryMethod.Unreliable;
        public byte id => UdpMessageId.ClientToServerDesyncDetect;

        public ClientToServerDesyncDetectUM() { }

        public ClientToServerDesyncDetectUM(int senderId, int lastCompleteFrameNum, string gameStateHash)
        {
            this.senderId = senderId;
            this.lastCompleteFrameNum = lastCompleteFrameNum;
            this.gameStateHash = gameStateHash;
        }
    }
}
