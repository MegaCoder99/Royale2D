
namespace Royale2D
{
    public class MapSection
    {
        // SHARED FIELDS
        public List<MapSectionLayer> layers = new List<MapSectionLayer>();
        public List<Instance> instances = new List<Instance>();
        public List<Zone> zones = new List<Zone>();
        public string defaultMusicName = "";

        // Alternatively to mapSection.indoorMappingToMain being set, this can be used for areas like death mountain and lost woods in overworld for simplification.
        // They have the same exact dimensions, but are just offset, and this is the offset position with respect to main overworld map section
        public GridCoords? mainSectionChildPos;

        // END SHARED FIELDS

        public Map map;
        public string name = "";
        public string fullPath = "";
        public string displayName => $"{name} ({map.name})";

        public int gridWidth;
        public int gridHeight;
        public int pixelWidth => gridWidth * 8;
        public int pixelHeight => gridHeight * 8;

        public List<GridZone> virtualSections = new List<GridZone>();
        public List<PixelZone> musicChangeZones = new List<PixelZone>();
        public List<PixelZone> noScrollZones = new List<PixelZone>();
        public List<GridZone> indoorMappingZones = new List<GridZone>();
        public GridRect? indoorMappingToMain;
        public int startLayer;

        public void Init(string fullPath, Map map)
        {
            this.name = Path.GetFileNameWithoutExtension(fullPath);
            this.fullPath = fullPath;
            this.map = map;
            gridHeight = layers[0].tileGrid.GetLength(0);
            gridWidth = layers[0].tileGrid.GetLength(1);
            foreach (MapSectionLayer layer in layers)
            {
                layer.Init(this, layers.IndexOf(layer));
            }
            foreach (Zone zone in zones)
            {
                switch (zone.zoneType)
                {
                    case ZoneType.VirtualSection:
                        virtualSections.Add(new GridZone(zone));
                        break;
                    case ZoneType.MusicChange:
                        musicChangeZones.Add(new PixelZone(zone));
                        break;
                    case ZoneType.NoScroll:
                        noScrollZones.Add(new PixelZone(zone));
                        break;
                    case ZoneType.IndoorMapping:
                        indoorMappingZones.Add(new GridZone(zone));
                        break;
                }
            }
        }

        public List<Actor> CreateActors(WorldSection section)
        {
            var actors = new List<Actor>();
            foreach (Instance instance in instances)
            {
                Actor? actor = instance.CreateActor(section);
                if (actor != null)
                {
                    actors.Add(actor);
                }
            }

            return actors;
        }

        public bool IsWoods()
        {
            return name == "woods" || name == "woods_grove";
        }
    }
}