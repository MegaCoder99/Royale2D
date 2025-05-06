namespace Royale2D
{
    public class LocalPlayerSyncedInputs
    {
        public SyncedPlayerData playerData;
        public int id => playerData.id;
        public string name => playerData.name;
        public Dictionary<int, FrameInput> frameToInputs = new Dictionary<int, FrameInput>();

        public LocalPlayerSyncedInputs(SyncedPlayerData playerData) : base()
        {
            this.playerData = playerData;
        }

        // Only to be used and read in game state/world's Update() method
        public IInputReader GetInputReader(int frameNum)
        {
            return new SyncedInputReader(
                frameToInputs[frameNum],
                frameNum > 0 ? frameToInputs[frameNum - 1] : null
            );
        }
    }
}
