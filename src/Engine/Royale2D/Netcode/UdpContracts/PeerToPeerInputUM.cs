using Lidgren.Network;
using ProtoBuf;

namespace Royale2D
{
    // Every client needs to sync its inputs with every other client. This message is the means to do that,
    // whether it gets sent directly via P2P or indirectly via Relay Server.
    [ProtoContract]
    public class PeerToPeerInputUM : IUdpMessage
    {
        // PERF these two may not be necessary for P2P?
        [ProtoMember(1)] public int senderId;
        [ProtoMember(2)] public int recipientId;

        // PERF if this list gets too large, UDP MTU limit might be reached which is bad
        // UPDATE: there may be a max limit here, based on delay window
        [ProtoMember(3)] public Dictionary<int, FrameInput>? inputs;

        // FYI this double duty saves some bandwidth but could it result in a input sync deadlock?
        [ProtoMember(4)] public int lastAckedFrame;

        public NetDeliveryMethod netDeliveryMethod => NetDeliveryMethod.Unreliable;
        public byte id => UdpMessageId.PeerToPeerInput;

        public PeerToPeerInputUM() { }

        public PeerToPeerInputUM(Dictionary<int, FrameInput> frameInputs, int lastFrameSenderAckedOfRecipient, int senderPlayerId, int recipientPlayerId)
        {
            this.inputs = frameInputs;
            this.lastAckedFrame = lastFrameSenderAckedOfRecipient;
            this.senderId = senderPlayerId;
            this.recipientId = recipientPlayerId;
        }

        public int GetByteSize()
        {
            int byteSize = 0;
            byteSize += 6;  // senderId, recipientId and lastAckedFrame
            if (inputs != null)
            {
                foreach (KeyValuePair<int, FrameInput> entry in inputs)
                {
                    byteSize += 4;  // frameNum
                    byteSize += 1;  // inputs
                }
            }
            return byteSize;
        }
    }
}
