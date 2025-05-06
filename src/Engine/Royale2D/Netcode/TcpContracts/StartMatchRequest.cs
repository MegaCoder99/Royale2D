using ProtoBuf;

namespace Royale2D
{
    [ProtoContract]
    public class StartMatchRequest : ITcpRequest
    {
        [ProtoMember(1)] public string matchName = "";
        public char tcpMessageId => TcpMessageId.Start;

        public StartMatchRequest()
        {
        }

        public StartMatchRequest(string matchName)
        {
            this.matchName = matchName;
        }
    }
}
