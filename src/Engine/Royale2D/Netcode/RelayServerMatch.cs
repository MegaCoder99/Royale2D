using Lidgren.Network;
using Shared;
using System.Diagnostics;
using System.Security.Policy;
using System.Threading;

namespace Royale2D
{
    public class RelayServerMatch
    {
        public RelayServer relayServer;
        public MatchSettings settings;
        public bool matchStarted;
        public bool desyncDetected;
        public int udpPort;
        public NetServer server;
        public List<ServerPlayer> players = new List<ServerPlayer>();
        public List<ServerPlayerSyncedInputs> playerSyncedInputs => players.Select(p => p.syncedInputs).ToList();
        public int nonDisconnectedPlayerCount => players.Count(p => !p.data.disconnected);

        // If and when we get a separate relay server project, measure its CPU usage from this value. Benchmark with the async approach.
        const int sleepTimeMs = 1;

        public float deltaTime;

        public float inputSyncTime;
        public float flushTime;
        public float playerSyncTime;
        public float timeWithNoPlayers;

        Thread? thread;

        public RelayServerMatch(RelayServer relayServer, MatchSettings settings, int udpPort)
        {
            this.relayServer = relayServer;
            this.settings = settings;
            this.udpPort = udpPort;

            NetPeerConfiguration config = new NetPeerConfiguration(Game.appId);
            config.MaximumConnections = Game.MaxConnections;
            config.Port = udpPort;
            config.AutoFlushSendQueue = false;
            config.ConnectionTimeout = Game.ConnectionTimeoutSeconds;
            config.AcceptIncomingConnections = true;
#if DEBUG
            config.SimulatedMinimumLatency = Debug.simulatedLatency;
            config.SimulatedLoss = Debug.simulatedPacketLoss;
            config.SimulatedDuplicatesChance = Debug.simulatedDuplicates;
#endif
            server = new NetServer(config);
            server.Start();
        }

        public void Start()
        {
            thread = new Thread(Work);
            thread.Start();
        }

        public void Work()
        {
            Stopwatch stopwatch = new Stopwatch();
            while (!disconnected)
            {
                stopwatch.Restart();
                Update();
                int sleepTime = sleepTimeMs - (int)stopwatch.ElapsedMilliseconds;
                if (sleepTime > 0) Thread.Sleep(sleepTime);
                deltaTime = stopwatch.ElapsedMilliseconds / 1000f;
                //Console.WriteLine("Sleep time: " + sleepTime.ToString());
            }

            thread?.Join();
        }

