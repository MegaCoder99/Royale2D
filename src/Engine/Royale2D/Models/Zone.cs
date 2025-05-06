namespace Royale2D
{
    public class ZoneType
    {
        public const string VirtualSection = "VirtualSection";
        public const string IndoorMapping = "IndoorMapping";
        public const string MusicChange = "MusicChange";
        public const string NoScroll = "NoScroll";
    }

    public class Zone
    {
        public string name = "";
        public string zoneType;
        public GridRect? gridRect;
        public Rect? rect;
    }

    public class GridZone
    {
        public string name = "";
        public string zoneType;
        public GridRect gridRect;

        public GridZone(string name, string zoneType, GridRect gridRect)
        {
            this.name = name;
            this.zoneType = zoneType;
            this.gridRect = gridRect;
        }

        public GridZone(Zone zone)
        {
            name = zone.name;
            zoneType = zone.zoneType;
            gridRect = zone.gridRect!.Value;
        }
    }

    public class PixelZone
    {
        public string name = "";
        public string zoneType;
        public Rect rect;
        public PixelZone(Zone zone)
        {
            name = zone.name;
            zoneType = zone.zoneType;
            rect = zone.rect!.Value;
        }
    }
}
