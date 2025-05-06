using ProtoBuf;

namespace Royale2D
{
    [ProtoContract]
    public class SyncedPlayerData
    {
        [ProtoMember(1)] public string name { get; set; } = "";
        [ProtoMember(2)] public int id { get; set; }
        [ProtoMember(3)] public string skin { get; set; } = "";     // PERF change it to a ushort index so it's 2 bytes
        [ProtoMember(4)] public Guid guid { get; set; }       // PERF too big?
        [ProtoMember(5)] public ushort? ping { get; set; }    // Player's ping to relay server, NOT the direct P2P connection ping if it exists
        [ProtoMember(6)] public bool isBot { get; set; }
        [ProtoMember(7)] public bool disconnected { get; set; }

        public SyncedPlayerData() { }

        public SyncedPlayerData(string name, int id)
        {
            this.name = name;
            this.id = id;
        }

        public SyncedPlayerData(string name, int id, string skin, Guid guid, bool isBot)
        {
            this.name = name;
            this.id = id;
            this.skin = skin;
            this.guid = guid;
            this.isBot = isBot;
        }
    }
}
