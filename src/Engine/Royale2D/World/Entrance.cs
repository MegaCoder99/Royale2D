namespace Royale2D
{
    public class Entrance
    {
        // Each entrance is uniquely identified by its entranceId + sectionName (there will always be 2 entrances with the same entranceId, one in both the corresponding sections)
        public string entranceId;
        public string sectionName;

        public FdPoint pos;
        public int layerIndex;
        public Direction enterDir;
        public bool fall = false;
        public bool land = false;
        public string overrideMusicName = "";
        public bool noMusicChange = false;
        public bool useOffset;
        public Entrance? linkedEntrance;
        public IntRect worldRect;
        
        public Entrance(string entranceId, string sectionName, IntPoint pos, Direction enterDir, bool fall, bool land, int width, int height, int layerIndex, string overrideMusicName)
        {
            this.entranceId = entranceId;
            this.sectionName = sectionName;
            this.pos = pos.ToFdPoint();
            this.enterDir = enterDir;
            this.fall = fall;
            this.land = land;
            this.layerIndex = layerIndex;
            this.useOffset = width > 16 || height > 16;
            this.overrideMusicName = overrideMusicName;
            worldRect = IntRect.CreateWHCentered(pos.x, pos.y, width, height);
        }

        public void Enter(Character character)
        {
            if (land)
            {
                return;
            }
            
            character.ChangeMapSection(this, linkedEntrance!);
        }
    }
}
