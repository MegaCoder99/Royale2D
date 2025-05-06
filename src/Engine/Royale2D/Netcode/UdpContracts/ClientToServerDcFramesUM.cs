using Lidgren.Network;
using ProtoBuf;

namespace Royale2D
{
    [ProtoContract]
    public class ClientToServerDcFramesUM : IUdpMessage
    {
        [ProtoMember(1)] public int senderId;
        [ProtoMember(2)] public int disconnectorId;
        [ProtoMember(3)] public Dictionary<int, FrameInput> inputs;

        public NetDeliveryMethod netDeliveryMethod => NetDeliveryMethod.Unreliable;
        public byte id => UdpMessageId.ClientToServerDcFrames;

        public ClientToServerDcFramesUM() { }

        public ClientToServerDcFramesUM(int senderId, int disconnectorId, Dictionary<int, FrameInput> inputs)
        {
            this.senderId = senderId;
            this.disconnectorId = disconnectorId;
            this.inputs = inputs;
        }

        public int GetByteSize()
        {
            int byteSize = 0;
            byteSize += 2;  // senderId and disconnectorId
            foreach (KeyValuePair<int, FrameInput> entry in inputs)
            {
                byteSize += 4;  // frameNum
                byteSize += 1;  // inputs
            }
            return byteSize;
        }
    }
}