        public void Update()
        {
            NetIncomingMessage? im;
            while ((im = server.ReadMessage()) != null)
            {
                // handle incoming message
                switch (im.MessageType)
                {
                    case NetIncomingMessageType.Data:
                        IUdpMessage udpMessage = UdpHelper.Get(im);
                        if (udpMessage is PeerToPeerInputUM p2pInputUM)
                        {
                            // Shouldn't need to batch in relay server, client will batch before sending
                            NetConnection? recipientConnection = GetConnectionFromPlayerId(p2pInputUM.recipientId);
                            if (recipientConnection != null)
                            {
                                UdpHelper.Send(server, recipientConnection, p2pInputUM);
                            }
                        }
                        else if (udpMessage is ClientToServerDcFramesUM dfm)
                        {
                            ProcessDisconnectFrameMessage(dfm);
                        }
                        else if (udpMessage is ClientToServerDesyncDetectUM ddm)
                        {
                            ProcessDesyncDetectMessage(ddm);
                        }
                        break;
                    case NetIncomingMessageType.StatusChanged:
                        NetConnectionStatus status = (NetConnectionStatus)im.ReadByte();
                        if (status == NetConnectionStatus.Connected)
                        {
                            string playerIdStr = im.SenderConnection!.RemoteHailMessage!.ReadString();
                            byte playerId = byte.Parse(playerIdStr);

                            Console.WriteLine($"Client connected to server: {im.SenderEndPoint}, player id {playerIdStr}");

                            ServerPlayer? player = players.FirstOrDefault(p => p.data.id == playerId);
                            if (player != null)
                            {
                                player.udpConnection = im.SenderConnection;
                                if (settings.isP2P)
                                {
                                    // player.data.ip = im.SenderEndPoint!.Address.ToString();
                                    // player.data.natPunchthruPort = (ushort)im.SenderEndPoint.Port;
                                }
                            }
                            else
                            {
                                Console.WriteLine("Player id not found.");
                            }
                        }
                        else if (status == NetConnectionStatus.Disconnected)
                        {
                            Console.WriteLine("Relay server: disconnect request received");
                            ServerPlayer? player = GetPlayerFromConnection(im.SenderConnection);
                            if (player != null && !player.data.disconnected)
                            {
                                Console.WriteLine("Player " + player.data.id + " disconnected");

                                if (matchStarted)
                                {
                                    // If the match starts, don't hard-remove the player, set their disconnected flag to true.
                                    // This gives them a chance to rejoin, and also makes the client and server logic simplier
                                    player.data.disconnected = true;
                                }
                                else
                                {
                                    players.Remove(player);
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
                server.Recycle(im);
            }

            if (players.Count == 0)
            {
                timeWithNoPlayers += deltaTime;
                if (timeWithNoPlayers > 5)
                {
                    Disconnect("No players detected, shutting down.");
                    return;
                }
            }
            else
            {
                timeWithNoPlayers = 0;
            }

            playerSyncTime += deltaTime;
            if (playerSyncTime > 0.5f)
            {
                playerSyncTime = 0;

                foreach (ServerPlayer player in players)
                {
                    if (player.udpConnection != null)
                    {
                        player.data.ping = player.udpConnection.GetUshortPing(false);
                    }
                }

                foreach (NetConnection conn in GetClientConnections())
                {
                    UdpHelper.Send(server, conn, new ServerToClientMatchSyncUM(players.Select(p => p.data).ToList(), matchStarted, desyncDetected));
                }
            }

            flushTime += deltaTime;
            if (flushTime >= 0)
            {
                flushTime = 0;
                server.FlushSendQueue();
            }
        }

        public Dictionary<int, string> frameToGameState = new();
        public HashSet<int> loggedDesyncFrames = new();
        public void ProcessDesyncDetectMessage(ClientToServerDesyncDetectUM ddm)
        {
            if (!frameToGameState.ContainsKey(ddm.lastCompleteFrameNum))
            {
                frameToGameState.Add(ddm.lastCompleteFrameNum, ddm.gameStateHash);
            }
            else if (frameToGameState[ddm.lastCompleteFrameNum] != ddm.gameStateHash)
            {
                if (!loggedDesyncFrames.Contains(ddm.lastCompleteFrameNum))
                {                     
                    loggedDesyncFrames.Add(ddm.lastCompleteFrameNum);
                    Console.WriteLine($"Desync detected. Frame {ddm.lastCompleteFrameNum} had hash {frameToGameState[ddm.lastCompleteFrameNum]} which does not match player {ddm.senderId}'s hash {ddm.gameStateHash}");
                }
                if (!desyncDetected)
                {
                    desyncDetected = true;
                    //Console.WriteLine($"Desync detected. Frame {ddm.lastCompleteFrameNum} had hash {frameToGameState[ddm.lastCompleteFrameNum]} which does not match player {ddm.senderId}'s hash {ddm.gameStateHash}");
                }
            }
        }

        public void ProcessDisconnectFrameMessage(ClientToServerDcFramesUM dfm)
        {
            ServerPlayerSyncedInputs senderSP = playerSyncedInputs.First(p => p.id == dfm.senderId);
            ServerPlayerSyncedInputs disconnectingSP = playerSyncedInputs.First(p => p.id == dfm.disconnectorId);

            if (disconnectingSP.disconnected)
            {
                disconnectingSP.playerToFinalFramesTheySaw[senderSP] = dfm.inputs;

                List<ServerPlayerSyncedInputs> nonDisconnectedPlayerKeys = disconnectingSP.playerToFinalFramesTheySaw.Keys.ToList().FindAll(p => !p.disconnected);

                // Only synthesize the completed final disconnect frames for a disconnected player if we have all the data from all other non-disconnected players
                if (nonDisconnectedPlayerKeys.Count >= nonDisconnectedPlayerCount)
                {
                    Console.WriteLine("Synthesized disconnect frames for player " + disconnectingSP.id + " with " + nonDisconnectedPlayerKeys.Count + " non-disconnected players.");
                    Dictionary<int, FrameInput> synthesizedDisconnectFrames = disconnectingSP.GetSynthesizedDisconnectFrames();
                    SendSynthesizedDisconnectFrames(senderSP.id, disconnectingSP.id, synthesizedDisconnectFrames);
                }
            }
        }

        public void SendSynthesizedDisconnectFrames(int recipientId, int disconnectingPlayerId, Dictionary<int, FrameInput> synthesizedDisconnectFrames)
        {
            NetConnection? recipientConnection = GetConnectionFromPlayerId(recipientId);
            if (recipientConnection != null)
            {
                var synthesizedDisconnectFramesUM = new ServerToClientDcFramesUM(recipientId, disconnectingPlayerId, synthesizedDisconnectFrames);
                UdpHelper.Send(server, recipientConnection, synthesizedDisconnectFramesUM);
            }
        }

        public List<NetConnection> GetClientConnections()
        {
            var connections = new List<NetConnection>();
            foreach (ServerPlayer player in players)
            {
                if (player.udpConnection != null)
                {
                    connections.Add(player.udpConnection);
                }
            }
            return connections;
        }

        public ServerPlayer? GetPlayerFromConnection(NetConnection? conn)
        {
            if (conn == null) return null;
            return players.FirstOrDefault(p => p.udpConnection == conn);
        }

        public NetConnection? GetConnectionFromPlayerId(int? playerId)
        {
            if (playerId == null) return null;
            return players.FirstOrDefault(p => p.data.id == playerId)?.udpConnection;
        }

        public ServerPlayer AddPlayer(PlayerRequestData playerRequestData)
        {
            // NETCODE isBot flag

            var syncedPlayerData = new SyncedPlayerData(
                GetFirstAvailableName(playerRequestData.name), 
                GetFirstAvailablePlayerId(), 
                playerRequestData.skin, 
                playerRequestData.guid,
                false
            );

            var player = new ServerPlayer(syncedPlayerData);
            players.Add(player);
            return player;
        }

        public int GetFirstAvailablePlayerId()
        {
            int id = 0;
            while (players.Any(p => p.data.id == id))
            {
                id++;
            }

            return id;
        }

        public string GetFirstAvailableName(string playerName)
        {
            // Name already in use: append number at end
            while (true)
            {
                if (players.Any(p => p.data.name == playerName))
                {
                    char lastCharInName = playerName[playerName.Length - 1];
                    if (int.TryParse(lastCharInName.ToString(), out var result))
                    {
                        if (result != 9)
                        {
                            result++;
                            playerName = playerName.Substring(0, playerName.Length - 1) + result.ToString();
                        }
                        else
                        {
                            playerName += "(1)";
                        }
                    }
                    else
                    {
                        if (playerName.Length < 8)
                        {
                            playerName += "1";
                        }
                        else
                        {
                            playerName = playerName.Remove(playerName.Length - 1) + "1";
                        }
                    }
                }
                else
                {
                    break;
                }
            }
            return playerName;
        }

        public bool disconnected;
        public void Disconnect(string message)
        {
            if (!disconnected)
            {
                disconnected = true;
                Console.WriteLine("Shutting down match: " + message);
                relayServer.RemoveMatch(this);
                server?.Shutdown(message);
            }
        }
    }
}
