using Lidgren.Network;

namespace Royale2D
{
    public class ServerPlayer
    {
        public SyncedPlayerData data;
        public ServerPlayerSyncedInputs syncedInputs;
        public NetConnection? udpConnection;

        public ServerPlayer(SyncedPlayerData data)
        {
            this.data = data;
            syncedInputs = new ServerPlayerSyncedInputs(data);
        }
    }
}
