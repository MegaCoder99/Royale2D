using Editor;

namespace MapEditor;

public class InstanceTypes
{
    public static readonly InstanceType Entrance = new InstanceType("Entrance", new BitmapDrawer(Resources.EntranceInstanceImageUri), "musicKey=<music key>");
    public static readonly InstanceType Generic = new InstanceType("Generic", new BitmapDrawer(Resources.DefaultInstanceImageUri), "Generic instance type");
    public static readonly InstanceType Fairy = new InstanceType("Fairy", new BitmapDrawer(Resources.DefaultInstanceImageUri), "Fairy");
    public static readonly InstanceType BigFairy = new InstanceType("BigFairy", new BitmapDrawer(Resources.DefaultInstanceImageUri), "Big Fairy");
    public static readonly InstanceType WorldNumber = new InstanceType("WorldNumber", new BitmapDrawer(Resources.DefaultInstanceImageUri), "World Number");
    public static readonly InstanceType FightersSword = new InstanceType("FightersSword", new BitmapDrawer(Resources.DefaultInstanceImageUri), "Fighter's Sword");
    public static readonly InstanceType ShopItem = new InstanceType("ShopItem", new BitmapDrawer(Resources.DefaultInstanceImageUri), "Shop Item");
    public static readonly InstanceType Npc = new InstanceType("Npc", new BitmapDrawer(Resources.DefaultInstanceImageUri), "NPC");

    public static readonly InstanceType[] all = 
    [
        Entrance,
        Generic,
        WorldNumber,
        FightersSword,
        ShopItem,
        Npc
    ];

    public static InstanceType GetInstanceTypeObj(string instanceTypeName)
    {
        return all.First(instanceObj => instanceObj.name == instanceTypeName);
    }
    
}

public class InstanceType
{
    public string name;
    public Drawer drawer;
    public string propertyHelpText;

    public InstanceType(string name, Drawer drawer, string propertyHelpText)
    {
        this.name = name;
        this.drawer = drawer;
        this.propertyHelpText = propertyHelpText;
    }

    public static InstanceType CreateFromUri(string name, Uri? iconResourceUri = null, string propertyHelpText = "")
    {
        return new InstanceType(name, new BitmapDrawer(iconResourceUri ?? Resources.DefaultInstanceImageUri), propertyHelpText);
    }

    public static InstanceType CreateFromEncoding(string name, string encodedIconPng, string propertyHelpText = "")
    {
        return new InstanceType(name, BitmapDrawer.FromBase64Encoding(encodedIconPng), propertyHelpText);
    }
}