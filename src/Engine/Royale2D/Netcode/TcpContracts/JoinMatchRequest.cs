using ProtoBuf;

namespace Royale2D
{
    [ProtoContract]
    public class JoinMatchRequest : ITcpRequest
    {
        [ProtoMember(1)] public string matchName;
        [ProtoMember(2)] public PlayerRequestData playerRequestData;
        public char tcpMessageId => TcpMessageId.Join;

        public JoinMatchRequest()
        {
        }

        public JoinMatchRequest(string matchName, PlayerRequestData playerRequestData)
        {
            this.matchName = matchName;
            this.playerRequestData = playerRequestData;
        }
    }
}
