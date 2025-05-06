using SimpleTCP;
using System.Collections.Concurrent;
using Message = SimpleTCP.Message;

namespace Royale2D
{
    public class RelayServer
    {
        SimpleTcpServer tcpServer;
        public ConcurrentBag<RelayServerMatch> matches = new ConcurrentBag<RelayServerMatch>();
        public const int baseUdpPort = 10000;
        public const int baseTcpPort = 10001;
        public bool disconnected;

        public RelayServer()
        {
            tcpServer = new SimpleTcpServer().Start(baseTcpPort);
            tcpServer.Delimiter = 0x13;
            tcpServer.DelimiterDataReceived += OnTcpServerDataReceived;
        }

        public void Update()
        {

        }

        public int GetFirstAvailableUdpPort()
        {
            int port = baseUdpPort;
            while (matches.Any(m => m.udpPort == port))
            {
                port++;
            }
            return port;
        }

        public void OnTcpServerDataReceived(object? sender, Message msg)
        {
            string message = msg.MessageString;
            if (string.IsNullOrEmpty(message))
            {
                Log("Message recieved was null or empty.");
            }

            char messageId = message[0];
            message = message.Substring(1);

            try
            {
                if (messageId == TcpMessageId.Create)
                {
                    CreateMatchRequest request = Helpers.DeserializeBase64<CreateMatchRequest>(message);
                    TcpResponse<CreateMatchResponse> response = CreateMatch(request);
                    msg.Reply(Helpers.SerializeBase64(response));
                }
                else if (messageId == TcpMessageId.Join)
                {
                    JoinMatchRequest request = Helpers.DeserializeBase64<JoinMatchRequest>(message);
                    TcpResponse<JoinMatchResponse> response = JoinMatch(request);
                    msg.Reply(Helpers.SerializeBase64(response));
                }
                else if (messageId == TcpMessageId.Start)
                {
                    StartMatchRequest request = Helpers.DeserializeBase64<StartMatchRequest>(message);
                    TcpResponse<bool> response = StartMatch(request);
                    msg.Reply(Helpers.SerializeBase64(response));
                }
                else
                {
                    throw new Exception("Invalid message id");
                }
            }
            catch (Exception ex)
            {
                Log($"Exception when processing message with id {messageId}: {ex.Message}");
                throw;
            }
        }

        public TcpResponse<CreateMatchResponse> CreateMatch(CreateMatchRequest request)
        {
            if (matches.Any(m => m.settings.matchName == request.settings.matchName))
            {
                Log("Match " + request.settings.matchName + " already exists.");
                return new TcpResponse<CreateMatchResponse>("Match already exists");
            }

            RelayServerMatch match = new RelayServerMatch(this, request.settings, GetFirstAvailableUdpPort());
            match.Start();
            matches.Add(match);

            ServerPlayer player = match.players.FirstOrDefault(p => p.data.guid == request.playerRequestData.guid) ??
                match.AddPlayer(request.playerRequestData);

            Log("Creating match " + request.settings.matchName + " on map " + request.settings.mapName + " and port " + match.udpPort);
            return new TcpResponse<CreateMatchResponse>(new CreateMatchResponse(match.settings, match.udpPort, player.data));
        }

        public TcpResponse<JoinMatchResponse> JoinMatch(JoinMatchRequest request)
        {
            RelayServerMatch? match = matches.FirstOrDefault(m => m.settings.matchName == request.matchName);
            if (match == null)
            {
                Log("Match " + request.matchName + " not found.");
                return new TcpResponse<JoinMatchResponse>("Match not found");
            }

            ServerPlayer player = match.players.FirstOrDefault(p => p.data.guid == request.playerRequestData.guid) ?? 
                match.AddPlayer(request.playerRequestData);
            
            Log("Returning match " + match.settings.matchName + " on map " + match.settings.mapName + " and port " + match.udpPort);
            return new TcpResponse<JoinMatchResponse>(new JoinMatchResponse(match.settings, match.udpPort, player.data));
        }

        public TcpResponse<bool> StartMatch(StartMatchRequest request)
        {
            RelayServerMatch? match = matches.FirstOrDefault(m => m.settings.matchName == request.matchName);
            if (match == null)
            {
                Log(request.matchName + " not found");
                return new TcpResponse<bool>("Match not found");
            }

            Log("Starting match");
            match.matchStarted = true;

            return new TcpResponse<bool>(true);
        }

        public void RemoveMatch(RelayServerMatch relayServerMatch)
        {
            matches.RemoveItem(relayServerMatch);
        }

        public void Disconnect(string message)
        {
            if (!disconnected)
            {
                Log("Shutting down relay server entirely.");
                disconnected = true;
                foreach (RelayServerMatch match in matches.ToList())
                {
                    match?.Disconnect(message);
                }

                tcpServer?.Stop();
            }
        }

        public void Log(string message)
        {
            Console.WriteLine("Relay Server: " + message);
        }
    }
}
