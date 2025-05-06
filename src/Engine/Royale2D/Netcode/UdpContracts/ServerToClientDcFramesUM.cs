using Lidgren.Network;
using ProtoBuf;

namespace Royale2D
{
    [ProtoContract]
    public class ServerToClientDcFramesUM : IUdpMessage
    {
        [ProtoMember(1)] public int recipientId;
        [ProtoMember(2)] public int disconnectingPlayerId;
        [ProtoMember(3)] public Dictionary<int, FrameInput> synthesizedDisconnectFrames;

        public NetDeliveryMethod netDeliveryMethod => NetDeliveryMethod.Unreliable;
        public byte id => UdpMessageId.ServerToClientDcFrames;

        public ServerToClientDcFramesUM() { }

        public ServerToClientDcFramesUM(int recipientId, int disconnectingPlayerId, Dictionary<int, FrameInput> synthesizedDisconnectFrames)
        {
            this.recipientId = recipientId;
            this.disconnectingPlayerId = disconnectingPlayerId;
            this.synthesizedDisconnectFrames = synthesizedDisconnectFrames;
        }
    }
}
