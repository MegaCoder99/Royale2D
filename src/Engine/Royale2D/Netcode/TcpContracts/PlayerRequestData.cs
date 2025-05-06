using ProtoBuf;

namespace Royale2D
{
    [ProtoContract]
    public class PlayerRequestData
    {
        [ProtoMember(1)] public string name = "";
        [ProtoMember(2)] public string skin = "";
        [ProtoMember(3)] public Guid guid;

        public PlayerRequestData() { }

        public PlayerRequestData(string name, string skin, Guid guid)
        {
            this.name = name;
            this.skin = skin;
            this.guid = guid;
        }
    }
}
