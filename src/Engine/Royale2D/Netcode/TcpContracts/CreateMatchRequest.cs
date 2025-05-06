using ProtoBuf;

namespace Royale2D
{
    [ProtoContract]
    public class CreateMatchRequest : ITcpRequest
    {
        [ProtoMember(1)] public MatchSettings settings;
        [ProtoMember(2)] public PlayerRequestData playerRequestData;
        public char tcpMessageId => TcpMessageId.Create;

        public CreateMatchRequest()
        {
        }

        public CreateMatchRequest(MatchSettings settings, PlayerRequestData playerRequestData)
        {
            this.settings = settings;
            this.playerRequestData = playerRequestData;
        }
    }
}
