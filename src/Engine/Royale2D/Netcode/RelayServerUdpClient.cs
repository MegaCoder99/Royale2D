using Lidgren.Network;
using System.Diagnostics;

namespace Royale2D
{
    public class RelayServerUdpClient
    {
        public NetClient client;

        public string relayServerIp;
        public int relayServerUdpPort;
        public int sendFlushFrames;
        public Action<IUdpMessage> receiveCallback;
        public Action disconnectCallback;
        bool disconnected;
        float lastHeartbeatTime;
        Stopwatch heartbeatStopwatch;

        public RelayServerUdpClient(string relayServerIp, int relayServerUdpPort, Action<IUdpMessage> receiveCallback, Action disconnectCallback)
        {
            this.relayServerIp = relayServerIp;
            this.relayServerUdpPort = relayServerUdpPort;
            this.receiveCallback = receiveCallback;
            this.disconnectCallback = disconnectCallback;

            NetPeerConfiguration config = new NetPeerConfiguration(Game.appId);
            config.MaximumConnections = Game.MaxConnections;
            config.AutoFlushSendQueue = false;
            config.ConnectionTimeout = Game.ConnectionTimeoutSeconds;
            config.AcceptIncomingConnections = true;
#if DEBUG
            config.SimulatedMinimumLatency = Debug.simulatedLatency;
            config.SimulatedLoss = Debug.simulatedPacketLoss;
            config.SimulatedDuplicatesChance = Debug.simulatedDuplicates;
#endif
            client = new NetClient(config);
            client.Start();

            heartbeatStopwatch = new Stopwatch();
        }

        public void Connect(int playerId)
        {
            NetOutgoingMessage hailMessage = client.CreateMessage(playerId.ToString());
            client.Connect(relayServerIp, relayServerUdpPort, hailMessage);

            int count = 0;
            while (count < 20 && client.ConnectionStatus != NetConnectionStatus.Connected)
            {
                // FYI this is necessary to get the connect message and successfully connect, but be warned that it will "eat up" any reliable/ordered Lidgren UDP packets by not processing them.
                // Thus it's best not to rely on Lidgren reliable/ordered as late joiners might not get them. (This is just one place where they could not actually be received)
                NetIncomingMessage? im;
                while ((im = client.ReadMessage()) != null)
                {
                    client.Recycle(im);
                }

                count++;
                Thread.Sleep(100);
            }

            if (client.ConnectionStatus != NetConnectionStatus.Connected)
            {
                throw new Exception("Failed to connect to relay server.");
            }
        }

        public void UpdateSend()
        {
            client.FlushSendQueue();
        }

        public void UpdateReceive()
        {
            heartbeatStopwatch.Start(); // Start here instead of constructor to avoid building up a potentially huge value at first and tripping up IsLagging()
            NetIncomingMessage? im;
            while ((im = client.ReadMessage()) != null)
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
                        if (status == NetConnectionStatus.Disconnected)
                        {
                            Console.WriteLine("Disconnected from relay server");
                            disconnectCallback.Invoke();
                        }
                        break;
                    default:
                        break;
                }
                heartbeatStopwatch.Restart();
                client.Recycle(im);
            }
        }

        public void Disconnect(string reason)
        {
            if (!disconnected)
            {
                disconnected = true;
                client?.Disconnect(reason);
            }
        }

        public bool IsLagging()
        {
            return heartbeatStopwatch.ElapsedMilliseconds > 5000;
        }
    }
}
