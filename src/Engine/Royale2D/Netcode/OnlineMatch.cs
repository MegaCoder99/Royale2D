using Lidgren.Network;

namespace Royale2D
{
    public class OnlineMatch : Match
    {
        public RelayServerTcpClient matchTcpClient;   // This is the TCP connection to the relay server.
        public RelayServerUdpClient matchUdpClient;   // This is the UDP connection to the relay server

        public RelayServer? relayServer;              // If running the relay server in the same process, this is the object for it

        bool leftMatch;
        public bool disconnected
        {
            get
            {
                bool dc = matchUdpClient.client.ConnectionStatus == NetConnectionStatus.Disconnected;
                if (dc)
                {
                }
                return dc;
            }
        }

        public NetcodeSafeRng netcodeSafeRng;
        public bool isHost => mainPlayer.id == 0;
        public bool isClient => !isHost;
        public string hostIp => "0.0.0.0";
        public OnlineWorldHost? onlineWorldHost => worldHost as OnlineWorldHost;
        public bool desyncDetected;

        private OnlineMatch(string relayServerIp, RelayServerTcpClient matchTcpClient, MatchSettings settings, int matchUdpPort, SyncedPlayerData mainPlayer, RelayServer? relayServer) :
            base(settings, mainPlayer)
        {
            try
            {
                this.matchTcpClient = matchTcpClient;
                this.relayServer = relayServer;

                matchUdpClient = new RelayServerUdpClient(relayServerIp, matchUdpPort, OnReceiveUdpMessage, () => Leave("Disconnected from match."));
                matchUdpClient.Connect(mainPlayer.id);
            }
            catch
            {
                Disconnect("error");
                throw;
            }

            netcodeSafeRng = new NetcodeSafeRng(settings.rngSeed);
        }

        public static OnlineMatch Create(MatchSettings matchSettings, string relayServerIp)
        {
            RelayServer? relayServer = null;
            RelayServerTcpClient? matchTcpClient = null;
            try
            {
                if (matchSettings.isLAN || matchSettings.hostRelayServerLocally)
                {
                    relayServer = new RelayServer();
                }

                matchTcpClient = new RelayServerTcpClient(relayServerIp);
                CreateMatchResponse matchResponse = matchTcpClient.SendTcpRequest<CreateMatchResponse>(new CreateMatchRequest(matchSettings, Options.main.getPlayerRequestData()));

                SyncedPlayerData mainPlayer = matchResponse.playerData;

                return new OnlineMatch(relayServerIp, matchTcpClient, matchResponse.settings, matchResponse.udpPort, mainPlayer, relayServer);
            }
            catch
            {
                relayServer?.Disconnect("error");
                matchTcpClient?.Disconnect("error");
                throw;
            }
        }

        public static OnlineMatch Join(string matchName, string relayServerIp)
        {
            RelayServerTcpClient? matchTcpClient = null;
            try
            {
                matchTcpClient = new RelayServerTcpClient(relayServerIp);
                JoinMatchResponse matchResponse = matchTcpClient.SendTcpRequest<JoinMatchResponse>(new JoinMatchRequest(matchName, Options.main.getPlayerRequestData()));

                SyncedPlayerData mainPlayer = matchResponse.playerData;
                return new OnlineMatch(relayServerIp, matchTcpClient, matchResponse.settings, matchResponse.udpPort, mainPlayer, null);
            }
            catch
            {
                matchTcpClient?.Disconnect("error");
                throw;
            }
        }

        public override void Start()
        {
            worldHost = new OnlineWorldHost(this);
            Menu.ChangeMenu(null);
        }

        public override void Update()
        {
            Debug.waitingForPlayers = false;

            matchUdpClient.UpdateReceive();

            worldHost?.Update();

            matchUdpClient.UpdateSend();
        }

        public void OnReceiveUdpMessage(IUdpMessage udpMessage)
        {
            if (udpMessage is ServerToClientMatchSyncUM matchSyncUM)
            {
                if (!matchSyncUM.matchStarted && !started)
                {
                    players = matchSyncUM.playerDatas;
                }
                else if (matchSyncUM.matchStarted && !started)
                {
                    Console.WriteLine("Received message from relay server to start match. Starting...");
                    players = matchSyncUM.playerDatas;
                    Start();
                }
                else if (matchSyncUM.matchStarted && started)
                {
                    SyncPlayerDatasAfterStart(matchSyncUM.playerDatas);
                }
                desyncDetected = matchSyncUM.desyncDetected;
            }
            else if (udpMessage is PeerToPeerInputUM peerInputUM)
            {
                // If our world is null, we can still receive others' input messages if their match/world started earlier than ours due to CPU execution speed or other factors
                // But we are covered by the "ack" system in that case so even if we miss the messages and "swallow" them here, we're fine,
                // the next round of UDP messages will cover us (benefits of a pull system vs a push system!)
                onlineWorldHost?.inputSyncer?.OnReceiveInputMessage(peerInputUM);
            }
            else if (udpMessage is ServerToClientDcFramesUM dcFramesUM)
            {
                onlineWorldHost?.inputSyncer?.OnReceiveDisconnectFrames(dcFramesUM.disconnectingPlayerId, dcFramesUM.synthesizedDisconnectFrames);
            }
        }

        public void SyncPlayerDatasAfterStart(List<SyncedPlayerData> serverPlayerDatas)
        {
            // We need to not replace player list after the match has started for two reasons:
            // 1. Some time after match starts the server will stop sending most static player data fields to preserve bandwidth (GMPERF implement this optimization)
            // 2. Also we need to preserve references to SyncedPlayerData in WorldHost's inputSyncer.remotePlayers
            foreach (SyncedPlayerData serverPlayerData in serverPlayerDatas)
            {
                SyncedPlayerData localPlayerData = players.First(p => p.id == serverPlayerData.id);
                localPlayerData.disconnected = serverPlayerData.disconnected;
                localPlayerData.ping = serverPlayerData.ping;
            }
        }

        public override void Leave(string forceLeaveMessage = "")
        {
            if (!leftMatch)
            {
                leftMatch = true;
                Disconnect(forceLeaveMessage == "" ? "Manually left" : forceLeaveMessage);
                base.Leave(forceLeaveMessage);
            }
        }

        public void Disconnect(string reason)
        {
            Console.WriteLine("Disconnecting. Reason: " + reason);
            matchTcpClient?.Disconnect(reason);
            matchUdpClient?.Disconnect(reason);

            // If the relay server is running locally in-proc (i.e. you are host and not running it separately), this will shut it down, ending the match for everyone
            relayServer?.Disconnect(reason);
        }
    }
}
