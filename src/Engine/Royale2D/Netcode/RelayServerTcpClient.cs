using SimpleTCP;
using Message = SimpleTCP.Message;

namespace Royale2D
{
    public class RelayServerTcpClient
    {
        SimpleTcpClient tcpClient;
        public const float TcpTimeout = 2;
        bool disconnected;

        public RelayServerTcpClient(string relayServerIp)
        {
            tcpClient = new SimpleTcpClient().Connect(relayServerIp, RelayServer.baseTcpPort);
        }

        private TcpResponse<TResponse> SendTcpRequestNoThrow<TResponse>(ITcpRequest request)
        {
            Message? message;
            try
            {
                message = tcpClient.WriteLineAndGetReply(request.tcpMessageId + Helpers.SerializeBase64(request), TimeSpan.FromSeconds(TcpTimeout));
                if (message == null)
                {
                    return new TcpResponse<TResponse>(default, "TCP Timeout");
                }
            }
            catch (Exception e)
            {
                return new TcpResponse<TResponse>(default, e.Message);
            }

            return Helpers.DeserializeBase64<TcpResponse<TResponse>>(message.MessageString);
        }

        public TResponse SendTcpRequest<TResponse>(ITcpRequest request)
        {
            TcpResponse<TResponse> tcpResponse = SendTcpRequestNoThrow<TResponse>(request);
            if (tcpResponse.result.failed || tcpResponse.value == null)
            {
                throw new Exception(tcpResponse.result.errorMessage);
            }

            return tcpResponse.value;
        }

        public void Disconnect(string reason)
        {
            if (!disconnected)
            {
                disconnected = true;
                tcpClient?.Disconnect();
                tcpClient?.Dispose();
            }
        }
    }
}
