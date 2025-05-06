using System.Drawing;

namespace MapEditor;

public class ZoneTypes
{
    public static readonly ZoneType VirtualSection = new ZoneType("VirtualSection", true, Color.Yellow);
    public static readonly ZoneType IndoorMapping = new ZoneType("IndoorMapping", true, Color.Pink, "CSV of entrance name overrides");
    public static readonly ZoneType GenericGridBased = new ZoneType("GenericGridBased", true, Color.Magenta);
    public static readonly ZoneType GenericPixelBased = new ZoneType("GenericPixelBased", false, Color.Orange);
    public static readonly ZoneType MusicChange = new ZoneType("MusicChange", false, Color.Orange);
    public static readonly ZoneType NoScroll = new ZoneType("NoScroll", false, Color.Magenta);

    public static readonly List<ZoneType> all = 
    [
        VirtualSection,
        IndoorMapping,
        GenericGridBased,
        GenericPixelBased,
        MusicChange,
        NoScroll
    ];

    public static ZoneType GetZoneTypeObj(string zoneTypeName)
    {
        return all.First(zoneObj => zoneObj.name == zoneTypeName);
    }
}

public class ZoneType
{
    public string name;
    public bool isGridBased;
    public Color displayColor;
    public string propertyHelpText;

    public ZoneType(string name, bool isGridBased, Color displayColor, string propertyHelpText = "")
    {
        this.name = name;
        this.isGridBased = isGridBased;
        this.displayColor = displayColor;
        this.propertyHelpText = propertyHelpText;
    }
}