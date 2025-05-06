namespace Royale2D
{
    public class DisconnectData
    {
        // If null, clients won't proceed until it's set so they know the frame of DC and the final inputs of DC'ing player
        public Dictionary<int, FrameInput>? finalFrameInputs;
        // Set locally once finalFrameInputs was synced in the corresponding remote player's frameToInputs
        public bool finalFrameInputsProcessed;
    }
}