using Lidgren.Network;

namespace Royale2D
{
    public interface IUdpMessage
    {
        public NetDeliveryMethod netDeliveryMethod { get; }
        public byte id { get; }
    }

    public class UdpMessageId
    {
        // If you add a new message, must also add it to UdpHelper.Get method
        public const byte MatchSync = 1;
        public const byte PeerToPeerInput = 2;
        public const byte ClientToServerDcFrames = 3;
        public const byte ServerToClientDcFrames = 4;
        public const byte ClientToServerDesyncDetect = 5;
    }
}
