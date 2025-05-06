namespace Royale2D
{
    // Every TCP request must implement this and return the assigned TcpMessageId.
    public interface ITcpRequest
    {
        public char tcpMessageId { get; }
    }

    public class TcpMessageId
    {
        public const char Create = '0';
        public const char Join = '1';
        public const char Start = '2';
    }
}
