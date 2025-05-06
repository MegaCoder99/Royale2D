namespace Royale2D
{
    public class RemotePlayerSyncedInputs
    {
        public SyncedPlayerData playerData;

        public Dictionary<int, FrameInput> frameToInputs = new Dictionary<int, FrameInput>();
        public DisconnectData disconnectData = new DisconnectData();
        public int lastFrameOfMineTheyAcked = -1;
        public bool noMoreInputs;

        public int id => playerData.id;
        public string name => playerData.name;
        public bool disconnected => playerData.disconnected;

        public RemotePlayerSyncedInputs(SyncedPlayerData playerData) : base()
        {
            this.playerData = playerData;
        }

        public FrameInput GetInputsForFrame(int frameNum)
        {
            if (frameToInputs.ContainsKey(frameNum))
            {
                return frameToInputs[frameNum];
            }
            else if (noMoreInputs)
            {
                return new FrameInput(0);
            }
            else if (disconnected)
            {
                if (disconnectData.finalFrameInputsProcessed && disconnectData.finalFrameInputs != null)
                {
                    int lastFrameProcessed = disconnectData.finalFrameInputs.Keys.Max();
                    if (frameNum > lastFrameProcessed)
                    {
                        return new FrameInput(0);
                    }
                }
            }
            
            throw new Exception("Should not have gotten here.");
        }

        public IInputReader GetInputReader(int frameNum)
        {
            return new SyncedInputReader(
                GetInputsForFrame(frameNum),
                frameNum > 0 ? GetInputsForFrame(frameNum - 1) : null
            );
        }

        private int _cachedLastCompleteFrame = -1;  // Purely an optimization to avoid iterating over all frames every time we need to get the last complete frame
        public int GetLastCompleteFrame()
        {
            int i;
            for (i = _cachedLastCompleteFrame + 1; i < frameToInputs.Keys.Count; i++)
            {
                if (!frameToInputs.ContainsKey(i))
                {
                    break;
                }
            }
            int lastCompleteFrame = i - 1;
            _cachedLastCompleteFrame = lastCompleteFrame;
            return lastCompleteFrame;
        }

        public bool IsWaitingForPlayer(int frameNum)
        {
            // Brief time after death or win state (or for specs) a remote char's inputs won't matter, and don't need to wait on them
            if (noMoreInputs) return false;
            if (disconnected && !disconnectData.finalFrameInputsProcessed) return false;
            if (!disconnected && !frameToInputs.ContainsKey(frameNum))
            {
                return true;
            }
            return false;
        }

        public string WaitingForPlayerDebugMessage(int frameNum)
        {
            if (IsWaitingForPlayer(frameNum))
            {
                return playerData.name + ":\nlast complete frame was " + GetLastCompleteFrame();
            }
            return "";
        }
    }
}
