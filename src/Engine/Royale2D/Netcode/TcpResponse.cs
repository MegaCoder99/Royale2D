using ProtoBuf;

namespace Royale2D
{
    // Every TCP response must be in this shape. If no response contract is necessary, use bool for the T with true as the value
    [ProtoContract]
    public class TcpResponse<T>
    {
        [ProtoMember(1)] public T? value;
        [ProtoMember(2)] public string errorMessage;

        public TcpResponse()
        {
            errorMessage = "";
        }

        public TcpResponse(T? value, string errorMessage)
        {
            this.value = value;
            this.errorMessage = errorMessage;
        }

        public TcpResponse(T value) : this(value, "")
        {
        }

        public TcpResponse(string errorMessage) : this(default, errorMessage)
        {
        }

        public TcpResponse(Exception exception) : this(exception.Message)
        {
        }

        public Result result
        {
            get
            {
                if (!string.IsNullOrEmpty(errorMessage)) return Result.Error(errorMessage);
                else if (value == null) return Result.Error("TcpResponse value was null.");
                else return Result.Success();
            }
        }
    }
}
