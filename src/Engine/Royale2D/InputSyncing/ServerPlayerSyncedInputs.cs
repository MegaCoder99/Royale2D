namespace Royale2D
{
    public class ServerPlayerSyncedInputs
    {
        public SyncedPlayerData data;
        public Dictionary<ServerPlayerSyncedInputs, Dictionary<int, FrameInput>> playerToFinalFramesTheySaw = new Dictionary<ServerPlayerSyncedInputs, Dictionary<int, FrameInput>>();
        public DisconnectData disconnectData = new DisconnectData();

        public bool disconnected => data.disconnected;
        public int id => data.id;

        public bool lagging; // Only used by sim

        public ServerPlayerSyncedInputs(SyncedPlayerData data)
        {
            this.data = data;
        }

        public Dictionary<int, FrameInput> GetSynthesizedDisconnectFrames()
        {
            var synthesizedFrameToInputs = new Dictionary<int, FrameInput>();
            foreach ((ServerPlayerSyncedInputs serverPlayer, Dictionary<int, FrameInput> frameToInput) in playerToFinalFramesTheySaw)
            {
                foreach ((int frameNum, FrameInput inputs) in frameToInput)
                {
                    synthesizedFrameToInputs[frameNum] = inputs;
                }
            }
            return synthesizedFrameToInputs;
        }
    }
}
