using Shared;
namespace Royale2D
{
    // ANOW1 this is special, since it's the only one that's mapped and no longer re-used
    public record Instance : InstanceModel
    {
        public Instance() : base("", "", new MyPoint(0, 0), "", 0, null)
        {

        }

        public Entrance? CreateEntrance(string sectionName)
        {
            int x = pos.x;
            int y = pos.y;

            IntPoint createPos = new IntPoint(x, y);

            if (instanceType == InstanceType.Entrance)
            {
                Direction dir = Direction.Down;
                string dirStr = entranceData!.direction;
                if (dirStr == "up") dir = Direction.Up;
                if (dirStr == "down") dir = Direction.Down;
                if (dirStr == "left") dir = Direction.Left;
                if (dirStr == "right") dir = Direction.Right;

                int width = 16;
                if (entranceData.width.IsSet()) width = int.Parse(entranceData!.width);

                int height = 16;
                if (entranceData.height.IsSet()) height = int.Parse(entranceData!.height);

                string overrideMusicName = properties.IsSet() ? properties.Split('=')[1] : "";

                Entrance entrance = new Entrance(name, sectionName, createPos, dir, dirStr == "fall", dirStr == "land", width, height, layerIndex, overrideMusicName);
                return entrance;
            }

            return null;
        }

        public Actor? CreateActor(WorldSection section)
        {
            int x = pos.x;
            int y = pos.y;

            FdPoint createPos = new FdPoint(x, y);
            if (instanceType == InstanceType.Npc)
            {
                string[] pieces = properties.Split("\r\n");
                string spriteName = pieces[0];
                string[] restOfPieces = pieces.Skip(1).ToArray();
                var dialog = new Dialog(string.Join('\n', restOfPieces));
                var actor = new Npc(section, createPos, spriteName, dialog);
                return actor;
            }
            else if (instanceType == InstanceType.ShopItem)
            {
                // <itemName>,<price> OR random,<rarity>(1-5)
                string[] pieces = properties.Split(',');
                string itemName = pieces[0];
                int priceOrIndex = int.Parse(pieces[1]);

                if (itemName == "random")
                {
                    ItemType item = Items.items.Keys.ToList().GetRandomElement();
                    return new ShopItem(section, createPos, item, 60);
                }
                else
                {
                    Item? item = Items.items.Values.FirstOrDefault(i => i.name == itemName);
                    if (item == null)
                    {
                        return null;
                    }
                    return new ShopItem(section, createPos, item.itemType, priceOrIndex);
                }
            }
            else if (instanceType == InstanceType.WorldNumber)
            {
                return new WorldNumber(section, createPos, int.Parse(properties));
            }
            else if (instanceType == InstanceType.MasterSwordWoods)
            {
                return new MasterSwordWoods(section, createPos);
            }
            else if (instanceType == InstanceType.Fairy)
            {
                return new Fairy(section, createPos, false);
            }
            else if (instanceType == InstanceType.BigFairy)
            {
                return new BigFairy(section, createPos);
            }
            else if (instanceType == InstanceType.FightersSword)
            {
                var fi = new FieldItem(section, createPos.AddXY(0, -7), new InventoryItem(ItemType.sword1), FdPoint.Zero, false);
                fi.DisableComponent<ShadowComponent>();
                fi.yDir = -1;
                return fi;
            }

            return null;
        }
    }
}
