namespace Royale2D
{
    public struct NudgeData
    {
        public IntPoint dir;
        public bool splitXY;  // Whether or not to nudge x and y axes separately, and do the ones that work
        public int priority;
        public bool useDiagSpeed;
        public bool isDiagOnRect;
        public bool isLedge;
        public bool useSpeedOfOne;

        public NudgeData(IntPoint dir, bool shouldSplitXY, int priority, bool useDiagSpeed, bool isDiagOnRect, bool isLedge, bool useSpeedOfOne)
        {
            this.dir = dir;
            this.splitXY = shouldSplitXY;
            this.priority = priority;
            this.useDiagSpeed = useDiagSpeed;
            this.isDiagOnRect = isDiagOnRect;
            this.isLedge = isLedge;
            this.useSpeedOfOne = useSpeedOfOne;
        }
    }
}
