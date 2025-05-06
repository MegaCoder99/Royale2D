using Lidgren.Network;

namespace Royale2D
{
    public static class UdpHelper
    {
        public static IUdpMessage Get(NetIncomingMessage im)
        {
            long udpMessageId = im.ReadByte();
            ushort argCount = BitConverter.ToUInt16(im.ReadBytes(2), 0);
            byte[] bytes = im.ReadBytes(argCount);
            
            if (udpMessageId == UdpMessageId.MatchSync)
            {
                return Helpers.Deserialize<ServerToClientMatchSyncUM>(bytes);
            }
            else if (udpMessageId == UdpMessageId.PeerToPeerInput)
            {
                return Helpers.Deserialize<PeerToPeerInputUM>(bytes);
            }
            else if (udpMessageId == UdpMessageId.ClientToServerDcFrames)
            {
                return Helpers.Deserialize<ClientToServerDcFramesUM>(bytes);
            }
            else if (udpMessageId == UdpMessageId.ServerToClientDcFrames)
            {
                return Helpers.Deserialize<ServerToClientDcFramesUM>(bytes);
            }
            else if (udpMessageId == UdpMessageId.ClientToServerDesyncDetect)
            {
                return Helpers.Deserialize<ClientToServerDesyncDetectUM>(bytes);
            }

            throw new Exception("Udp message id not found");
        }

        public static void Send(NetPeer peer, NetConnection recipient, IUdpMessage udpMessage)
        {
            NetOutgoingMessage om = peer.CreateMessage();
            WriteMessage(om, udpMessage);
            peer.SendMessage(om, recipient, udpMessage.netDeliveryMethod);
        }

        public static void Send(NetClient client, IUdpMessage udpMessage)
        {
            NetOutgoingMessage om = client.CreateMessage();
            WriteMessage(om, udpMessage);
            client.SendMessage(om, udpMessage.netDeliveryMethod);
        }

        public static void Send(NetServer server, IUdpMessage udpMessage)
        {
            NetOutgoingMessage om = server.CreateMessage();
            WriteMessage(om, udpMessage);

            foreach (NetConnection connection in server.Connections)
            {
                if (!connection.RemoteEndPoint.Equals(server.Configuration.LocalAddress))
                {
                    server.SendMessage(om, connection, udpMessage.netDeliveryMethod);
                }
            }
        }

        private static void WriteMessage(NetOutgoingMessage om, IUdpMessage udpMessage)
        {
            om.Write(udpMessage.id);
            byte[] bytes = Helpers.Serialize(udpMessage);
            om.Write((ushort)bytes.Length);
            om.Write(bytes);
        }
    }
}