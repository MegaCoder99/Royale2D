using Lidgren.Network;

namespace Royale2D
{
    // REFACTOR remove?
    /*
    // Direct P2P UDP connection to all other players, only used if P2P match setting is on, functioning as a useful IP routing path shortcut
    // for input syncing to reduce input lag window by 50% or more (this is huge!) by avoiding having to go thru relay server first.
    // The downside is more connections/bandwidth/complexity, and if you want to play over the internet, it requires NAT punchthru which is not 100% reliable.
    // If playing on a VPN like Hamachi, direct connections should always work and you should always use this option
    // Note, there will always be a relay server connection for syncing less time sensitive data
    public class UdpInputSyncPeer
    {
        public NetPeer peer;
        public int flushFrames;
        public Action<IUdpMessage> receiveCallback;
        public Action disconnectCallback;
        public List<PlayerConnection> playerConnections = new List<PlayerConnection>();

        public UdpInputSyncPeer(int mainPlayerId, Action<IUdpMessage> receiveCallback, Action disconnectCallback)
        {
            this.receiveCallback = receiveCallback;
            this.disconnectCallback = disconnectCallback;
            
            NetPeerConfiguration config = new NetPeerConfiguration(Game.appId);
            config.MaximumConnections = Game.MaxConnections;
            config.Port = PlayerBag.GetUdpPeerListenPort(mainPlayerId);
            config.AutoFlushSendQueue = false;
            config.ConnectionTimeout = Game.ConnectionTimeoutSeconds;
            config.AcceptIncomingConnections = true;
#if DEBUG
            config.SimulatedMinimumLatency = Debug.simulatedLatency;
            config.SimulatedLoss = Debug.simulatedPacketLoss;
            config.SimulatedDuplicatesChance = Debug.simulatedDuplicates;
#endif
            peer = new NetPeer(config);
            peer.Start();
        }

        public void InitUdpConnections(OnlineMatch match)
        {
            foreach (Player otherPlayer in match.otherPlayers)
            {
                if (string.IsNullOrEmpty(otherPlayer.ip)) throw new Exception("Player ip null or empty");

                PlayerConnection? existingPlayerConnection = playerConnections.FirstOrDefault(pc => pc.remotePlayerId == otherPlayer.id);

                // Only one side needs initialize the connection for both sides to get connected.
                // By convention, we'll have the lower player id always init the connection.
                if (existingPlayerConnection == null && match.mainPlayer.id < otherPlayer.id)
                {
                    int port = otherPlayer.GetUdpPeerListenPort();
                    Console.WriteLine($"Attempting to connect directly to peer at {otherPlayer.ip}:{port}");
                    // If this fails, we should probably retry N times. If that fails, we should just disconnect.
                    // Also, make sure this WILL throw an error and not just time out. If it can time out, we may need a connection timeout detection and throw that as error too.
                    peer.Connect(otherPlayer.ip, port);
                }
            }
        }

        public void UpdateReceive(OnlineMatch match)
        {
            NetIncomingMessage? im;
            while ((im = peer.ReadMessage()) != null)
            {
                // handle incoming message
                switch (im.MessageType)
                {
                    case NetIncomingMessageType.Data:
                        IUdpMessage udpMessage = UdpHelper.Get(im);
                        receiveCallback.Invoke(udpMessage);
                        break;
                    case NetIncomingMessageType.StatusChanged:
                        NetConnectionStatus status = (NetConnectionStatus)im.ReadByte();
                        if (status == NetConnectionStatus.Connected)
                        {
                            Console.WriteLine($"Successfully connected directly to peer at {im.SenderEndPoint}");
                            string ip = im.SenderEndPoint!.Address.ToString();
                            int port = im.SenderEndPoint.Port;

                            Player player = match.otherPlayers.First(p => p.ip == ip && p.GetUdpPeerListenPort() == port);
                            playerConnections.Add(new PlayerConnection(player.id, im.SenderConnection!));
                        }
                        else if (status == NetConnectionStatus.Disconnected)
                        {
                            Console.WriteLine("A P2P connection disconnected");
                            
                            // If one of the P2P links broke, just disconnect from the entire match regardless of the connection health to relay server.
                            PlayerConnection? pc = playerConnections.FirstOrDefault(p => p.connection == im.SenderConnection);
                            Player? player = match.otherPlayers.FirstOrDefault(p => p.id == pc?.remotePlayerId);
                            
                            if (player != null) Console.WriteLine("The P2P connection remote player id was " + player.id);

                            // Don't do this if the player was already considered disconnected by the relay server however.
                            // Think carefully about timing, race conditions here, this logic seems VERY iffy. Maybe a way for clients to report P2P health to relay server is better?
                            if (player != null) //&& !player.disconnected
                            {
                                disconnectCallback.Invoke();
                            }
                        }
                        break;
                    default:
                        break;
                }
                peer.Recycle(im);
            }
        }

        public void UpdateSend()
        {
            peer.FlushSendQueue();
        }

        public bool AllUdpP2PPeersConnected(Match match)
        {
            if (playerConnections.Count == 0) return true;
            return playerConnections.Count == match.otherPlayers.Count && playerConnections.All(p => p.connection?.Status == NetConnectionStatus.Connected);
        }

        bool disconnected;
        public void Disconnect(string reason)
        {
            if (!disconnected)
            {
                disconnected = true;
                peer?.Shutdown(reason);
            }
        }
    }
    */
}