namespace Royale2D
{
    public class RollingAverage
    {
        public List<long> values = [];
        public float average => values.Count > 0 ? (float)values.Average() : 0;
        public RollingAverage()
        {
        }

        public void AddValue(long value)
        {
            values.Add(value);
            if (values.Count > 60)
            {
                values.RemoveAt(0);
            }
        }

        string lastDisplayValue = "";

        // Use this if you want to poll every frame, will only update at a certain cadence
        public string GetLoopDisplayValue()
        {
            if (Game.frameCount % 60 == 0)
            {
                lastDisplayValue = average.ToString("0.0");
            }

            return lastDisplayValue;
        }
    }
}
